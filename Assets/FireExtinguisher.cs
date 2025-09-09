using UnityEngine;
using UnityEngine.InputSystem;

public class FireExtinguisher : MonoBehaviour
{
    [Header("FX")]
    public ParticleSystem sprayParticles;
    public Transform nozzle;              // tip of the extinguisher

    [Header("Spray Geometry")]
    public float range = 3f;              // meters
    [Range(1f, 45f)] public float coneAngle = 20f; // degrees half-angle
    public LayerMask fireMask;            // set this to your "Fire" layer ONLY

    [Header("Effect")]
    public float dps = 35f;               // damage per second at perfect distance/angle
    public AnimationCurve distanceFalloff = AnimationCurve.Linear(0,1, 1,0.3f);

    [Header("Pressure")]
    public float maxPressure = 100f;
    public float consumePerSec = 10f;     // how fast we spend pressure while spraying
    public float rechargePerSec = 2f;     // how fast we recharge when idle
    public float Pressure01 => Mathf.Clamp01(currentPressure / maxPressure);  // 0..1
    public float CurrentPressure => currentPressure;
    // Input
    private InputAction triggerAction;

    // State
    private bool isSpraying = false;
    private float currentPressure;

    void Awake()
    {
        // Robust XR trigger as a button with a press threshold
        triggerAction = new InputAction(
            name: "Trigger",
            type: InputActionType.Button
        );
        triggerAction.AddBinding("<XRController>{RightHand}/trigger")
                     .WithInteraction("press(pressPoint=0.15)");
        triggerAction.AddBinding("<Mouse>/rightButton"); // Fallback (mouse)
    }

    void OnEnable()
    {
        triggerAction.Enable();
        triggerAction.performed += _ => StartSpraying();
        triggerAction.canceled  += _ => StopSpraying();
    }

    void OnDisable()
    {
        triggerAction.performed -= _ => StartSpraying();
        triggerAction.canceled  -= _ => StopSpraying();
        triggerAction.Disable();
    }

    void Start()
    {
        currentPressure = maxPressure;

        // Make sure particles are world-space so the jet doesn’t bend when moving
        if (sprayParticles)
        {
            var main = sprayParticles.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            sprayParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    void Update()
    {
        // Optional extra safety: if trigger is analog and events miss, poll value
        if (triggerAction != null)
        {
            // If you prefer polling instead of events, uncomment:
            // bool pressed = triggerAction.IsPressed();
            // if (pressed && !isSpraying) StartSpraying();
            // if (!pressed && isSpraying) StopSpraying();
        }

        // Pressure management
        if (isSpraying)
        {
            currentPressure -= consumePerSec * Time.deltaTime;
            if (currentPressure <= 0f)
            {
                currentPressure = 0f;
                StopSpraying();
            }
            else
            {
                // Apply effect while spraying
                SprayConeHit();

                // Scale emission with remaining pressure so the jet weakens
                if (sprayParticles)
                {
                    float p01 = Mathf.Clamp01(currentPressure / maxPressure);
                    var emission = sprayParticles.emission;
                    emission.rateOverTime = Mathf.Lerp(8f, 60f, p01); // tweak to taste
                }
            }
        }
        else if (currentPressure < maxPressure)
        {
            currentPressure = Mathf.Min(maxPressure, currentPressure + rechargePerSec * Time.deltaTime);
        }
    }

    void StartSpraying()
    {
        if (isSpraying || currentPressure <= 0f) return;
        isSpraying = true;
        if (sprayParticles)
        {
            var em = sprayParticles.emission; em.enabled = true;   // 保险起见
            sprayParticles.Play(true);
            Debug.Log("[FX] play particles, pressure=" + currentPressure);
        }
    }

    void StopSpraying()
    {
        if (!isSpraying) return;
        isSpraying = false;
        if (sprayParticles) sprayParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    void SprayConeHit()
    {
        if (!nozzle) return;

        // Use an overlap to find candidate colliders in a sphere ahead of the nozzle
        Vector3 origin = nozzle.position;
        Collider[] hits = Physics.OverlapSphere(origin, range, fireMask, QueryTriggerInteraction.Ignore);

        foreach (var hit in hits)
        {
            // Vector from nozzle to the closest point on the collider
            Vector3 toHit = hit.ClosestPoint(origin) - origin;
            float dist = toHit.magnitude;
            if (dist < 0.0001f) continue;

            // Cone check
            float angle = Vector3.Angle(nozzle.forward, toHit / dist);
            if (angle > coneAngle) continue;

            // Distance falloff (0..1)
            float w = distanceFalloff.Evaluate(Mathf.Clamp01(dist / range));

            // Apply DPS
            if (hit.TryGetComponent<FireController>(out var fire))
            {
                fire.ApplyExtinguish(dps * w * Time.deltaTime);
            }
            else
            {
                var parent = hit.GetComponentInParent<FireController>();
                if (parent) parent.ApplyExtinguish(dps * w * Time.deltaTime);
            }
        }
    }

#if UNITY_EDITOR
    // Gizmos to visualize cone/range
    void OnDrawGizmosSelected()
    {
        if (!nozzle) return;
        Gizmos.color = new Color(0.6f, 0.8f, 1f, 0.35f);
        Gizmos.DrawWireSphere(nozzle.position, range);

        // Draw a few rays to indicate cone
        int steps = 12;
        for (int i = 0; i < steps; i++)
        {
            float yaw = (360f / steps) * i;
            Quaternion rot = Quaternion.AngleAxis(yaw, nozzle.forward) * Quaternion.AngleAxis(coneAngle, nozzle.right);
            Vector3 dir = rot * nozzle.forward;
            Gizmos.DrawRay(nozzle.position, dir * range);
        }
    }
#endif
}
