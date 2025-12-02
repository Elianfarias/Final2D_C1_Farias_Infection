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
            allCombatants = new List<CharacterInstance>();
            allCombatants.AddRange(player);
            allCombatants.AddRange(enemies);

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
            return currentTurn != null;
        }

        public void Clear()
        {
            allCombatants?.Clear();
            turnQueue?.Clear();
            currentTurn = null;
        }
    }
}