using UnityEngine;
using RPGCorruption.Data;
using RPGCorruption.Map;

namespace RPGCorruption.Core
{
    /// <summary>
    /// Helper para configurar rápidamente una escena de testing.
    /// Adjunta este script a un GameObject en la escena y presiona botones en el Inspector.
    /// </summary>
    public class GameSetup : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private GameConfig gameConfig;

        [Header("Player Setup")]
        [SerializeField] private CharacterData playerCharacterData;
        [SerializeField] private int startingLevel = 1;
        [SerializeField] private Vector2 spawnPosition = Vector2.zero;

        [Header("Enemy Setup")]
        [SerializeField] private EnemyData[] testEnemies;
        [SerializeField] private int numberOfEnemies = 3;
        [SerializeField] private Vector2 enemySpawnAreaMin = new Vector2(-8, -8);
        [SerializeField] private Vector2 enemySpawnAreaMax = new Vector2(8, 8);

        [Header("References (Auto-populated)")]
        [SerializeField] private TileGrid tileGrid;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private EnemySpawner enemySpawner;

        private void Awake()
        {
            // Inicializar GameConfig si está asignado
            if (gameConfig != null)
            {
                GameConfig.Instance = gameConfig;
                Debug.Log("GameConfig initialized!");
            }
            else
            {
                Debug.LogWarning("GameConfig not assigned! Please assign it in the inspector.");
            }
        }

        #region Setup Buttons (Inspector)

        [ContextMenu("1. Setup Scene")]
        public void SetupScene()
        {
            Debug.Log("=== Setting up scene ===");

            CreateCamera();
            CreateTileGrid();
            CreatePlayer();
            CreateEnemySpawner();
            CreateHelpOverlay();

            Debug.Log("Scene setup complete!");
        }

        [ContextMenu("2. Create Camera")]
        public void CreateCamera()
        {
            mainCamera = Camera.main;

            if (mainCamera == null)
            {
                GameObject cameraObj = new GameObject("Main Camera");
                mainCamera = cameraObj.AddComponent<Camera>();
                mainCamera.tag = "MainCamera";
                mainCamera.orthographic = true;
                mainCamera.orthographicSize = 5;
                mainCamera.transform.position = new Vector3(0, 0, -10);

                Debug.Log("Camera created!");
            }
            else
            {
                Debug.Log("Camera already exists!");
            }
        }

        [ContextMenu("3. Create TileGrid")]
        public void CreateTileGrid()
        {
            tileGrid = FindObjectOfType<TileGrid>();

            if (tileGrid == null)
            {
                GameObject gridObj = new GameObject("TileGrid");
                tileGrid = gridObj.AddComponent<TileGrid>();

                Debug.Log("TileGrid created!");
            }
            else
            {
                Debug.Log("TileGrid already exists!");
            }
        }

        [ContextMenu("4. Create Player")]
        public void CreatePlayer()
        {
            playerController = FindObjectOfType<PlayerController>();

            if (playerController == null)
            {
                GameObject playerObj = new GameObject("Player");
                playerObj.transform.position = new Vector3(spawnPosition.x, spawnPosition.y, 0);

                // Agregar componentes
                SpriteRenderer sr = playerObj.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 10;

                // Crear sprite temporal (cuadrado azul)
                Texture2D texture = new Texture2D(32, 32);
                Color[] pixels = new Color[32 * 32];
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = Color.blue;
                }
                texture.SetPixels(pixels);
                texture.Apply();
                sr.sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);

                playerController = playerObj.AddComponent<PlayerController>();
                playerObj.AddComponent<PlayerMovement>();

                // Inicializar con CharacterData si está asignado
                if (playerCharacterData != null)
                {
                    playerController.InitializeCharacter(playerCharacterData, startingLevel);
                }

                Debug.Log("Player created!");
            }
            else
            {
                Debug.Log("Player already exists!");
            }
        }

        [ContextMenu("5. Test Movement")]
        public void TestMovement()
        {
            if (playerController != null && playerController.Movement != null)
            {
                Vector3 testPosition = new Vector3(5, 5, 0);
                playerController.Movement.SetTargetPosition(testPosition);
                Debug.Log($"Player moving to {testPosition}");
            }
            else
            {
                Debug.LogWarning("Player not found! Create player first.");
            }
        }

        [ContextMenu("6. Create Help Overlay")]
        public void CreateHelpOverlay()
        {
            // Buscar si ya existe
            RPGCorruption.UI.HelpOverlay existing = FindObjectOfType<RPGCorruption.UI.HelpOverlay>();

            if (existing == null)
            {
                GameObject helpObj = new GameObject("HelpOverlay");
                helpObj.AddComponent<RPGCorruption.UI.HelpOverlay>();

                Debug.Log("HelpOverlay created! Press H to toggle help menu.");
            }
            else
            {
                Debug.Log("HelpOverlay already exists!");
            }
        }

        [ContextMenu("7. Create Enemy Spawner")]
        public void CreateEnemySpawner()
        {
            enemySpawner = FindObjectOfType<EnemySpawner>();

            if (enemySpawner == null)
            {
                GameObject spawnerObj = new GameObject("EnemySpawner");
                enemySpawner = spawnerObj.AddComponent<EnemySpawner>();

                Debug.Log("EnemySpawner created!");

                // Si hay enemigos de prueba asignados, spawnerlos
                if (testEnemies != null && testEnemies.Length > 0)
                {
                    SpawnTestEnemies();
                }
            }
            else
            {
                Debug.Log("EnemySpawner already exists!");
            }
        }

        [ContextMenu("8. Spawn Test Enemies")]
        public void SpawnTestEnemies()
        {
            if (enemySpawner == null)
            {
                CreateEnemySpawner();
            }

            if (testEnemies == null || testEnemies.Length == 0)
            {
                Debug.LogWarning("No test enemies assigned! Assign EnemyData in the inspector.");
                return;
            }

            // Spawn enemigos en posiciones aleatorias
            for (int i = 0; i < numberOfEnemies; i++)
            {
                // Elegir enemigo aleatorio
                EnemyData randomEnemy = testEnemies[Random.Range(0, testEnemies.Length)];

                // Posición aleatoria en el área
                Vector3 randomPos = new Vector3(
                    Random.Range(enemySpawnAreaMin.x, enemySpawnAreaMax.x),
                    Random.Range(enemySpawnAreaMin.y, enemySpawnAreaMax.y),
                    0
                );

                // Snap to grid si TileGrid existe
                if (tileGrid != null)
                {
                    randomPos = tileGrid.SnapToGrid(randomPos);
                }

                enemySpawner.SpawnEnemy(randomEnemy, randomPos);
            }

            Debug.Log($"Spawned {numberOfEnemies} test enemies!");
        }

        #endregion

        #region Auto-Find References

        private void OnValidate()
        {
            // Auto-encontrar referencias si no están asignadas
            if (tileGrid == null)
            {
                tileGrid = FindObjectOfType<TileGrid>();
            }

            if (playerController == null)
            {
                playerController = FindObjectOfType<PlayerController>();
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (enemySpawner == null)
            {
                enemySpawner = FindObjectOfType<EnemySpawner>();
            }
        }

        #endregion

        #region Debug Info

        private void OnGUI()
        {
            // Estilo mejorado
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 18;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = Color.white;

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 15;
            labelStyle.normal.textColor = Color.white;

            GUIStyle instructionStyle = new GUIStyle(GUI.skin.label);
            instructionStyle.fontSize = 13;
            instructionStyle.fontStyle = FontStyle.Italic;
            instructionStyle.normal.textColor = Color.yellow;

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.1f, 0.8f));

            GUILayout.BeginArea(new Rect(Screen.width - 310, 10, 300, 250));
            GUILayout.BeginVertical(boxStyle);

            GUILayout.Space(5);
            GUILayout.Label("Game Setup", titleStyle);
            GUILayout.Space(10);

            GUILayout.Label($"GameConfig: {(gameConfig != null ? "✓" : "✗")}", labelStyle);
            GUILayout.Label($"TileGrid: {(tileGrid != null ? "✓" : "✗")}", labelStyle);
            GUILayout.Label($"Player: {(playerController != null ? "✓" : "✗")}", labelStyle);
            GUILayout.Label($"Camera: {(mainCamera != null ? "✓" : "✗")}", labelStyle);
            GUILayout.Label($"Enemies: {(enemySpawner != null ? enemySpawner.ActiveEnemyCount.ToString() : "✗")}", labelStyle);

            GUILayout.Space(10);
            GUILayout.Label("Right-click script → Context Menu", instructionStyle);

            GUILayout.Space(5);
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        /// <summary>
        /// Crea una textura de color sólido
        /// </summary>
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

        #endregion
    }
}