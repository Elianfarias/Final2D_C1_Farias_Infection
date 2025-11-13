using UnityEngine;
using System;
using System.Collections.Generic;

namespace RPGCorruption.Data.Runtime
{
    /// <summary>
    /// Instancia runtime de un personaje.
    /// Contiene el estado actual (HP, MP, nivel, infección) basado en un CharacterData template.
    /// Esta clase ES SERIALIZABLE para el sistema de guardado.
    /// </summary>
    [Serializable]
    public class CharacterInstance
    {
        [Header("Referencia al Template")]
        [SerializeField] private string characterDataId; // ID del CharacterData (para serialización)

        [Header("Estado Actual")]
        [SerializeField] private int currentHP;
        [SerializeField] private int currentMP;
        [SerializeField] private int maxHP;
        [SerializeField] private int maxMP;

        [Header("Progresión")]
        [SerializeField] private int level = 1;
        [SerializeField] private int currentExp = 0;

        [Header("Stats Calculados")]
        [SerializeField] private int attack;
        [SerializeField] private int defense;
        [SerializeField] private int speed;
        [SerializeField] private float critChance;

        [Header("Sistema de Infección")]
        [SerializeField] private int infectionLevel = 0;

        [Header("Equipamiento")]
        [SerializeField] private string equippedWeaponId;
        [SerializeField] private string equippedArmorId;
        [SerializeField] private string equippedHelmetId;
        [SerializeField] private string equippedAccessory1Id;
        [SerializeField] private string equippedAccessory2Id;

        [Header("Estados Activos")]
        [SerializeField] private List<ActiveStatusEffect> activeStatuses = new List<ActiveStatusEffect>();

        [Header("Habilidades Aprendidas")]
        [SerializeField] private List<string> learnedSkillIds = new List<string>();

        // Referencia no serializada al template
        [NonSerialized] private CharacterData template;

        // Properties públicas
        public string CharacterDataId => characterDataId;
        public CharacterData Template => template;

        public int CurrentHP => currentHP;
        public int CurrentMP => currentMP;
        public int MaxHP => maxHP;
        public int MaxMP => maxMP;

        public int Level => level;
        public int CurrentExp => currentExp;

        public int Attack => attack;
        public int Defense => defense;
        public int Speed => speed;
        public float CritChance => critChance;

        public int InfectionLevel => infectionLevel;

        public List<ActiveStatusEffect> ActiveStatuses => activeStatuses;
        public List<string> LearnedSkillIds => learnedSkillIds;

        // Estado
        public bool IsAlive => currentHP > 0;
        public bool IsDead => currentHP <= 0;
        public bool IsFullHP => currentHP >= maxHP;
        public bool IsFullMP => currentMP >= maxMP;
        public float HPPercent => maxHP > 0 ? (float)currentHP / maxHP : 0f;
        public float MPPercent => maxMP > 0 ? (float)currentMP / maxMP : 0f;
        public float InfectionPercent => GameConfig.Instance.MaxInfectionLevel > 0
            ? (float)infectionLevel / GameConfig.Instance.MaxInfectionLevel : 0f;

        /// <summary>
        /// Constructor: crea una instancia desde un CharacterData template
        /// </summary>
        public CharacterInstance(CharacterData characterData, int startingLevel = 1)
        {
            if (characterData == null)
            {
                Debug.LogError("Cannot create CharacterInstance with null CharacterData!");
                return;
            }

            template = characterData;
            characterDataId = characterData.CharacterId;
            level = Mathf.Max(1, startingLevel);

            // Calcular stats base según el nivel
            RecalculateStats();

            // Inicializar HP/MP al máximo
            currentHP = maxHP;
            currentMP = maxMP;

            // Aprender habilidades iniciales
            if (characterData.StartingSkills != null)
            {
                foreach (var skill in characterData.StartingSkills)
                {
                    if (skill != null)
                        learnedSkillIds.Add(skill.SkillId);
                }
            }
        }

        /// <summary>
        /// Constructor sin parámetros para deserialización
        /// </summary>
        public CharacterInstance() { }

        /// <summary>
        /// Inicializa la referencia al template después de deserializar
        /// </summary>
        public void InitializeTemplate(CharacterData characterData)
        {
            template = characterData;
            if (characterData != null && string.IsNullOrEmpty(characterDataId))
            {
                characterDataId = characterData.CharacterId;
            }
        }

        #region HP/MP Management

        /// <summary>
        /// Recibe daño
        /// </summary>
        public int TakeDamage(int damage)
        {
            if (damage <= 0) return 0;

            int actualDamage = Mathf.Min(damage, currentHP);
            currentHP -= actualDamage;
            currentHP = Mathf.Max(0, currentHP);

            return actualDamage;
        }

        /// <summary>
        /// Cura HP
        /// </summary>
        public int Heal(int amount)
        {
            if (amount <= 0 || IsDead) return 0;

            int actualHeal = Mathf.Min(amount, maxHP - currentHP);
            currentHP += actualHeal;
            currentHP = Mathf.Min(currentHP, maxHP);

            return actualHeal;
        }

        /// <summary>
        /// Consume MP
        /// </summary>
        public bool ConsumeMP(int amount)
        {
            if (amount <= 0) return true;
            if (currentMP < amount) return false;

            currentMP -= amount;
            return true;
        }

        /// <summary>
        /// Restaura MP
        /// </summary>
        public int RestoreMP(int amount)
        {
            if (amount <= 0) return 0;

            int actualRestore = Mathf.Min(amount, maxMP - currentMP);
            currentMP += actualRestore;
            currentMP = Mathf.Min(currentMP, maxMP);

            return actualRestore;
        }

        /// <summary>
        /// Revive al personaje
        /// </summary>
        public void Revive(float hpPercent = 0.5f)
        {
            if (IsAlive) return;

            currentHP = Mathf.RoundToInt(maxHP * Mathf.Clamp01(hpPercent));
            currentHP = Mathf.Max(1, currentHP);
        }

        /// <summary>
        /// Cura completamente
        /// </summary>
        public void FullRestore()
        {
            currentHP = maxHP;
            currentMP = maxMP;
            activeStatuses.Clear();
        }

        #endregion

        #region Infection System

        /// <summary>
        /// Aumenta el nivel de infección
        /// </summary>
        public void IncreaseInfection(int amount)
        {
            if (amount <= 0) return;

            infectionLevel += amount;
            infectionLevel = Mathf.Min(infectionLevel, GameConfig.Instance.MaxInfectionLevel);

            // Recalcular stats si la infección afecta
            RecalculateStats();
        }

        /// <summary>
        /// Reduce el nivel de infección
        /// </summary>
        public void DecreaseInfection(int amount)
        {
            if (amount <= 0) return;

            infectionLevel -= amount;
            infectionLevel = Mathf.Max(0, infectionLevel);

            // Recalcular stats si la infección afecta
            RecalculateStats();
        }

        /// <summary>
        /// Purifica completamente la infección
        /// </summary>
        public void PurifyInfection()
        {
            infectionLevel = 0;
            RecalculateStats();
        }

        /// <summary>
        /// Verifica si la infección está en nivel crítico
        /// </summary>
        public bool IsInfectionCritical()
        {
            return GameConfig.Instance.IsInfectionCritical(infectionLevel);
        }

        /// <summary>
        /// Verifica si está completamente corrompido
        /// </summary>
        public bool IsFullyCorrupted()
        {
            return GameConfig.Instance.IsFullyCorrupted(infectionLevel);
        }

        #endregion

        #region Status Effects

        /// <summary>
        /// Aplica un estado alterado
        /// </summary>
        public void ApplyStatus(StatusEffect status, int duration)
        {
            if (status == StatusEffect.None) return;

            // Verificar si ya tiene el estado
            var existing = activeStatuses.Find(s => s.Status == status);
            if (existing != null)
            {
                // Renovar duración
                existing.RemainingTurns = Mathf.Max(existing.RemainingTurns, duration);
            }
            else
            {
                // Agregar nuevo estado
                activeStatuses.Add(new ActiveStatusEffect(status, duration));
            }
        }

        /// <summary>
        /// Remueve un estado específico
        /// </summary>
        public void RemoveStatus(StatusEffect status)
        {
            activeStatuses.RemoveAll(s => s.Status == status);
        }

        /// <summary>
        /// Verifica si tiene un estado específico
        /// </summary>
        public bool HasStatus(StatusEffect status)
        {
            return activeStatuses.Exists(s => s.Status == status);
        }

        /// <summary>
        /// Procesa los estados al final del turno
        /// </summary>
        public void ProcessEndOfTurnStatuses()
        {
            for (int i = activeStatuses.Count - 1; i >= 0; i--)
            {
                activeStatuses[i].RemainingTurns--;

                // Aplicar efectos del estado (veneno, quemadura, etc.)
                ApplyStatusEffect(activeStatuses[i].Status);

                // Remover estados expirados
                if (activeStatuses[i].RemainingTurns <= 0)
                {
                    activeStatuses.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Aplica el efecto de un estado (daño por turno, etc.)
        /// </summary>
        private void ApplyStatusEffect(StatusEffect status)
        {
            switch (status)
            {
                case StatusEffect.Poisoned:
                    TakeDamage(Mathf.RoundToInt(maxHP * 0.05f)); // 5% del HP máximo
                    break;
                case StatusEffect.Burning:
                    TakeDamage(Mathf.RoundToInt(maxHP * 0.1f)); // 10% del HP máximo
                    break;
                    // Otros efectos se pueden agregar aquí
            }
        }

        /// <summary>
        /// Limpia todos los estados
        /// </summary>
        public void ClearAllStatuses()
        {
            activeStatuses.Clear();
        }

        #endregion

        #region Progression

        /// <summary>
        /// Otorga experiencia
        /// </summary>
        public bool GainExperience(int exp)
        {
            if (exp <= 0) return false;

            currentExp += exp;

            // Verificar si sube de nivel
            int expRequired = GameConfig.Instance.GetExpRequiredForLevel(level + 1);
            if (currentExp >= expRequired && level < GameConfig.Instance.MaxLevel)
            {
                LevelUp();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sube de nivel
        /// </summary>
        private void LevelUp()
        {
            level++;

            // Recalcular stats
            int oldMaxHP = maxHP;
            int oldMaxMP = maxMP;

            RecalculateStats();

            // Curar la diferencia de HP/MP (bonus de level up)
            currentHP += (maxHP - oldMaxHP);
            currentMP += (maxMP - oldMaxMP);

            currentHP = Mathf.Min(currentHP, maxHP);
            currentMP = Mathf.Min(currentMP, maxMP);

            Debug.Log($"{template.CharacterName} subió al nivel {level}!");
        }

        /// <summary>
        /// Aprende una nueva habilidad
        /// </summary>
        public bool LearnSkill(string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return false;
            if (learnedSkillIds.Contains(skillId)) return false;

            learnedSkillIds.Add(skillId);
            return true;
        }

        #endregion

        #region Stats Calculation

        /// <summary>
        /// Recalcula todos los stats basándose en nivel, equipamiento e infección
        /// </summary>
        public void RecalculateStats()
        {
            if (template == null)
            {
                Debug.LogWarning("Cannot recalculate stats without template!");
                return;
            }

            // Stats base del template según nivel
            maxHP = template.GetStatAtLevel(StatType.MaxHP, level);
            maxMP = template.GetStatAtLevel(StatType.MaxMP, level);
            attack = template.GetStatAtLevel(StatType.Attack, level);
            defense = template.GetStatAtLevel(StatType.Defense, level);
            speed = template.GetStatAtLevel(StatType.Speed, level);
            critChance = template.BaseCritChance;

            // TODO: Aplicar bonus de equipamiento cuando se implemente EquipmentSystem

            // Aplicar modificadores de infección
            ApplyInfectionModifiers();

            // Aplicar modificadores de estados
            ApplyStatusModifiers();
        }

        /// <summary>
        /// Aplica modificadores de infección a los stats
        /// </summary>
        private void ApplyInfectionModifiers()
        {
            if (infectionLevel <= 0) return;

            // Reducción de defensa
            float defModifier = GameConfig.Instance.GetInfectionDefenseModifier(infectionLevel);
            defense = Mathf.RoundToInt(defense * defModifier);

            // Aumento de ataque (no aplicamos aquí, se aplica en el cálculo de daño)
            // Esto es intencional para mantener los stats "puros"
        }

        /// <summary>
        /// Aplica modificadores de estados activos
        /// </summary>
        private void ApplyStatusModifiers()
        {
            foreach (var status in activeStatuses)
            {
                switch (status.Status)
                {
                    case StatusEffect.Strengthened:
                        attack = Mathf.RoundToInt(attack * 1.5f);
                        break;
                    case StatusEffect.Weakened:
                        attack = Mathf.RoundToInt(attack * 0.5f);
                        break;
                }
            }
        }

        /// <summary>
        /// Obtiene el modificador de daño considerando infección
        /// </summary>
        public float GetDamageModifier()
        {
            float modifier = 1f;

            // Bonus de infección
            if (infectionLevel > 0)
            {
                modifier *= GameConfig.Instance.GetInfectionDamageModifier(infectionLevel);
            }

            return modifier;
        }

        #endregion

        #region Equipment (preparado para futuro)

        public void EquipWeapon(string itemId)
        {
            equippedWeaponId = itemId;
            RecalculateStats();
        }

        public void EquipArmor(string itemId)
        {
            equippedArmorId = itemId;
            RecalculateStats();
        }

        public void EquipHelmet(string itemId)
        {
            equippedHelmetId = itemId;
            RecalculateStats();
        }

        public void EquipAccessory(string itemId, int slot)
        {
            if (slot == 1)
                equippedAccessory1Id = itemId;
            else if (slot == 2)
                equippedAccessory2Id = itemId;

            RecalculateStats();
        }

        #endregion
    }

    /// <summary>
    /// Estado alterado activo en un personaje
    /// </summary>
    [Serializable]
    public class ActiveStatusEffect
    {
        [SerializeField] private StatusEffect status;
        [SerializeField] private int remainingTurns;

        public StatusEffect Status => status;
        public int RemainingTurns
        {
            get => remainingTurns;
            set => remainingTurns = value;
        }

        public ActiveStatusEffect(StatusEffect status, int duration)
        {
            this.status = status;
            this.remainingTurns = duration;
        }
    }
}