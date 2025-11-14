using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using RPGCorruption.Data;
using RPGCorruption.Data.Runtime;

namespace RPGCorruption.Combat
{
    /// <summary>
    /// Helper para iniciar batallas y pasar datos entre escenas.
    /// Actúa como puente entre el mapa y la escena de batalla.
    /// </summary>
    public class BattleInitializer : MonoBehaviour
    {
        // Datos temporales para pasar entre escenas
        private static CharacterInstance playerCharacter;
        private static List<EnemyData> enemyDataList;
        private static string returnSceneName;

        // Singleton
        private static BattleInitializer instance;
        public static BattleInitializer Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("BattleInitializer");
                    instance = go.AddComponent<BattleInitializer>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        /// <summary>
        /// Inicia una batalla con un solo enemigo
        /// </summary>
        public static void StartBattle(CharacterInstance player, EnemyData enemy)
        {
            StartBattle(player, new List<EnemyData> { enemy });
        }

        /// <summary>
        /// Inicia una batalla con múltiples enemigos
        /// </summary>
        public static void StartBattle(CharacterInstance player, List<EnemyData> enemies)
        {
            if (player == null)
            {
                Debug.LogError("Cannot start battle: Player is null!");
                return;
            }

            if (enemies == null || enemies.Count == 0)
            {
                Debug.LogError("Cannot start battle: No enemies!");
                return;
            }

            // Guardar datos de la batalla
            playerCharacter = player;
            enemyDataList = new List<EnemyData>(enemies);
            returnSceneName = SceneManager.GetActiveScene().name;

            Debug.Log($"Starting battle with {enemies.Count} enemy(ies). Will return to: {returnSceneName}");

            // Cargar escena de batalla
            SceneManager.LoadScene("BattleScene");
        }

        /// <summary>
        /// Obtiene los datos de la batalla actual
        /// </summary>
        public static void GetBattleData(out CharacterInstance player, out List<EnemyData> enemies)
        {
            player = playerCharacter;
            enemies = enemyDataList;

            if (player == null || enemies == null)
            {
                Debug.LogWarning("Battle data is null! Was StartBattle() called?");
            }
        }

        /// <summary>
        /// Vuelve a la escena del mapa
        /// </summary>
        public static void ReturnToMap()
        {
            if (string.IsNullOrEmpty(returnSceneName))
            {
                Debug.LogWarning("Return scene name is null, loading default scene...");
                returnSceneName = "SampleScene"; // O tu escena de mapa por defecto
            }

            Debug.Log($"Returning to: {returnSceneName}");

            // Limpiar datos de batalla
            ClearBattleData();

            // Volver a la escena
            SceneManager.LoadScene(returnSceneName);
        }

        /// <summary>
        /// Limpia los datos temporales
        /// </summary>
        private static void ClearBattleData()
        {
            playerCharacter = null;
            enemyDataList = null;
        }

        /// <summary>
        /// Verifica si hay datos de batalla disponibles
        /// </summary>
        public static bool HasBattleData()
        {
            return playerCharacter != null && enemyDataList != null;
        }
    }
}