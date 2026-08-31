using UnityEngine;

public class CombatAnimator : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Animation Parameters")]
    [SerializeField] private string attackParameter = "Attack";
    [SerializeField] private string hitParameter = "Hit";
    [SerializeField] private string deathParameter = "Death";

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    public void PlayAttack()
    {
        if (animator == null)
            return;

        animator.SetTrigger(attackParameter);
    }

    public void PlayHit()
    {
        if (animator == null)
            return;

        animator.SetTrigger(hitParameter);
    }

    public void PlayDeath()
    {
        if (animator == null)
            return;

        animator.SetTrigger(deathParameter);
    }
}
