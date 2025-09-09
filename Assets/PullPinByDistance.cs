using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable), typeof(Rigidbody), typeof(Collider))]
public class PullPinByDistance : MonoBehaviour
{
    public ExtinguisherPASS_PullOnly1 target;

    [Header("Slot/Direction")]
    public Transform slotParent;
    public Vector3 localPullDir = Vector3.right; // 在 slotParent 的局部坐标
    public float pullDistance = 0.08f;           // 米
    public float startDeadzone = 0.005f;         // 米
    public float lateralTolerance = 0.015f;      // 米
    public bool requireForward = true;

    [Header("Behavior")]
    public bool autoUnparentOnPulled = true;
    public bool enableGravityAfterPulled = true;
    public float snapBackTime = 0.15f;
    public bool lockRotationWhileSeated = true;

    [Header("Debug")]
    public bool debugLogs = true;
    public float logEvery = 0.1f; // 秒
    [SerializeField] private float progress01;     // 0..1
    [SerializeField] private float signedDist;     // m（沿方向，带符号）
    [SerializeField] private float lateralOffset;  // m（横向偏移）

    public bool Pulled => pulled;
    public float Progress01 => progress01;
    public float SignedDist => signedDist;
    public float LateralOffset => lateralOffset;

    XRGrabInteractable grab;
    Rigidbody rb;
    Transform initialParent;

    Vector3 startLocalPos;
    Quaternion startLocalRot;
    bool grabbed = false;
    bool pulled = false;
    float lastLog;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        // ✅ 没拖就从父层级自动找 PASS 控制器，避免 target 为空
        if (!target) target = GetComponentInParent<ExtinguisherPASS_PullOnly1>();

        initialParent = transform.parent;
        if (!slotParent) slotParent = initialParent;

        startLocalPos = slotParent.InverseTransformPoint(transform.position);
        startLocalRot = Quaternion.Inverse(slotParent.rotation) * transform.rotation;

        rb.isKinematic = true;
        rb.useGravity = false;
        if (lockRotationWhileSeated) rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void OnEnable()
    {
        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }
    void OnDisable()
    {
        grab.selectEntered.RemoveListener(OnGrab);
        grab.selectExited.RemoveListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs _)
    {
        grabbed = true;
        rb.isKinematic = false;
        rb.useGravity = false;     // 抓在手里不受重力
        rb.constraints = RigidbodyConstraints.None;
        if (debugLogs) Debug.Log("[Pin] Grabbed", this);
    }

    void OnRelease(SelectExitEventArgs _)
    {
        grabbed = false;

        if (!pulled)
        {
            StartCoroutine(SnapBack());      // 未拔出 → 回槽
        }
        else
        {
            transform.SetParent(null, true); // 彻底脱离父级
            rb.isKinematic = false;
            rb.useGravity = true;           // ✅ 松手才掉地上
        }
    }

    System.Collections.IEnumerator SnapBack()
    {
        transform.SetParent(slotParent, true);

        Vector3 p0 = transform.position;
        Quaternion r0 = transform.rotation;
        Vector3 p1 = slotParent.TransformPoint(startLocalPos);
        Quaternion r1 = slotParent.rotation * startLocalRot;

        rb.isKinematic = true;
        rb.useGravity = false;

        float t = 0f;
        while (t < snapBackTime)
        {
            t += Time.deltaTime;
            float u = t / Mathf.Max(0.0001f, snapBackTime);
            u = u * u * (3f - 2f * u); // smoothstep
            transform.position = Vector3.Lerp(p0, p1, u);
            transform.rotation = Quaternion.Slerp(r0, r1, u);
            yield return null;
        }

        transform.position = p1;
        transform.rotation = r1;

        if (lockRotationWhileSeated) rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        if (pulled || !grabbed) return;

        // 以 slotParent 为参考计算
        Vector3 worldStart = slotParent.TransformPoint(startLocalPos);
        Vector3 worldDir = slotParent.TransformDirection(localPullDir).normalized;
        Vector3 delta = transform.position - worldStart;

        float s = Vector3.Dot(delta, worldDir);
        if (requireForward) s = Mathf.Max(0f, s);
        signedDist = s;

        Vector3 along = worldDir * s;
        Vector3 lateral = delta - along;
        lateralOffset = lateral.magnitude;

        float prog = 0f;
        if (lateralOffset <= lateralTolerance)
        {
            float usable = Mathf.Max(0f, s - startDeadzone);
            prog = Mathf.Clamp01(usable / Mathf.Max(0.0001f, (pullDistance - startDeadzone)));
        }
        progress01 = prog;

        if (debugLogs && Time.time >= lastLog + logEvery)
        {
            lastLog = Time.time;
            Debug.Log($"[Pin] prog={progress01:F2}  dist={signedDist * 100f:F1}cm  lateral={lateralOffset * 100f:F1}cm / tol={lateralTolerance * 100f:F1}cm", this);
        }

        if (progress01 >= 1f)
        {
            pulled = true;

            if (autoUnparentOnPulled)
                transform.SetParent(null, true);

            if (target) target.SetPinPulled();    // ✅ 通知 PASS → UI 切到 “S” 且允许喷

            rb.isKinematic = false;
            rb.useGravity = false;               // 刚拔出仍在手里，不下坠
            if (debugLogs) Debug.Log("[Pin] PULLED ✅", this);

        }
    }
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!slotParent) return;

        Vector3 start = Application.isPlaying ? slotParent.TransformPoint(startLocalPos) : transform.position;
        Vector3 dir   = slotParent.TransformDirection(localPullDir).normalized;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(start, start + dir * pullDistance);
        Gizmos.DrawSphere(start, 0.008f);
        Gizmos.DrawSphere(start + dir * pullDistance, 0.012f);

        // 容差圆
        DrawCircle(start, dir, lateralTolerance);
        DrawCircle(start + dir * pullDistance, dir, lateralTolerance);
    }
    static void DrawCircle(Vector3 c, Vector3 n, float r, int steps=32)
    {
        Vector3 t = Vector3.Cross(n, Vector3.up); if (t.sqrMagnitude < 1e-4f) t = Vector3.Cross(n, Vector3.right);
        t.Normalize();
        Vector3 b = Vector3.Cross(n, t);
        Vector3 prev = c + t * r;
        for (int i=1;i<=steps;i++){
            float a = (i/(float)steps)*Mathf.PI*2f;
            Vector3 p = c + (t*Mathf.Cos(a) + b*Mathf.Sin(a))*r;
            Gizmos.DrawLine(prev, p);
            prev = p;
        }
    }
#endif
}