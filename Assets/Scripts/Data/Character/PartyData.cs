using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RPGCorruption.Data.Runtime
{
    /// <summary>
    /// Datos del grupo del jugador.
    /// Gestiona hasta 3 personajes activos, oro del grupo y formación.
    /// </summary>
    [Serializable]
    public class PartyData
    {
        [Header("Miembros del Grupo")]
        [SerializeField] private List<CharacterInstance> members = new List<CharacterInstance>();

        [Header("Economía")]
        [SerializeField] private int gold = 0;

        [Header("Estadísticas")]
        [SerializeField] private int totalBattlesWon = 0;
        [SerializeField] private int totalEnemiesDefeated = 0;
        [SerializeField] private float totalPlayTime = 0f;

        // Properties
        public List<CharacterInstance> Members => members;
        public int MemberCount => members.Count;
        public int Gold => gold;
        public int TotalBattlesWon => totalBattlesWon;
        public int TotalEnemiesDefeated => totalEnemiesDefeated;
        public float TotalPlayTime => totalPlayTime;

        // Estado del grupo
        public bool IsFull => members.Count >= GameConfig.Instance.MaxPartySize;
        public bool IsEmpty => members.Count == 0;
        public bool AllMembersDead => members.Count > 0 && members.All(m => m.IsDead);
        public bool AnyMemberAlive => members.Any(m => m.IsAlive);

        /// <summary>
        /// Constructor por defecto
        /// </summary>
        public PartyData()
        {
            members = new List<CharacterInstance>();
            gold = GameConfig.Instance.StartingGold;
        }

        /// <summary>
        /// Constructor con miembros iniciales
        /// </summary>
        public PartyData(List<CharacterInstance> initialMembers, int startingGold = -1)
        {
            members = new List<CharacterInstance>(initialMembers);
            gold = startingGold >= 0 ? startingGold : GameConfig.Instance.StartingGold;
        }

        #region Member Management

        /// <summary>
        /// Agrega un miembro al grupo
        /// </summary>
        public bool AddMember(CharacterInstance character)
        {
            if (character == null)
            {
                Debug.LogWarning("Cannot add null character to party!");
                return false;
            }

            if (IsFull)
            {
                Debug.LogWarning("Party is full! Cannot add more members.");
                return false;
            }

            if (members.Contains(character))
            {
                Debug.LogWarning($"{character.Template.CharacterName} is already in the party!");
                return false;
            }

            members.Add(character);
            Debug.Log($"{character.Template.CharacterName} joined the party!");
            return true;
        }

        /// <summary>
        /// Remueve un miembro del grupo
        /// </summary>
        public bool RemoveMember(CharacterInstance character)
        {
            if (character == null) return false;

            bool removed = members.Remove(character);
            if (removed)
            {
                Debug.Log($"{character.Template.CharacterName} left the party.");
            }
            return removed;
        }

        /// <summary>
        /// Obtiene un miembro por índice
        /// </summary>
        public CharacterInstance GetMember(int index)
        {
            if (index < 0 || index >= members.Count) return null;
            return members[index];
        }

        /// <summary>
        /// Obtiene todos los miembros vivos
        /// </summary>
        public List<CharacterInstance> GetAliveMembers()
        {
            return members.Where(m => m.IsAlive).ToList();
        }

        /// <summary>
        /// Obtiene todos los miembros muertos
        /// </summary>
        public List<CharacterInstance> GetDeadMembers()
        {
            return members.Where(m => m.IsDead).ToList();
        }

        /// <summary>
        /// Intercambia la posición de dos miembros
        /// </summary>
        public bool SwapMembers(int indexA, int indexB)
        {
            if (indexA < 0 || indexA >= members.Count) return false;
            if (indexB < 0 || indexB >= members.Count) return false;
            if (indexA == indexB) return false;

            var temp = members[indexA];
            members[indexA] = members[indexB];
            members[indexB] = temp;

            return true;
        }

        #endregion

        #region Gold Management

        /// <summary>
        /// Añade oro al grupo
        /// </summary>
        public void AddGold(int amount)
        {
            if (amount <= 0) return;

            gold += amount;
            Debug.Log($"Gained {amount} gold! Total: {gold}");
        }

        /// <summary>
        /// Gasta oro del grupo
        /// </summary>
        public bool SpendGold(int amount)
        {
            if (amount <= 0) return false;

            if (gold < amount)
            {
                Debug.LogWarning($"Not enough gold! Need {amount}, have {gold}");
                return false;
            }

            gold -= amount;
            Debug.Log($"Spent {amount} gold. Remaining: {gold}");
            return true;
        }

        /// <summary>
        /// Verifica si el grupo tiene suficiente oro
        /// </summary>
        public bool HasGold(int amount)
        {
            return gold >= amount;
        }

        #endregion

        #region Group Actions

        /// <summary>
        /// Cura completamente a todo el grupo
        /// </summary>
        public void FullRestoreAll()
        {
            foreach (var member in members)
            {
                member.FullRestore();
            }

            Debug.Log("Party fully restored!");
        }

        /// <summary>
        /// Otorga experiencia a todos los miembros vivos
        /// </summary>
        public List<CharacterInstance> GainExperienceAll(int baseExp)
        {
            if (baseExp <= 0) return new List<CharacterInstance>();

            // Aplicar multiplicador de experiencia del GameConfig
            int actualExp = Mathf.RoundToInt(baseExp * GameConfig.Instance.ExpGainMultiplier);

            var leveledUpCharacters = new List<CharacterInstance>();

            foreach (var member in members)
            {
                if (member.IsAlive)
                {
                    bool leveledUp = member.GainExperience(actualExp);
                    if (leveledUp)
                    {
                        leveledUpCharacters.Add(member);
                    }
                }
            }

            return leveledUpCharacters;
        }

        /// <summary>
        /// Aplica infección pasiva a todo el grupo (por ejemplo, por armas corruptas)
        /// </summary>
        public void ApplyPassiveInfection()
        {
            // TODO: Implementar cuando tengamos el sistema de equipamiento completo
            // foreach (var member in members)
            // {
            //     if (member.EquippedWeapon?.IsCorrupted == true)
            //     {
            //         member.IncreaseInfection(member.EquippedWeapon.CorruptionPerTurn);
            //     }
            // }
        }

        /// <summary>
        /// Calcula el nivel promedio del grupo
        /// </summary>
        public int GetAverageLevel()
        {
            if (IsEmpty) return 1;
            return Mathf.RoundToInt((float)members.Average(m => m.Level));
        }

        /// <summary>
        /// Calcula el nivel de infección promedio del grupo
        /// </summary>
        public int GetAverageInfection()
        {
            if (IsEmpty) return 0;
            return Mathf.RoundToInt((float)members.Average(m => m.InfectionLevel));
        }

        /// <summary>
        /// Verifica si algún miembro está críticamente infectado
        /// </summary>
        public bool HasCriticalInfection()
        {
            return members.Any(m => m.IsInfectionCritical());
        }

        /// <summary>
        /// Verifica si algún miembro está completamente corrompido
        /// </summary>
        public bool HasFullyCorruptedMember()
        {
            return members.Any(m => m.IsFullyCorrupted());
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Registra una victoria en batalla
        /// </summary>
        public void RecordBattleVictory(int enemiesDefeated)
        {
            totalBattlesWon++;
            totalEnemiesDefeated += enemiesDefeated;
        }

        /// <summary>
        /// Actualiza el tiempo de juego
        /// </summary>
        public void UpdatePlayTime(float deltaTime)
        {
            totalPlayTime += deltaTime;
        }

        /// <summary>
        /// Obtiene el tiempo de juego formateado
        /// </summary>
        public string GetFormattedPlayTime()
        {
            int hours = Mathf.FloorToInt(totalPlayTime / 3600f);
            int minutes = Mathf.FloorToInt((totalPlayTime % 3600f) / 60f);
            int seconds = Mathf.FloorToInt(totalPlayTime % 60f);

            return $"{hours:00}:{minutes:00}:{seconds:00}";
        }

        #endregion

        #region Serialization Helper

        /// <summary>
        /// Inicializa las referencias de templates después de deserializar
        /// </summary>
        public void InitializeTemplates(Dictionary<string, CharacterData> characterDatabase)
        {
            foreach (var member in members)
            {
                if (characterDatabase.TryGetValue(member.CharacterDataId, out CharacterData template))
                {
                    member.InitializeTemplate(template);
                }
                else
                {
                    Debug.LogError($"Could not find CharacterData with ID: {member.CharacterDataId}");
                }
            }
        }

        #endregion
    }
}