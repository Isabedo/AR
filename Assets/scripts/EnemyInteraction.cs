using UnityEngine;
using UnityEngine.UI;

public class EnemyInteraction : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private bool canRespond = true;

    [Header("Combate (usado por CombatManager / AttackController)")]
    public int armorClass = 14;
    public int maxHP = 20;
    public int currentHP;

    [Header("UI de vida (opcional)")]
    public Slider healthBarSlider;

    private CombatAnimator combatAnimator;

    private PlayerInteraction currentPlayer;

    private void Awake()
    {
        combatAnimator = GetComponent<CombatAnimator>();
        currentHP = maxHP;
        UpdateHealthBar();
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
        // Aquí posteriormente se conectará el contraataque del enemigo (su propia tirada).
        // No hay daño todavía.
    }

    public void ReceiveDamage(int damage)
    {
        if (currentHP <= 0)
            return;

        currentHP = Mathf.Max(0, currentHP - damage);
        Debug.Log($"{name} recibió {damage} de daño ({currentHP}/{maxHP} HP)");

        UpdateHealthBar();

        if (currentHP <= 0)
        {
            Die();
        }
        else if (combatAnimator != null)
        {
            combatAnimator.PlayHit();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBarSlider != null)
            healthBarSlider.value = maxHP > 0 ? (float)currentHP / maxHP : 0f;
    }

    public void Die()
    {
        canRespond = false;

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