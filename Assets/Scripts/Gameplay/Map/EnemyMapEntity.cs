using UnityEngine;
using RPGCorruption.Data;

namespace RPGCorruption.Map
{

    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class EnemyMapEntity : MonoBehaviour
    {
        [Header("Enemy Data")]
        [SerializeField] private EnemyData enemyTemplate;
        [SerializeField] private bool isBoss = false;

        [Header("Behavior")]
        [SerializeField] private bool isStationary = true;
        [SerializeField] private float detectionRange = 3f;
        [SerializeField] private float moveSpeed = 2f;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color aggroColor = Color.red;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;

        private CircleCollider2D triggerCollider;
        private PlayerController player;
        private bool isAggro = false;
        private bool wasDefeated = false;

        // Properties
        public EnemyData EnemyTemplate => enemyTemplate;
        public bool IsBoss => isBoss;
        public bool WasDefeated => wasDefeated;
        public string EnemyId => enemyTemplate != null ? $"{enemyTemplate.CharacterId}_{GetInstanceID()}" : "";

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            triggerCollider = GetComponent<CircleCollider2D>();

            triggerCollider.isTrigger = true;
            triggerCollider.radius = 0.5f;
        }

        private void Start()
        {
            player = Object.FindFirstObjectByType<PlayerController>();

            if (enemyTemplate != null)
                UpdateVisual();

            spriteRenderer.sortingOrder = 5;
        }

        private void Update()
        {
            if (wasDefeated || player == null) return;

            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            if (!isStationary && distanceToPlayer <= detectionRange)
            {
                if (!isAggro)
                {
                    OnAggroStarted();
                }

                MoveTowardsPlayer();
            }
            else if (isAggro && distanceToPlayer > detectionRange * 1.5f)
            {
                OnAggroEnded();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (wasDefeated) return;

            if (other.TryGetComponent<PlayerController>(out var playerController))
            {
                InitiateBattle(playerController);
            }
        }

        private void UpdateVisual()
        {
            if (enemyTemplate.MapSprite != null)
                spriteRenderer.sprite = enemyTemplate.MapSprite;
            else if (enemyTemplate.BattleSprite != null)
                spriteRenderer.sprite = enemyTemplate.BattleSprite;

            spriteRenderer.color = normalColor;

            if (isBoss)
                transform.localScale = Vector3.one * 1.5f;
        }

        private void MoveTowardsPlayer()
        {
            Vector3 direction = (player.transform.position - transform.position).normalized;
            transform.position += moveSpeed * Time.deltaTime * direction;
        }

        private void OnAggroStarted()
        {
            isAggro = true;
            spriteRenderer.color = aggroColor;
        }

        private void OnAggroEnded()
        {
            isAggro = false;
            spriteRenderer.color = normalColor;
        }

        private void InitiateBattle(PlayerController playerController)
        {
            if (enemyTemplate == null)
                return;

            Debug.Log($"⚔️ Battle started with {enemyTemplate.CharacterName} (Level {enemyTemplate.Level})!");

            // TODO: Aquí se llamará al BattleManager cuando esté implementado
            // Por ahora, solo mostramos un mensaje y "derrotamos" al enemigo

            // Simular victoria temporal
            OnBattleWon();
        }

        public void OnBattleWon()
        {
            wasDefeated = true;

            StartCoroutine(DefeatAnimation());
        }

        private System.Collections.IEnumerator DefeatAnimation()
        {
            // Fade out
            float duration = 0.5f;
            float elapsed = 0f;
            Color startColor = spriteRenderer.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f - (elapsed / duration);
                spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

                // Rotar y encoger
                transform.Rotate(0, 0, 360 * Time.deltaTime * 2);
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, elapsed / duration);

                yield return null;
            }

            gameObject.SetActive(false);
        }

        [ContextMenu("Reset Enemy")]
        public void ResetEnemy()
        {
            wasDefeated = false;
            isAggro = false;
            gameObject.SetActive(true);
            transform.localScale = isBoss ? Vector3.one * 1.5f : Vector3.one;
            spriteRenderer.color = normalColor;
        }

        private void OnDrawGizmosSelected()
        {
            if (!showDebugInfo) return;

            // Rango de detección
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Collider trigger
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }

        private void OnGUI()
        {
            if (!showDebugInfo || enemyTemplate == null || wasDefeated) return;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.8f);

            if (screenPos.z > 0)
            {
                GUIStyle style = new(GUI.skin.label)
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleCenter
                };

                style.normal.textColor = isAggro ? Color.red : Color.white;

                string text = $"{enemyTemplate.CharacterName}\nLv.{enemyTemplate.Level}";
                if (isBoss) text = $"★ {text} ★";

                screenPos.y = Screen.height - screenPos.y;

                GUI.Label(new Rect(screenPos.x - 50, screenPos.y - 30, 100, 40), text, style);
            }
        }
    }
}