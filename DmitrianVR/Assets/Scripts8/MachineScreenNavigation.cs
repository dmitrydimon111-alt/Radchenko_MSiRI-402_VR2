using TMPro;
using UnityEngine;

public class MachineScreenNavigation : MonoBehaviour
{
    [Header("Панели интерфейса")]
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject machinePanel;
    [SerializeField] private GameObject programPanel;

    [Header("Информация о программе")]
    [SerializeField] private TMP_Text selectedProgramText;

    // Текущая выбранная программа
    public string SelectedProgram { get; private set; } = "";

    private void Start()
    {
        Debug.Log("ScreenManager запущен");

        // Начальное состояние интерфейса
        ShowHome();

        UpdateSelectedProgramText();
    }

    public void ShowHome()
    {
        Debug.Log("Вызвана функция ShowHome");

        homePanel.SetActive(true);
        machinePanel.SetActive(false);
        programPanel.SetActive(false);
    }

    public void ShowMachine()
    {
        Debug.Log("Вызвана функция ShowMachine");

        homePanel.SetActive(false);
        machinePanel.SetActive(true);
        programPanel.SetActive(false);
    }

    public void ShowPrograms()
    {
        Debug.Log("Вызвана функция ShowPrograms");

        homePanel.SetActive(false);
        machinePanel.SetActive(false);
        programPanel.SetActive(true);

        // Показываем сохранённый выбор, а не сбрасываем его
        UpdateSelectedProgramText();
    }

    // Выбор программы
    public void SelectProgram(string programName)
    {
        Debug.Log("Вызвана SelectProgram: " + programName);

        SelectedProgram = programName.Trim().ToLowerInvariant();

        UpdateSelectedProgramText();

        Debug.Log("Выбрана программа: " + SelectedProgram);
    }

    // Обновление текста на экране
    private void UpdateSelectedProgramText()
    {
        if (selectedProgramText == null)
            return;

        if (string.IsNullOrEmpty(SelectedProgram))
        {
            selectedProgramText.text = "Выберите программу";
        }
        else
        {
            selectedProgramText.text =
                "Выбрана программа: " + SelectedProgram;
        }
    }
}