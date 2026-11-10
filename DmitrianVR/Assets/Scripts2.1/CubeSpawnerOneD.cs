using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
    // Исходный объект
    public GameObject cubePrefab;

    // Количество копий
    public int cloneCount = 5;

    // Расстояние между копиями
    public float distance = 2f;

    void Start()
    {
        // Создание копий
        for (int i = 1; i <= cloneCount; i++)
        {
            // Определение позиции новой копии
            Vector3 newPosition = transform.position +
                Vector3.up * distance * i;

            // Создание копии объекта
            Instantiate(cubePrefab, newPosition, transform.rotation);
        }
    }
}