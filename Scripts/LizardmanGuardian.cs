using System.Collections;
using UnityEngine;

/// <summary>One-hit guardian that plays retargeted hit and death animations before completion.</summary>
public sealed class LizardmanGuardian : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Collider hitCollider;
    [SerializeField] private RelicGameManager gameManager;
    [SerializeField, Min(0.5f)] private float deathFallbackSeconds = 6f;

    private static readonly int HitTriggerHash = Animator.StringToHash("Hit");
    private static readonly int DieTriggerHash = Animator.StringToHash("Die");
    private bool defeated;

    public bool IsDefeated => defeated;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (hitCollider == null)
        {
            hitCollider = GetComponent<Collider>();
        }
    }

    public void BindGameManager(RelicGameManager manager)
    {
        gameManager = manager;
    }

    public void ReceiveMeleeHit()
    {
        if (defeated || !isActiveAndEnabled)
        {
            return;
        }

        defeated = true;
        LizardmanAI ai = GetComponent<LizardmanAI>();
        if (ai == null)
        {
            ai = GetComponentInParent<LizardmanAI>();
        }

        if (ai != null)
        {
            ai.StopForDeath();
        }

        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null)
        {
            body.linearVelocity = Vector3.zero;
            body.isKinematic = true;
        }

        if (hitCollider != null)
        {
            hitCollider.enabled = false;
        }

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            StartCoroutine(PlayHitThenDeath());
        }
        else
        {
            CompleteEncounter();
        }
    }

    private IEnumerator PlayHitThenDeath()
    {
        animator.SetTrigger(HitTriggerHash);
        float hitWindow = 0.45f;
        while (hitWindow > 0f)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName("Hit") && state.normalizedTime >= 0.25f)
            {
                break;
            }

            hitWindow -= Time.deltaTime;
            yield return null;
        }

        animator.SetTrigger(DieTriggerHash);
        yield return PlayDeathAndComplete();
    }

    private IEnumerator PlayDeathAndComplete()
    {
        float remaining = deathFallbackSeconds;
        bool deathStarted = false;

        while (remaining > 0f)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName("Death"))
            {
                deathStarted = true;
                if (state.normalizedTime >= 1f)
                {
                    break;
                }
            }
            else if (deathStarted && !animator.IsInTransition(0))
            {
                break;
            }

            remaining -= Time.deltaTime;
            yield return null;
        }

        CompleteEncounter();
    }

    private void CompleteEncounter()
    {
        RelicGameManager manager = gameManager;
        gameObject.SetActive(false);
        if (manager != null)
        {
            manager.CompleteFinalEncounter();
        }
    }
}
