using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable), typeof(Rigidbody))]
public class SafetyPinXR : MonoBehaviour
{
    public bool IsEngaged { get; private set; } = true;

    XRGrabInteractable grab;
    Rigidbody rb;
    Transform originalParent;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        originalParent = transform.parent;

        // Lock at start so it can't fly
        rb.isKinematic = true;
        rb.useGravity  = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Ensure attachTransform has no weird offset
        if (grab.attachTransform == null)
        {
            var t = new GameObject("PinAttach").transform;
            t.SetPositionAndRotation(transform.position, transform.rotation);
            t.SetParent(transform);   // local zero
            grab.attachTransform = t;
        }

        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    void OnDestroy()
    {
        grab.selectEntered.RemoveListener(OnGrab);
        grab.selectExited.RemoveListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        if (!IsEngaged) return;

        // Detach from extinguisher when first grabbed
        transform.SetParent(null, true);
        rb.isKinematic = false;
        rb.useGravity  = true;
        IsEngaged = false;

        // Allow movement when held
        rb.constraints = RigidbodyConstraints.None;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        // After release it just falls naturally
        rb.useGravity = true;
        rb.isKinematic = false;
    }

    // Optional utility to reset for testing
    [ContextMenu("Reset Pin")]
    void ResetPin()
    {
        IsEngaged = true;
        transform.SetParent(originalParent, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        rb.isKinematic = true;
        rb.useGravity  = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }
}
