using UnityEngine;

public class WorkpieceClampZone : MonoBehaviour
{
    public bool IsWorkpieceInstalled { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Workpiece1"))
        {
            IsWorkpieceInstalled = true;
            Debug.Log("Заготовка установлена в тисках.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Workpiece1"))
        {
            IsWorkpieceInstalled = false;
            Debug.Log("Заготовка извлечена из тисков.");
        }
    }
}
