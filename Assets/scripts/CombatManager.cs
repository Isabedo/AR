using UnityEngine;
using DnDAR.Combat;

/// <summary>
/// Coordina el combate: sabe qué jugador y qué enemigo están en rango,
/// le pide el preview a AttackController y aplica el resultado a EnemyInteraction.
/// Ni PlayerInteraction ni EnemyInteraction se llaman directamente entre sí;
/// pasan siempre por aquí (según el TODO que dejó tu compañera en EncounterEnemy).
/// </summary>
public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    private PlayerInteraction currentPlayer;
    private EnemyInteraction currentEnemy;

    public bool HasActiveEncounter => currentPlayer != null && currentEnemy != null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>Llamado por PlayerInteraction cuando entra en rango de un enemigo.</summary>
    public void RegisterEncounter(PlayerInteraction player, EnemyInteraction enemy)
    {
        currentPlayer = player;
        currentEnemy = enemy;
    }

    /// <summary>Llamado por PlayerInteraction cuando sale de rango.</summary>
    public void ClearEncounter(EnemyInteraction enemy)
    {
        if (currentEnemy != enemy)
            return;

        currentPlayer = null;
        currentEnemy = null;
    }

    /// <summary>Usado por AttackController al tocar "Atacar" para mostrar el preview.</summary>
    public bool TryGetPreview(out AttackCalculator.AttackPreview preview)
    {
        preview = default;
        if (!HasActiveEncounter)
            return false;

        preview = AttackCalculator.GetPreview(
            currentPlayer.attackBonus,
            currentEnemy.armorClass,
            currentPlayer.damageNotation);

        return true;
    }

    /// <summary>Usado por AttackController al confirmar el ataque.</summary>
    public bool TryResolveAttack(out AttackResult result)
    {
        result = default;
        if (!HasActiveEncounter)
            return false;

        var player = currentPlayer;
        var enemy = currentEnemy;

        result = AttackController.ResolveAttackRoll(
            player.attackBonus,
            enemy.armorClass,
            player.damageNotation);

        var playerAnimator = player.GetCombatAnimator();
        if (playerAnimator != null)
            playerAnimator.PlayAttack();

        if (result.Hit)
            enemy.ReceiveDamage(result.DamageDealt);

        return true;
    }
}