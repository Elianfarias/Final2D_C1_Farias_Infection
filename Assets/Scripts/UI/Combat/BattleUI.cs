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
            if (attackButton != null)
                attackButton.onClick.AddListener(OnAttackButtonClicked);

            if (skillButton != null)
                skillButton.onClick.AddListener(OnSkillButtonClicked);

            if (itemButton != null)
                itemButton.onClick.AddListener(OnItemButtonClicked);

            if (runButton != null)
                runButton.onClick.AddListener(OnRunButtonClicked);

            if (battleManager != null)
            {
                battleManager.OnStateChanged += OnBattleStateChanged;
                battleManager.OnDamageDealt += OnDamageDealt;
                battleManager.OnCharacterDefeated += OnCharacterDefeated;
                battleManager.OnBattleWon += OnBattleWon;
                battleManager.OnBattleLost += OnBattleLost;
            }

            StartCoroutine(DelayedUpdate());

            ShowActionPanel(false);
            ShowTargetSelection(false);
            ShowResultsPanel(false);
        }

        private void OnDestroy()
        {
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

        private System.Collections.IEnumerator DelayedUpdate()
        {
            yield return null;

            UpdateAllCharacterInfo();
        }

        private void OnBattleStateChanged(BattleState newState)
        {
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

        private void OnDamageDealt(CharacterInstance target, int damage)
        {
            ShowMessage($"{target.Template.CharacterName} took {damage} damage!");
            UpdateCharacterInfo(target);
        }

        private void OnCharacterDefeated(CharacterInstance character)
        {
            ShowMessage($"{character.Template.CharacterName} was defeated!");
            UpdateCharacterInfo(character);
        }

        private void OnBattleWon(BattleRewards rewards)
        {
            ShowMessage("Victory!");
            ShowActionPanel(false);
            ShowResultsPanel(true);

            // TODO: Mostrar recompensas en el panel de resultados
        }

        private void OnBattleLost()
        {
            ShowMessage("Defeat...");
            ShowActionPanel(false);
            ShowResultsPanel(true);
        }

        public void UpdateAllCharacterInfo()
        {
            ClearCharacterInfo();

            if (battleManager.PlayerParty != null)
            {
                foreach (var player in battleManager.PlayerParty)
                {
                    CreateCharacterInfoUI(player, playerInfoParent, playerInfoUI);
                }
            }

            if (battleManager.EnemyParty != null)
            {
                foreach (var enemy in battleManager.EnemyParty)
                {
                    CreateCharacterInfoUI(enemy, enemyInfoParent, enemyInfoUI);
                }
            }
        }

        private void CreateCharacterInfoUI(CharacterInstance character, Transform parent, List<GameObject> list)
        {
            if (characterInfoPrefab == null)
                return;

            GameObject infoObj = Instantiate(characterInfoPrefab, parent);

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

        private void UpdateCharacterInfo(CharacterInstance character)
        {
            // Por simplicidad, actualizamos todo
            UpdateAllCharacterInfo();
        }

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

        private void OnAttackButtonClicked()
        {
            ShowTargetSelection(true);
            CreateEnemyTargetButtons();
        }

        private void OnSkillButtonClicked()
        {
            ShowMessage("Skills not yet implemented!");

            // TODO: Mostrar lista de habilidades disponibles
        }

        private void OnItemButtonClicked()
        {
            ShowMessage("Items not yet implemented!");

            // TODO: Mostrar inventario
        }

        private void OnRunButtonClicked()
        {
            battleManager.PlayerChooseRun();
        }

        private void OnTargetSelected(CharacterInstance target)
        {
            ShowTargetSelection(false);
            battleManager.PlayerChooseAttack(target);
        }

        #endregion

        #region Target Selection

        private void CreateEnemyTargetButtons()
        {
            ClearEnemyTargetButtons();

            if (battleManager.EnemyParty == null || targetSelectionPanel == null)
                return;

            foreach (var enemy in battleManager.EnemyParty)
            {
                if (enemy.IsDead) continue;

                GameObject buttonObj = new GameObject($"Target_{enemy.Template.CharacterName}");
                buttonObj.transform.SetParent(targetSelectionPanel.transform);

                Button button = buttonObj.AddComponent<Button>();

                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(buttonObj.transform);
                TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
                text.text = enemy.Template.CharacterName;
                text.alignment = TextAlignmentOptions.Center;

                CharacterInstance target = enemy;
                button.onClick.AddListener(() => OnTargetSelected(target));

                enemyTargetButtons.Add(button);
            }
        }

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

        private void ShowActionPanel(bool show)
        {
            if (actionPanel != null)
                actionPanel.SetActive(show);
        }

        private void ShowTargetSelection(bool show)
        {
            if (targetSelectionPanel != null)
                targetSelectionPanel.SetActive(show);

            if (!show)
                ClearEnemyTargetButtons();
        }

        private void ShowResultsPanel(bool show)
        {
            if (resultsPanel != null)
                resultsPanel.SetActive(show);
        }

        private void ShowMessage(string message)
        {
            if (battleMessageText != null)
                battleMessageText.text = message;
        }

        #endregion
    }
}