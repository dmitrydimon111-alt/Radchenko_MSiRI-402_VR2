using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MachineTrainingController : MonoBehaviour
{
    [System.Serializable]
    public class TrainingStep
    {
        public string stepName;

        [TextArea(3, 6)]
        public string taskDescription;

        public Sprite operationImage;
    }

    [Header("Связь с другими контроллерами")]
    [SerializeField] private MachineScreenNavigation screenNavigation;
    [SerializeField] private MachineProgramController programController;
    [SerializeField] private WorkpieceClampZone clampZone;
    [SerializeField] private VRSlidingDoor[] doors;

    [Header("Элементы тренировочного экрана")]
    [SerializeField] private TMP_Text stageNumberText;
    [SerializeField] private TMP_Text taskText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text conditionText;
    [SerializeField] private Image visualizationImage;

    [Header("Экран завершения обучения")]
    [SerializeField] private Sprite completionImage;

    [Header("Кнопки")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private TMP_Text nextButtonText;

    [Header("Этапы обучения MATRITSA")]
    [SerializeField] private TrainingStep[] trainingSteps = new TrainingStep[6];

    private int currentStageIndex = 0;
    private bool trainingCompleted = false;
    private bool lastConditionState;

    private void OnEnable()
    {
        UpdateTrainingScreen();
    }

    private void Update()
    {
        if (trainingCompleted || trainingSteps == null ||
            trainingSteps.Length == 0 ||
            currentStageIndex >= trainingSteps.Length)
            return;

        // Проверяем текущее условие в реальном времени
        bool conditionMet = IsCurrentStageCompleted();

        if (conditionMet != lastConditionState)
        {
            lastConditionState = conditionMet;
            UpdateTrainingScreen();
        }
    }

    // Проверка выполнения текущего этапа
    private bool IsCurrentStageCompleted()
    {
        switch (currentStageIndex)
        {
            case 0:
                // Заготовка установлена в тиски
                return clampZone != null &&
                       clampZone.IsWorkpieceInstalled;

            case 1:
                // Все двери закрыты
                return AreAllDoorsClosed();

            case 2:
                // Выбрана программа MATRITSA
                return screenNavigation != null &&
                       screenNavigation.SelectedProgram == "matritsa";

            case 3:
                // Программа запущена или уже завершена
                return programController != null &&
                       (programController.IsProgramRunning ||
                        programController.HasCompletedProgram);

            case 4:
                // Программа завершена
                return programController != null &&
                       programController.HasCompletedProgram;

            case 5:
                // Последний этап — подтверждение пользователя
                return true;

            default:
                return false;
        }
    }

    // Проверка состояния всех дверей
    private bool AreAllDoorsClosed()
    {
        if (doors == null || doors.Length == 0)
            return false;

        foreach (VRSlidingDoor door in doors)
        {
            if (door == null || !door.IsFullyClosed)
                return false;
        }

        return true;
    }

    // Переход вперёд
    public void NextStage()
    {
        if (trainingCompleted)
            return;

        if (trainingSteps == null ||
            currentStageIndex >= trainingSteps.Length)
            return;

        if (!IsCurrentStageCompleted())
        {
            Debug.LogWarning(
                "Сначала выполните задание текущего этапа!"
            );

            UpdateTrainingScreen();
            return;
        }

        // Завершение последнего этапа
        if (currentStageIndex == trainingSteps.Length - 1)
        {
            CompleteTraining();
            return;
        }

        currentStageIndex++;
        UpdateTrainingScreen();
    }

    // Переход назад
    public void PreviousStage()
    {
        if (currentStageIndex <= 0 || trainingCompleted)
            return;

        currentStageIndex--;
        UpdateTrainingScreen();
    }

    // Обновление тренировочного экрана
    private void UpdateTrainingScreen()
    {
        // Если обучение уже завершено, восстанавливаем поздравительный экран
        if (trainingCompleted)
        {
            ShowCompletionScreen();
            return;
        }

        if (trainingSteps == null || trainingSteps.Length == 0)
            return;

        if (currentStageIndex >= trainingSteps.Length)
            return;

        TrainingStep step = trainingSteps[currentStageIndex];

        bool conditionMet = IsCurrentStageCompleted();
        lastConditionState = conditionMet;

        if (stageNumberText != null)
        {
            stageNumberText.text =
                "НОМЕР ТЕКУЩЕГО ЭТАПА: " +
                (currentStageIndex + 1) + " / " +
                trainingSteps.Length +
                "\n" + step.stepName;
        }

        if (taskText != null)
            taskText.text = step.taskDescription;

        if (visualizationImage != null)
        {
            visualizationImage.sprite = step.operationImage;
            visualizationImage.enabled =
                step.operationImage != null;
        }

        if (progressText != null)
        {
            int progress = Mathf.RoundToInt(
                (float)currentStageIndex /
                trainingSteps.Length * 100f
            );

            progressText.text = "ПРОГРЕСС: " + progress + "%";
        }

        if (conditionText != null)
        {
            conditionText.text = conditionMet
                ? "ЗАДАНИЕ ВЫПОЛНЕНО. Можно продолжить."
                : GetConditionMessage();
        }

        if (nextButton != null)
            nextButton.interactable = conditionMet;

        if (previousButton != null)
            previousButton.interactable = currentStageIndex > 0;

        if (nextButtonText != null)
        {
            nextButtonText.text =
                currentStageIndex == trainingSteps.Length - 1
                ? "ЗАВЕРШИТЬ"
                : "ВПЕРЕД";
        }
    }

    // Сообщение о том, что нужно сделать
    private string GetConditionMessage()
    {
        switch (currentStageIndex)
        {
            case 0:
                return "Ожидание установки заготовки в тиски.";

            case 1:
                return "Закройте все защитные двери станка.";

            case 2:
                return "Выберите программу MATRITSA.";

            case 3:
                return "Перейдите к экрану станка и запустите программу.";

            case 4:
                return "Дождитесь завершения обработки.";

            case 5:
                return "Нажмите «ЗАВЕРШИТЬ» для окончания обучения.";

            default:
                return "";
        }
    }

    // Завершение обучения
    private void CompleteTraining()
    {
        trainingCompleted = true;
        ShowCompletionScreen();

        Debug.Log("Обучение MATRITSA успешно завершено!");
    }

    // Отображение поздравительного экрана
    private void ShowCompletionScreen()
    {
        if (stageNumberText != null)
            stageNumberText.text = "ОБУЧЕНИЕ ЗАВЕРШЕНО";

        if (taskText != null)
        {
            taskText.text =
                "Поздравляем! Вы успешно прошли все этапы " +
                "обучения по программе MATRITSA.";
        }

        if (progressText != null)
            progressText.text = "ПРОГРЕСС: 100%";

        if (conditionText != null)
            conditionText.text = "Практическое обучение завершено.";

        // Показываем поздравительное изображение
        if (visualizationImage != null)
        {
            visualizationImage.sprite = completionImage;
            visualizationImage.enabled = completionImage != null;
        }

        if (nextButton != null)
            nextButton.interactable = false;

        if (previousButton != null)
            previousButton.interactable = false;

        if (nextButtonText != null)
            nextButtonText.text = "ЗАВЕРШЕНО";
    }
}