using UnityEngine;

/// <summary>Follows the player and orbits while the right mouse button is held.</summary>
public sealed class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 3.2f, -4.5f);
    [SerializeField] private Vector3 lookAtOffset = new Vector3(0f, 1.1f, 0f);
    [SerializeField, Min(0f)] private float followSharpness = 8f;
    [SerializeField, Min(0f)] private float mouseSensitivity = 3f;
    [SerializeField] private float minPitch = -15f;
    [SerializeField] private float maxPitch = 70f;

    private float orbitDistance;
    private float orbitYaw;
    private float orbitPitch;
    private bool isOrbiting;
    private bool previousCursorVisible;
    private CursorLockMode previousCursorLock;

    private void Start()
    {
        if (target == null)
        {
            return;
        }

        Vector3 initialOffset = offset - lookAtOffset;
        orbitDistance = Mathf.Max(0.1f, initialOffset.magnitude);
        orbitYaw = Mathf.Atan2(-initialOffset.x, -initialOffset.z) * Mathf.Rad2Deg;
        orbitPitch = Mathf.Asin(Mathf.Clamp(initialOffset.y / orbitDistance, -1f, 1f)) * Mathf.Rad2Deg;
        orbitPitch = Mathf.Clamp(orbitPitch, minPitch, maxPitch);

        SnapToTarget();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            BeginOrbit();
        }

        if (Input.GetMouseButton(1))
        {
            if (!isOrbiting)
            {
                BeginOrbit();
            }

            ApplyOrbitInput(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        }
        else if (isOrbiting)
        {
            EndOrbit();
        }

        Vector3 focusPoint = target.position + lookAtOffset;
        Quaternion orbitRotation = Quaternion.Euler(orbitPitch, orbitYaw, 0f);
        Vector3 desiredPosition = focusPoint + orbitRotation * (Vector3.back * orbitDistance);
        float blend = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);

        transform.position = Vector3.Lerp(transform.position, desiredPosition, blend);
        Quaternion desiredRotation = Quaternion.LookRotation(focusPoint - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, blend);
    }

    private void BeginOrbit()
    {
        if (isOrbiting)
        {
            return;
        }

        isOrbiting = true;
        previousCursorVisible = Cursor.visible;
        previousCursorLock = Cursor.lockState;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ApplyOrbitInput(float mouseX, float mouseY)
    {
        orbitYaw += mouseX * mouseSensitivity;
        orbitPitch = Mathf.Clamp(orbitPitch - mouseY * mouseSensitivity, minPitch, maxPitch);
    }

    private void EndOrbit()
    {
        if (!isOrbiting)
        {
            return;
        }

        isOrbiting = false;
        Cursor.lockState = previousCursorLock;
        Cursor.visible = previousCursorVisible;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            EndOrbit();
        }
    }

    private void OnDisable()
    {
        EndOrbit();
    }

    private void SnapToTarget()
    {
        Vector3 focusPoint = target.position + lookAtOffset;
        Quaternion orbitRotation = Quaternion.Euler(orbitPitch, orbitYaw, 0f);
        transform.position = focusPoint + orbitRotation * (Vector3.back * orbitDistance);
        transform.rotation = Quaternion.LookRotation(focusPoint - transform.position, Vector3.up);
    }
}
