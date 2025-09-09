using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;

public class ExtinguisherOverlayHUD : MonoBehaviour
{
    [Header("Refs")]
    public XRGrabInteractable grab;  // 灭火器上的 XRGrabInteractable（可留空自动抓）
    public FireExtinguisher fx;      // 灭火器脚本（可留空自动抓）
    [Tooltip("要显示/隐藏的 HUD 根物体（例如 Canvas 里装着 BarBg/BarFill 的 Panel）")]
    public GameObject hudRoot;
    [Tooltip("BarBg 下的 BarFill（Image，Type=Filled, Horizontal, Origin=Left）")]
    public Image barFill;
    public TextMeshProUGUI label;    // 可选：显示 80/100

    [Header("Behavior")]
    public bool onlyShowWhenHeld = true;  // 只在抓住时显示
    public bool startHidden = true;       // 初始隐藏

    [Header("Color Gradient")]
    public bool useGradient = true;
    public Color lowColor = Color.red;                 // 0%
    public Color midColor = new Color(1f, 0.65f, 0f);  // ~50% 橙
    public Color highColor = Color.green;               // 100%

    int holdCount = 0;

    void Reset()
    {
        if (!grab) grab = GetComponent<XRGrabInteractable>();
        if (!fx) fx = GetComponent<FireExtinguisher>();
    }

    void Awake()
    {
        if (startHidden && hudRoot) hudRoot.SetActive(false);
    }

    void OnEnable()
    {
        if (!grab) grab = GetComponent<XRGrabInteractable>();
        if (grab)
        {
            grab.selectEntered.AddListener(OnGrab);
            grab.selectExited.AddListener(OnRelease);
        }
    }

    void OnDisable()
    {
        if (grab)
        {
            grab.selectEntered.RemoveListener(OnGrab);
            grab.selectExited.RemoveListener(OnRelease);
        }
    }

    void Update()
    {
        if (!fx || !barFill) return;

        float p01 = Mathf.Clamp01(fx.Pressure01);
        barFill.fillAmount = p01;

        if (useGradient)
        {
            // 0→0.5 用 红→橙，0.5→1 用 橙→绿
            Color c = (p01 < 0.5f)
                ? Color.Lerp(lowColor, midColor, p01 / 0.5f)
                : Color.Lerp(midColor, highColor, (p01 - 0.5f) / 0.5f);
            barFill.color = c;
        }

        if (label) label.text = $"{fx.CurrentPressure:0}/{fx.maxPressure:0}";
    }

    void OnGrab(SelectEnterEventArgs _)
    {
        holdCount++;
        if (hudRoot && onlyShowWhenHeld) hudRoot.SetActive(true);
    }

    void OnRelease(SelectExitEventArgs _)
    {
        holdCount = Mathf.Max(holdCount - 1, 0);
        if (hudRoot && onlyShowWhenHeld && holdCount == 0) hudRoot.SetActive(false);
    }
}