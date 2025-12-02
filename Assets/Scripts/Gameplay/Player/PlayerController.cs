using UnityEngine;
using RPGCorruption.Data;
using RPGCorruption.Data.Runtime;

namespace RPGCorruption.Map
{
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Character Data")]
        [SerializeField] private CharacterData characterTemplate;
        [Header("Visual")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        private CharacterInstance characterInstance;
        private PlayerMovement movement;

        public CharacterInstance Character => characterInstance;
        public PlayerMovement Movement => movement;

        private void Awake()
        {
            movement = GetComponent<PlayerMovement>();

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            if (characterTemplate != null)
                InitializeCharacter(characterTemplate, level: 1);
        }

        private void Update()
        {
            if (characterInstance != null)
                UpdateVisuals();
        }

        public void InitializeCharacter(CharacterData template, int level = 1)
        {
            if (template == null)
                return;

            characterInstance = new CharacterInstance(template, level);
            characterTemplate = template;

            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (characterInstance == null || characterInstance.Template == null) return;

            // Usar sprite normal o corrupto según infeccion
            if (characterInstance.IsInfectionCritical() && characterInstance.Template.CorruptedSprite != null)
                spriteRenderer.sprite = characterInstance.Template.CorruptedSprite;
            else if (characterInstance.Template.BattleSprite != null)
                spriteRenderer.sprite = characterInstance.Template.BattleSprite;
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color32[] pix = new Color32[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new(width, height);
            result.SetPixels32(pix);
            result.Apply();
            return result;
        }

        private void OnGUI()
        {
            if (characterInstance == null) return;

            GUIStyle titleStyle = new(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold
            };
            titleStyle.normal.textColor = Color.white;

            GUIStyle labelStyle = new(GUI.skin.label)
            {
                fontSize = 16
            };
            labelStyle.normal.textColor = Color.white;

            GUIStyle warningStyle = new(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };
            warningStyle.normal.textColor = Color.red;

            GUIStyle boxStyle = new(GUI.skin.box);
            boxStyle.normal.background = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.1f, 0.8f));

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
    }
}