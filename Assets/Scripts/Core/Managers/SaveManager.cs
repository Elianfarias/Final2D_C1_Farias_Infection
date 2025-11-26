using RPGCorruption.Data.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RPGCorruption.Core.Managers
{
    public class SaveManager : MonoBehaviour
    {
        private static SaveManager instance;
        public static SaveManager Instance
        {
            get
            {
                return instance;
            }
        }

        [Header("Configuration")]
        [SerializeField] private int maxSaveSlots = 3;
        [SerializeField] private bool createBackups = true;

        private string saveDirectory;
        private const string SAVE_FILE_PREFIX = "save_slot_";
        private const string SAVE_FILE_EXTENSION = ".json";
        private const string BACKUP_SUFFIX = "_backup";

        private SaveData currentSave;
        private int currentSlotIndex = -1;

        public bool HasCurrentSave => currentSave != null;
        public int CurrentSlotIndex => currentSlotIndex;
        public SaveData CurrentSave => currentSave;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");

            if (!Directory.Exists(saveDirectory))
                Directory.CreateDirectory(saveDirectory);
        }

        #region Save Operations

        public bool SaveGame(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxSaveSlots)
                return false;

            if (currentSave == null)
                return false;

            if (!currentSave.IsValid())
                return false;

            // Actualizar metadata
            currentSave.UpdateTimestamp();
            currentSlotIndex = slotIndex;

            if (createBackups && SaveExists(slotIndex))
                CreateBackup(slotIndex);

            string json = currentSave.ToJson(prettyPrint: true);
            if (string.IsNullOrEmpty(json))
                return false;

            string filePath = GetSaveFilePath(slotIndex);

            try
            {
                File.WriteAllText(filePath, json);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading game: {e.Message}");
                return false;
            }
        }

        public bool QuickSave()
        {
            if (currentSlotIndex < 0)
                currentSlotIndex = 0;

            return SaveGame(currentSlotIndex);
        }

        public void SaveToAllSlots()
        {
            for (int i = 0; i < maxSaveSlots; i++)
            {
                SaveGame(i);
            }
        }

        #endregion

        #region Load Operations

        public bool LoadGame(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxSaveSlots)
                return false;

            if (!SaveExists(slotIndex))
                return false;

            string filePath = GetSaveFilePath(slotIndex);

            try
            {
                string json = File.ReadAllText(filePath);

                SaveData loadedSave = SaveData.FromJson(json);

                if (loadedSave == null)
                    return false;

                if (!loadedSave.IsValid())
                    return false;

                currentSave = loadedSave;
                currentSlotIndex = slotIndex;

                InitializeSaveTemplates();

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading game: {e.Message}");
                return false;
            }
        }

        public bool QuickLoad()
        {
            int lastSlot = GetMostRecentSaveSlot();

            if (lastSlot < 0)
                return false;

            return LoadGame(lastSlot);
        }

        #endregion

        #region New Game

        public SaveData CreateNewGame()
        {
            currentSave = new SaveData();
            currentSlotIndex = -1;

            return currentSave;
        }

        public bool CreateNewGameInSlot(int slotIndex)
        {
            CreateNewGame();
            return SaveGame(slotIndex);
        }

        #endregion

        #region Delete Operations

        public bool DeleteSave(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxSaveSlots)
                return false;

            string filePath = GetSaveFilePath(slotIndex);

            if (!File.Exists(filePath))
                return false;

            try
            {
                File.Delete(filePath);

                string backupPath = GetBackupFilePath(slotIndex);
                if (File.Exists(backupPath))
                    File.Delete(backupPath);

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

        public void DeleteAllSaves()
        {
            for (int i = 0; i < maxSaveSlots; i++)
            {
                DeleteSave(i);
            }
        }

        #endregion

        #region Backup Operations

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

        public bool SaveExists(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxSaveSlots) return false;
            return File.Exists(GetSaveFilePath(slotIndex));
        }

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
                            saves.Add(save.GetSummary());
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Could not load save summary for slot {i}: {e.Message}");
                    }
                }
            }

            return saves;
        }

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

        private string GetSaveFilePath(int slotIndex)
        {
            return Path.Combine(saveDirectory, $"{SAVE_FILE_PREFIX}{slotIndex}{SAVE_FILE_EXTENSION}");
        }

        private string GetBackupFilePath(int slotIndex)
        {
            return Path.Combine(saveDirectory, $"{SAVE_FILE_PREFIX}{slotIndex}{BACKUP_SUFFIX}{SAVE_FILE_EXTENSION}");
        }

        private void InitializeSaveTemplates()
        {
            if (currentSave == null) return;

            // TODO: Esto requiere acceso a las bases de datos de ScriptableObjects
            // Será implementado cuando tengamos el DatabaseManager
            Debug.Log("Template initialization will be implemented with DatabaseManager");
        }

        #endregion

        #region Debug

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