using UnityEngine;

/// <summary>Applies one close-range hit during the player's existing Attack animation.</summary>
[RequireComponent(typeof(PlayerMovement))]
public sealed class PlayerMeleeCombat : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField, Min(0.1f)] private float hitRadius = 1.1f;
    [SerializeField, Min(0.1f)] private float attackRange = 2f;
    [SerializeField, Range(0f, 1f)] private float minimumFrontDot = 0.25f;
    [SerializeField, Range(0f, 1f)] private float hitTime = 0.35f;
    [SerializeField] private Vector3 hitCenterOffset = new Vector3(0f, 0.95f, 0.7f);

    private static readonly int AttackStateHash = Animator.StringToHash("FKM_Attack_01");
    private bool hitResolvedForCurrentAttack;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        if (animator == null)
        {
            return;
        }

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if (!state.IsName("FKM_Attack_01"))
        {
            hitResolvedForCurrentAttack = false;
            return;
        }

        if (hitResolvedForCurrentAttack || state.normalizedTime < hitTime)
        {
            return;
        }

        hitResolvedForCurrentAttack = true;
        Vector3 center = transform.TransformPoint(hitCenterOffset);
        Collider[] hits = Physics.OverlapSphere(center, hitRadius);
        foreach (Collider hit in hits)
        {
            LizardmanGuardian[] guardians = hit.GetComponentsInParent<LizardmanGuardian>(true);
            foreach (LizardmanGuardian guardian in guardians)
            {
                if (guardian == null || !guardian.isActiveAndEnabled || guardian.IsDefeated)
                {
                    continue;
                }

                Vector3 toGuardian = guardian.transform.position - transform.position;
                toGuardian.y = 0f;
                if (toGuardian.sqrMagnitude > attackRange * attackRange)
                {
                    continue;
                }

                if (toGuardian.sqrMagnitude > 0.001f &&
                    Vector3.Dot(transform.forward, toGuardian.normalized) < minimumFrontDot)
                {
                    continue;
                }

                guardian.ReceiveMeleeHit();
                return;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.TransformPoint(hitCenterOffset), hitRadius);
    }
}
