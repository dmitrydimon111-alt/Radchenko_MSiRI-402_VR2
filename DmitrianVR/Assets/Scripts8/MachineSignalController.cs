using TMPro;
using UnityEngine;

public class MachineSignalController : MonoBehaviour
{
    [Header("Секции сигнальной лампы")]
    [SerializeField] private GameObject orangeLamp;
    [SerializeField] private GameObject greenLamp;
    [SerializeField] private GameObject redLamp;

    [Header("Сообщение об ошибке")]
    [SerializeField] private GameObject errorPanel;
    [SerializeField] private TMP_Text errorText;

    private void Awake()
    {
        if (errorPanel != null)
            errorPanel.SetActive(false);
    }

    private void Start()
    {
        SetIdle();
    }

    // Ожидание
    public void SetIdle()
    {
        orangeLamp.SetActive(true);
        greenLamp.SetActive(false);
        redLamp.SetActive(false);

        if (errorPanel != null)
            errorPanel.SetActive(false);

        if (errorText != null)
            errorText.text = "";
    }

    // Работа станка
    public void SetRunning()
    {
        orangeLamp.SetActive(false);
        greenLamp.SetActive(true);
        redLamp.SetActive(false);

        if (errorPanel != null)
            errorPanel.SetActive(false);

        if (errorText != null)
            errorText.text = "";
    }

    // Аварийное состояние
    public void SetError(string message)
    {
        orangeLamp.SetActive(false);
        greenLamp.SetActive(false);
        redLamp.SetActive(true);

        if (errorPanel != null)
            errorPanel.SetActive(true);

        if (errorText != null)
            errorText.text = "ОШИБКА: " + message;

        Debug.LogError("CNC: " + message);
    }
}

