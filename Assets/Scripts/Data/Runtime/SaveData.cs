using UnityEngine;
using System;

namespace RPGCorruption.Data.Runtime
{
    /// <summary>
    /// Datos completos de una partida guardada.
    /// Envuelve PartyData, InventoryInstance, WorldState y metadata.
    /// Es completamente serializable a JSON para persistencia.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        [Header("Metadata")]
        [SerializeField] private string saveName = "SaveSlot";
        [SerializeField] private int slotIndex = 0;
        [SerializeField] private string saveDate;
        [SerializeField] private string gameVersion = "1.0.0";

        [Header("Game State")]
        [SerializeField] private PartyData party;
        [SerializeField] private InventoryInstance inventory;
        [SerializeField] private WorldState worldState;

        // Properties
        public string SaveName => saveName;
        public int SlotIndex => slotIndex;
        public string SaveDate => saveDate;
        public string GameVersion => gameVersion;

        public PartyData Party => party;
        public InventoryInstance Inventory => inventory;
        public WorldState WorldState => worldState;

        /// <summary>
        /// Constructor por defecto (para nueva partida)
        /// </summary>
        public SaveData()
        {
            saveName = "New Save";
            slotIndex = 0;
            saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            gameVersion = Application.version;

            party = new PartyData();
            inventory = new InventoryInstance();
            worldState = new WorldState();
        }

        /// <summary>
        /// Constructor con datos existentes
        /// </summary>
        public SaveData(int slotIndex, PartyData party, InventoryInstance inventory, WorldState worldState)
        {
            this.slotIndex = slotIndex;
            this.saveName = $"Save Slot {slotIndex + 1}";
            this.saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            this.gameVersion = Application.version;

            this.party = party;
            this.inventory = inventory;
            this.worldState = worldState;
        }

        /// <summary>
        /// Actualiza el timestamp del guardado
        /// </summary>
        public void UpdateTimestamp()
        {
            saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Establece el nombre del save
        /// </summary>
        public void SetSaveName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                saveName = name;
            }
        }

        /// <summary>
        /// Serializa a JSON
        /// </summary>
        public string ToJson(bool prettyPrint = false)
        {
            try
            {
                return JsonUtility.ToJson(this, prettyPrint);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error serializing SaveData to JSON: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Deserializa desde JSON
        /// </summary>
        public static SaveData FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("Cannot deserialize null or empty JSON!");
                return null;
            }

            try
            {
                return JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error deserializing SaveData from JSON: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Obtiene información resumida del save (para UI de load)
        /// </summary>
        public SaveSummary GetSummary()
        {
            return new SaveSummary
            {
                SlotIndex = slotIndex,
                SaveName = saveName,
                SaveDate = saveDate,
                PlayTime = party?.GetFormattedPlayTime() ?? "00:00:00",
                PartyLevel = party?.GetAverageLevel() ?? 1,
                CurrentChapter = worldState?.CurrentChapter ?? 1,
                Location = worldState?.CurrentSceneName ?? "Unknown"
            };
        }

        /// <summary>
        /// Valida la integridad del save
        /// </summary>
        public bool IsValid()
        {
            if (party == null)
            {
                Debug.LogError("SaveData: Party is null!");
                return false;
            }

            if (inventory == null)
            {
                Debug.LogError("SaveData: Inventory is null!");
                return false;
            }

            if (worldState == null)
            {
                Debug.LogError("SaveData: WorldState is null!");
                return false;
            }

            if (party.IsEmpty)
            {
                Debug.LogError("SaveData: Party has no members!");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Crea una copia profunda del SaveData (para backup)
        /// </summary>
        public SaveData CreateBackup()
        {
            string json = ToJson();
            return FromJson(json);
        }
    }

    /// <summary>
    /// Resumen de un save file para mostrar en UI
    /// </summary>
    [Serializable]
    public struct SaveSummary
    {
        public int SlotIndex;
        public string SaveName;
        public string SaveDate;
        public string PlayTime;
        public int PartyLevel;
        public int CurrentChapter;
        public string Location;
    }
}