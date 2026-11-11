using TMPro;
using UnityEngine;

public class MachinePanel : MonoBehaviour
{
    [Header("Текстовые поля")]
    public TMP_Text stateText;
    public TMP_Text axesText;
    public TMP_Text messageText;

    // Отображение координат
    public void ShowCoordinates(float x, float y, float z)
    {
        axesText.text =
            $"X: {x:F1} мм\n" +
            $"Y: {y:F1} мм\n" +
            $"Z: {z:F1} мм";
    }

    // Отображение состояния станка
    public void ShowState(bool running)
    {
        stateText.text = running
            ? "Состояние: РАБОТАЕТ"
            : "Состояние: ОСТАНОВЛЕН";
    }

    // Сообщения о подключении
    public void ShowMessage(string message)
    {
        messageText.text = message;
    }
}
