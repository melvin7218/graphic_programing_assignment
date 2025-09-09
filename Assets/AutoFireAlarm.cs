using UnityEngine;
using System.Linq;

public class AutoFireAlarm : MonoBehaviour
{
    [Header("Alarm Settings")]
    public Light alarmLight;       // Assign your red alarm light in Inspector
    public AudioSource alarmSound; // Assign your alarm AudioSource in Inspector

    private ParticleSystem[] fireParticles;
    private bool isAlarmOn = false;

    void Start()
    {
        // Automatically find all ParticleSystems tagged as "Fire"
        fireParticles = GameObject.FindGameObjectsWithTag("Fire")
                                   .Select(go => go.GetComponent<ParticleSystem>())
                                   .Where(ps => ps != null)
                                   .ToArray();
    }

    void Update()
    {
        // Check if any fire particle system is still alive
        bool fireAlive = false;
        foreach (ParticleSystem ps in fireParticles)
        {
            if (ps.IsAlive(true)) // true = include children
            {
                fireAlive = true;
                break;
            }
        }

        // Toggle alarm based on fire status
        if (fireAlive && !isAlarmOn)
        {
            TurnAlarmOn();
        }
        else if (!fireAlive && isAlarmOn)
        {
            TurnAlarmOff();
        }
    }

    void TurnAlarmOn()
    {
        isAlarmOn = true;
        alarmLight.enabled = true;
        alarmSound.Play();
    }

    void TurnAlarmOff()
    {
        isAlarmOn = false;
        alarmLight.enabled = false;
        alarmSound.Stop();
    }
}
