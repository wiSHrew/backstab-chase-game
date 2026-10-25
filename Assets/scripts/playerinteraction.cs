using UnityEngine;
using UnityEngine.InputSystem;

public interface IInteractable
{
    void Interact(GameObject interactor);
}

public class playerinteraction : MonoBehaviour
{
    [SerializeField] private Transform viewTransform;
    [SerializeField, Min(0f)] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactionLayers = Physics.DefaultRaycastLayers;

    private void Awake()
    {
        if (viewTransform == null)
        {
            Camera playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera != null)
            {
                viewTransform = playerCamera.transform;
            }
        }

        if (viewTransform == null)
        {
            Debug.LogError("Assign a view transform or add a Camera as a child of the player.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
        {
            return;
        }

        if (Physics.Raycast(viewTransform.position, viewTransform.forward, out RaycastHit hit,
                interactionRange, interactionLayers, QueryTriggerInteraction.Ignore) &&
            hit.collider.GetComponentInParent<IInteractable>() is IInteractable interactable)
        {
            Debug.Log($"Interacted with {hit.collider.name}.", hit.collider);
            interactable.Interact(gameObject);
        }
    }
}
