using UnityEngine;
using UnityEngine.Events;

public class basicinteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private UnityEvent onInteract = new UnityEvent();

    public void Interact(GameObject interactor)
    {
        onInteract.Invoke();
    }
}
