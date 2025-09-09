using UnityEngine;

public class FireAlarmTwinkle : MonoBehaviour
{
    public Light alarmLight;          // Assign your Spot Light in Inspector
    public float flashSpeed = 4f;     // Speed of flashing
    public float maxIntensity = 8f;   // Brightest point
    public float minIntensity = 0f;   // Darkest point

    private bool increasing = true;
    private float currentIntensity;

    void Start()
    {
        if (alarmLight == null)
            alarmLight = GetComponentInChildren<Light>();

        if (alarmLight.type != LightType.Spot)
            Debug.LogWarning("FireAlarmTwinkle expects a Spot Light!");

        currentIntensity = minIntensity;
        alarmLight.intensity = currentIntensity;
    }

    void Update()
    {
        // Increase/Decrease intensity for twinkle effect
        if (increasing)
        {
            currentIntensity += flashSpeed * Time.deltaTime;
            if (currentIntensity >= maxIntensity)
            {
                currentIntensity = maxIntensity;
                increasing = false;
            }
        }
        else
        {
            currentIntensity -= flashSpeed * Time.deltaTime;
            if (currentIntensity <= minIntensity)
            {
                currentIntensity = minIntensity;
                increasing = true;
            }
        }

        alarmLight.intensity = currentIntensity;
    }
}
