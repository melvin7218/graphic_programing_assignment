using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(XRGrabInteractable))]
public class ExtinguisherPASS_PullOnly1 : MonoBehaviour
{
    [Header("Lock/FX")]
    public FireExtinguisher sprayer;     // 你的喷射脚本（必填）
    [Tooltip("可选：喷射相关音效（在未解锁时会被Stop）")]
    public AudioSource spraySFX;         // 可选：本脚本内的独立音源引用

    [Header("UI (二选一)")]
    public Text uiText;                  // 旧UGUI Text（可留空）
    public TMP_Text tmpText;             // TMP文本（推荐）

    XRGrabInteractable grab;
    bool grabbed = false;
    bool pinPulled = false;

    // 对外只读
    public bool IsPinPulled => pinPulled;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();

        // 开局上锁并关掉特效
        LockSprayer(true);
        SetPrompt("Grab the extinguisher");
    }

    void OnEnable()
    {
        grab.selectEntered.AddListener(HandleEnter);
        grab.selectExited.AddListener(HandleExit);
    }

    void OnDisable()
    {
        grab.selectEntered.RemoveListener(HandleEnter);
        grab.selectExited.RemoveListener(HandleExit);
    }

    void HandleEnter(SelectEnterEventArgs _)
    {
        grabbed = true;
        UpdateStateAndPrompt();
    }

    void HandleExit(SelectExitEventArgs _)
    {
        grabbed = false;
        UpdateStateAndPrompt();
    }

    // 🔔 由“插销脚本”（如 PullPinByDistancePro）在拔出时调用
    public void SetPinPulled()
    {
        if (pinPulled) return;
        pinPulled = true;
        UpdateStateAndPrompt();
    }

    void UpdateStateAndPrompt()
    {
        bool armed = grabbed && pinPulled;     // 被抓住 + 已拔销 → 解锁
        LockSprayer(!armed);

        if (!grabbed) { SetPrompt("Grab the extinguisher"); return; }
        if (!pinPulled) { SetPrompt("P: Pull the pin"); return; }
        /* 跳过 AIM，直接 S 阶段 */
        SetPrompt("Ready to spray!");
    }

    void LockSprayer(bool lockIt)
    {
        if (!sprayer) return;

        if (lockIt)
        {
            // 为保险：停喷，关粒子/音效，再禁用脚本
            if (sprayer.sprayParticles)
            {
                var em = sprayer.sprayParticles.emission;
                em.enabled = false;
                sprayer.sprayParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            if (spraySFX) spraySFX.Stop();

            sprayer.enabled = false;
        }
        else
        {
            // 解锁只需启用脚本；真正喷射仍由玩家扳机触发
            sprayer.enabled = true;

            // 粒子系统由 FireExtinguisher 的 Start/Update 控制
            // 不主动 Play，等 StartSpraying() 时再 Play
        }
    }

    void SetPrompt(string s)
    {
        if (tmpText) tmpText.text = s;
        if (uiText) uiText.text = s;
    }
}
