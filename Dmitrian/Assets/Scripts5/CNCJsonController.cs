using System;
using System.Collections;
using System.Globalization;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class CncJsonController : MonoBehaviour
{
    [Header("Подключение к серверу")]
    [SerializeField] private string url = "https://cnc.kovalev.team/get/3";
    [SerializeField] private float interval = 0.5f;

    [Header("Узлы станка")]
    [SerializeField] private Transform nodeX;
    [SerializeField] private Transform nodeY;
    [SerializeField] private Transform nodeZ;

    [Header("Направления движения")]
    [SerializeField] private Vector3 directionX = Vector3.right;
    [SerializeField] private Vector3 directionY = Vector3.up;
    [SerializeField] private Vector3 directionZ = Vector3.forward;

    [Header("Масштаб перемещения")]
    [SerializeField] private float movementScale = 0.001f;
    [SerializeField] private float yMovementMultiplier = 0.01f;

    private Vector3 startX;
    private Vector3 startY;
    private Vector3 startZ;

    private Coroutine readingCoroutine;

    private void Start()
    {
        if (nodeX == null || nodeY == null || nodeZ == null)
        {
            Debug.LogError("CNC: Не назначены узлы станка.");
            enabled = false;
            return;
        }

        if (nodeY.parent != nodeX)
        {
            Debug.LogError(
                "CNC: Spindle front (Y) должен быть дочерним объектом Spindle front (X).");
            enabled = false;
            return;
        }

        startX = nodeX.localPosition;
        startY = nodeY.localPosition;
        startZ = nodeZ.localPosition;

        Debug.Log("CNC: Контроллер готов.");

        StartReading();
    }

    public void StartReading()
    {
        if (!enabled || readingCoroutine != null)
            return;

        readingCoroutine = StartCoroutine(ReadCoordinates());
        Debug.Log("CNC: Считывание запущено.");
    }

    public void StopReading()
    {
        if (readingCoroutine == null)
            return;

        StopCoroutine(readingCoroutine);
        readingCoroutine = null;

        Debug.Log("CNC: Считывание остановлено.");
    }

    private IEnumerator ReadCoordinates()
    {
        while (true)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = 10;

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        JObject json = JObject.Parse(
                            request.downloadHandler.text);

                        float x = GetCoordinate(json, "X");
                        float y = GetCoordinate(json, "Y");
                        float z = GetCoordinate(json, "Z");

                        // Ось X: перемещение шпинделя
                        nodeX.localPosition =
                            startX + directionX.normalized * x * movementScale;

                        // Ось Y: перемещение относительно шпинделя X
                        nodeY.localPosition =
                            startY + directionY.normalized * y * movementScale * yMovementMultiplier;

                        // Ось Z: перемещение опоры стола
                        nodeZ.localPosition =
                            startZ + directionZ.normalized * z * movementScale;

                        Debug.Log(
                            $"CNC: X={x:F3}; Y={y:F3}; Z={z:F3} мм");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(
                            "CNC: Ошибка обработки данных: " + e.Message);
                    }
                }
                else
                {
                    Debug.LogError(
                        "CNC: Ошибка подключения: " + request.error);
                }
            }

            yield return new WaitForSecondsRealtime(
                Mathf.Max(0.1f, interval));
        }
    }

    private float GetCoordinate(JObject json, string axis)
    {
        string target = "Перемещение по оси " + axis;

        foreach (JToken token in json.DescendantsAndSelf())
        {
            if (!(token is JArray row) || row.Count < 2)
                continue;

            if (row[0].Type != JTokenType.String)
                continue;

            string name = row[0].ToString().Trim()
                .Replace('Х', 'X')
                .Replace('х', 'x');

            if (!string.Equals(
                name,
                target,
                StringComparison.OrdinalIgnoreCase))
                continue;

            string valueText = row[1].ToString()
                .Replace("мм", "")
                .Replace("MM", "")
                .Replace(',', '.')
                .Trim();

            if (float.TryParse(
                valueText,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float value))
            {
                return value;
            }

            throw new FormatException(
                "Некорректное значение оси " + axis + ": " + valueText);
        }

        throw new FormatException(
            "Не найдена координата оси " + axis);
    }

    private void OnDisable()
    {
        StopReading();
    }
}
