using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    [Header("Configuración Interacción")]
    public float interactionRange = 1f;
    public LayerMask interactableLayer;

    [Header("Feedback")]
    public GameObject interactionPrompt;

    private IInteractable currentInteractable;

    private void Update()
    {
        CheckForInteractables();

        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    private void CheckForInteractables()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, interactionRange, interactableLayer);

        IInteractable closestInteractable = null;
        float closestDistance = float.MaxValue;

        foreach (var hit in hitColliders)
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable != null && interactable.CanInteract())
            {
                float distance = Vector2.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
        }

        // Actualizar interactable actual
        if (currentInteractable != closestInteractable)
        {
            currentInteractable = closestInteractable;
            UpdateInteractionPrompt();
        }
    }

    private void UpdateInteractionPrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(currentInteractable != null);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}