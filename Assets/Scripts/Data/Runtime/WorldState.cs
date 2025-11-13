using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGCorruption.Data.Runtime
{
    /// <summary>
    /// Estado del mundo del juego.
    /// Almacena progreso de historia, flags, enemigos derrotados, cofres abiertos, etc.
    /// </summary>
    [Serializable]
    public class WorldState
    {
        [Header("Progreso de Historia")]
        [SerializeField] private int currentChapter = 1;
        [SerializeField] private string currentQuestId;
        [SerializeField] private HashSet<string> completedQuests = new HashSet<string>();

        [Header("Templos Sagrados")]
        [SerializeField] private bool temple1Completed = false;
        [SerializeField] private bool temple2Completed = false;
        [SerializeField] private bool temple3Completed = false;

        [Header("Mundo")]
        [SerializeField] private Vector2 playerPosition;
        [SerializeField] private string currentSceneName;

        [Header("Entidades Derrotadas")]
        [SerializeField] private HashSet<string> defeatedEnemies = new HashSet<string>();
        [SerializeField] private HashSet<string> defeatedBosses = new HashSet<string>();

        [Header("Interacciones")]
        [SerializeField] private HashSet<string> openedChests = new HashSet<string>();
        [SerializeField] private HashSet<string> triggeredEvents = new HashSet<string>();
        [SerializeField] private HashSet<string> discoveredLocations = new HashSet<string>();

        [Header("NPCs y Diálogos")]
        [SerializeField] private HashSet<string> metNPCs = new HashSet<string>();
        [SerializeField] private Dictionary<string, int> npcRelationships = new Dictionary<string, int>();

        [Header("Decisiones Narrativas")]
        [SerializeField] private Dictionary<string, bool> storyChoices = new Dictionary<string, bool>();
        [SerializeField] private int corruptionChoices = 0; // Cuenta decisiones corruptas
        [SerializeField] private int purificationChoices = 0; // Cuenta decisiones puras

        [Header("Sistema de Infección Global")]
        [SerializeField] private int worldInfectionLevel = 0;
        [SerializeField] private HashSet<string> purifiedZones = new HashSet<string>();
        [SerializeField] private HashSet<string> corruptedZones = new HashSet<string>();

        // Properties
        public int CurrentChapter => currentChapter;
        public string CurrentQuestId => currentQuestId;
        public Vector2 PlayerPosition => playerPosition;
        public string CurrentSceneName => currentSceneName;
        public int WorldInfectionLevel => worldInfectionLevel;

        // Estado de templos
        public bool Temple1Completed => temple1Completed;
        public bool Temple2Completed => temple2Completed;
        public bool Temple3Completed => temple3Completed;
        public int CompletedTemplesCount =>
            (temple1Completed ? 1 : 0) +
            (temple2Completed ? 1 : 0) +
            (temple3Completed ? 1 : 0);
        public bool AllTemplesCompleted => temple1Completed && temple2Completed && temple3Completed;

        // Tendencia moral
        public int MoralTendency => purificationChoices - corruptionChoices;
        public bool TendsTowardsPurification => MoralTendency > 0;
        public bool TendsTowardsCorruption => MoralTendency < 0;

        /// <summary>
        /// Constructor por defecto
        /// </summary>
        public WorldState()
        {
            currentChapter = 1;
            playerPosition = Vector2.zero;
            currentSceneName = "WorldMap";
        }

        #region Story Progress

        /// <summary>
        /// Avanza al siguiente capítulo
        /// </summary>
        public void AdvanceChapter()
        {
            currentChapter++;
            Debug.Log($"Advanced to Chapter {currentChapter}");
        }

        /// <summary>
        /// Completa una misión
        /// </summary>
        public void CompleteQuest(string questId)
        {
            if (string.IsNullOrEmpty(questId)) return;

            completedQuests.Add(questId);
            Debug.Log($"Quest completed: {questId}");
        }

        /// <summary>
        /// Establece la misión actual
        /// </summary>
        public void SetCurrentQuest(string questId)
        {
            currentQuestId = questId;
        }

        /// <summary>
        /// Verifica si una misión está completada
        /// </summary>
        public bool IsQuestCompleted(string questId)
        {
            return completedQuests.Contains(questId);
        }

        /// <summary>
        /// Completa un templo
        /// </summary>
        public void CompleteTemple(int templeNumber)
        {
            switch (templeNumber)
            {
                case 1:
                    temple1Completed = true;
                    Debug.Log("Temple 1 completed!");
                    break;
                case 2:
                    temple2Completed = true;
                    Debug.Log("Temple 2 completed!");
                    break;
                case 3:
                    temple3Completed = true;
                    Debug.Log("Temple 3 completed!");
                    break;
            }
        }

        #endregion

        #region World Position

        /// <summary>
        /// Actualiza la posición del jugador
        /// </summary>
        public void UpdatePlayerPosition(Vector2 position)
        {
            playerPosition = position;
        }

        /// <summary>
        /// Actualiza la escena actual
        /// </summary>
        public void UpdateCurrentScene(string sceneName)
        {
            currentSceneName = sceneName;
        }

        #endregion

        #region Enemies and Bosses

        /// <summary>
        /// Marca un enemigo como derrotado
        /// </summary>
        public void DefeatEnemy(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId)) return;
            defeatedEnemies.Add(enemyId);
        }

        /// <summary>
        /// Marca un boss como derrotado
        /// </summary>
        public void DefeatBoss(string bossId)
        {
            if (string.IsNullOrEmpty(bossId)) return;
            defeatedBosses.Add(bossId);
            Debug.Log($"Boss defeated: {bossId}");
        }

        /// <summary>
        /// Verifica si un enemigo específico fue derrotado
        /// </summary>
        public bool IsEnemyDefeated(string enemyId)
        {
            return defeatedEnemies.Contains(enemyId);
        }

        /// <summary>
        /// Verifica si un boss específico fue derrotado
        /// </summary>
        public bool IsBossDefeated(string bossId)
        {
            return defeatedBosses.Contains(bossId);
        }

        #endregion

        #region World Interactions

        /// <summary>
        /// Marca un cofre como abierto
        /// </summary>
        public void OpenChest(string chestId)
        {
            if (string.IsNullOrEmpty(chestId)) return;
            openedChests.Add(chestId);
        }

        /// <summary>
        /// Verifica si un cofre fue abierto
        /// </summary>
        public bool IsChestOpened(string chestId)
        {
            return openedChests.Contains(chestId);
        }

        /// <summary>
        /// Marca un evento como activado
        /// </summary>
        public void TriggerEvent(string eventId)
        {
            if (string.IsNullOrEmpty(eventId)) return;
            triggeredEvents.Add(eventId);
        }

        /// <summary>
        /// Verifica si un evento fue activado
        /// </summary>
        public bool IsEventTriggered(string eventId)
        {
            return triggeredEvents.Contains(eventId);
        }

        /// <summary>
        /// Descubre una nueva localización
        /// </summary>
        public void DiscoverLocation(string locationId)
        {
            if (string.IsNullOrEmpty(locationId)) return;

            if (discoveredLocations.Add(locationId))
            {
                Debug.Log($"New location discovered: {locationId}");
            }
        }

        /// <summary>
        /// Verifica si una localización fue descubierta
        /// </summary>
        public bool IsLocationDiscovered(string locationId)
        {
            return discoveredLocations.Contains(locationId);
        }

        #endregion

        #region NPCs

        /// <summary>
        /// Marca un NPC como conocido
        /// </summary>
        public void MeetNPC(string npcId)
        {
            if (string.IsNullOrEmpty(npcId)) return;

            if (metNPCs.Add(npcId))
            {
                npcRelationships[npcId] = 0; // Relación neutral inicial
                Debug.Log($"Met NPC: {npcId}");
            }
        }

        /// <summary>
        /// Verifica si conoció a un NPC
        /// </summary>
        public bool HasMetNPC(string npcId)
        {
            return metNPCs.Contains(npcId);
        }

        /// <summary>
        /// Modifica la relación con un NPC
        /// </summary>
        public void ModifyNPCRelationship(string npcId, int change)
        {
            if (!npcRelationships.ContainsKey(npcId))
            {
                npcRelationships[npcId] = 0;
            }

            npcRelationships[npcId] += change;
            npcRelationships[npcId] = Mathf.Clamp(npcRelationships[npcId], -100, 100);
        }

        /// <summary>
        /// Obtiene el nivel de relación con un NPC
        /// </summary>
        public int GetNPCRelationship(string npcId)
        {
            return npcRelationships.TryGetValue(npcId, out int value) ? value : 0;
        }

        #endregion

        #region Story Choices

        /// <summary>
        /// Registra una decisión narrativa
        /// </summary>
        public void RecordChoice(string choiceId, bool choice)
        {
            if (string.IsNullOrEmpty(choiceId)) return;
            storyChoices[choiceId] = choice;
        }

        /// <summary>
        /// Obtiene una decisión previa
        /// </summary>
        public bool? GetChoice(string choiceId)
        {
            return storyChoices.TryGetValue(choiceId, out bool value) ? value : null;
        }

        /// <summary>
        /// Registra una decisión corrupta
        /// </summary>
        public void RecordCorruptionChoice()
        {
            corruptionChoices++;
        }

        /// <summary>
        /// Registra una decisión de purificación
        /// </summary>
        public void RecordPurificationChoice()
        {
            purificationChoices++;
        }

        #endregion

        #region World Infection

        /// <summary>
        /// Aumenta la infección global del mundo
        /// </summary>
        public void IncreaseWorldInfection(int amount)
        {
            worldInfectionLevel += amount;
            worldInfectionLevel = Mathf.Clamp(worldInfectionLevel, 0, 100);

            if (worldInfectionLevel >= 100)
            {
                Debug.LogWarning("World corruption has reached critical levels!");
            }
        }

        /// <summary>
        /// Reduce la infección global del mundo
        /// </summary>
        public void DecreaseWorldInfection(int amount)
        {
            worldInfectionLevel -= amount;
            worldInfectionLevel = Mathf.Max(0, worldInfectionLevel);
        }

        /// <summary>
        /// Purifica una zona
        /// </summary>
        public void PurifyZone(string zoneId)
        {
            if (string.IsNullOrEmpty(zoneId)) return;

            purifiedZones.Add(zoneId);
            corruptedZones.Remove(zoneId);
            Debug.Log($"Zone purified: {zoneId}");
        }

        /// <summary>
        /// Corrompe una zona
        /// </summary>
        public void CorruptZone(string zoneId)
        {
            if (string.IsNullOrEmpty(zoneId)) return;

            corruptedZones.Add(zoneId);
            purifiedZones.Remove(zoneId);
            Debug.Log($"Zone corrupted: {zoneId}");
        }

        /// <summary>
        /// Verifica si una zona está purificada
        /// </summary>
        public bool IsZonePurified(string zoneId)
        {
            return purifiedZones.Contains(zoneId);
        }

        /// <summary>
        /// Verifica si una zona está corrupta
        /// </summary>
        public bool IsZoneCorrupted(string zoneId)
        {
            return corruptedZones.Contains(zoneId);
        }

        #endregion
    }
}