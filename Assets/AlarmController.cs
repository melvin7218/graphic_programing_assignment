using UnityEngine;

public class AlarmController : MonoBehaviour
{
    [Header("Alarm Components")]
    public Light alarmLight;           // Assign a Spot Light in Inspector
    public AudioSource alarmAudio;     // Assign AudioSource with alarm clip

    [Header("Settings")]
    public float flashSpeed = 5f;      // How fast the light flashes
    public float maxIntensity = 8f;    // Brightness of spotlight

    private bool isAlarmOn = false;

    void Start()
    {
        // Ensure light and audio are OFF at the start
        if (alarmLight != null)
        {
            alarmLight.enabled = false;
            alarmLight.intensity = 0f;
        }
        if (alarmAudio != null)
        {
            alarmAudio.Stop();
        }
    }

    void Update()
    {
        if (isAlarmOn && alarmLight != null)
        {
            // Flash spotlight intensity
            alarmLight.intensity = Mathf.PingPong(Time.time * flashSpeed, maxIntensity);
        }
    }

    public void ToggleAlarm()
    {
        isAlarmOn = !isAlarmOn;

        if (isAlarmOn)
        {
            if (alarmLight != null) alarmLight.enabled = true;
            if (alarmAudio != null && !alarmAudio.isPlaying) alarmAudio.Play();
        }
        else
        {
            if (alarmLight != null)
            {
                alarmLight.intensity = 0f;
                alarmLight.enabled = false;
            }
            if (alarmAudio != null && alarmAudio.isPlaying) alarmAudio.Stop();
        }
    }
}
