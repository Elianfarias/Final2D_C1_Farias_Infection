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
            if (gameConfig != null)
                GameConfig.Instance = gameConfig;
        }

        #region Setup Buttons (Inspector)

        [ContextMenu("1. Setup Scene")]
        public void SetupScene()
        {

            CreateCamera();
            CreateTileGrid();
            CreatePlayer();
            CreateEnemySpawner();

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

            }
        }

        [ContextMenu("3. Create TileGrid")]
        public void CreateTileGrid()
        {
            tileGrid = Object.FindFirstObjectByType<TileGrid>();

            if (tileGrid == null)
            {
                GameObject gridObj = new("TileGrid");
                tileGrid = gridObj.AddComponent<TileGrid>();
            }
        }

        [ContextMenu("4. Create Player")]
        public void CreatePlayer()
        {
            playerController = Object.FindFirstObjectByType<PlayerController>();

            if (playerController == null)
            {
                GameObject playerObj = new("Player");
                playerObj.transform.position = (Vector3)spawnPosition;

                // Agregar componentes
                SpriteRenderer sr = playerObj.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 10;

                playerController = playerObj.AddComponent<PlayerController>();
                playerObj.AddComponent<PlayerMovement>();

                // Inicializar con CharacterData si está asignado
                if (playerCharacterData != null)
                {
                    playerController.InitializeCharacter(playerCharacterData, startingLevel);
                }
            }
        }

        [ContextMenu("5. Test Movement")]
        public void TestMovement()
        {
            if (playerController != null && playerController.Movement != null)
            {
                Vector3 testPosition = new Vector3(5, 5, 0);
                playerController.Movement.SetTargetPosition(testPosition);
            }
        }

        [ContextMenu("7. Create Enemy Spawner")]
        public void CreateEnemySpawner()
        {
            enemySpawner = Object.FindFirstObjectByType<EnemySpawner>();

            if (enemySpawner == null)
            {
                GameObject spawnerObj = new("EnemySpawner");
                enemySpawner = spawnerObj.AddComponent<EnemySpawner>();

                // Si hay enemigos de prueba asignados, spawnerlos
                if (testEnemies != null && testEnemies.Length > 0)
                    SpawnTestEnemies();
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
                CreateEnemySpawner();

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
                    randomPos = tileGrid.SnapToGrid(randomPos);

                enemySpawner.SpawnEnemy(randomEnemy, randomPos);
            }
        }

        #endregion

        #region Auto-Find References

        private void OnValidate()
        {
            // Auto-encontrar referencias si no están asignadas
            if (tileGrid == null)
                tileGrid = Object.FindFirstObjectByType<TileGrid>();

            if (playerController == null)
                playerController = Object.FindFirstObjectByType<PlayerController>();

            if (mainCamera == null)
                mainCamera = Camera.main;

            if (enemySpawner == null)
                enemySpawner = Object.FindFirstObjectByType<EnemySpawner>();
        }

        #endregion

        #region Debug Info

        private void OnGUI()
        {
            GUIStyle titleStyle = new(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };
            titleStyle.normal.textColor = Color.white;

            GUIStyle labelStyle = new(GUI.skin.label)
            {
                fontSize = 15
            };
            labelStyle.normal.textColor = Color.white;

            GUIStyle instructionStyle = new(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Italic
            };
            instructionStyle.normal.textColor = Color.yellow;

            GUIStyle gUIStyle = new(GUI.skin.box);
            GUIStyle boxStyle = gUIStyle;
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