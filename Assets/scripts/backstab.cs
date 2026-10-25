using UnityEngine;

public class backstab : MonoBehaviour, IInteractable
{
    [SerializeField] private Vector2 xBounds = new Vector2(-46f, 48f);
    [SerializeField] private Vector2 zBounds = new Vector2(-48f, 49f);
    [SerializeField, Min(0f)] private float runSpeed = 4f;
    [SerializeField, Min(0f)] private float turnSpeed = 540f;
    [SerializeField, Min(0f)] private float backstabRange = 2.5f;
    [SerializeField, Range(-1f, 1f)] private float backstabAngleThreshold = -0.35f;

    private Vector3 destination;
    private bool defeated;

    private void Start()
    {
        ChooseDestination();
    }

    private void Update()
    {
        if (defeated)
        {
            return;
        }

        Vector3 direction = destination - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
        {
            ChooseDestination();
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime);

        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            runSpeed * Time.deltaTime);
    }

    private void ChooseDestination()
    {
        destination = new Vector3(
            Random.Range(xBounds.x, xBounds.y),
            transform.position.y,
            Random.Range(zBounds.x, zBounds.y));
    }

    public void Interact(GameObject interactor)
    {
        if (defeated || interactor == null)
        {
            return;
        }

        Vector3 directionToPlayer = interactor.transform.position - transform.position;
        directionToPlayer.y = 0f;

        if (directionToPlayer.sqrMagnitude > backstabRange * backstabRange)
        {
            Debug.Log("Get closer to backstab the NPC.", this);
            return;
        }

        if (directionToPlayer.sqrMagnitude < 0.001f ||
            Vector3.Dot(transform.forward, directionToPlayer.normalized) > backstabAngleThreshold)
        {
            Debug.Log("Get behind the NPC to backstab it.", this);
            return;
        }

        defeated = true;
        Debug.Log("Backstab successful!", this);
        Destroy(gameObject);
    }
}
