using RPGCorruption.Data.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RPGCorruption.Core.Managers
{
    /// <summary>
    /// Gestor del sistema de guardado.
    /// Maneja guardado/carga de múltiples slots, backups y validación.
    /// Patrón Singleton.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        private static SaveManager instance;
        public static SaveManager Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogError("SaveManager instance not found! Make sure it exists in the scene.");
                }
                return instance;
            }
        }

        [Header("Configuration")]
        [SerializeField] private int maxSaveSlots = 3;
        [SerializeField] private bool useEncryption = false; // Para futura implementación
        [SerializeField] private bool createBackups = true;

        // Paths
        private string saveDirectory;
        private const string SAVE_FILE_PREFIX = "save_slot_";
        private const string SAVE_FILE_EXTENSION = ".json";
        private const string BACKUP_SUFFIX = "_backup";

        // Current loaded save
        private SaveData currentSave;
        private int currentSlotIndex = -1;

        // Properties
        public bool HasCurrentSave => currentSave != null;
        public int CurrentSlotIndex => currentSlotIndex;
        public SaveData CurrentSave => currentSave;

        private void Awake()
        {
            // Singleton pattern
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            // Establecer directorio de guardado
            saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");

            // Crear directorio si no existe
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
                Debug.Log($"Created save directory at: {saveDirectory}");
            }
        }

        #region Save Operations

        /// <summary>
        /// Guarda la partida actual en un slot específico
        /// </summary>
        public bool SaveGame(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxSaveSlots)
            {
                Debug.LogError($"Invalid slot index: {slotIndex}. Must be between 0 and {maxSaveSlots - 1}");
                return false;
            }

            if (currentSave == null)
            {
                Debug.LogError("No current save data to save!");
                return false;
            }

            // Validar antes de guardar
            if (!currentSave.IsValid())
            {
                Debug.LogError("Current save data is invalid!");
                return false;
            }

            // Actualizar metadata
            currentSave.UpdateTimestamp();
            currentSlotIndex = slotIndex;

            // Crear backup si existe un save previo
            if (createBackups && SaveExists(slotIndex))
            {
                CreateBackup(slotIndex);
            }

            // Serializar a JSON
            string json = currentSave.ToJson(prettyPrint: true);
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("Failed to serialize save data!");
                return false;
            }

            // Guardar archivo
            string filePath = GetSaveFilePath(slotIndex);
            try
            {
                File.WriteAllText(filePath, json);
                Debug.Log($"Game saved successfully to slot {slotIndex}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving game: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Guarda rápido (quick save)
        /// </summary>
        public bool QuickSave()
        {
            if (currentSlotIndex < 0)
            {
                Debug.LogWarning("No slot selected for quick save! Using slot 0.");
                currentSlotIndex = 0;
            }

            return SaveGame(currentSlotIndex);
        }

        /// <summary>
        /// Guarda en todos los slots (para testing)
        /// </summary>
        public void SaveToAllSlots()
        {
            for (int i = 0; i < maxSaveSlots; i++)
            {
                SaveGame(i);
            }
        }

        #endregion

        #region Load Operations

        /// <summary>
        /// Carga una partida desde un slot específico
        /// </summary>
        public bool LoadGame(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxSaveSlots)
            {
                Debug.LogError($"Invalid slot index: {slotIndex}");
                return false;
            }

            if (!SaveExists(slotIndex))
            {
                Debug.LogWarning($"No save found in slot {slotIndex}");
                return false;
            }

            string filePath = GetSaveFilePath(slotIndex);

            try
            {
                // Leer archivo
                string json = File.ReadAllText(filePath);

                // Deserializar
                SaveData loadedSave = SaveData.FromJson(json);

                if (loadedSave == null)
                {
                    Debug.LogError("Failed to deserialize save data!");
                    return false;
                }

                // Validar
                if (!loadedSave.IsValid())
                {
                    Debug.LogError("Loaded save data is invalid!");
                    return false;
                }

                // Establecer como save actual
                currentSave = loadedSave;
                currentSlotIndex = slotIndex;

                // Inicializar templates (necesario después de deserializar)
                InitializeSaveTemplates();

                Debug.Log($"Game loaded successfully from slot {slotIndex}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading game: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Carga rápida (quick load)
        /// </summary>
        public bool QuickLoad()
        {
            // Buscar el último save usado
            int lastSlot = GetMostRecentSaveSlot();

            if (lastSlot < 0)
            {
                Debug.LogWarning("No save files found!");
                return false;
            }

            return LoadGame(lastSlot);
        }

        #endregion

        #region New Game

        /// <summary>
        /// Crea una nueva partida
        /// </summary>
        public SaveData CreateNewGame()
        {
            currentSave = new SaveData();
            currentSlotIndex = -1; // Sin slot hasta que se guarde

            Debug.Log("New game created!");
            return currentSave;
        }

        /// <summary>
        /// Crea una nueva partida y la guarda inmediatamente
        /// </summary>
        public bool CreateNewGameInSlot(int slotIndex)
        {
            CreateNewGame();
            return SaveGame(slotIndex);
        }

        #endregion

        #region Delete Operations

        /// <summary>
        /// Elimina un save específico
        /// </summary>
        public bool DeleteSave(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxSaveSlots)
            {
                Debug.LogError($"Invalid slot index: {slotIndex}");
                return false;
            }

            string filePath = GetSaveFilePath(slotIndex);

            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"No save to delete in slot {slotIndex}");
                return false;
            }

            try
            {
                File.Delete(filePath);

                // También eliminar backup si existe
                string backupPath = GetBackupFilePath(slotIndex);
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }

                Debug.Log($"Save deleted from slot {slotIndex}");

                // Si era el save actual, limpiarlo
                if (currentSlotIndex == slotIndex)
                {
                    currentSave = null;
                    currentSlotIndex = -1;
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error deleting save: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Elimina todos los saves
        /// </summary>
        public void DeleteAllSaves()
        {
            for (int i = 0; i < maxSaveSlots; i++)
            {
                DeleteSave(i);
            }

            Debug.Log("All saves deleted!");
        }

        #endregion

        #region Backup Operations

        /// <summary>
        /// Crea un backup del save
        /// </summary>
        private void CreateBackup(int slotIndex)
        {
            string savePath = GetSaveFilePath(slotIndex);
            string backupPath = GetBackupFilePath(slotIndex);

            try
            {
                if (File.Exists(savePath))
                {
                    File.Copy(savePath, backupPath, overwrite: true);
                    Debug.Log($"Backup created for slot {slotIndex}");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to create backup: {e.Message}");
            }
        }

        /// <summary>
        /// Restaura desde backup
        /// </summary>
        public bool RestoreFromBackup(int slotIndex)
        {
            string savePath = GetSaveFilePath(slotIndex);
            string backupPath = GetBackupFilePath(slotIndex);

            if (!File.Exists(backupPath))
            {
                Debug.LogWarning($"No backup found for slot {slotIndex}");
                return false;
            }

            try
            {
                File.Copy(backupPath, savePath, overwrite: true);
                Debug.Log($"Restored from backup for slot {slotIndex}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to restore from backup: {e.Message}");
                return false;
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Verifica si existe un save en un slot
        /// </summary>
        public bool SaveExists(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxSaveSlots) return false;
            return File.Exists(GetSaveFilePath(slotIndex));
        }

        /// <summary>
        /// Obtiene todos los saves disponibles
        /// </summary>
        public List<SaveSummary> GetAllSaves()
        {
            List<SaveSummary> saves = new();

            for (int i = 0; i < maxSaveSlots; i++)
            {
                if (SaveExists(i))
                {
                    try
                    {
                        string json = File.ReadAllText(GetSaveFilePath(i));
                        SaveData save = SaveData.FromJson(json);

                        if (save != null && save.IsValid())
                        {
                            saves.Add(save.GetSummary());
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Could not load save summary for slot {i}: {e.Message}");
                    }
                }
            }

            return saves;
        }

        /// <summary>
        /// Obtiene el slot del save más reciente
        /// </summary>
        private int GetMostRecentSaveSlot()
        {
            int mostRecentSlot = -1;
            DateTime mostRecentTime = DateTime.MinValue;

            for (int i = 0; i < maxSaveSlots; i++)
            {
                if (SaveExists(i))
                {
                    try
                    {
                        FileInfo fileInfo = new(GetSaveFilePath(i));
                        if (fileInfo.LastWriteTime > mostRecentTime)
                        {
                            mostRecentTime = fileInfo.LastWriteTime;
                            mostRecentSlot = i;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Error checking save slot {i}: {e.Message}");
                    }
                }
            }

            return mostRecentSlot;
        }

        /// <summary>
        /// Obtiene la ruta del archivo de save
        /// </summary>
        private string GetSaveFilePath(int slotIndex)
        {
            return Path.Combine(saveDirectory, $"{SAVE_FILE_PREFIX}{slotIndex}{SAVE_FILE_EXTENSION}");
        }

        /// <summary>
        /// Obtiene la ruta del archivo de backup
        /// </summary>
        private string GetBackupFilePath(int slotIndex)
        {
            return Path.Combine(saveDirectory, $"{SAVE_FILE_PREFIX}{slotIndex}{BACKUP_SUFFIX}{SAVE_FILE_EXTENSION}");
        }

        /// <summary>
        /// Inicializa las referencias de templates después de cargar
        /// </summary>
        private void InitializeSaveTemplates()
        {
            if (currentSave == null) return;

            // TODO: Esto requiere acceso a las bases de datos de ScriptableObjects
            // Será implementado cuando tengamos el DatabaseManager
            Debug.Log("Template initialization will be implemented with DatabaseManager");
        }

        #endregion

        #region Debug

        /// <summary>
        /// Imprime información de debug sobre los saves
        /// </summary>
        [ContextMenu("Debug: Print Save Info")]
        public void DebugPrintSaveInfo()
        {
            Debug.Log($"=== SAVE SYSTEM INFO ===");
            Debug.Log($"Save Directory: {saveDirectory}");
            Debug.Log($"Max Slots: {maxSaveSlots}");
            Debug.Log($"Has Current Save: {HasCurrentSave}");
            Debug.Log($"Current Slot: {currentSlotIndex}");

            for (int i = 0; i < maxSaveSlots; i++)
            {
                Debug.Log($"Slot {i}: {(SaveExists(i) ? "EXISTS" : "EMPTY")}");
            }
        }

        #endregion
    }
}