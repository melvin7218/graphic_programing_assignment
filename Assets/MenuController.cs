using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] GameObject aboutPanel;
    [SerializeField] GameObject buttonGroup;

    [Header("XR Settings")]
    [SerializeField] bool autoConfigureXR = true;

    void Start()
    {
        if (autoConfigureXR)
            ConfigureXRUI();

        // 确保初始状态正确
        if (aboutPanel != null)
            aboutPanel.SetActive(false);

        if (buttonGroup != null)
            buttonGroup.SetActive(true);
    }

    void ConfigureXRUI()
    {
        // 确保Canvas有TrackedDeviceGraphicRaycaster
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.GetComponent<TrackedDeviceGraphicRaycaster>() == null)
        {
            canvas.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
        }

        // 确保EventSystem使用XR输入
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem != null)
        {
            // 移除标准输入模块
            StandaloneInputModule stdInput = eventSystem.GetComponent<StandaloneInputModule>();
            if (stdInput != null)
                Destroy(stdInput);

            // 添加XR输入模块
            if (eventSystem.GetComponent<XRUIInputModule>() == null)
                eventSystem.gameObject.AddComponent<XRUIInputModule>();
        }
    }

    public void OnAbout()
    {
        if (aboutPanel != null) aboutPanel.SetActive(true);
        if (buttonGroup != null) buttonGroup.SetActive(false);
    }

    public void OnCloseAbout()
    {
        if (aboutPanel != null) aboutPanel.SetActive(false);
        if (buttonGroup != null) buttonGroup.SetActive(true);
    }
}