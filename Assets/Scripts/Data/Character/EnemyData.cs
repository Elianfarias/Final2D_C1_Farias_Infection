using UnityEngine;
using System.Collections.Generic;

namespace RPGCorruption.Data
{
    /// <summary>
    /// Datos espec�ficos de enemigos: recompensas, comportamiento y drop tables.
    /// Extiende CharacterData con informaci�n adicional para NPCs hostiles.
    /// </summary>
    [CreateAssetMenu(fileName = "New Enemy", menuName = "RPG/Enemy Data")]
    public class EnemyData : CharacterData
    {
        [Header("Clasificacion de Enemigo")]
        [SerializeField] private EnemyType enemyType;
        [SerializeField] private LayerMask layer;
        [SerializeField] private string sortingLayerName;
        [SerializeField] private int level = 1;
        [SerializeField] private bool isBoss;
        [SerializeField] private bool isMiniBoss;

        [Header("Recompensas")]
        [SerializeField] private int expReward;
        [SerializeField] private int goldReward;
        [SerializeField] private List<ItemDrop> dropTable;

        [Header("Comportamiento IA")]
        [SerializeField] private AIBehavior aiBehavior;
        [SerializeField] private float aggroRange = 5f;

        [Header("Visual en Mapa")]
        [SerializeField] private Sprite mapSprite;
        [SerializeField] private Vector2 mapSize = Vector2.one;

        [Header("Infecci�n")]
        [SerializeField] private int baseInfectionLevel;
        [SerializeField] private bool canInfectPlayers = true;

        // Properties
        public EnemyType Type => enemyType;
        public int Level => level;
        public bool IsBoss => isBoss;
        public bool IsMiniBoss => isMiniBoss;
        public LayerMask Layer => layer;
        public string SortingLayerName => sortingLayerName;

        public int ExpReward => expReward;
        public int GoldReward => goldReward;
        public List<ItemDrop> DropTable => dropTable;

        public AIBehavior AIBehavior => aiBehavior;
        public float AggroRange => aggroRange;

        public Sprite MapSprite => mapSprite;
        public Vector2 MapSize => mapSize;

        public int BaseInfectionLevel => baseInfectionLevel;
        public bool CanInfectPlayers => canInfectPlayers;

        /// <summary>
        /// Determina qu� items dropea este enemigo al ser derrotado
        /// </summary>
        public List<ItemData> RollDrops()
        {
            List<ItemData> droppedItems = new List<ItemData>();

            foreach (var drop in dropTable)
            {
                float roll = Random.Range(0f, 1f);
                if (roll <= drop.DropChance)
                {
                    droppedItems.Add(drop.Item);
                }
            }

            return droppedItems;
        }
    }

    [System.Serializable]
    public class ItemDrop
    {
        [SerializeField] private ItemData item;
        [SerializeField, Range(0f, 1f)] private float dropChance = 0.1f; // 10% por defecto

        public ItemData Item => item;
        public float DropChance => dropChance;
    }
}