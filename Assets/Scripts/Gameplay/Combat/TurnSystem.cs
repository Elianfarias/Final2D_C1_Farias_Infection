using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RPGCorruption.Data.Runtime;

namespace RPGCorruption.Combat
{
    /// <summary>
    /// Sistema de turnos basado en velocidad.
    /// Los personajes con mayor Speed actúan primero.
    /// </summary>
    public class TurnSystem
    {
        private List<CharacterInstance> allCombatants;
        private Queue<CharacterInstance> turnQueue;
        private CharacterInstance currentTurn;

        public CharacterInstance CurrentTurn => currentTurn;
        public bool HasNextTurn => turnQueue != null && turnQueue.Count > 0;

        /// <summary>
        /// Inicializa el sistema de turnos con todos los combatientes
        /// </summary>
        public void Initialize(List<CharacterInstance> player, List<CharacterInstance> enemies)
        {
            // Combinar jugadores y enemigos
            allCombatants = new List<CharacterInstance>();
            allCombatants.AddRange(player);
            allCombatants.AddRange(enemies);

            // Ordenar por velocidad (de mayor a menor)
            var sortedBySpeed = allCombatants
                .Where(c => c.IsAlive)
                .OrderByDescending(c => c.Speed)
                .ToList();

            // Crear cola de turnos
            turnQueue = new Queue<CharacterInstance>();
            foreach (var combatant in sortedBySpeed)
            {
                turnQueue.Enqueue(combatant);
            }

            Debug.Log($"Turn order initialized. {turnQueue.Count} combatants ready.");
        }

        /// <summary>
        /// Avanza al siguiente turno
        /// </summary>
        public CharacterInstance NextTurn()
        {
            // Si no hay más turnos, reiniciar el ciclo
            if (turnQueue.Count == 0)
            {
                RefreshTurnQueue();
            }

            // Obtener siguiente combatiente vivo
            while (turnQueue.Count > 0)
            {
                var next = turnQueue.Dequeue();

                if (next.IsAlive)
                {
                    currentTurn = next;
                    Debug.Log($"Turn: {currentTurn.Template.CharacterName} (HP: {currentTurn.CurrentHP}/{currentTurn.MaxHP})");
                    return currentTurn;
                }
            }

            // No hay más combatientes vivos
            currentTurn = null;
            return null;
        }

        /// <summary>
        /// Refresca la cola de turnos (nuevo ciclo)
        /// </summary>
        private void RefreshTurnQueue()
        {
            var sortedBySpeed = allCombatants
                .Where(c => c.IsAlive)
                .OrderByDescending(c => c.Speed)
                .ToList();

            turnQueue = new Queue<CharacterInstance>();
            foreach (var combatant in sortedBySpeed)
            {
                turnQueue.Enqueue(combatant);
            }

            Debug.Log("Turn queue refreshed for new cycle.");
        }

        /// <summary>
        /// Verifica si es el turno de un jugador
        /// </summary>
        public bool IsPlayerTurn()
        {
            if (currentTurn == null) return false;

            // Los jugadores no son enemigos
            // Podríamos agregar una property IsPlayer en CharacterInstance
            // Por ahora, asumimos que si no está en la lista de enemigos, es jugador
            return true; // Simplificado
        }

        /// <summary>
        /// Limpia el sistema de turnos
        /// </summary>
        public void Clear()
        {
            allCombatants?.Clear();
            turnQueue?.Clear();
            currentTurn = null;
        }
    }
}