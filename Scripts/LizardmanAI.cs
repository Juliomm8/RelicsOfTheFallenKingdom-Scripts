using UnityEngine;

/// <summary>Small ground-based chase and attack behavior for the final guardian.</summary>
[RequireComponent(typeof(Rigidbody))]
public sealed class LizardmanAI : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private LizardmanGuardian guardian;
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private float moveSpeed = 2.4f;
    [SerializeField] private float attackCooldown = 1.8f;
    [SerializeField] private float turnSpeed = 540f;

    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private Rigidbody body;
    private Transform player;
    private Vector3 chaseDirection;
    private bool playerDetected;
    private bool shouldMove;
    private bool stopped;
    private float nextTargetSearchTime;
    private float nextAttackTime;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (guardian == null)
        {
            guardian = GetComponent<LizardmanGuardian>();
        }

        body.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.Continuous;
        FindPlayer();
    }

    private void Update()
    {
        if (stopped || (guardian != null && guardian.IsDefeated))
        {
            SetMoving(false);
            shouldMove = false;
            return;
        }

        if (player == null && Time.time >= nextTargetSearchTime)
        {
            nextTargetSearchTime = Time.time + 1f;
            FindPlayer();
        }

        if (player == null)
        {
            playerDetected = false;
            shouldMove = false;
            SetMoving(false);
            return;
        }

        Vector3 toPlayer = player.position - body.position;
        toPlayer.y = 0f;
        float distance = toPlayer.magnitude;

        if (distance > detectionRadius || distance < 0.001f)
        {
            playerDetected = false;
            shouldMove = false;
            SetMoving(false);
            return;
        }

        playerDetected = true;
        chaseDirection = toPlayer / distance;
        if (distance > attackRange)
        {
            shouldMove = true;
            SetMoving(true);
            return;
        }

        shouldMove = false;
        SetMoving(false);
        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            if (animator != null)
            {
                animator.SetTrigger(AttackHash);
            }
        }
    }

    private void FixedUpdate()
    {
        if (body == null || stopped)
        {
            return;
        }

        Vector3 velocity = body.linearVelocity;
        velocity.x = shouldMove ? chaseDirection.x * moveSpeed : 0f;
        velocity.z = shouldMove ? chaseDirection.z * moveSpeed : 0f;
        body.linearVelocity = velocity;

        if (!playerDetected || player == null)
        {
            return;
        }

        Vector3 facing = player.position - body.position;
        facing.y = 0f;
        if (facing.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(facing.normalized, Vector3.up);
            body.MoveRotation(Quaternion.RotateTowards(
                body.rotation,
                targetRotation,
                turnSpeed * Time.fixedDeltaTime));
        }
    }

    public void StopForDeath()
    {
        stopped = true;
        playerDetected = false;
        shouldMove = false;
        SetMoving(false);
    }

    private void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        player = playerObject != null ? playerObject.transform : null;
    }

    private void SetMoving(bool moving)
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetBool(IsMovingHash, moving);
        }
    }
}
