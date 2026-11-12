
using System.Collections;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class VRSlidingDoor : MonoBehaviour
{
    [Header("Контрольные точки")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;

    [Header("Настройки")]
    [SerializeField] private float moveSpeed = 1.5f;

    private bool isOpen = false;
    private bool isLocked = false;
    private Coroutine movement;

    private Vector3 startPosition;
    private Vector3 moveDirection;
    private float moveDistance;

    // Дверь закрыта, если она находится в endpoint
    public bool IsFullyClosed => !isOpen && movement == null;

    private void Start()
    {
        if (startPoint == null || endPoint == null)
        {
            Debug.LogError("Door: Не назначены контрольные точки!");
            enabled = false;
            return;
        }

        startPosition = startPoint.position;

        Vector3 difference = endPoint.position - startPoint.position;

        moveDistance = difference.magnitude;
        moveDirection = difference.normalized;

        // Начальное положение — закрытая дверь
        transform.position = endPoint.position;
    }

    public void HandHoverUpdate(Hand hand)
    {
        GrabTypes grabType = hand.GetGrabStarting();

        if (grabType != GrabTypes.None)
        {
            ToggleDoor();
        }
    }

    public void ToggleDoor()
    {
        // Запрет перемещения во время работы программы
        if (isLocked)
        {
            Debug.Log("Дверь заблокирована: программа выполняется!");
            return;
        }

        isOpen = !isOpen;

        if (movement != null)
            StopCoroutine(movement);

        movement = StartCoroutine(MoveDoor());
    }

    private IEnumerator MoveDoor()
    {
        // Открытая дверь — startPoint
        // Закрытая дверь — endPoint
        float targetDistance = isOpen ? 0f : moveDistance;

        float currentDistance = Vector3.Dot(
            transform.position - startPosition,
            moveDirection
        );

        while (Mathf.Abs(currentDistance - targetDistance) > 0.001f)
        {
            currentDistance = Mathf.MoveTowards(
                currentDistance,
                targetDistance,
                moveSpeed * Time.deltaTime
            );

            transform.position =
                startPosition + moveDirection * currentDistance;

            yield return null;
        }

        transform.position = startPosition +
            moveDirection * targetDistance;

        movement = null;
    }

    // Блокировка или разблокировка двери
    public void SetLocked(bool locked)
    {
        isLocked = locked;

        Debug.Log(locked
            ? "Дверь заблокирована"
            : "Дверь разблокирована");
    }
}
