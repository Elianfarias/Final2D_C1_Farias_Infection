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

        public CharacterInstance NextTurn()
        {
            if (turnQueue.Count == 0)
                RefreshTurnQueue();

            while (turnQueue.Count > 0)
            {
                var next = turnQueue.Dequeue();

                if (next.IsAlive)
                {
                    currentTurn = next;
                    return currentTurn;
                }
            }

            currentTurn = null;
            return null;
        }

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
        }

        public bool IsPlayerTurn()
        {
            if (currentTurn == null) return false;

            return true;
        }

        public void Clear()
        {
            allCombatants?.Clear();
            turnQueue?.Clear();
            currentTurn = null;
        }
    }
}