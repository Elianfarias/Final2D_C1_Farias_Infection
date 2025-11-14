using UnityEngine;
using RPGCorruption.Data;
using RPGCorruption.Data.Runtime;

namespace RPGCorruption.Map
{
    /// <summary>
    /// Controlador del jugador en el mapa.
    /// Conecta el CharacterInstance con la representación visual.
    /// </summary>
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Character Data")]
        [SerializeField] private CharacterData characterTemplate;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        private CharacterInstance characterInstance;
        private PlayerMovement movement;

        // Properties
        public CharacterInstance Character => characterInstance;
        public PlayerMovement Movement => movement;

        private void Awake()
        {
            movement = GetComponent<PlayerMovement>();

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        private void Start()
        {
            // Si hay un template asignado, crear instancia
            if (characterTemplate != null)
            {
                InitializeCharacter(characterTemplate, level: 1);
            }
        }

        /// <summary>
        /// Inicializa el personaje desde un template
        /// </summary>
        public void InitializeCharacter(CharacterData template, int level = 1)
        {
            if (template == null)
            {
                Debug.LogError("Cannot initialize character with null template!");
                return;
            }

            characterInstance = new CharacterInstance(template, level);
            characterTemplate = template;

            UpdateVisuals();

            Debug.Log($"Player character initialized: {characterInstance.Template.CharacterName} (Level {level})");
        }

        /// <summary>
        /// Actualiza los sprites según el estado del personaje
        /// </summary>
        private void UpdateVisuals()
        {
            if (characterInstance == null || characterInstance.Template == null) return;

            // Usar sprite normal o corrupto según infección
            if (characterInstance.IsInfectionCritical() && characterInstance.Template.CorruptedSprite != null)
            {
                spriteRenderer.sprite = characterInstance.Template.CorruptedSprite;
            }
            else if (characterInstance.Template.BattleSprite != null)
            {
                spriteRenderer.sprite = characterInstance.Template.BattleSprite;
            }
        }

        /// <summary>
        /// Actualiza el estado del personaje (llamar cada frame si es necesario)
        /// </summary>
        private void Update()
        {
            // Actualizar visuales si cambió la infección
            if (characterInstance != null)
            {
                UpdateVisuals();
            }
        }

        #region Debug

        /// <summary>
        /// Para testing: aplica daño al personaje
        /// </summary>
        [ContextMenu("Debug: Take 20 Damage")]
        public void Debug_TakeDamage()
        {
            if (characterInstance != null)
            {
                characterInstance.TakeDamage(20);
                Debug.Log($"HP: {characterInstance.CurrentHP}/{characterInstance.MaxHP}");
            }
        }

        /// <summary>
        /// Para testing: cura al personaje
        /// </summary>
        [ContextMenu("Debug: Heal Full")]
        public void Debug_HealFull()
        {
            if (characterInstance != null)
            {
                characterInstance.FullRestore();
                Debug.Log("Fully healed!");
            }
        }

        /// <summary>
        /// Para testing: aumenta infección
        /// </summary>
        [ContextMenu("Debug: Increase Infection +20")]
        public void Debug_IncreaseInfection()
        {
            if (characterInstance != null)
            {
                characterInstance.IncreaseInfection(20);
                Debug.Log($"Infection: {characterInstance.InfectionLevel}");
            }
        }

        /// <summary>
        /// Para testing: sube de nivel
        /// </summary>
        [ContextMenu("Debug: Level Up")]
        public void Debug_LevelUp()
        {
            if (characterInstance != null)
            {
                characterInstance.GainExperience(1000);
                Debug.Log($"Level: {characterInstance.Level}");
            }
        }

        /// <summary>
        /// Muestra info del personaje
        /// </summary>
        private void OnGUI()
        {
            if (characterInstance == null) return;

            // Estilo con fuente más grande
            GUIStyle titleStyle = new(GUI.skin.label);
            titleStyle.fontSize = 20;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = Color.white;

            GUIStyle labelStyle = new(GUI.skin.label);
            labelStyle.fontSize = 16;
            labelStyle.normal.textColor = Color.white;

            GUIStyle warningStyle = new(GUI.skin.label);
            warningStyle.fontSize = 18;
            warningStyle.fontStyle = FontStyle.Bold;
            warningStyle.normal.textColor = Color.red;

            GUIStyle boxStyle = new(GUI.skin.box);
            boxStyle.normal.background = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.1f, 0.8f));

            // HUD mejorado en la esquina superior izquierda
            GUILayout.BeginArea(new Rect(10, 10, 350, 250));
            GUILayout.BeginVertical(boxStyle);

            GUILayout.Space(5);
            GUILayout.Label(characterInstance.Template.CharacterName, titleStyle);
            GUILayout.Space(10);

            GUILayout.Label($"Level: {characterInstance.Level}", labelStyle);
            GUILayout.Label($"HP: {characterInstance.CurrentHP}/{characterInstance.MaxHP}", labelStyle);
            GUILayout.Label($"MP: {characterInstance.CurrentMP}/{characterInstance.MaxMP}", labelStyle);
            GUILayout.Label($"Infection: {characterInstance.InfectionLevel}%", labelStyle);

            if (characterInstance.IsInfectionCritical())
            {
                GUILayout.Space(5);
                GUILayout.Label("⚠ CRITICAL INFECTION!", warningStyle);
            }

            GUILayout.Space(5);
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
        #endregion
    }
}