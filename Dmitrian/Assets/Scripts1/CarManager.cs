using UnityEngine;

public class CarManager : MonoBehaviour
{
    public Car myCar;

    void Start()
    {
        myCar.brand = "BMW";
        myCar.model = "M5";
        myCar.year = 2024;

        myCar.ShowInfo();
        myCar.StartEngine();

        Debug.Log("Марка из второго класса: " + myCar.brand);
    }
}