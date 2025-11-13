using UnityEngine;
using System.Collections.Generic;

namespace RPGCorruption.Data
{
    /// <summary>
    /// Plantilla inmutable de un personaje (héroe o enemigo).
    /// Define stats base, habilidades disponibles y datos visuales.
    /// </summary>
    [CreateAssetMenu(fileName = "New Character", menuName = "RPG/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("Identificación")]
        [SerializeField] private string characterName;
        [SerializeField] private string characterId; // Para referencias en save
        [TextArea(3, 5)]
        [SerializeField] private string description;

        [Header("Stats Base (Nivel 1)")]
        [SerializeField] private int baseMaxHP = 100;
        [SerializeField] private int baseMaxMP = 50;
        [SerializeField] private int baseAttack = 10;
        [SerializeField] private int baseDefense = 5;
        [SerializeField] private int baseSpeed = 10;
        [SerializeField] private float baseCritChance = 0.05f; // 5%

        [Header("Crecimiento por Nivel")]
        [SerializeField] private int hpGrowth = 15;
        [SerializeField] private int mpGrowth = 8;
        [SerializeField] private int attackGrowth = 2;
        [SerializeField] private int defenseGrowth = 1;
        [SerializeField] private int speedGrowth = 1;

        [Header("Habilidades Iniciales")]
        [SerializeField] private List<SkillData> startingSkills;

        [Header("Visual")]
        [SerializeField] private Sprite portrait;
        [SerializeField] private Sprite battleSprite;
        [SerializeField] private Sprite corruptedSprite; // Cuando está infectado

        // Properties
        public string CharacterName => characterName;
        public string CharacterId => characterId;
        public string Description => description;

        public int BaseMaxHP => baseMaxHP;
        public int BaseMaxMP => baseMaxMP;
        public int BaseAttack => baseAttack;
        public int BaseDefense => baseDefense;
        public int BaseSpeed => baseSpeed;
        public float BaseCritChance => baseCritChance;

        public int HPGrowth => hpGrowth;
        public int MPGrowth => mpGrowth;
        public int AttackGrowth => attackGrowth;
        public int DefenseGrowth => defenseGrowth;
        public int SpeedGrowth => speedGrowth;

        public List<SkillData> StartingSkills => startingSkills;

        public Sprite Portrait => portrait;
        public Sprite BattleSprite => battleSprite;
        public Sprite CorruptedSprite => corruptedSprite;

        /// <summary>
        /// Calcula un stat en base al nivel
        /// </summary>
        public int GetStatAtLevel(StatType stat, int level)
        {
            int baseStat = stat switch
            {
                StatType.MaxHP => baseMaxHP,
                StatType.MaxMP => baseMaxMP,
                StatType.Attack => baseAttack,
                StatType.Defense => baseDefense,
                StatType.Speed => baseSpeed,
                _ => 0
            };

            int growth = stat switch
            {
                StatType.MaxHP => hpGrowth,
                StatType.MaxMP => mpGrowth,
                StatType.Attack => attackGrowth,
                StatType.Defense => defenseGrowth,
                StatType.Speed => speedGrowth,
                _ => 0
            };

            return baseStat + (growth * (level - 1));
        }
    }
}