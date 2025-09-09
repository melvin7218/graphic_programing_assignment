using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class AlarmButton : MonoBehaviour
{
    public AlarmController alarmController;

    private void Awake()
    {
        var interactable = GetComponent<XRBaseInteractable>();
        interactable.selectEntered.AddListener(OnPressed);
    }

    private void OnPressed(SelectEnterEventArgs args)
    {
        alarmController.ToggleAlarm();
    }
}
