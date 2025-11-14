using UnityEngine;
using System.Collections;

namespace RPGCorruption.Map
{
    /// <summary>
    /// Controla el movimiento del jugador en el mapa con click-to-move.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private bool snapToGrid = true;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject targetIndicatorPrefab;
        [SerializeField] private Color hoverColor = Color.yellow;

        private Vector3 targetPosition;
        private bool isMoving = false;
        private GameObject targetIndicator;
        private SpriteRenderer spriteRenderer;

        // Estado
        public bool IsMoving => isMoving;
        public Vector3 TargetPosition => targetPosition;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            targetPosition = transform.position;

            // Crear indicador de destino si no se asignó prefab
            if (targetIndicatorPrefab == null)
            {
                CreateDefaultIndicator();
            }
        }

        private void Update()
        {
            HandleInput();
            HandleMovement();
        }

        /// <summary>
        /// Maneja el input del mouse
        /// </summary>
        private void HandleInput()
        {
            // Click izquierdo para mover
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorldPos.z = 0;

                SetTargetPosition(mouseWorldPos);
            }

            // Mostrar feedback al pasar el mouse
            if (Input.GetMouseButton(0) == false)
            {
                ShowHoverFeedback();
            }
        }

        /// <summary>
        /// Establece la posición objetivo
        /// </summary>
        public void SetTargetPosition(Vector3 position)
        {
            if (snapToGrid && TileGrid.Instance != null)
            {
                targetPosition = TileGrid.Instance.SnapToGrid(position);
            }
            else
            {
                targetPosition = position;
                targetPosition.z = 0;
            }

            isMoving = true;

            // Actualizar indicador visual
            UpdateTargetIndicator();
        }

        /// <summary>
        /// Maneja el movimiento hacia el objetivo
        /// </summary>
        private void HandleMovement()
        {
            if (!isMoving) return;

            // Mover hacia el objetivo
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            // Verificar si llegó al destino
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;

                // Ocultar indicador
                if (targetIndicator != null)
                {
                    targetIndicator.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Muestra feedback visual al pasar el mouse
        /// </summary>
        private void ShowHoverFeedback()
        {
            // TODO: Implementar highlight de tiles al pasar el mouse
            // Por ahora solo cambiamos el cursor
        }

        /// <summary>
        /// Actualiza el indicador de destino
        /// </summary>
        private void UpdateTargetIndicator()
        {
            if (targetIndicator == null) return;

            targetIndicator.transform.position = targetPosition;
            targetIndicator.SetActive(true);
        }

        /// <summary>
        /// Crea un indicador de destino por defecto
        /// </summary>
        private void CreateDefaultIndicator()
        {
            targetIndicator = new GameObject("TargetIndicator");
            SpriteRenderer sr = targetIndicator.AddComponent<SpriteRenderer>();

            // Crear sprite circular simple
            Texture2D texture = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];

            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float dx = x - 16;
                    float dy = y - 16;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);

                    if (distance < 12 && distance > 10)
                    {
                        pixels[y * 32 + x] = hoverColor;
                    }
                    else
                    {
                        pixels[y * 32 + x] = Color.clear;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            sr.sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
            sr.sortingOrder = -1;

            targetIndicator.SetActive(false);
        }

        /// <summary>
        /// Detiene el movimiento
        /// </summary>
        public void Stop()
        {
            isMoving = false;
            targetPosition = transform.position;

            if (targetIndicator != null)
            {
                targetIndicator.SetActive(false);
            }
        }

        /// <summary>
        /// Teleporta el jugador a una posición
        /// </summary>
        public void TeleportTo(Vector3 position)
        {
            Stop();

            if (snapToGrid && TileGrid.Instance != null)
            {
                transform.position = TileGrid.Instance.SnapToGrid(position);
            }
            else
            {
                transform.position = position;
                transform.position = new Vector3(transform.position.x, transform.position.y, 0);
            }

            targetPosition = transform.position;
        }

        /// <summary>
        /// Dibuja debug info
        /// </summary>
        private void OnDrawGizmos()
        {
            if (isMoving)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, targetPosition);
                Gizmos.DrawWireSphere(targetPosition, 0.3f);
            }
        }
    }
}