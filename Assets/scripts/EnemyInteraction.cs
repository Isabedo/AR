using UnityEngine;

public class EnemyInteraction : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private bool canRespond = true;

    private CombatAnimator combatAnimator;

    private PlayerInteraction currentPlayer;

    private void Awake()
    {
        combatAnimator = GetComponent<CombatAnimator>();
    }

    public void ReceiveAttack(PlayerInteraction player)
    {
        if (!canRespond)
            return;

        currentPlayer = player;

        Debug.Log($"{name} recibió un ataque de {player.name}");

        // El enemigo recibe el golpe
        if (combatAnimator != null)
        {
            combatAnimator.PlayHit();
        }
        else
        {
            Debug.LogWarning(
                $"No se encontró CombatAnimator en {name}"
            );
        }

        // Por ahora simulamos una respuesta.
        Invoke(nameof(AttackPlayer), 1f);
    }

    private void AttackPlayer()
    {
        if (currentPlayer == null)
            return;

        Debug.Log($"{name} ataca a {currentPlayer.name}");

        if (combatAnimator != null)
        {
            combatAnimator.PlayAttack();
        }

        // TODO:
        // Aquí posteriormente se conectará el sistema de combate.
        // No hay daño todavía.
    }

    public void ReceiveDamage(float damage)
    {
        // TODO:
        // Sistema de combate.
    }

    public void Die()
    {
        if (combatAnimator != null)
        {
            combatAnimator.PlayDeath();
        }
    }

    public PlayerInteraction GetCurrentPlayer()
    {
        return currentPlayer;
    }
}
