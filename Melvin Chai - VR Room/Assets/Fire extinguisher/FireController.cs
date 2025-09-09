using UnityEngine;

public class FireController : MonoBehaviour
{
    public ParticleSystem fireFx;
    public Light fireLight;
    public AudioSource fireAudio;

    public float maxHeat = 100f;
    private float heat;
    private bool isOut;

    void Awake()
    {
        heat = maxHeat;
        UpdateFx(1f);
    }

    public void ApplyExtinguish(float amount)
    {
        if (isOut) return;

        heat = Mathf.Max(0f, heat - amount);
        UpdateFx(heat / maxHeat);

        if (heat <= 0f) Extinguish();
    }

    private void UpdateFx(float t)
    {
        if (fireFx)
        {
            var emission = fireFx.emission;
            emission.rateOverTime = Mathf.Lerp(0f, 60f, t);
        }

        if (fireLight) fireLight.intensity = Mathf.Lerp(0f, 2.5f, t);
        if (fireAudio) fireAudio.volume = Mathf.Lerp(0f, 0.9f, t);
    }

    private void Extinguish()
    {
        isOut = true;
        if (fireFx) fireFx.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        UpdateFx(0f);
    }
}
