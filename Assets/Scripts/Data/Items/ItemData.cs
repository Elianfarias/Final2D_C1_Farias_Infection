using UnityEngine;

namespace RPGCorruption.Data
{
    /// <summary>
    /// Plantilla de un item del juego (consumible, equipamiento, reliquia).
    /// Define efectos, precio, rareza y requisitos.
    /// </summary>
    [CreateAssetMenu(fileName = "New Item", menuName = "RPG/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Identificación")]
        [SerializeField] private string itemName;
        [SerializeField] private string itemId;
        [TextArea(2, 4)]
        [SerializeField] private string description;

        [Header("Tipo y Categoría")]
        [SerializeField] private ItemType itemType;
        [SerializeField] private ItemRarity rarity;

        [Header("Economía")]
        [SerializeField] private int buyPrice;
        [SerializeField] private int sellPrice;
        [SerializeField] private int maxStack = 99;

        [Header("Efectos (Consumibles)")]
        [SerializeField] private int hpRestore;
        [SerializeField] private int mpRestore;
        [SerializeField] private int infectionChange; // Negativo para reducir, positivo para aumentar
        [SerializeField] private StatusEffect curesStatus = StatusEffect.None;
        [SerializeField] private bool revivesAlly; // Para items de revivir

        [Header("Stats (Equipamiento)")]
        [SerializeField] private EquipmentSlot equipmentSlot;
        [SerializeField] private int attackBonus;
        [SerializeField] private int defenseBonus;
        [SerializeField] private int speedBonus;
        [SerializeField] private int maxHPBonus;
        [SerializeField] private int maxMPBonus;
        [SerializeField] private float critBonus;

        [Header("Propiedades Especiales")]
        [SerializeField] private bool isCorrupted; // Arma corrupta: más poder, más infección
        [SerializeField] private int corruptionPerTurn; // Infección pasiva por turno
        [SerializeField] private bool isKeyItem; // No se puede vender/tirar

        [Header("Visual")]
        [SerializeField] private Sprite icon;
        [SerializeField] private Sprite equipmentSprite; // Para mostrar en personaje

        // Properties
        public string ItemName => itemName;
        public string ItemId => itemId;
        public string Description => description;

        public ItemType Type => itemType;
        public ItemRarity Rarity => rarity;

        public int BuyPrice => buyPrice;
        public int SellPrice => sellPrice;
        public int MaxStack => maxStack;

        public int HPRestore => hpRestore;
        public int MPRestore => mpRestore;
        public int InfectionChange => infectionChange;
        public StatusEffect CuresStatus => curesStatus;
        public bool RevivesAlly => revivesAlly;

        public EquipmentSlot EquipmentSlot => equipmentSlot;
        public int AttackBonus => attackBonus;
        public int DefenseBonus => defenseBonus;
        public int SpeedBonus => speedBonus;
        public int MaxHPBonus => maxHPBonus;
        public int MaxMPBonus => maxMPBonus;
        public float CritBonus => critBonus;

        public bool IsCorrupted => isCorrupted;
        public int CorruptionPerTurn => corruptionPerTurn;
        public bool IsKeyItem => isKeyItem;

        public Sprite Icon => icon;
        public Sprite EquipmentSprite => equipmentSprite;

        /// <summary>
        /// Retorna el color asociado a la rareza del item
        /// </summary>
        public Color GetRarityColor()
        {
            return rarity switch
            {
                ItemRarity.Common => Color.white,
                ItemRarity.Uncommon => Color.green,
                ItemRarity.Rare => Color.blue,
                ItemRarity.Epic => new Color(0.64f, 0.21f, 0.93f), // Púrpura
                ItemRarity.Legendary => new Color(1f, 0.5f, 0f), // Naranja
                ItemRarity.Corrupted => new Color(0.5f, 0f, 0.2f), // Rojo oscuro
                _ => Color.white
            };
        }
    }
}