using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private bool canInteract = true;

    [Header("Combat")]
    [SerializeField] private bool canStartCombat = true;

    private CombatAnimator combatAnimator;

    private void Awake()
    {
        combatAnimator = GetComponent<CombatAnimator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        EntityType otherEntity = other.GetComponentInParent<EntityType>();

        if (otherEntity == null)
            return;

        switch (otherEntity.entityType)
        {
            case EntityType.Type.Player:
                InteractWithPlayer(otherEntity);
                break;

            case EntityType.Type.Enemy:
                EncounterEnemy(otherEntity);
                break;
        }
    }

    private void InteractWithPlayer(EntityType otherPlayer)
    {
        if (!canInteract)
            return;

        Debug.Log($"{name} interactúa con {otherPlayer.name}");

        // TODO:
        // Interacción jugador-jugador.
    }

    private void EncounterEnemy(EntityType enemyEntity)
    {
        if (!canStartCombat)
            return;

        Debug.Log($"{name} comienza combate con {enemyEntity.name}");

        // 1. El jugador ataca
        if (combatAnimator != null)
        {
            combatAnimator.PlayAttack();
        }

        // 2. Buscamos el sistema de interacción del enemigo
        EnemyInteraction enemy =
            enemyEntity.GetComponentInParent<EnemyInteraction>();

        if (enemy != null)
        {
            enemy.ReceiveAttack(this);
        }
        else
        {
            Debug.LogWarning(
                $"No se encontró EnemyInteraction en {enemyEntity.name}"
            );
        }

        // TODO:
        // Aquí posteriormente se conectará CombatManager.
    }
}

