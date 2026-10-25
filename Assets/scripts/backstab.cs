using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class backstab : MonoBehaviour, IInteractable
{
    private const float BackstabCanvasDuration = 2f;
    private const float FallDuration = 0.4f;

    [SerializeField] private Vector2 xBounds = new Vector2(-46f, 48f);
    [SerializeField] private Vector2 zBounds = new Vector2(-48f, 49f);
    [SerializeField, Min(0f)] private float runSpeed = 4f;
    [SerializeField, Min(0f)] private float turnSpeed = 540f;
    [SerializeField, Min(0f)] private float backstabRange = 2.5f;
    [SerializeField, Range(-1f, 1f)] private float backstabAngleThreshold = -0.35f;
    [SerializeField] private GameObject backstabCanvas;

    private Vector3 destination;
    private Transform playerTransform;
    private Image firstBackstabIndicator;
    private Image secondBackstabIndicator;
    private GameObject backstabMessage;
    private Color firstIndicatorDefaultColor;
    private Color secondIndicatorDefaultColor;
    private bool defeated;

    private void Start()
    {
        if (backstabCanvas == null)
        {
            Debug.LogError("Assign the canvas containing the backstab indicators in the Inspector.", this);
        }
        else
        {
            backstabCanvas.SetActive(true);
            firstBackstabIndicator = FindIndicator("crosshair");
            secondBackstabIndicator = FindIndicator("crosshair (1)");

            if (firstBackstabIndicator != null)
            {
                firstIndicatorDefaultColor = firstBackstabIndicator.color;
            }

            if (secondBackstabIndicator != null)
            {
                secondIndicatorDefaultColor = secondBackstabIndicator.color;
            }

            Transform messageTransform = backstabCanvas.transform.Find("Text (TMP)");
            if (messageTransform != null)
            {
                backstabMessage = messageTransform.gameObject;
                backstabMessage.SetActive(false);
            }
        }

        playerinteraction player = FindAnyObjectByType<playerinteraction>();
        if (player == null)
        {
            Debug.LogError("A player with a playerinteraction component is required for backstab indicators.", this);
        }
        else
        {
            playerTransform = player.transform;
        }

        ChooseDestination();
    }

    private Image FindIndicator(string indicatorName)
    {
        Transform indicatorTransform = backstabCanvas.transform.Find(indicatorName);
        Image indicator = indicatorTransform != null ? indicatorTransform.GetComponent<Image>() : null;

        if (indicator == null)
        {
            Debug.LogError($"The canvas must have an Image child named '{indicatorName}'.", this);
        }

        return indicator;
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
        }
        else
        {
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

        UpdateBackstabIndicators();
    }

    private void UpdateBackstabIndicators()
    {
        bool canBackstab = playerTransform != null && CanBackstab(playerTransform.position);

        if (firstBackstabIndicator != null)
        {
            firstBackstabIndicator.color = canBackstab ? Color.red : firstIndicatorDefaultColor;
        }

        if (secondBackstabIndicator != null)
        {
            secondBackstabIndicator.color = canBackstab ? Color.red : secondIndicatorDefaultColor;
        }
    }

    private bool CanBackstab(Vector3 playerPosition)
    {
        Vector3 directionToPlayer = playerPosition - transform.position;
        directionToPlayer.y = 0f;

        if (directionToPlayer.sqrMagnitude < 0.001f ||
            directionToPlayer.sqrMagnitude > backstabRange * backstabRange)
        {
            return false;
        }

        Vector3 enemyForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        return enemyForward.sqrMagnitude >= 0.001f &&
            Vector3.Dot(enemyForward, directionToPlayer.normalized) <= backstabAngleThreshold;
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

        if (!CanBackstab(interactor.transform.position))
        {
            Debug.Log("Get behind the NPC and within range to backstab it.", this);
            return;
        }

        defeated = true;
        Debug.Log("Backstab successful!", this);

        if (backstabMessage == null)
        {
            Debug.LogError("The canvas must have a Text (TMP) child for the backstab message.", this);
        }
        else
        {
            backstabMessage.SetActive(true);
        }

        StartCoroutine(FallOverAndRemoveNpc());
    }

    private IEnumerator FallOverAndRemoveNpc()
    {
        Quaternion startRotation = transform.rotation;
        Quaternion fallenRotation = startRotation * Quaternion.Euler(90f, 0f, 0f);
        float elapsed = 0f;

        while (elapsed < FallDuration)
        {
            elapsed += Time.deltaTime;
            float fallProgress = Mathf.SmoothStep(0f, 1f, elapsed / FallDuration);
            transform.rotation = Quaternion.Slerp(startRotation, fallenRotation, fallProgress);
            yield return null;
        }

        transform.rotation = fallenRotation;
        yield return new WaitForSeconds(BackstabCanvasDuration - FallDuration);

        if (backstabMessage != null)
        {
            backstabMessage.SetActive(false);
        }

        Destroy(gameObject);
    }
}
