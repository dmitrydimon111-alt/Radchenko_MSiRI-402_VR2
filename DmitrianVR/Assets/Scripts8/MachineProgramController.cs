using System.Collections;
using TMPro;
using UnityEngine;

public class MachineProgramController : MonoBehaviour
{
    [Header("Навигация и выбор программы")]
    [SerializeField] private MachineScreenNavigation screenNavigation;

    [Header("Считывание координат CNC")]
    [SerializeField] private CncJson8 cncJson;

    [Header("Сигнальная лампа")]
    [SerializeField] private MachineSignalController signalController;

    [Header("Двери станка")]
    [SerializeField] private VRSlidingDoor[] doors;

    [Header("Проверка установки заготовки")]
    [SerializeField] private WorkpieceClampZone clampZone;

    [Header("Модели для обработки")]
    [SerializeField] private Transform workpiece;
    [SerializeField] private Transform finishedPart;

    [Header("Отображение программы")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TMP_Text programStatusText;

    [Header("Настройки обработки")]
    [SerializeField] private float processingDuration = 60f;
    [SerializeField] private float completionMessageDuration = 2f;

    private bool isProgramRunning = false;

    // Свойства для MachineTrainingController
    public bool IsProgramRunning => isProgramRunning;
    public bool HasCompletedProgram { get; private set; } = false;

    private void Start()
    {
        // Скрываем панель сообщения при запуске сцены
        if (messagePanel != null)
            messagePanel.SetActive(false);

        // Очищаем текст сообщения
        if (programStatusText != null)
            programStatusText.text = "";

        HasCompletedProgram = false;
    }

    // ЗАПУСК ПРОГРАММЫ ОБРАБОТКИ
    public void StartProgram()
    {
        if (isProgramRunning)
            return;

        // Проверка выбранной программы
        if (screenNavigation == null)
        {
            Debug.LogError(
                "CNC: Не назначен MachineScreenNavigation!"
            );
            return;
        }

        if (screenNavigation.SelectedProgram != "matritsa")
        {
            Debug.LogWarning(
                "Выбрана неверная программа для обработки!"
            );
            return;
        }

        // Проверка назначенных моделей
        if (workpiece == null || finishedPart == null)
        {
            Debug.LogError(
                "CNC: Не назначены модели заготовки и готовой детали!"
            );
            return;
        }

        // Проверка установки заготовки в тисках
        if (clampZone == null || !clampZone.IsWorkpieceInstalled)
        {
            Debug.LogWarning(
                "Установите заготовку в тиски перед запуском программы!"
            );
            return;
        }

        // Проверка закрытия дверей
        if (!AreDoorsClosed())
        {
            if (signalController != null)
                signalController.SetError("Двери станка не закрыты!");

            Debug.LogWarning("Запуск программы заблокирован!");
            return;
        }

        // Сбрасываем признак завершения предыдущей обработки
        HasCompletedProgram = false;

        // Запускаем программу
        isProgramRunning = true;

        // Блокируем двери
        foreach (VRSlidingDoor door in doors)
        {
            if (door != null)
                door.SetLocked(true);
        }

        // Включаем зелёную лампу
        if (signalController != null)
            signalController.SetRunning();

        // Показываем панель сообщения
        if (messagePanel != null)
            messagePanel.SetActive(true);

        if (programStatusText != null)
        {
            programStatusText.text =
                "Выполнение программы matritsa: в процессе";
        }

        // Одновременно запускаем считывание координат
        if (cncJson != null)
        {
            cncJson.StartReading();
        }
        else
        {
            Debug.LogWarning(
                "CNC: Не назначен CncJson8. " +
                "Обработка запущена без считывания координат."
            );
        }

        // Запускаем цикл обработки
        StartCoroutine(ProcessProgram());

        Debug.Log("CNC: Программа MATRITSA запущена.");
    }

    // ОСНОВНОЙ ЦИКЛ ОБРАБОТКИ
    private IEnumerator ProcessProgram()
    {
        // Запоминаем начальные позиции моделей
        Vector3 workpieceStartPosition = workpiece.position;
        Vector3 finishedPartStartPosition = finishedPart.position;

        // Запоминаем начальные повороты моделей
        Quaternion workpieceStartRotation = workpiece.rotation;
        Quaternion finishedPartStartRotation = finishedPart.rotation;

        float elapsedTime = 0f;

        // Плавное перемещение моделей
        while (elapsedTime < processingDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsedTime / processingDuration
            );

            workpiece.position = Vector3.Lerp(
                workpieceStartPosition,
                finishedPartStartPosition,
                progress
            );

            finishedPart.position = Vector3.Lerp(
                finishedPartStartPosition,
                workpieceStartPosition,
                progress
            );

            workpiece.rotation = Quaternion.Slerp(
                workpieceStartRotation,
                finishedPartStartRotation,
                progress
            );

            finishedPart.rotation = Quaternion.Slerp(
                finishedPartStartRotation,
                workpieceStartRotation,
                progress
            );

            yield return null;
        }

        // Фиксируем конечные позиции моделей
        workpiece.position = finishedPartStartPosition;
        finishedPart.position = workpieceStartPosition;

        workpiece.rotation = finishedPartStartRotation;
        finishedPart.rotation = workpieceStartRotation;

        // Останавливаем считывание координат
        if (cncJson != null)
            cncJson.StopReading();

        // Выводим сообщение о завершении обработки
        if (programStatusText != null)
        {
            programStatusText.text =
                "Выполнение программы matritsa: завершено";
        }

        Debug.Log("Программа MATRITSA завершена!");

        // Сообщаем системе обучения о завершении обработки
        HasCompletedProgram = true;

        // Оставляем сообщение на экране на заданное время
        yield return new WaitForSeconds(completionMessageDuration);

        // Скрываем панель
        if (messagePanel != null)
            messagePanel.SetActive(false);

        // Очищаем текст
        if (programStatusText != null)
            programStatusText.text = "";

        // Завершаем программу
        FinishProgram();
    }

    // ПРОВЕРКА ЗАКРЫТИЯ ВСЕХ ДВЕРЕЙ
    private bool AreDoorsClosed()
    {
        if (doors == null || doors.Length == 0)
        {
            Debug.LogError("CNC: Не назначены двери станка!");
            return false;
        }

        foreach (VRSlidingDoor door in doors)
        {
            if (door == null || !door.IsFullyClosed)
                return false;
        }

        return true;
    }

    // ЗАВЕРШЕНИЕ ПРОГРАММЫ
    public void FinishProgram()
    {
        if (!isProgramRunning)
            return;

        // На всякий случай останавливаем считывание координат
        if (cncJson != null)
            cncJson.StopReading();

        isProgramRunning = false;

        // Разблокируем двери
        foreach (VRSlidingDoor door in doors)
        {
            if (door != null)
                door.SetLocked(false);
        }

        // Возвращаем оранжевую лампу
        if (signalController != null)
            signalController.SetIdle();

        Debug.Log("Станок перешёл в режим ожидания.");
    }
}