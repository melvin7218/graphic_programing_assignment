using UnityEngine;

public class ExplodeOnProximity : MonoBehaviour
{
    [Header("Trigger")]
    public bool oneShot = true;                 // 只触发一次
    public string triggerTag = "Player";        // 触发者的Tag（也可用 MainCamera）

    [Header("Audio")]
    public AudioClip explosionSfx;              // 爆炸音效（可空）
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("Effect (启用已有对象)")]
    public GameObject effectObjectToEnable;     // 例如场景里放好的火焰对象（初始会被隐藏）

    [Header("Effect (实例化预制体 - 第一个)")]
    public GameObject explosionPrefab;          // 例如爆炸粒子预制体（可空）
    public float prefabAutoDestroyDelay = 5f;   // 生成后多少秒销毁，<=0 不销毁

    [Header("Effect (实例化预制体 - 第二个)")]
    public GameObject secondExplosionPrefab;    // 第二个爆炸预制体（可空）
    public float secondPrefabAutoDestroyDelay = 5f; // 第二个预制体的销毁延迟

    [Header("Force (可选)")]
    public float explosionForce = 600f;         // 爆炸力
    public float explosionRadius = 4f;          // 半径
    public LayerMask forceLayers = ~0;          // 受力的层

    AudioSource src;
    bool fired;

    void Awake()
    {
        // 准备音源
        src = GetComponent<AudioSource>();
        if (!src) src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.spatialBlend = 1f;   // 3D 声音

        // 预先隐藏"已有特效对象"
        if (effectObjectToEnable) effectObjectToEnable.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (fired && oneShot) return;

        // 触发者匹配：允许 Player 或 MainCamera（你也可只保留一个）
        bool ok = other.CompareTag(triggerTag) || other.CompareTag("MainCamera");
        if (!ok) return;

        fired = true;

        // 1) 播放音效
        if (explosionSfx) src.PlayOneShot(explosionSfx, sfxVolume);

        // 2) 启用场景中已有的特效对象（例如常驻火焰）
        if (effectObjectToEnable) effectObjectToEnable.SetActive(true);

        // 3) 实例化第一个爆炸预制体（例如一次性粒子）
        if (explosionPrefab)
        {
            var go = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            if (prefabAutoDestroyDelay > 0f) Destroy(go, prefabAutoDestroyDelay);
        }

        // 4) 实例化第二个爆炸预制体（完全相同的设置）
        if (secondExplosionPrefab)
        {
            var secondGo = Instantiate(secondExplosionPrefab, transform.position, Quaternion.identity);
            if (secondPrefabAutoDestroyDelay > 0f) Destroy(secondGo, secondPrefabAutoDestroyDelay);
        }

        // 5) 对周围可受力物体施加爆炸力（可选）
        if (explosionForce > 0f && explosionRadius > 0f)
        {
            var cols = Physics.OverlapSphere(transform.position, explosionRadius, forceLayers);
            foreach (var c in cols)
            {
                if (c.attachedRigidbody)
                    c.attachedRigidbody.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }
    }
}