using UnityEngine;

public class Car : MonoBehaviour
{
    public string brand = "Toyota";
    public string model = "Camry";
    public int year = 2022;

    public void ShowInfo()
    {
        Debug.Log("Марка: " + brand);
        Debug.Log("Модель: " + model);
        Debug.Log("Год: " + year);
    }

    public void StartEngine()
    {
        Debug.Log(brand + " " + model + ": двигатель запущен!");
    }

    void Start()
    {
        ShowInfo();
    }
}