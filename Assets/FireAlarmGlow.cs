using UnityEngine;

public class FireAlarmGlow : MonoBehaviour
{
    public Renderer alarmRenderer;    // Assign the mesh (the red bulb)
    public Color glowColor = Color.red;
    public float flashSpeed = 4f;     // Flash frequency
    public float maxEmission = 3f;    // Bright glow
    public float minEmission = 0f;    // Dark

    private Material mat;

    void Start()
    {
        if (alarmRenderer == null)
            alarmRenderer = GetComponent<Renderer>();

        mat = alarmRenderer.material;
    }

    void Update()
    {
        float emission = Mathf.Abs(Mathf.Sin(Time.time * flashSpeed));
        Color finalColor = glowColor * Mathf.LinearToGammaSpace(
            Mathf.Lerp(minEmission, maxEmission, emission)
        );
        mat.SetColor("_EmissionColor", finalColor);
    }
}
