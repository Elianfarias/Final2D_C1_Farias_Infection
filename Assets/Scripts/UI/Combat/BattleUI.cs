using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using RPGCorruption.Data.Runtime;

namespace RPGCorruption.Combat
{
    /// <summary>
    /// Interfaz de usuario para el sistema de combate.
    /// Muestra información de personajes y botones de acción.
    /// </summary>
    public class BattleUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BattleManager battleManager;

        [Header("Action Buttons")]
        [SerializeField] private Button attackButton;
        [SerializeField] private Button skillButton;
        [SerializeField] private Button itemButton;
        [SerializeField] private Button runButton;

        [Header("Info Panels")]
        [SerializeField] private TextMeshProUGUI battleMessageText;
        [SerializeField] private Transform playerInfoParent;
        [SerializeField] private Transform enemyInfoParent;

        [Header("Prefabs")]
        [SerializeField] private GameObject characterInfoPrefab;

        [Header("Panels")]
        [SerializeField] private GameObject actionPanel;
        [SerializeField] private GameObject targetSelectionPanel;
        [SerializeField] private GameObject resultsPanel;

        private List<GameObject> playerInfoUI = new List<GameObject>();
        private List<GameObject> enemyInfoUI = new List<GameObject>();
        private List<Button> enemyTargetButtons = new List<Button>();

        private void Start()
        {
            // Configurar botones
            if (attackButton != null)
                attackButton.onClick.AddListener(OnAttackButtonClicked);

            if (skillButton != null)
                skillButton.onClick.AddListener(OnSkillButtonClicked);

            if (itemButton != null)
                itemButton.onClick.AddListener(OnItemButtonClicked);

            if (runButton != null)
                runButton.onClick.AddListener(OnRunButtonClicked);

            // Suscribirse a eventos del BattleManager
            if (battleManager != null)
            {
                battleManager.OnStateChanged += OnBattleStateChanged;
                battleManager.OnDamageDealt += OnDamageDealt;
                battleManager.OnCharacterDefeated += OnCharacterDefeated;
                battleManager.OnBattleWon += OnBattleWon;
                battleManager.OnBattleLost += OnBattleLost;
            }

            // Ocultar paneles inicialmente
            ShowActionPanel(false);
            ShowTargetSelection(false);
            ShowResultsPanel(false);
        }

        private void OnDestroy()
        {
            // Desuscribirse de eventos
            if (battleManager != null)
            {
                battleManager.OnStateChanged -= OnBattleStateChanged;
                battleManager.OnDamageDealt -= OnDamageDealt;
                battleManager.OnCharacterDefeated -= OnCharacterDefeated;
                battleManager.OnBattleWon -= OnBattleWon;
                battleManager.OnBattleLost -= OnBattleLost;
            }
        }

        #region UI Updates

        /// <summary>
        /// Actualiza la UI cuando cambia el estado de batalla
        /// </summary>
        private void OnBattleStateChanged(BattleState newState)
        {
            Debug.Log($"UI: Battle state changed to {newState}");

            switch (newState)
            {
                case BattleState.Start:
                    ShowMessage("Battle Start!");
                    UpdateAllCharacterInfo();
                    ShowActionPanel(false);
                    ShowTargetSelection(false);
                    break;

                case BattleState.PlayerTurn:
                    ShowMessage($"{battleManager.CurrentTurn.Template.CharacterName}'s turn!");
                    ShowActionPanel(true);
                    ShowTargetSelection(false);
                    break;

                case BattleState.EnemyTurn:
                    ShowMessage($"{battleManager.CurrentTurn.Template.CharacterName} is thinking...");
                    ShowActionPanel(false);
                    ShowTargetSelection(false);
                    break;

                case BattleState.Busy:
                    ShowActionPanel(false);
                    break;
            }
        }

        /// <summary>
        /// Actualiza la UI cuando se inflige daño
        /// </summary>
        private void OnDamageDealt(CharacterInstance target, int damage)
        {
            ShowMessage($"{target.Template.CharacterName} took {damage} damage!");
            UpdateCharacterInfo(target);
        }

        /// <summary>
        /// Actualiza la UI cuando un personaje es derrotado
        /// </summary>
        private void OnCharacterDefeated(CharacterInstance character)
        {
            ShowMessage($"{character.Template.CharacterName} was defeated!");
            UpdateCharacterInfo(character);
        }

        /// <summary>
        /// Muestra pantalla de victoria
        /// </summary>
        private void OnBattleWon(BattleRewards rewards)
        {
            ShowMessage("Victory!");
            ShowActionPanel(false);
            ShowResultsPanel(true);

            // TODO: Mostrar recompensas en el panel de resultados
        }

        /// <summary>
        /// Muestra pantalla de derrota
        /// </summary>
        private void OnBattleLost()
        {
            ShowMessage("Defeat...");
            ShowActionPanel(false);
            ShowResultsPanel(true);
        }

        /// <summary>
        /// Actualiza la información de todos los personajes
        /// </summary>
        public void UpdateAllCharacterInfo()
        {
            // Limpiar UI existente
            ClearCharacterInfo();

            // Crear UI para jugadores
            if (battleManager.PlayerParty != null)
            {
                foreach (var player in battleManager.PlayerParty)
                {
                    CreateCharacterInfoUI(player, playerInfoParent, playerInfoUI);
                }
            }

            // Crear UI para enemigos
            if (battleManager.EnemyParty != null)
            {
                foreach (var enemy in battleManager.EnemyParty)
                {
                    CreateCharacterInfoUI(enemy, enemyInfoParent, enemyInfoUI);
                }
            }
        }

        /// <summary>
        /// Crea un elemento de UI para un personaje
        /// </summary>
        private void CreateCharacterInfoUI(CharacterInstance character, Transform parent, List<GameObject> list)
        {
            if (characterInfoPrefab == null)
            {
                Debug.LogWarning("Character Info Prefab not assigned!");
                return;
            }

            GameObject infoObj = Instantiate(characterInfoPrefab, parent);

            // Actualizar texto (asumiendo que el prefab tiene estos componentes)
            TextMeshProUGUI nameText = infoObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI hpText = infoObj.transform.Find("HPText")?.GetComponent<TextMeshProUGUI>();
            Slider hpSlider = infoObj.transform.Find("HPSlider")?.GetComponent<Slider>();

            if (nameText != null)
                nameText.text = character.Template.CharacterName;

            if (hpText != null)
                hpText.text = $"HP: {character.CurrentHP}/{character.MaxHP}";

            if (hpSlider != null)
            {
                hpSlider.maxValue = character.MaxHP;
                hpSlider.value = character.CurrentHP;
            }

            list.Add(infoObj);
        }

        /// <summary>
        /// Actualiza la info de un personaje específico
        /// </summary>
        private void UpdateCharacterInfo(CharacterInstance character)
        {
            // Por simplicidad, actualizamos todo
            // En un juego real, actualizarías solo el elemento específico
            UpdateAllCharacterInfo();
        }

        /// <summary>
        /// Limpia toda la info de personajes
        /// </summary>
        private void ClearCharacterInfo()
        {
            foreach (var obj in playerInfoUI)
            {
                if (obj != null) Destroy(obj);
            }
            playerInfoUI.Clear();

            foreach (var obj in enemyInfoUI)
            {
                if (obj != null) Destroy(obj);
            }
            enemyInfoUI.Clear();
        }

        #endregion

        #region Button Handlers

        /// <summary>
        /// Cuando el jugador presiona Attack
        /// </summary>
        private void OnAttackButtonClicked()
        {
            Debug.Log("Attack button clicked");

            // Mostrar selección de objetivos enemigos
            ShowTargetSelection(true);
            CreateEnemyTargetButtons();
        }

        /// <summary>
        /// Cuando el jugador presiona Skill
        /// </summary>
        private void OnSkillButtonClicked()
        {
            Debug.Log("Skill button clicked");
            ShowMessage("Skills not yet implemented!");

            // TODO: Mostrar lista de habilidades disponibles
        }

        /// <summary>
        /// Cuando el jugador presiona Item
        /// </summary>
        private void OnItemButtonClicked()
        {
            Debug.Log("Item button clicked");
            ShowMessage("Items not yet implemented!");

            // TODO: Mostrar inventario
        }

        /// <summary>
        /// Cuando el jugador presiona Run
        /// </summary>
        private void OnRunButtonClicked()
        {
            Debug.Log("Run button clicked");
            battleManager.PlayerChooseRun();
        }

        /// <summary>
        /// Cuando el jugador selecciona un objetivo
        /// </summary>
        private void OnTargetSelected(CharacterInstance target)
        {
            Debug.Log($"Target selected: {target.Template.CharacterName}");

            ShowTargetSelection(false);
            battleManager.PlayerChooseAttack(target);
        }

        #endregion

        #region Target Selection

        /// <summary>
        /// Crea botones para seleccionar enemigos como objetivos
        /// </summary>
        private void CreateEnemyTargetButtons()
        {
            // Limpiar botones anteriores
            ClearEnemyTargetButtons();

            if (battleManager.EnemyParty == null || targetSelectionPanel == null)
                return;

            // Crear un botón por cada enemigo vivo
            foreach (var enemy in battleManager.EnemyParty)
            {
                if (enemy.IsDead) continue;

                GameObject buttonObj = new GameObject($"Target_{enemy.Template.CharacterName}");
                buttonObj.transform.SetParent(targetSelectionPanel.transform);

                Button button = buttonObj.AddComponent<Button>();

                // Agregar texto al botón
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(buttonObj.transform);
                TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
                text.text = enemy.Template.CharacterName;
                text.alignment = TextAlignmentOptions.Center;

                // Configurar onClick
                CharacterInstance target = enemy; // Capturar en closure
                button.onClick.AddListener(() => OnTargetSelected(target));

                enemyTargetButtons.Add(button);
            }
        }

        /// <summary>
        /// Limpia los botones de selección de objetivos
        /// </summary>
        private void ClearEnemyTargetButtons()
        {
            foreach (var button in enemyTargetButtons)
            {
                if (button != null) Destroy(button.gameObject);
            }
            enemyTargetButtons.Clear();
        }

        #endregion

        #region Panel Visibility

        /// <summary>
        /// Muestra u oculta el panel de acciones
        /// </summary>
        private void ShowActionPanel(bool show)
        {
            if (actionPanel != null)
                actionPanel.SetActive(show);
        }

        /// <summary>
        /// Muestra u oculta el panel de selección de objetivos
        /// </summary>
        private void ShowTargetSelection(bool show)
        {
            if (targetSelectionPanel != null)
                targetSelectionPanel.SetActive(show);

            if (!show)
                ClearEnemyTargetButtons();
        }

        /// <summary>
        /// Muestra u oculta el panel de resultados
        /// </summary>
        private void ShowResultsPanel(bool show)
        {
            if (resultsPanel != null)
                resultsPanel.SetActive(show);
        }

        /// <summary>
        /// Muestra un mensaje en el área de batalla
        /// </summary>
        private void ShowMessage(string message)
        {
            if (battleMessageText != null)
                battleMessageText.text = message;

            Debug.Log($"Battle Message: {message}");
        }

        #endregion
    }
}