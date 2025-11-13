using UnityEngine;

namespace RPGCorruption.Data
{
    /// <summary>
    /// Plantilla de una habilidad usable en combate.
    /// Define costo, efectos, targeting y animaciones.
    /// </summary>
    [CreateAssetMenu(fileName = "New Skill", menuName = "RPG/Skill Data")]
    public class SkillData : ScriptableObject
    {
        [Header("Identificación")]
        [SerializeField] private string skillName;
        [SerializeField] private string skillId;
        [TextArea(2, 4)]
        [SerializeField] private string description;

        [Header("Costos")]
        [SerializeField] private int mpCost;
        [SerializeField] private int infectionCost; // Costo de infección al usar

        [Header("Efectos")]
        [SerializeField] private SkillType skillType;
        [SerializeField] private TargetType targetType;
        [SerializeField] private int basePower; // Daño o curación base
        [SerializeField] private float powerScaling = 1f; // Escalado con Attack del usuario

        [Header("Efectos Secundarios")]
        [SerializeField] private bool canCrit = true;
        [SerializeField] private StatusEffect inflictedStatus = StatusEffect.None;
        [SerializeField] private float statusChance = 0f; // Probabilidad de aplicar estado
        [SerializeField] private int infectionDamage = 0; // Infección que causa/cura

        [Header("Visual & Audio")]
        [SerializeField] private Sprite icon;
        [SerializeField] private GameObject vfxPrefab;
        [SerializeField] private AudioClip sfx;
        [SerializeField] private Color skillColor = Color.white;

        // Properties
        public string SkillName => skillName;
        public string SkillId => skillId;
        public string Description => description;

        public int MPCost => mpCost;
        public int InfectionCost => infectionCost;

        public SkillType Type => skillType;
        public TargetType TargetType => targetType;
        public int BasePower => basePower;
        public float PowerScaling => powerScaling;

        public bool CanCrit => canCrit;
        public StatusEffect InflictedStatus => inflictedStatus;
        public float StatusChance => statusChance;
        public int InfectionDamage => infectionDamage;

        public Sprite Icon => icon;
        public GameObject VfxPrefab => vfxPrefab;
        public AudioClip Sfx => sfx;
        public Color SkillColor => skillColor;

        /// <summary>
        /// Calcula el daño/curación final basado en stats del usuario
        /// </summary>
        public int CalculatePower(int userAttackStat)
        {
            return Mathf.RoundToInt(basePower + (userAttackStat * powerScaling));
        }
    }
}