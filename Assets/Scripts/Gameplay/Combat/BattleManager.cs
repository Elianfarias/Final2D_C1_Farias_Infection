using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCorruption.Data;
using RPGCorruption.Data.Runtime;

namespace RPGCorruption.Combat
{
    public class BattleManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float actionDelay = 1f;
        [SerializeField] private bool allowEscape = true;

        [Header("Battle Data")]
        [SerializeField] private List<CharacterInstance> playerParty;
        [SerializeField] private List<CharacterInstance> enemyParty;

        // Systems
        private TurnSystem turnSystem;
        private BattleRewards rewards;
        private BattleState currentState;

        // Events
        public System.Action<BattleState> OnStateChanged;
        public System.Action<CharacterInstance, int> OnDamageDealt;
        public System.Action<CharacterInstance> OnCharacterDefeated;
        public System.Action<BattleRewards> OnBattleWon;
        public System.Action OnBattleLost;

        // Properties
        public BattleState CurrentState => currentState;
        public CharacterInstance CurrentTurn => turnSystem?.CurrentTurn;
        public List<CharacterInstance> PlayerParty => playerParty;
        public List<CharacterInstance> EnemyParty => enemyParty;
        public bool IsPlayerTurn => currentState == BattleState.PlayerTurn;

        private void Awake()
        {
            turnSystem = new TurnSystem();
            rewards = new BattleRewards();
        }

        private void Start()
        {
            if (BattleInitializer.HasBattleData())
            {
                BattleInitializer.GetBattleData(
                    out CharacterInstance player,
                    out List<EnemyData> enemies
                );

                List<CharacterInstance> playerParty = new() { player };

                StartBattle(playerParty, enemies);
            }
        }

        public void StartBattle(List<CharacterInstance> players, List<EnemyData> enemyData)
        {
            enemyParty = new List<CharacterInstance>();
            foreach (var enemy in enemyData)
            {
                var enemyInstance = new CharacterInstance(enemy, enemy.Level);
                enemyParty.Add(enemyInstance);
            }

            playerParty = new List<CharacterInstance>(players);

            turnSystem.Initialize(playerParty, enemyParty);

            ChangeState(BattleState.Start);

            StartCoroutine(BattleLoop());
        }

        private IEnumerator BattleLoop()
        {
            yield return new WaitForSeconds(0.5f);

            while (currentState != BattleState.Victory &&
                   currentState != BattleState.Defeat &&
                   currentState != BattleState.Escaped)
            {
                var currentCharacter = turnSystem.NextTurn();

                if (currentCharacter == null)
                    break;

                bool isPlayerCharacter = playerParty.Contains(currentCharacter);

                if (isPlayerCharacter)
                {
                    ChangeState(BattleState.PlayerTurn);

                    yield return new WaitUntil(() => currentState != BattleState.PlayerTurn);
                }
                else
                {
                    ChangeState(BattleState.EnemyTurn);
                    yield return StartCoroutine(ExecuteEnemyTurn(currentCharacter));
                }

                if (CheckBattleEnd())
                    break;

                yield return new WaitForSeconds(0.3f);
            }

            yield return StartCoroutine(EndBattle());
        }

        private IEnumerator ExecuteEnemyTurn(CharacterInstance enemy)
        {
            ChangeState(BattleState.Busy);

            yield return new WaitForSeconds(actionDelay);

            var alivePlayer = GetRandomAliveTarget(playerParty);

            if (alivePlayer != null)
                PerformAttack(enemy, alivePlayer);

            yield return new WaitForSeconds(actionDelay);
        }

        private void ChangeState(BattleState newState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(currentState);
        }

        #region Player Actions

        public void PlayerChooseAttack(CharacterInstance target)
        {
            if (currentState != BattleState.PlayerTurn)
                return;

            StartCoroutine(ExecutePlayerAttack(target));
        }

        private IEnumerator ExecutePlayerAttack(CharacterInstance target)
        {
            ChangeState(BattleState.Busy);

            yield return new WaitForSeconds(actionDelay * 0.5f);

            PerformAttack(CurrentTurn, target);

            yield return new WaitForSeconds(actionDelay);

            ChangeState(BattleState.Start);
        }

        public void PlayerChooseRun()
        {
            if (currentState != BattleState.PlayerTurn)
                return;

            if (!allowEscape)
                return;

            StartCoroutine(ExecuteEscape());
        }

        private IEnumerator ExecuteEscape()
        {
            ChangeState(BattleState.Busy);

            yield return new WaitForSeconds(actionDelay);

            float escapeChance = 50f;

            float avgPlayerSpeed = GetAverageSpeed(playerParty);
            float avgEnemySpeed = GetAverageSpeed(enemyParty);

            if (avgPlayerSpeed > avgEnemySpeed)
                escapeChance += 25f;

            float roll = Random.Range(0f, 100f);

            if (roll <= escapeChance)
                ChangeState(BattleState.Escaped);
            else
                ChangeState(BattleState.Start);
        }

        #endregion

        #region Combat Actions

        private void PerformAttack(CharacterInstance attacker, CharacterInstance target)
        {
            if (attacker == null || target == null)
                return;

            int baseDamage = Mathf.Max(1, attacker.Attack - target.Defense);

            float variance = Random.Range(0.8f, 1.2f);
            int finalDamage = Mathf.RoundToInt(baseDamage * variance);

            target.TakeDamage(finalDamage);

            OnDamageDealt?.Invoke(target, finalDamage);

            if (target.IsDead)
                OnCharacterDefeated?.Invoke(target);
        }

        #endregion

        #region Battle End

        private bool CheckBattleEnd()
        {
            bool allEnemiesDead = true;
            foreach (var enemy in enemyParty)
            {
                if (enemy.IsAlive)
                {
                    allEnemiesDead = false;
                    break;
                }
            }

            if (allEnemiesDead)
            {
                ChangeState(BattleState.Victory);
                return true;
            }

            bool allPlayersDead = true;
            foreach (var player in playerParty)
            {
                if (player.IsAlive)
                {
                    allPlayersDead = false;
                    break;
                }
            }

            if (allPlayersDead)
            {
                ChangeState(BattleState.Defeat);
                return true;
            }

            return false;
        }

        private IEnumerator EndBattle()
        {
            yield return new WaitForSeconds(1f);

            if (currentState == BattleState.Victory)
            {
                List<EnemyData> defeatedEnemies = new();
                foreach (var enemy in enemyParty)
                {
                    defeatedEnemies.Add(enemy.Template as EnemyData);
                }

                rewards.CalculateRewards(defeatedEnemies);

                OnBattleWon?.Invoke(rewards);
            }
            else if (currentState == BattleState.Defeat)
                OnBattleLost?.Invoke();

            yield return new WaitForSeconds(2f);

            BattleInitializer.ReturnToMap();
        }

        #endregion

        #region Utility

        private CharacterInstance GetRandomAliveTarget(List<CharacterInstance> targets)
        {
            var aliveTargets = new List<CharacterInstance>();
            foreach (var target in targets)
            {
                if (target.IsAlive)
                    aliveTargets.Add(target);
            }

            if (aliveTargets.Count == 0) return null;

            return aliveTargets[Random.Range(0, aliveTargets.Count)];
        }

        private float GetAverageSpeed(List<CharacterInstance> characters)
        {
            if (characters.Count == 0) return 0f;

            float total = 0f;
            foreach (var character in characters)
            {
                total += character.Speed;
            }

            return total / characters.Count;
        }

        #endregion
    }
}