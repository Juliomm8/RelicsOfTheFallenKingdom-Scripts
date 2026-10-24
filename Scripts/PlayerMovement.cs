using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Ataque")]
    [SerializeField] private float attackCooldown = 0.2f;

    private Rigidbody rb;
    private Animator animator;
    private Transform movementCamera;

    private Vector3 movement;

    private bool isRunning;
    private bool isAttacking;

    private float nextAttackTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

        Camera mainCamera = Camera.main;
        movementCamera = mainCamera != null ? mainCamera.transform : null;
    }

    private void Update()
    {
        if (!isAttacking)
        {
            ReadKeyboard();

            if (Input.GetMouseButtonDown(0))
            {
                TryAttack();
            }
        }
        else
        {
            movement = Vector3.zero;
            isRunning = false;
        }

        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        if (isAttacking)
            return;

        Move();
        Rotate();
    }

    private void ReadKeyboard()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.W))
            vertical += 1f;

        if (Input.GetKey(KeyCode.S))
            vertical -= 1f;

        if (Input.GetKey(KeyCode.D))
            horizontal += 1f;

        if (Input.GetKey(KeyCode.A))
            horizontal -= 1f;

        movement = CalculateCameraRelativeMovement(horizontal, vertical);

        isRunning =
            movement.sqrMagnitude > 0.01f &&
            (Input.GetKey(KeyCode.LeftShift) ||
             Input.GetKey(KeyCode.RightShift));
    }

    private Vector3 CalculateCameraRelativeMovement(float horizontal, float vertical)
    {
        Vector3 cameraForward = movementCamera != null
            ? Vector3.ProjectOnPlane(movementCamera.forward, Vector3.up)
            : Vector3.forward;
        Vector3 cameraRight = movementCamera != null
            ? Vector3.ProjectOnPlane(movementCamera.right, Vector3.up)
            : Vector3.right;

        if (cameraForward.sqrMagnitude > 0.001f)
            cameraForward.Normalize();
        else
            cameraForward = Vector3.forward;

        if (cameraRight.sqrMagnitude > 0.001f)
            cameraRight.Normalize();
        else
            cameraRight = Vector3.right;

        return (cameraForward * vertical + cameraRight * horizontal).normalized;
    }

    private void Move()
    {
        if (movement.sqrMagnitude <= 0.001f)
            return;

        float speed =
            isRunning ? runSpeed : walkSpeed;

        Vector3 newPosition =
            rb.position +
            movement * speed * Time.fixedDeltaTime;

        rb.MovePosition(newPosition);
    }

    private void Rotate()
    {
        if (movement.sqrMagnitude <= 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(movement, Vector3.up);

        Quaternion smoothRotation =
            Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );

        rb.MoveRotation(smoothRotation);
    }

    private void UpdateAnimations()
    {
        if (animator == null)
            return;

        float speed =
            isAttacking ? 0f : movement.magnitude;

        animator.SetFloat("Speed", speed);

        animator.SetBool(
            "Running",
            !isAttacking && isRunning
        );
    }

    private void TryAttack()
    {
        if (isAttacking)
            return;

        if (Time.time < nextAttackTime)
            return;

        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        // Detener completamente al jugador.
        movement = Vector3.zero;
        isRunning = false;

        animator.SetFloat("Speed", 0f);
        animator.SetBool("Running", false);
        animator.SetTrigger("Attack");

        Debug.Log("ATAQUE");

        // Esperar un frame para que el Animator entre al estado Attack.
        yield return null;

        // Esperar hasta que realmente entre en la animación de ataque.
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("FKM_Attack_01"))
        {
            yield return null;
        }

        // Mantener bloqueado al jugador mientras la animación no haya terminado.
        while (animator.GetCurrentAnimatorStateInfo(0).IsName("FKM_Attack_01") &&
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            movement = Vector3.zero;
            yield return null;
        }

        // El ataque terminó: permitimos moverse otra vez.
        isAttacking = false;

        nextAttackTime = Time.time + attackCooldown;
    }
}