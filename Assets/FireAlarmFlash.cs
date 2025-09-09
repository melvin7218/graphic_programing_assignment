using UnityEngine;

public class FireAlarmFlash : MonoBehaviour
{
    [Header("Alarm Light Settings")]
    public Light alarmLight;
    public float flashInterval = 1f;
    public float flashDuration = 0.05f;
    public float flashIntensity = 8f;

    [Header("Alarm Audio (Optional)")]
    public AudioSource alarmSound; // assign your alarm sound here

    private float timer = 0f;
    private bool alarmOn = false; // toggle state

    void Update()
    {
        if (!alarmOn || alarmLight == null) return;

        timer += Time.deltaTime;

        if (timer % flashInterval < flashDuration)
            alarmLight.intensity = flashIntensity;
        else
            alarmLight.intensity = 0f;
    }

    // --- Toggle Function (call this from VR button) ---
    public void ToggleAlarm()
    {
        alarmOn = !alarmOn;

        if (!alarmOn)
        {
            // Turn OFF everything
            alarmLight.intensity = 0f;
            if (alarmSound != null) alarmSound.Stop();
        }
        else
        {
            // Turn ON alarm
            timer = 0f; // reset timer
            if (alarmSound != null) alarmSound.Play();
        }
    }
}
