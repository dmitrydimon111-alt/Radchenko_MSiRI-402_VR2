using UnityEngine;

public class CubeSpawnerAdv : MonoBehaviour
{
    public GameObject cubePrefab;

    public Vector3[] positions =
    {
        new Vector3(2, 0, 0),
        new Vector3(0, 2, 0),
        new Vector3(0, 0, 2),
        new Vector3(2, 2, 2),
        new Vector3(-2, 0, 2)
    };

    void Start()
    {
        foreach (Vector3 position in positions)
        {
            Instantiate(
                cubePrefab,
                transform.position + position,
                transform.rotation
            );
        }
    }
}