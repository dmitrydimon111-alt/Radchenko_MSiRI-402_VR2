
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
    [SerializeField] private int channel = 1;
    [SerializeField] private float interval = 0.5f;

    [Header("Узлы станка")]
    [SerializeField] private Transform nodeX;
    [SerializeField] private Transform nodeY;
    [SerializeField] private Transform nodeZ;

    [Header("Направления движения")]
    [SerializeField] private Vector3 directionX = Vector3.right;
    [SerializeField] private Vector3 directionY = Vector3.forward;
    [SerializeField] private Vector3 directionZ = Vector3.up;

    private Vector3 startX;
    private Vector3 startY;
    private Vector3 startZ;

    private const float MmToMeters = 0.001f;

    private IEnumerator Start()
    {
        if (nodeX == null || nodeY == null || nodeZ == null)
        {
            Debug.LogError("CNC: Назначьте узлы X, Y и Z в Inspector.");
            yield break;
        }

        startX = nodeX.localPosition;
        startY = nodeY.localPosition;
        startZ = nodeZ.localPosition;

        Debug.Log("CNC: Контроллер запущен.");

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

                        nodeX.localPosition =
                            startX + directionX.normalized * x * MmToMeters;

                        nodeY.localPosition =
                            startY + directionY.normalized * y * MmToMeters;

                        nodeZ.localPosition =
                            startZ + directionZ.normalized * z * MmToMeters;

                        Debug.Log(
                            $"CNC: X={x}; Y={y}; Z={z} мм");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(
                            "CNC: Ошибка чтения JSON: " + e.Message);
                    }
                }
                else
                {
                    Debug.LogError(
                        "CNC: Ошибка запроса: " + request.error);
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

            string normalizedTarget = target
                .Replace('Х', 'X')
                .Replace('х', 'x');

            if (string.Equals(
                name,
                normalizedTarget,
                StringComparison.OrdinalIgnoreCase))
            {
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
                    Debug.Log(
                        "CNC: Найдена координата " +
                        axis + " = " + value + " мм");

                    return value;
                }

                throw new FormatException(
                    "Не удалось преобразовать координату " +
                    axis + ": " + valueText);
            }
        }

        throw new FormatException(
            "Не найдена координата " + axis);
    }
}
