using UnityEngine;

public class CubeSpawnerRel : MonoBehaviour
{
    // Исходный объект Wowan
    public GameObject cubePrefab;

    // Количество копий
    public int cloneCount = 5;

    // Смещение относительно координат Wowan
    public float distanceX = 0f;
    public float distanceY = 2f;
    public float distanceZ = 0f;

    void Start()
    {
        // Получаем Transform исходного куба
        Transform wowanTransform = cubePrefab.transform;

        // Создание копий
        for (int i = 1; i <= cloneCount; i++)
        {
            // Рассчитываем локальное смещение
            Vector3 offset = new Vector3(
                distanceX * i,
                distanceY * i,
                distanceZ * i
            );

            // Переводим локальные координаты в мировые
            Vector3 newPosition =
                wowanTransform.TransformPoint(offset);

            // Создаём копию
            Instantiate(
                cubePrefab,
                newPosition,
                wowanTransform.rotation
            );
        }
    }
}