using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using RPGCorruption.Data.Runtime;

namespace RPGCorruption.Combat
{
    public class BattleUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BattleManager battleManager;

        [Header("Action Buttons")]
        [SerializeField] private Button attackButton;
        [SerializeField] private Button skillButton;
        [SerializeField] private Button itemButton;
        [SerializeField] private Button runButton;
        [SerializeField] private Button targetButton;

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

        private List<GameObject> playerInfoUI = new();
        private List<GameObject> enemyInfoUI = new();
        private List<Button> enemyTargetButtons = new();

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

            Transform nameTextTransform = infoObj.transform.Find("NameText");
            TextMeshProUGUI nameText = nameTextTransform != null ? nameTextTransform.GetComponent<TextMeshProUGUI>() : null;

            Transform hpTextTransform = infoObj.transform.Find("HPText");
            TextMeshProUGUI hpText = hpTextTransform != null ? hpTextTransform.GetComponent<TextMeshProUGUI>() : null;

            Transform hpSliderTransform = infoObj.transform.Find("HPSlider");
            Slider hpSlider = hpSliderTransform != null ? hpSliderTransform.GetComponent<Slider>() : null;

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

            if (battleManager == null || battleManager.EnemyParty == null || targetSelectionPanel == null)
                return;

            if (targetButton == null)
                return;

            foreach (var enemy in battleManager.EnemyParty)
            {
                if (enemy.IsDead) continue;

                Button buttonInstance = Instantiate(targetButton, targetSelectionPanel.transform);

                TextMeshProUGUI textComponent = null;
                Transform textTransform = buttonInstance.transform.GetComponentInChildren<Transform>();
                if (textTransform != null)
                    textComponent = buttonInstance.GetComponentInChildren<TextMeshProUGUI>();

                if (textComponent != null)
                {
                    textComponent.text = enemy.Template.CharacterName;
                    textComponent.alignment = TextAlignmentOptions.Center;
                }

                CharacterInstance target = enemy;
                buttonInstance.onClick.AddListener(() => OnTargetSelected(target));

                enemyTargetButtons.Add(buttonInstance);
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