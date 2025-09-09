using UnityEngine;

public class AudioLouder : MonoBehaviour
{
    public AudioSource source;

    void Start()
    {
        source.volume = 1.0f; // Max (0.0 to 1.0)
    }
}
