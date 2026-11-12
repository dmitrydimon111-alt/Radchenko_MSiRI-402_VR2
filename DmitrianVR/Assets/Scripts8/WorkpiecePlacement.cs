using System.Collections;
using TMPro;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class WorkpiecePlacement : MonoBehaviour
{
    [Header("Положение заготовки")]
    [SerializeField] private Transform positionTarget;
    [SerializeField] private float animationDuration = 1.5f;

    [Header("Заготовка")]
    [SerializeField] private string workpieceTag = "Workpiece";

    [Header("Интерфейс")]
    [SerializeField] private TMP_Text statusText;

    private bool isPlacing;
    private bool isPlaced;

    private void Start()
    {
        if (statusText != null)
            statusText.text = "Ожидание установки заготовки";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isPlacing || isPlaced)
            return;

        Rigidbody rb = other.attachedRigidbody;

        if (rb == null)
            rb = other.GetComponentInParent<Rigidbody>();

        if (rb == null)
            return;

        if (!rb.gameObject.CompareTag(workpieceTag) &&
            !other.CompareTag(workpieceTag))
            return;

        if (positionTarget == null)
        {
            Debug.LogError("CNC: Не назначена точка PositionTarget!");
            return;
        }

        StartCoroutine(PlaceWorkpiece(rb));
    }

    private IEnumerator PlaceWorkpiece(Rigidbody rb)
    {
        isPlacing = true;

        // Запоминаем исходное положение
        Vector3 startPosition = rb.position;
        Quaternion startRotation = rb.rotation;

        // Получаем конечное положение
        Vector3 targetPosition = positionTarget.position;
        Quaternion targetRotation = positionTarget.rotation;

        // Временно отключаем физику
        rb.isKinematic = true;
        rb.useGravity = false;

        // Плавная анимация установки
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / animationDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            rb.position = Vector3.Lerp(
                startPosition, targetPosition, smoothT);

            rb.rotation = Quaternion.Slerp(
                startRotation, targetRotation, smoothT);

            yield return null;
        }

        // Точно устанавливаем заготовку
        rb.position = targetPosition;
        rb.rotation = targetRotation;

        // Отключаем компоненты SteamVR
        Throwable[] throwables =
            rb.GetComponentsInChildren<Throwable>(true);

        Interactable[] interactables =
            rb.GetComponentsInChildren<Interactable>(true);

        foreach (Throwable component in throwables)
        {
            if (component != null)
                component.enabled = false;
        }

        foreach (Interactable component in interactables)
        {
            if (component != null)
                component.enabled = false;
        }

        // Фиксируем заготовку
        rb.isKinematic = true;
        rb.useGravity = false;

        // Отключаем коллайдеры заготовки
        Collider[] colliders =
            rb.GetComponentsInChildren<Collider>(true);

        foreach (Collider col in colliders)
        {
            if (col != null)
                col.enabled = false;
        }

        // Завершаем установку
        isPlaced = true;
        isPlacing = false;

        if (statusText != null)
            statusText.text = "Заготовка успешно установлена!";

        Debug.Log("Заготовка успешно установлена и заблокирована!");
    }
}

