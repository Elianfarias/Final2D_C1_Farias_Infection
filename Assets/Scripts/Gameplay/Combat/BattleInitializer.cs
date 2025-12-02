using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using RPGCorruption.Data;
using RPGCorruption.Data.Runtime;

namespace RPGCorruption.Combat
{
    public class BattleInitializer : MonoBehaviour
    {
        private static CharacterInstance playerCharacter;
        private static List<EnemyData> enemyDataList;
        private static string returnSceneName;

        private static BattleInitializer instance;
        public static BattleInitializer Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new("BattleInitializer");
                    instance = go.AddComponent<BattleInitializer>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        public static void StartBattle(CharacterInstance player, EnemyData enemy)
        {
            StartBattle(player, new List<EnemyData> { enemy });
        }

        public static void StartBattle(CharacterInstance player, List<EnemyData> enemies)
        {
            if (player == null)
                return;

            if (enemies == null || enemies.Count == 0)
                return;

            playerCharacter = player;
            enemyDataList = new List<EnemyData>(enemies);
            returnSceneName = SceneManager.GetActiveScene().name;

            SceneManager.LoadScene("BattleScene");
        }

        public static void GetBattleData(out CharacterInstance player, out List<EnemyData> enemies)
        {
            player = playerCharacter;
            enemies = enemyDataList;
        }

        public static void ReturnToMap()
        {
            if (string.IsNullOrEmpty(returnSceneName))
                returnSceneName = "MapScene";

            ClearBattleData();

            SceneManager.LoadScene(returnSceneName);
        }

        private static void ClearBattleData()
        {
            playerCharacter = null;
            enemyDataList = null;
        }

        public static bool HasBattleData()
        {
            return playerCharacter != null && enemyDataList != null;
        }
    }
}