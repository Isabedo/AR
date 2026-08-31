using UnityEngine;

public class EntityType : MonoBehaviour
{
    public enum Type
    {
        Player,
        Enemy
    }

    [Header("Entity")]
    public Type entityType;
}