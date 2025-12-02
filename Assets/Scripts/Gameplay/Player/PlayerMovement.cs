using UnityEngine;
using System.Collections.Generic;
using RPGCorruption.Data;

namespace RPGCorruption.Map
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private CharacterData characterTemplate;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float stoppingDistance = 0.1f;

        [Header("Visual Feedback")]
        [SerializeField] private bool showPath = true;
        [SerializeField] private Color pathColor = Color.cyan;

        private Pathfinding pathfinding;
        private List<Vector3> currentPath;
        private int currentWaypointIndex = 0;
        private bool isMoving = false;
        private Animator animator;

        public bool IsMoving => isMoving;
        public List<Vector3> CurrentPath => currentPath;

        private void Start()
        {
            pathfinding = Pathfinding.Instance;
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            HandleMouseInput();
            MoveAlongPath();
        }

        private void LateUpdate()
        {
            if (!showPath || currentPath == null || currentPath.Count == 0)
                return;

            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Debug.DrawLine(currentPath[i], currentPath[i + 1], pathColor);
            }
        }

        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorldPos.z = 0;

                SetDestination(mouseWorldPos);
            }
        }

        public void SetDestination(Vector3 targetPosition)
        {
            if (pathfinding == null)
                return;

            currentPath = pathfinding.FindPath(transform.position, targetPosition);

            if (currentPath != null && currentPath.Count > 0)
            {
                currentWaypointIndex = 0;
                isMoving = true;
            }
            else
                isMoving = false;
        }

        private void MoveAlongPath()
        {
            if (!isMoving || currentPath == null || currentPath.Count == 0)
            {
                animator.SetInteger("State", (int)AxisWalkEnum.Idle);
                return;
            }

            if (currentWaypointIndex >= currentPath.Count)
            {
                StopMoving();
                return;
            }

            Vector3 targetWaypoint = currentPath[currentWaypointIndex];
            Vector3 moveDirection = (targetWaypoint - transform.position).normalized;
            transform.position += moveSpeed * Time.deltaTime * moveDirection;
            bool moveHorizontal = Mathf.Abs(moveDirection.x) > Mathf.Abs(moveDirection.y);

            if (moveHorizontal)
            {
                if (moveDirection.x > 0)
                    animator.SetInteger("State", (int)AxisWalkEnum.Right);
                else
                    animator.SetInteger("State", (int)AxisWalkEnum.Left);
            }
            else
            {
                if (moveDirection.y > 0)
                    animator.SetInteger("State", (int)AxisWalkEnum.Top);
                else
                    animator.SetInteger("State", (int)AxisWalkEnum.Down);
            }

            float distanceToWaypoint = Vector3.Distance(transform.position, targetWaypoint);

            if (distanceToWaypoint <= stoppingDistance)
            {
                currentWaypointIndex++;

                if (currentWaypointIndex >= currentPath.Count)
                    StopMoving();
            }
        }

        public void StopMoving()
        {
            isMoving = false;
            currentPath = null;
            currentWaypointIndex = 0;
        }

        private void OnDrawGizmos()
        {
            if (!showPath || currentPath == null || currentPath.Count == 0)
                return;

            Gizmos.color = pathColor;

            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
            }

            foreach (Vector3 waypoint in currentPath)
            {
                Gizmos.DrawSphere(waypoint, 0.15f);
            }

            if (isMoving && currentWaypointIndex < currentPath.Count)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(currentPath[currentWaypointIndex], 0.25f);
            }
        }
    }
}