using UnityEngine;

namespace RPGCorruption.UI
{
    /// <summary>
    /// Overlay de ayuda que muestra controles y comandos disponibles.
    /// Presiona H para mostrar/ocultar.
    /// </summary>
    public class HelpOverlay : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private KeyCode toggleKey = KeyCode.H;
        [SerializeField] private bool showOnStart = true;

        private bool isVisible = true;

        private void Start()
        {
            isVisible = showOnStart;
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                isVisible = !isVisible;
            }
        }

        private void OnGUI()
        {
            if (!isVisible) return;

            // Estilos
            GUIStyle titleStyle = new(GUI.skin.label);
            titleStyle.fontSize = 20;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = Color.yellow;
            titleStyle.alignment = TextAnchor.MiddleCenter;

            GUIStyle headerStyle = new(GUI.skin.label);
            headerStyle.fontSize = 16;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = Color.cyan;

            GUIStyle labelStyle = new(GUI.skin.label);
            labelStyle.fontSize = 14;
            labelStyle.normal.textColor = Color.white;

            GUIStyle keyStyle = new(GUI.skin.label);
            keyStyle.fontSize = 14;
            keyStyle.fontStyle = FontStyle.Bold;
            keyStyle.normal.textColor = Color.green;

            GUIStyle boxStyle = new(GUI.skin.box);
            boxStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.9f));

            // Centrar en pantalla
            float width = 500;
            float height = 400;
            float x = (Screen.width - width) / 2;
            float y = (Screen.height - height) / 2;

            GUILayout.BeginArea(new Rect(x, y, width, height));
            GUILayout.BeginVertical(boxStyle);

            GUILayout.Space(10);
            GUILayout.Label("HELP & CONTROLS", titleStyle);
            GUILayout.Space(20);

            // Controles
            GUILayout.Label("MOVEMENT", headerStyle);
            GUILayout.Space(5);
            DrawControl("Left Click", "Move to clicked position");
            GUILayout.Space(10);

            // Debug commands
            GUILayout.Label("DEBUG COMMANDS", headerStyle);
            GUILayout.Label("(Right-click PlayerController script in Inspector)", labelStyle);
            GUILayout.Space(5);
            DrawControl("Take Damage", "Apply 20 damage to test HP");
            DrawControl("Heal Full", "Restore HP and MP to maximum");
            DrawControl("Increase Infection", "Add 20% infection");
            DrawControl("Level Up", "Gain 1000 XP (may level up)");
            GUILayout.Space(10);

            // Teclas
            GUILayout.Label("KEYBOARD SHORTCUTS", headerStyle);
            GUILayout.Space(5);
            DrawControl("H", "Toggle this help menu");
            GUILayout.Space(10);

            // Info adicional
            GUILayout.Label("TIP: Watch the HUD in top-left corner for character stats!", labelStyle);

            GUILayout.Space(10);
            GUILayout.EndVertical();
            GUILayout.EndArea();

            // Botón para cerrar (esquina superior derecha del panel)
            if (GUI.Button(new Rect(x + width - 100, y + 10, 90, 30), "Close [H]"))
            {
                isVisible = false;
            }
        }

        private void DrawControl(string control, string description)
        {
            GUIStyle keyStyle = new(GUI.skin.label);
            keyStyle.fontSize = 14;
            keyStyle.fontStyle = FontStyle.Bold;
            keyStyle.normal.textColor = Color.green;

            GUIStyle descStyle = new(GUI.skin.label);
            descStyle.fontSize = 14;
            descStyle.normal.textColor = Color.white;

            GUILayout.BeginHorizontal();
            GUILayout.Label(control, keyStyle, GUILayout.Width(150));
            GUILayout.Label("→  " + description, descStyle);
            GUILayout.EndHorizontal();
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}