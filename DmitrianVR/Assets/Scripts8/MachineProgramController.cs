using System.Collections;
using TMPro;
using UnityEngine;

public class MachineProgramController : MonoBehaviour
{
    [Header("Навигация и выбор программы")]
    [SerializeField] private MachineScreenNavigation screenNavigation;

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
    [SerializeField] private float processingDuration = 12.5f;
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

    // Запуск программы обработки
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

        // Запуск программы
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

        // Показываем панель и выводим сообщение о начале обработки
        if (messagePanel != null)
            messagePanel.SetActive(true);

        if (programStatusText != null)
        {
            programStatusText.text =
                "Выполнение программы matritsa: в процессе";
        }

        StartCoroutine(ProcessProgram());
    }

    // Основной цикл обработки
    private IEnumerator ProcessProgram()
    {
        // Запоминаем начальные позиции моделей
        Vector3 workpieceStartPosition = workpiece.position;
        Vector3 finishedPartStartPosition = finishedPart.position;

        // Запоминаем начальные повороты моделей
        Quaternion workpieceStartRotation = workpiece.rotation;
        Quaternion finishedPartStartRotation = finishedPart.rotation;

        float elapsedTime = 0f;

        // Плавное перемещение моделей навстречу друг другу
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

        // Выводим сообщение о завершении обработки
        if (programStatusText != null)
        {
            programStatusText.text =
                "Выполнение программы matritsa: завершено";
        }

        Debug.Log("Программа MATRITSA завершена!");

        // Сообщаем системе обучения, что обработка завершена
        HasCompletedProgram = true;

        // Оставляем сообщение на экране на заданное время
        yield return new WaitForSeconds(completionMessageDuration);

        // Скрываем всю панель
        if (messagePanel != null)
            messagePanel.SetActive(false);

        // Очищаем текст
        if (programStatusText != null)
            programStatusText.text = "";

        // Завершаем программу и возвращаем станок в ожидание
        FinishProgram();
    }

    // Проверка закрытия всех дверей
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

    // Завершение программы
    public void FinishProgram()
    {
        if (!isProgramRunning)
            return;

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