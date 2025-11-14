using UnityEngine;
using RPGCorruption.Data;

namespace RPGCorruption.Map
{
    /// <summary>
    /// Representa un enemigo en el mapa.
    /// Cuando el jugador colisiona con él, inicia un encuentro de batalla.
    /// </summary>
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

            // Configurar collider como trigger
            triggerCollider.isTrigger = true;
            triggerCollider.radius = 0.5f;
        }

        private void Start()
        {
            // Buscar al jugador
            player = FindObjectOfType<PlayerController>();

            // Configurar visual
            if (enemyTemplate != null)
            {
                UpdateVisual();
            }
            else
            {
                Debug.LogWarning($"EnemyMapEntity '{gameObject.name}' doesn't have an EnemyData assigned!");
                CreatePlaceholderSprite();
            }

            // Ajustar sorting order para que aparezca sobre el suelo
            spriteRenderer.sortingOrder = 5;
        }

        private void Update()
        {
            if (wasDefeated || player == null) return;

            // Verificar detección del jugador
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            if (!isStationary && distanceToPlayer <= detectionRange)
            {
                if (!isAggro)
                {
                    OnAggroStarted();
                }

                // Mover hacia el jugador si no es estacionario
                MoveTowardsPlayer();
            }
            else if (isAggro && distanceToPlayer > detectionRange * 1.5f)
            {
                OnAggroEnded();
            }
        }

        /// <summary>
        /// Actualiza el sprite según el enemyTemplate
        /// </summary>
        private void UpdateVisual()
        {
            if (enemyTemplate.MapSprite != null)
            {
                spriteRenderer.sprite = enemyTemplate.MapSprite;
            }
            else if (enemyTemplate.BattleSprite != null)
            {
                spriteRenderer.sprite = enemyTemplate.BattleSprite;
            }
            else
            {
                CreatePlaceholderSprite();
            }

            spriteRenderer.color = normalColor;

            // Hacer más grande si es boss
            if (isBoss)
            {
                transform.localScale = Vector3.one * 1.5f;
            }
        }

        /// <summary>
        /// Crea un sprite placeholder si no hay asignado
        /// </summary>
        private void CreatePlaceholderSprite()
        {
            Texture2D texture = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];

            Color enemyColor = isBoss ? new Color(0.8f, 0f, 0.2f) : new Color(0.8f, 0.2f, 0.2f);

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = enemyColor;
            }

            texture.SetPixels(pixels);
            texture.Apply();

            spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
        }

        /// <summary>
        /// Mueve el enemigo hacia el jugador
        /// </summary>
        private void MoveTowardsPlayer()
        {
            Vector3 direction = (player.transform.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }

        /// <summary>
        /// Cuando el enemigo detecta al jugador
        /// </summary>
        private void OnAggroStarted()
        {
            isAggro = true;
            spriteRenderer.color = aggroColor;
            Debug.Log($"{enemyTemplate?.CharacterName ?? "Enemy"} detected player!");
        }

        /// <summary>
        /// Cuando el jugador sale del rango
        /// </summary>
        private void OnAggroEnded()
        {
            isAggro = false;
            spriteRenderer.color = normalColor;
        }

        /// <summary>
        /// Cuando el jugador colisiona con este enemigo
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (wasDefeated) return;

            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                InitiateBattle(playerController);
            }
        }

        /// <summary>
        /// Inicia una batalla con este enemigo
        /// </summary>
        private void InitiateBattle(PlayerController playerController)
        {
            if (enemyTemplate == null)
            {
                Debug.LogError("Cannot initiate battle: No EnemyData assigned!");
                return;
            }

            Debug.Log($"⚔️ Battle started with {enemyTemplate.CharacterName} (Level {enemyTemplate.Level})!");

            // TODO: Aquí se llamará al BattleManager cuando esté implementado
            // Por ahora, solo mostramos un mensaje y "derrotamos" al enemigo

            // Simular victoria temporal
            OnBattleWon();
        }

        /// <summary>
        /// Marca el enemigo como derrotado
        /// </summary>
        public void OnBattleWon()
        {
            wasDefeated = true;

            // Efecto visual de derrota
            StartCoroutine(DefeatAnimation());
        }

        /// <summary>
        /// Animación de derrota
        /// </summary>
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

            // Desactivar o destruir
            gameObject.SetActive(false);
            // Destroy(gameObject); // Descomentar si prefieres destruir
        }

        /// <summary>
        /// Reactiva el enemigo (para testing)
        /// </summary>
        [ContextMenu("Reset Enemy")]
        public void ResetEnemy()
        {
            wasDefeated = false;
            isAggro = false;
            gameObject.SetActive(true);
            transform.localScale = isBoss ? Vector3.one * 1.5f : Vector3.one;
            spriteRenderer.color = normalColor;
        }

        /// <summary>
        /// Dibuja el rango de detección en el editor
        /// </summary>
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

        /// <summary>
        /// Muestra info del enemigo en GUI
        /// </summary>
        private void OnGUI()
        {
            if (!showDebugInfo || enemyTemplate == null || wasDefeated) return;

            // Convertir posición del mundo a pantalla
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.8f);

            if (screenPos.z > 0) // Solo si está frente a la cámara
            {
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.fontSize = 12;
                style.alignment = TextAnchor.MiddleCenter;
                style.normal.textColor = isAggro ? Color.red : Color.white;

                string text = $"{enemyTemplate.CharacterName}\nLv.{enemyTemplate.Level}";
                if (isBoss) text = $"★ {text} ★";

                // Convertir Y (Unity usa esquina inferior izquierda, GUI usa superior izquierda)
                screenPos.y = Screen.height - screenPos.y;

                GUI.Label(new Rect(screenPos.x - 50, screenPos.y - 30, 100, 40), text, style);
            }
        }
    }
}