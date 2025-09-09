using UnityEngine;
using System.Collections;

public class ExplosionController : MonoBehaviour
{
    [Header("Explosion")]
    [SerializeField] private ParticleSystem explosionPrefab;   // assign PS_Explosion prefab
    [SerializeField] private AudioClip explosionSfx;           // optional sound
    [SerializeField] private float explosionDelay = 3f;        // time after game start

    [Header("Fire")]
    [SerializeField] private FireController[] fireObjects;     // drag your Fire objects here
    [SerializeField] private float fireDelay = 2f;             // time after explosion
    [SerializeField] private float spreadInterval = 0f;        // >0 to make fire appear one by one

    private void Start()
    {
        // ensure all fires are disabled at start
        foreach (var fire in fireObjects)
        {
            if (fire) fire.gameObject.SetActive(false);
        }

        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        // wait before explosion
        yield return new WaitForSeconds(explosionDelay);

        // spawn explosion effect
        if (explosionPrefab)
        {
            var ps = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            ps.Play();
        }

        if (explosionSfx)
        {
            AudioSource.PlayClipAtPoint(explosionSfx, transform.position);
        }

        // wait before enabling fire
        yield return new WaitForSeconds(fireDelay);

        for (int i = 0; i < fireObjects.Length; i++)
        {
            var fire = fireObjects[i];
            if (!fire) continue;

            fire.gameObject.SetActive(true); // enable fire object
            fire.StartFire();                // trigger fire FX

            if (spreadInterval > 0f)
                yield return new WaitForSeconds(spreadInterval);
        }
    }
}
