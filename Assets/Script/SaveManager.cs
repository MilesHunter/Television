using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int highestLevelUnlocked = 0;
    public List<int> completedLevels = new List<int>();
    public Dictionary<string, bool> achievements = new Dictionary<string, bool>();
    public float totalPlayTime = 0f;
    public string lastPlayedDate = "";

    // Settings
    public float masterVolume = 1f;
    public float musicVolume = 1f;
    public float sfxVolume = 1f;
    public bool fullscreen = true;
    public int qualityLevel = 2;
}

public class SaveManager : MonoBehaviour
{
    [Header("Save Settings")]
    public string saveFileName = "gamesave.json";
    public bool autoSave = true;
    public float autoSaveInterval = 30f; // seconds

    // Singleton
    public static SaveManager Instance { get; private set; }

    // Current save data
    private SaveData currentSaveData;
    private string savePath;
    private float lastSaveTime;

    // Events
    public System.Action<SaveData> OnDataLoaded;
    public System.Action<SaveData> OnDataSaved;
    public System.Action<int> OnLevelCompleted;

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSaveSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Load save data on start
        LoadGame();
    }

    void Update()
    {
        // Auto save
        if (autoSave && Time.time - lastSaveTime > autoSaveInterval)
        {
            SaveGame();
        }
    }

    void InitializeSaveSystem()
    {
        // Set save path
        savePath = System.IO.Path.Combine(Application.persistentDataPath, saveFileName);

        // Initialize save data
        currentSaveData = new SaveData();

        Debug.Log($"Save system initialized. Save path: {savePath}");
    }

    #region Save/Load Operations

    public void SaveGame()
    {
        try
        {
            // Update last played date
            currentSaveData.lastPlayedDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Convert to JSON
            string jsonData = JsonUtility.ToJson(currentSaveData, true);

            // Write to file
            System.IO.File.WriteAllText(savePath, jsonData);

            lastSaveTime = Time.time;
            OnDataSaved?.Invoke(currentSaveData);

            Debug.Log("Game saved successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }

    public void LoadGame()
    {
        try
        {
            if (System.IO.File.Exists(savePath))
            {
                // Read from file
                string jsonData = System.IO.File.ReadAllText(savePath);

                // Parse JSON
                currentSaveData = JsonUtility.FromJson<SaveData>(jsonData);

                OnDataLoaded?.Invoke(currentSaveData);
                Debug.Log("Game loaded successfully");
            }
            else
            {
                // Create new save data
                currentSaveData = new SaveData();
                SaveGame(); // Create initial save file
                Debug.Log("No save file found, created new save data");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}");
            // Fallback to new save data
            currentSaveData = new SaveData();
        }
    }

    public void DeleteSave()
    {
        try
        {
            if (System.IO.File.Exists(savePath))
            {
                System.IO.File.Delete(savePath);
                Debug.Log("Save file deleted");
            }

            // Reset to new save data
            currentSaveData = new SaveData();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to delete save: {e.Message}");
        }
    }

    #endregion

    #region Level Progress

    public void CompleteLevel(int levelIndex)
    {
        if (!currentSaveData.completedLevels.Contains(levelIndex))
        {
            currentSaveData.completedLevels.Add(levelIndex);
        }

        // Update highest level unlocked
        int nextLevel = levelIndex + 1;
        if (nextLevel > currentSaveData.highestLevelUnlocked)
        {
            currentSaveData.highestLevelUnlocked = nextLevel;
        }

        OnLevelCompleted?.Invoke(levelIndex);

        if (autoSave)
        {
            SaveGame();
        }

        Debug.Log($"Level {levelIndex} completed. Next level unlocked: {nextLevel}");
    }

    public bool IsLevelUnlocked(int levelIndex)
    {
        return levelIndex <= currentSaveData.highestLevelUnlocked;
    }

    public bool IsLevelCompleted(int levelIndex)
    {
        return currentSaveData.completedLevels.Contains(levelIndex);
    }

    public int GetHighestUnlockedLevel()
    {
        return currentSaveData.highestLevelUnlocked;
    }

    public List<int> GetCompletedLevels()
    {
        return new List<int>(currentSaveData.completedLevels);
    }

    #endregion

    #region Achievements

    public void UnlockAchievement(string achievementId)
    {
        if (!currentSaveData.achievements.ContainsKey(achievementId))
        {
            currentSaveData.achievements[achievementId] = true;

            if (autoSave)
            {
                SaveGame();
            }

            Debug.Log($"Achievement unlocked: {achievementId}");
        }
    }

    public bool IsAchievementUnlocked(string achievementId)
    {
        return currentSaveData.achievements.ContainsKey(achievementId) &&
               currentSaveData.achievements[achievementId];
    }

    #endregion

    #region Settings

    public void SetMasterVolume(float volume)
    {
        currentSaveData.masterVolume = Mathf.Clamp01(volume);
        AudioListener.volume = currentSaveData.masterVolume;
    }

    public void SetMusicVolume(float volume)
    {
        currentSaveData.musicVolume = Mathf.Clamp01(volume);
    }

    public void SetSFXVolume(float volume)
    {
        currentSaveData.sfxVolume = Mathf.Clamp01(volume);
    }

    public void SetFullscreen(bool fullscreen)
    {
        currentSaveData.fullscreen = fullscreen;
        Screen.fullScreen = fullscreen;
    }

    public void SetQualityLevel(int qualityLevel)
    {
        currentSaveData.qualityLevel = qualityLevel;
        QualitySettings.SetQualityLevel(qualityLevel);
    }

    #endregion

    #region Getters

    public SaveData GetSaveData()
    {
        return currentSaveData;
    }

    public float GetMasterVolume() => currentSaveData.masterVolume;
    public float GetMusicVolume() => currentSaveData.musicVolume;
    public float GetSFXVolume() => currentSaveData.sfxVolume;
    public bool GetFullscreen() => currentSaveData.fullscreen;
    public int GetQualityLevel() => currentSaveData.qualityLevel;

    #endregion

    #region Application Events

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && autoSave)
        {
            SaveGame();
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && autoSave)
        {
            SaveGame();
        }
    }

    void OnApplicationQuit()
    {
        SaveGame();
    }

    #endregion
}