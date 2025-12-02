using UnityEngine;
using System.Collections.Generic;
using RPGCorruption.Data;

namespace RPGCorruption.Map
{

    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Configuration")]
        [SerializeField] private List<EnemySpawnData> enemiesToSpawn = new();
        [SerializeField] private bool spawnOnStart = true;

        [Header("Random Spawning")]
        [SerializeField] private bool enableRandomSpawning = false;
        [SerializeField] private int maxRandomEnemies = 5;
        [SerializeField] private Vector2 spawnAreaMin = new(-10, -10);
        [SerializeField] private Vector2 spawnAreaMax = new(10, 10);
        [SerializeField] private List<EnemyData> randomEnemyPool = new();

        private List<EnemyMapEntity> spawnedEnemies = new();

        // Properties
        public int ActiveEnemyCount => spawnedEnemies.FindAll(e => e != null && !e.WasDefeated).Count;
        public int TotalEnemyCount => spawnedEnemies.Count;

        private void Start()
        {
            if (spawnOnStart)
                SpawnAllEnemies();
        }

        [ContextMenu("Spawn All Enemies")]
        public void SpawnAllEnemies()
        {
            spawnedEnemies.Clear();

            foreach (var spawnData in enemiesToSpawn)
            {
                if (spawnData.enemyData != null)
                    SpawnEnemy(spawnData.enemyData, spawnData.position, spawnData.isBoss);
            }

            if (enableRandomSpawning && randomEnemyPool.Count > 0)
                SpawnRandomEnemies();
        }

        public EnemyMapEntity SpawnEnemy(EnemyData enemyData, Vector3 position, bool isBoss = false)
        {
            if (enemyData == null)
                return null;

            GameObject enemyObj;

            enemyObj = CreateEnemyGameObject(position);

            enemyObj.name = $"{enemyData.CharacterName} ({enemyData.Level})";
            enemyObj.layer = (int)Mathf.Log(enemyData.Layer.value, 2);
            SpriteRenderer spriteEnemy = enemyObj.GetComponent<SpriteRenderer>();
            spriteEnemy.sortingLayerName = enemyData.SortingLayerName;

            if (!enemyObj.TryGetComponent<EnemyMapEntity>(out var enemy))
            {
                enemy = enemyObj.AddComponent<EnemyMapEntity>();
            }

            var field = typeof(EnemyMapEntity).GetField("enemyTemplate",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(enemy, enemyData);

            var bossField = typeof(EnemyMapEntity).GetField("isBoss",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            bossField?.SetValue(enemy, isBoss);

            enemy.SendMessage("Start", SendMessageOptions.DontRequireReceiver);

            spawnedEnemies.Add(enemy);

            return enemy;
        }

        private GameObject CreateEnemyGameObject(Vector3 position)
        {
            GameObject enemyObj = new("Enemy");
            enemyObj.transform.position = position;
            enemyObj.transform.parent = transform;

            SpriteRenderer sr = enemyObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 5;

            CircleCollider2D collider = enemyObj.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.5f;

            return enemyObj;
        }

        private void SpawnRandomEnemies()
        {
            for (int i = 0; i < maxRandomEnemies; i++)
            {
                Vector3 randomPos = new(
                    Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                    Random.Range(spawnAreaMin.y, spawnAreaMax.y),
                    0
                );

                EnemyData randomEnemy = randomEnemyPool[Random.Range(0, randomEnemyPool.Count - 1)];

                SpawnEnemy(randomEnemy, randomPos);
            }
        }

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
        }

        [ContextMenu("Reset All Enemies")]
        public void ResetAllEnemies()
        {
            foreach (var enemy in spawnedEnemies)
            {
                if (enemy != null)
                    enemy.ResetEnemy();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!enableRandomSpawning) return;

            Gizmos.color = new Color(1, 1, 0, 0.3f);

            Vector3 center = new(
                (spawnAreaMin.x + spawnAreaMax.x) / 2,
                (spawnAreaMin.y + spawnAreaMax.y) / 2,
                0
            );

            Vector3 size = new(
                spawnAreaMax.x - spawnAreaMin.x,
                spawnAreaMax.y - spawnAreaMin.y,
                0
            );

            Gizmos.DrawCube(center, size);
            Gizmos.DrawWireCube(center, size);
        }

        private void OnGUI()
        {
            GUIStyle style = new(GUI.skin.label);
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

    [System.Serializable]
    public class EnemySpawnData
    {
        public EnemyData enemyData;
        public Vector3 position;
        public bool isBoss = false;
    }
}