using UnityEngine;

public class CubeSpawner3D : MonoBehaviour
{
    public GameObject cubePrefab;

    public int cloneCount = 5;

    public float distanceX = 2f;
    public float distanceY = 1f;
    public float distanceZ = 2f;

    void Start()
    {
        for (int i = 1; i <= cloneCount; i++)
        {
            Vector3 newPosition = transform.position +
                new Vector3(
                    distanceX * i,
                    distanceY * i,
                    distanceZ * i
                );

            Instantiate(cubePrefab, newPosition, transform.rotation);
        }
    }
}