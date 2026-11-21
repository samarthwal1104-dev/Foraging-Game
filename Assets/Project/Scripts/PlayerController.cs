using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float standingSpeed = 8f;
    public float crouchSpeed = 4f;
    public float gravity = -19.62f;

    [Header("Crouch Parameters")]
    public float crouchHeight = 1.0f;
    public float crouchTransitionDuration = 0.2f;

    private CharacterController controller;
    private Transform cameraTransform;
    private float standingHeight;
    private Vector3 standingCenter;
    private float standingCameraY;
    private float crouchingCameraY;

    public bool isCrouching { get; private set; } = false;
    private bool isPenaltyActive = false;
    private Coroutine crouchRoutine;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = GetComponentInChildren<Camera>().transform;

        standingHeight = controller.height;
        standingCenter = controller.center;
        standingCameraY = cameraTransform.localPosition.y;
        crouchingCameraY = standingCameraY - (standingHeight - crouchHeight);

        // Fix first-crouch jerk by snapping camera height properly at start
        Vector3 camPos = cameraTransform.localPosition;
        camPos.y = standingCameraY;
        cameraTransform.localPosition = camPos;
    }

    void Update()
    {
        HandleMovement();
        HandleCrouchInput();
        ApplyGravity();
    }

    private void HandleMovement()
    {
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float currentSpeed = isCrouching ? crouchSpeed : standingSpeed;
        if (isPenaltyActive) currentSpeed *= 0.5f;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    private void HandleCrouchInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (DataLogger.Instance != null) DataLogger.Instance.LogCrouchStart();
            if (crouchRoutine != null) StopCoroutine(crouchRoutine);
            crouchRoutine = StartCoroutine(CrouchTransition(true));
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            if (HasHeadroom())
            {
                if (DataLogger.Instance != null) DataLogger.Instance.LogCrouchEnd();
                if (crouchRoutine != null) StopCoroutine(crouchRoutine);
                crouchRoutine = StartCoroutine(CrouchTransition(false));
            }
        }
    }

    private IEnumerator CrouchTransition(bool wantsToCrouch)
    {
        isCrouching = wantsToCrouch;
        float targetHeight = wantsToCrouch ? crouchHeight : standingHeight;
        Vector3 targetCenter = wantsToCrouch ? new Vector3(standingCenter.x, crouchHeight / 2f, standingCenter.z) : standingCenter;
        float targetCameraY = wantsToCrouch ? crouchingCameraY : standingCameraY;

        float currentHeight = controller.height;
        Vector3 currentCenter = controller.center;
        float currentCameraY = cameraTransform.localPosition.y;
        float elapsed = 0f;

        while (elapsed < crouchTransitionDuration)
        {
            float t = Mathf.Sin((elapsed / crouchTransitionDuration) * Mathf.PI * 0.5f);
            controller.height = Mathf.Lerp(currentHeight, targetHeight, t);
            controller.center = Vector3.Lerp(currentCenter, targetCenter, t);
            Vector3 camPos = cameraTransform.localPosition;
            camPos.y = Mathf.Lerp(currentCameraY, targetCameraY, t);
            cameraTransform.localPosition = camPos;

            elapsed += Time.deltaTime;
            yield return null;
        }

        controller.height = targetHeight;
        controller.center = targetCenter;
        Vector3 finalCam = cameraTransform.localPosition;
        finalCam.y = targetCameraY;
        cameraTransform.localPosition = finalCam;
    }

    private bool HasHeadroom()
    {
        float castDistance = standingHeight - controller.height;
        Vector3 castOrigin = transform.position + new Vector3(0, controller.center.y + controller.height / 2, 0);
        return !Physics.SphereCast(castOrigin, controller.radius, Vector3.up, out RaycastHit hit, castDistance);
    }

    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Obstacle") && !isPenaltyActive)
        {
            if (DataLogger.Instance != null) DataLogger.Instance.LogObstacleCollision();
            StartCoroutine(CollisionPenalty());
        }

        if (hit.gameObject.CompareTag("Apple"))
        {
            Apple apple = hit.gameObject.GetComponent<Apple>(); // You already had this
            if (apple != null)
            {
                if (ResourceManager.Instance != null) ResourceManager.Instance.AddScore(apple.pointValue);

                if (DataLogger.Instance != null)
                {
                    DataLogger.Instance.LogAppleCollected(hit.gameObject.transform.position, apple);
                }

                hit.collider.enabled = false;
                Destroy(hit.gameObject);
            }
        }
    }

    private IEnumerator CollisionPenalty()
    {
        isPenaltyActive = true;
        yield return new WaitForSeconds(0.5f);
        isPenaltyActive = false;
    }
}
