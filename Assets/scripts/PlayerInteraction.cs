using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private bool canInteract = true;

    [Header("Combat")]
    [SerializeField] private bool canStartCombat = true;
    public int attackBonus = 5;
    public string damageNotation = "1d6+2";
    public int armorClass = 13;

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

    private void OnTriggerExit(Collider other)
    {
        EntityType otherEntity = other.GetComponentInParent<EntityType>();

        if (otherEntity == null || otherEntity.entityType != EntityType.Type.Enemy)
            return;

        EnemyInteraction enemy = otherEntity.GetComponentInParent<EnemyInteraction>();

        if (enemy != null && CombatManager.Instance != null)
        {
            CombatManager.Instance.ClearEncounter(enemy);
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

        EnemyInteraction enemy =
            enemyEntity.GetComponentInParent<EnemyInteraction>();

        if (enemy == null)
        {
            Debug.LogWarning(
                $"No se encontró EnemyInteraction en {enemyEntity.name}"
            );
            return;
        }

        Debug.Log($"{name} está en rango de combate con {enemy.name}");

        // El combate ya no se resuelve aquí: solo avisamos al CombatManager
        // de que hay un encuentro activo. El preview y la tirada real
        // se disparan desde AttackController (botón Atacar).
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.RegisterEncounter(this, enemy);
        }
        else
        {
            Debug.LogWarning("No hay un CombatManager en la escena");
        }
    }

    public CombatAnimator GetCombatAnimator()
    {
        return combatAnimator;
    }
}