using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCorruption.Data;
using RPGCorruption.Data.Runtime;

namespace RPGCorruption.Combat
{
    /// <summary>
    /// Controlador principal del sistema de combate.
    /// Maneja el flujo de batalla, turnos, acciones y resultado.
    /// </summary>
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

        // Events (para conectar con UI)
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

        /// <summary>
        /// Inicia una batalla con el grupo de jugadores vs enemigos
        /// </summary>
        public void StartBattle(List<CharacterInstance> players, List<EnemyData> enemyData)
        {
            Debug.Log("=== BATTLE START ===");

            // Crear instancias de enemigos
            enemyParty = new List<CharacterInstance>();
            foreach (var enemy in enemyData)
            {
                var enemyInstance = new CharacterInstance(enemy, enemy.Level);
                enemyParty.Add(enemyInstance);
                Debug.Log($"Enemy: {enemy.CharacterName} (Lv.{enemy.Level}) HP:{enemyInstance.MaxHP}");
            }

            // Guardar referencia del grupo de jugadores
            playerParty = new List<CharacterInstance>(players);

            // Inicializar sistema de turnos
            turnSystem.Initialize(playerParty, enemyParty);

            // Cambiar a estado inicial
            ChangeState(BattleState.Start);

            // Comenzar primer turno
            StartCoroutine(BattleLoop());
        }

        /// <summary>
        /// Loop principal de batalla
        /// </summary>
        private IEnumerator BattleLoop()
        {
            // Mensaje de inicio
            yield return new WaitForSeconds(0.5f);

            while (currentState != BattleState.Victory &&
                   currentState != BattleState.Defeat &&
                   currentState != BattleState.Escaped)
            {
                // Obtener siguiente turno
                var currentCharacter = turnSystem.NextTurn();

                if (currentCharacter == null)
                {
                    Debug.LogError("No more turns available!");
                    break;
                }

                // Determinar si es turno del jugador o enemigo
                bool isPlayerCharacter = playerParty.Contains(currentCharacter);

                if (isPlayerCharacter)
                {
                    // Turno del jugador - esperar input
                    ChangeState(BattleState.PlayerTurn);

                    // El jugador debe llamar a ExecutePlayerAction() cuando elija una acción
                    yield return new WaitUntil(() => currentState != BattleState.PlayerTurn);
                }
                else
                {
                    // Turno del enemigo - IA automática
                    ChangeState(BattleState.EnemyTurn);
                    yield return StartCoroutine(ExecuteEnemyTurn(currentCharacter));
                }

                // Verificar condición de victoria/derrota
                if (CheckBattleEnd())
                {
                    break;
                }

                yield return new WaitForSeconds(0.3f);
            }

            // Batalla terminada
            yield return StartCoroutine(EndBattle());
        }

        /// <summary>
        /// Ejecuta el turno del enemigo (IA simple)
        /// </summary>
        private IEnumerator ExecuteEnemyTurn(CharacterInstance enemy)
        {
            ChangeState(BattleState.Busy);

            yield return new WaitForSeconds(actionDelay);

            // IA simple: atacar a un jugador aleatorio vivo
            var alivePlayer = GetRandomAliveTarget(playerParty);

            if (alivePlayer != null)
            {
                PerformAttack(enemy, alivePlayer);
            }

            yield return new WaitForSeconds(actionDelay);
        }

        /// <summary>
        /// Cambia el estado de la batalla
        /// </summary>
        private void ChangeState(BattleState newState)
        {
            currentState = newState;
            Debug.Log($"Battle State: {currentState}");
            OnStateChanged?.Invoke(currentState);
        }

        #region Player Actions

        /// <summary>
        /// El jugador elige atacar
        /// </summary>
        public void PlayerChooseAttack(CharacterInstance target)
        {
            if (currentState != BattleState.PlayerTurn)
            {
                Debug.LogWarning("Not player's turn!");
                return;
            }

            StartCoroutine(ExecutePlayerAttack(target));
        }

        /// <summary>
        /// Ejecuta el ataque del jugador
        /// </summary>
        private IEnumerator ExecutePlayerAttack(CharacterInstance target)
        {
            ChangeState(BattleState.Busy);

            yield return new WaitForSeconds(actionDelay * 0.5f);

            PerformAttack(CurrentTurn, target);

            yield return new WaitForSeconds(actionDelay);

            // Turno terminado, volver al loop
            ChangeState(BattleState.Start);
        }

        /// <summary>
        /// El jugador intenta escapar
        /// </summary>
        public void PlayerChooseRun()
        {
            if (currentState != BattleState.PlayerTurn)
            {
                Debug.LogWarning("Not player's turn!");
                return;
            }

            if (!allowEscape)
            {
                Debug.Log("Cannot escape from this battle!");
                return;
            }

            StartCoroutine(ExecuteEscape());
        }

        /// <summary>
        /// Intenta escapar de la batalla
        /// </summary>
        private IEnumerator ExecuteEscape()
        {
            ChangeState(BattleState.Busy);

            yield return new WaitForSeconds(actionDelay);

            // Probabilidad de escape: 50% base
            float escapeChance = 50f;

            // Aumentar probabilidad si jugadores son más rápidos
            float avgPlayerSpeed = GetAverageSpeed(playerParty);
            float avgEnemySpeed = GetAverageSpeed(enemyParty);

            if (avgPlayerSpeed > avgEnemySpeed)
            {
                escapeChance += 25f;
            }

            float roll = Random.Range(0f, 100f);

            if (roll <= escapeChance)
            {
                Debug.Log("Successfully escaped!");
                ChangeState(BattleState.Escaped);
            }
            else
            {
                Debug.Log("Failed to escape!");
                // Turno perdido, continuar batalla
                ChangeState(BattleState.Start);
            }
        }

        #endregion

        #region Combat Actions

        /// <summary>
        /// Realiza un ataque básico
        /// </summary>
        private void PerformAttack(CharacterInstance attacker, CharacterInstance target)
        {
            if (attacker == null || target == null)
            {
                Debug.LogError("Invalid attacker or target!");
                return;
            }

            // Calcular daño: Attack del atacante - Defense del objetivo
            int baseDamage = Mathf.Max(1, attacker.Attack - target.Defense);

            // Agregar variación aleatoria (±20%)
            float variance = Random.Range(0.8f, 1.2f);
            int finalDamage = Mathf.RoundToInt(baseDamage * variance);

            // Aplicar daño
            target.TakeDamage(finalDamage);

            Debug.Log($"{attacker.Template.CharacterName} attacks {target.Template.CharacterName} for {finalDamage} damage!");

            // Notificar evento
            OnDamageDealt?.Invoke(target, finalDamage);

            // Verificar si el objetivo murió
            if (target.IsDead)
            {
                Debug.Log($"{target.Template.CharacterName} was defeated!");
                OnCharacterDefeated?.Invoke(target);
            }
        }

        #endregion

        #region Battle End

        /// <summary>
        /// Verifica si la batalla debe terminar
        /// </summary>
        private bool CheckBattleEnd()
        {
            // Victoria: todos los enemigos derrotados
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

            // Derrota: todos los jugadores derrotados
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

        /// <summary>
        /// Finaliza la batalla y otorga recompensas
        /// </summary>
        private IEnumerator EndBattle()
        {
            yield return new WaitForSeconds(1f);

            if (currentState == BattleState.Victory)
            {
                Debug.Log("=== VICTORY ===");

                // Calcular recompensas
                List<EnemyData> defeatedEnemies = new List<EnemyData>();
                foreach (var enemy in enemyParty)
                {
                    defeatedEnemies.Add(enemy.Template as EnemyData);
                }

                rewards.CalculateRewards(defeatedEnemies);

                // TODO: Otorgar recompensas al grupo cuando tengamos PartyData
                // rewards.GrantRewards(partyData);

                OnBattleWon?.Invoke(rewards);
            }
            else if (currentState == BattleState.Defeat)
            {
                Debug.Log("=== DEFEAT ===");
                OnBattleLost?.Invoke();
            }
            else if (currentState == BattleState.Escaped)
            {
                Debug.Log("=== ESCAPED ===");
                // Volver al mapa sin recompensas
            }

            yield return new WaitForSeconds(2f);

            // TODO: Volver al mapa o mostrar pantalla de game over
        }

        #endregion

        #region Utility

        /// <summary>
        /// Obtiene un objetivo aleatorio vivo
        /// </summary>
        private CharacterInstance GetRandomAliveTarget(List<CharacterInstance> targets)
        {
            var aliveTargets = new List<CharacterInstance>();
            foreach (var target in targets)
            {
                if (target.IsAlive)
                {
                    aliveTargets.Add(target);
                }
            }

            if (aliveTargets.Count == 0) return null;

            return aliveTargets[Random.Range(0, aliveTargets.Count)];
        }

        /// <summary>
        /// Calcula la velocidad promedio de un grupo
        /// </summary>
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

        #region Debug

        [ContextMenu("Debug: Start Test Battle")]
        public void Debug_StartTestBattle()
        {
            // Crear datos de prueba
            var testPlayers = new List<CharacterInstance>();
            var testEnemies = new List<EnemyData>();

            // TODO: Cargar desde assets reales
            Debug.Log("Test battle needs CharacterData and EnemyData assets!");
        }

        #endregion
    }
}