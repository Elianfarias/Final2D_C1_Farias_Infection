using UnityEngine;
using System.Collections.Generic;
using RPGCorruption.Data;

namespace RPGCorruption.Map
{
    /// <summary>
    /// Sistema para spawner enemigos en el mapa.
    /// Puede colocarlos manualmente o generar encuentros aleatorios.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Configuration")]
        [SerializeField] private List<EnemySpawnData> enemiesToSpawn = new List<EnemySpawnData>();
        [SerializeField] private bool spawnOnStart = true;

        [Header("Random Spawning")]
        [SerializeField] private bool enableRandomSpawning = false;
        [SerializeField] private int maxRandomEnemies = 5;
        [SerializeField] private Vector2 spawnAreaMin = new Vector2(-10, -10);
        [SerializeField] private Vector2 spawnAreaMax = new Vector2(10, 10);
        [SerializeField] private List<EnemyData> randomEnemyPool = new List<EnemyData>();

        [Header("Prefab")]
        [SerializeField] private GameObject enemyPrefab;

        private List<EnemyMapEntity> spawnedEnemies = new List<EnemyMapEntity>();

        // Properties
        public int ActiveEnemyCount => spawnedEnemies.FindAll(e => e != null && !e.WasDefeated).Count;
        public int TotalEnemyCount => spawnedEnemies.Count;

        private void Start()
        {
            if (spawnOnStart)
            {
                SpawnAllEnemies();
            }
        }

        /// <summary>
        /// Spawner todos los enemigos configurados
        /// </summary>
        [ContextMenu("Spawn All Enemies")]
        public void SpawnAllEnemies()
        {
            // Limpiar lista de enemigos anteriores
            spawnedEnemies.Clear();

            // Spawner enemigos configurados manualmente
            foreach (var spawnData in enemiesToSpawn)
            {
                if (spawnData.enemyData != null)
                {
                    SpawnEnemy(spawnData.enemyData, spawnData.position, spawnData.isBoss);
                }
            }

            // Spawner enemigos aleatorios si está habilitado
            if (enableRandomSpawning && randomEnemyPool.Count > 0)
            {
                SpawnRandomEnemies();
            }

            Debug.Log($"Spawned {spawnedEnemies.Count} enemies in the map!");
        }

        /// <summary>
        /// Spawner un enemigo específico en una posición
        /// </summary>
        public EnemyMapEntity SpawnEnemy(EnemyData enemyData, Vector3 position, bool isBoss = false)
        {
            if (enemyData == null)
            {
                Debug.LogError("Cannot spawn enemy: EnemyData is null!");
                return null;
            }

            GameObject enemyObj;

            // Usar prefab si está asignado, sino crear desde cero
            if (enemyPrefab != null)
            {
                enemyObj = Instantiate(enemyPrefab, position, Quaternion.identity, transform);
            }
            else
            {
                enemyObj = CreateEnemyGameObject(position);
            }

            enemyObj.name = $"{enemyData.CharacterName} ({enemyData.Level})";

            // Configurar componente EnemyMapEntity
            EnemyMapEntity enemy = enemyObj.GetComponent<EnemyMapEntity>();
            if (enemy == null)
            {
                enemy = enemyObj.AddComponent<EnemyMapEntity>();
            }

            // Usar reflection para asignar el enemyData (ya que es SerializeField privado)
            var field = typeof(EnemyMapEntity).GetField("enemyTemplate",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(enemy, enemyData);

            var bossField = typeof(EnemyMapEntity).GetField("isBoss",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            bossField?.SetValue(enemy, isBoss);

            // Forzar Start() si el objeto ya estaba activo
            enemy.SendMessage("Start", SendMessageOptions.DontRequireReceiver);

            spawnedEnemies.Add(enemy);

            return enemy;
        }

        /// <summary>
        /// Crea un GameObject de enemigo básico
        /// </summary>
        private GameObject CreateEnemyGameObject(Vector3 position)
        {
            GameObject enemyObj = new GameObject("Enemy");
            enemyObj.transform.position = position;
            enemyObj.transform.parent = transform;

            // Agregar componentes necesarios
            SpriteRenderer sr = enemyObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 5;

            CircleCollider2D collider = enemyObj.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.5f;

            return enemyObj;
        }

        /// <summary>
        /// Spawner enemigos en posiciones aleatorias
        /// </summary>
        private void SpawnRandomEnemies()
        {
            for (int i = 0; i < maxRandomEnemies; i++)
            {
                // Posición aleatoria dentro del área
                Vector3 randomPos = new Vector3(
                    Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                    Random.Range(spawnAreaMin.y, spawnAreaMax.y),
                    0
                );

                // Enemigo aleatorio del pool
                EnemyData randomEnemy = randomEnemyPool[Random.Range(0, randomEnemyPool.Count)];

                SpawnEnemy(randomEnemy, randomPos);
            }
        }

        /// <summary>
        /// Limpia todos los enemigos spawneados
        /// </summary>
        [ContextMenu("Clear All Enemies")]
        public void ClearAllEnemies()
        {
            foreach (var enemy in spawnedEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy.gameObject);
                }
            }

            spawnedEnemies.Clear();
            Debug.Log("All enemies cleared!");
        }

        /// <summary>
        /// Resetea todos los enemigos derrotados
        /// </summary>
        [ContextMenu("Reset All Enemies")]
        public void ResetAllEnemies()
        {
            foreach (var enemy in spawnedEnemies)
            {
                if (enemy != null)
                {
                    enemy.ResetEnemy();
                }
            }

            Debug.Log("All enemies reset!");
        }

        /// <summary>
        /// Dibuja el área de spawn aleatorio en el editor
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (!enableRandomSpawning) return;

            Gizmos.color = new Color(1, 1, 0, 0.3f);

            Vector3 center = new Vector3(
                (spawnAreaMin.x + spawnAreaMax.x) / 2,
                (spawnAreaMin.y + spawnAreaMax.y) / 2,
                0
            );

            Vector3 size = new Vector3(
                spawnAreaMax.x - spawnAreaMin.x,
                spawnAreaMax.y - spawnAreaMin.y,
                0
            );

            Gizmos.DrawCube(center, size);
            Gizmos.DrawWireCube(center, size);
        }

        /// <summary>
        /// Muestra info de enemigos en GUI
        /// </summary>
        private void OnGUI()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 14;
            style.normal.textColor = Color.white;

            GUILayout.BeginArea(new Rect(10, 200, 300, 100));
            GUILayout.BeginVertical("box");

            GUILayout.Label($"Enemies: {ActiveEnemyCount} / {TotalEnemyCount}", style);

            if (GUILayout.Button("Respawn All Enemies"))
            {
                ClearAllEnemies();
                SpawnAllEnemies();
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }

    /// <summary>
    /// Datos de spawn de un enemigo específico
    /// </summary>
    [System.Serializable]
    public class EnemySpawnData
    {
        public EnemyData enemyData;
        public Vector3 position;
        public bool isBoss = false;
    }
}