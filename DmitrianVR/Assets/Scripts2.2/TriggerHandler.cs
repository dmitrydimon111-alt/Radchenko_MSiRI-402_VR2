using UnityEngine;

public class TriggerHandler : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Kolian")
        {
            Debug.Log("Wowan вошёл в зону Kolian!");

            Invoke(nameof(TriggerMessage), 2f);
        }
    }

    private void TriggerMessage()
    {
        Debug.Log("Kolian: Ты вошёл на мою территорию!");
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "Kolian")
        {
            Debug.Log("Wowan покинул территорию Kolian!");
        }
    }
}
