using UnityEngine;
using RPGCorruption.Data;
using RPGCorruption.Map;

namespace RPGCorruption.Core
{
    public class GameSetup : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private GameConfig gameConfig;

        private void Awake()
        {
            if (gameConfig != null)
                GameConfig.Instance = gameConfig;
        }
    }
}