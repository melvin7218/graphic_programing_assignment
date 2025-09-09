using UnityEngine;

public class VRButtonToggle : MonoBehaviour
{
    [Header("Alarm Settings")]
    public Light alarmLight;          // assign in inspector
    public AudioSource alarmSound;    // assign in inspector

    private bool isAlarmOn = false;   // state tracking

    private void OnTriggerEnter(Collider other)
    {
        // Check if the trigger is from a VR controller
        if (other.CompareTag("VRController"))
        {
            ToggleAlarm();
        }
    }

    void ToggleAlarm()
    {
        isAlarmOn = !isAlarmOn; // toggle state

        // Toggle light and sound
        alarmLight.enabled = isAlarmOn;

        if (isAlarmOn)
            alarmSound.Play();
        else
            alarmSound.Stop();
    }
}
