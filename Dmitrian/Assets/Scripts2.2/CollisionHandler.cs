using UnityEngine;

public class CollisionHandler : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Anton")
        {
            Debug.Log("Wowan столкнулся с Anton!");

            Invoke(nameof(CollisionMessage), 2f);
        }
    }

    private void CollisionMessage()
    {
        Debug.Log("Anton: Осторожнее! Смотри, куда идёшь!");
    }
}