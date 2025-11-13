using UnityEngine;

namespace RPGCorruption.Data
{
    /// <summary>
    /// Configuración global del juego: balance, curvas de progresión, límites del sistema.
    /// Un único asset que centraliza valores de diseño.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "RPG/Game Configuration")]
    public class GameConfig : ScriptableObject
    {
        private static GameConfig instance;

        /// <summary>
        /// Instancia global del GameConfig.
        /// IMPORTANTE: Debe ser asignada manualmente en el GameManager al inicio.
        /// </summary>
        public static GameConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogError("GameConfig instance not set! Assign it in GameManager.Awake()");
                }
                return instance;
            }
            set => instance = value;
        }

        [Header("Progresión")]
        [SerializeField] private int maxLevel = 50;
        [SerializeField] private AnimationCurve expCurve; // Curva de XP requerida por nivel
        [SerializeField] private int baseExpRequired = 100;
        [SerializeField] private float expMultiplier = 1.5f;

        [Header("Sistema de Combate")]
        [SerializeField] private float baseCritMultiplier = 1.5f;
        [SerializeField] private int maxPartySize = 3;
        [SerializeField] private float speedToTurnRatio = 1f; // Conversión de Speed a orden de turnos

        [Header("Sistema de Infección")]
        [SerializeField] private int maxInfectionLevel = 100;
        [SerializeField] private float infectionDefenseDebuff = 0.3f; // -30% def por infección
        [SerializeField] private float infectionDamageBonus = 0.5f; // +50% daño por infección
        [SerializeField] private int corruptionThreshold = 80; // Nivel donde empiezan efectos críticos
        [SerializeField] private int fullCorruptionLevel = 100; // Game Over

        [Header("Economía")]
        [SerializeField] private int startingGold = 500;
        [SerializeField] private float shopPriceMultiplier = 1f;
        [SerializeField] private float sellPriceRatio = 0.5f; // Vendes al 50% del precio

        [Header("Inventario")]
        [SerializeField] private int maxInventorySlots = 50;
        [SerializeField] private int maxConsumableStack = 99;

        [Header("Dificultad")]
        [SerializeField] private float enemyStatMultiplier = 1f;
        [SerializeField] private float expGainMultiplier = 1f;
        [SerializeField] private float goldGainMultiplier = 1f;

        // Properties
        public int MaxLevel => maxLevel;
        public int BaseExpRequired => baseExpRequired;
        public float ExpMultiplier => expMultiplier;

        public float BaseCritMultiplier => baseCritMultiplier;
        public int MaxPartySize => maxPartySize;
        public float SpeedToTurnRatio => speedToTurnRatio;

        public int MaxInfectionLevel => maxInfectionLevel;
        public float InfectionDefenseDebuff => infectionDefenseDebuff;
        public float InfectionDamageBonus => infectionDamageBonus;
        public int CorruptionThreshold => corruptionThreshold;
        public int FullCorruptionLevel => fullCorruptionLevel;

        public int StartingGold => startingGold;
        public float ShopPriceMultiplier => shopPriceMultiplier;
        public float SellPriceRatio => sellPriceRatio;

        public int MaxInventorySlots => maxInventorySlots;
        public int MaxConsumableStack => maxConsumableStack;

        public float EnemyStatMultiplier => enemyStatMultiplier;
        public float ExpGainMultiplier => expGainMultiplier;
        public float GoldGainMultiplier => goldGainMultiplier;

        /// <summary>
        /// Calcula la experiencia requerida para alcanzar un nivel específico
        /// </summary>
        public int GetExpRequiredForLevel(int level)
        {
            if (level <= 1) return 0;
            if (level > maxLevel) return int.MaxValue;

            // Si hay curva definida, usarla
            if (expCurve != null && expCurve.length > 0)
            {
                float normalized = (float)(level - 1) / (maxLevel - 1);
                return Mathf.RoundToInt(expCurve.Evaluate(normalized) * baseExpRequired * Mathf.Pow(expMultiplier, level - 1));
            }

            // Fórmula por defecto: exponencial
            return Mathf.RoundToInt(baseExpRequired * Mathf.Pow(expMultiplier, level - 1));
        }

        /// <summary>
        /// Calcula el modificador de defensa basado en el nivel de infección
        /// </summary>
        public float GetInfectionDefenseModifier(int infectionLevel)
        {
            if (infectionLevel <= 0) return 1f;

            float infectionPercent = (float)infectionLevel / maxInfectionLevel;
            return 1f - (infectionDefenseDebuff * infectionPercent);
        }

        /// <summary>
        /// Calcula el modificador de daño basado en el nivel de infección
        /// </summary>
        public float GetInfectionDamageModifier(int infectionLevel)
        {
            if (infectionLevel <= 0) return 1f;

            float infectionPercent = (float)infectionLevel / maxInfectionLevel;
            return 1f + (infectionDamageBonus * infectionPercent);
        }

        /// <summary>
        /// Verifica si un nivel de infección es crítico
        /// </summary>
        public bool IsInfectionCritical(int infectionLevel)
        {
            return infectionLevel >= corruptionThreshold;
        }

        /// <summary>
        /// Verifica si la infección alcanzó el nivel de Game Over
        /// </summary>
        public bool IsFullyCorrupted(int infectionLevel)
        {
            return infectionLevel >= fullCorruptionLevel;
        }
    }
}