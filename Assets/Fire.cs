// FireController.cs
using UnityEngine;

public class Fire : MonoBehaviour
{
    [Header("FX")]
    public ParticleSystem fireFx;
    public Light fireLight;        // optional
    public AudioSource fireAudio;  // optional

    [Header("Tuning")]
    public float maxHeat = 100f;   // total "health"
    public float reigniteDelay = 0f; // 0 = never reignite automatically

    float heat;
    bool isOut;

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

    void UpdateFx(float t)
    {
        if (fireFx)
        {
            var emission = fireFx.emission;
            emission.rateOverTime = Mathf.Lerp(0f, 60f, t); // scale with heat

            var main = fireFx.main;
            main.startSize = Mathf.Lerp(0.05f, 0.6f, t);
        }
        if (fireLight) fireLight.intensity = Mathf.Lerp(0f, 2.5f, t);
        if (fireAudio) fireAudio.volume   = Mathf.Lerp(0f, 0.9f, t);
    }

    void Extinguish()
    {
        isOut = true;
        UpdateFx(0f);
        if (fireFx) fireFx.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        // Optional: enable smoke/char effects here
        if (reigniteDelay > 0f) Invoke(nameof(Reignite), reigniteDelay);
    }

    void Reignite()
    {
        isOut = false;
        heat = maxHeat * 0.3f; // partial reignite if you want
        if (fireFx) fireFx.Play();
        UpdateFx(heat / maxHeat);
    }
}
