using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public bool pauseOnStart = false;
    public KeyCode pauseKey = KeyCode.Escape;

    [Header("Level Management")]
    public string[] levelScenes;
    public int currentLevelIndex = 0;

    // Singleton
    public static GameManager Instance { get; private set; }

    // Game state
    private bool isPaused = false;
    private bool isGameActive = true;

    // System references
    private FilterSystemManager filterSystem;
    private PlayerController player;

    // Events
    public System.Action<bool> OnGamePaused;
    public System.Action<int> OnLevelChanged;
    public System.Action OnGameOver;

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Initialize game systems
        InitializeGame();

        // Pause on start if needed
        if (pauseOnStart)
        {
            PauseGame();
        }
    }

    void Update()
    {
        // Handle pause input
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }

        // Handle other global inputs
        HandleGlobalInput();
    }

    void InitializeGame()
    {
        // Find and cache system references
        filterSystem = FindObjectOfType<FilterSystemManager>();
        player = FindObjectOfType<PlayerController>();

        // Ensure filter system exists
        if (filterSystem == null)
        {
            GameObject filterSystemObj = new GameObject("FilterSystemManager");
            filterSystem = filterSystemObj.AddComponent<FilterSystemManager>();
        }

        Debug.Log("Game systems initialized");
    }

    #region Game State Management

    public void PauseGame()
    {
        if (!isPaused)
        {
            isPaused = true;
            Time.timeScale = 0f;
            OnGamePaused?.Invoke(true);
            Debug.Log("Game paused");
        }
    }

    public void ResumeGame()
    {
        if (isPaused)
        {
            isPaused = false;
            Time.timeScale = 1f;
            OnGamePaused?.Invoke(false);
            Debug.Log("Game resumed");
        }
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void GameOver()
    {
        isGameActive = false;
        OnGameOver?.Invoke();
        Debug.Log("Game Over");
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    #endregion

    #region Level Management

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levelScenes.Length)
        {
            currentLevelIndex = levelIndex;
            Time.timeScale = 1f;
            isPaused = false;
            SceneManager.LoadScene(levelScenes[levelIndex]);
            OnLevelChanged?.Invoke(levelIndex);
            Debug.Log($"Loading level {levelIndex}: {levelScenes[levelIndex]}");
        }
        else
        {
            Debug.LogWarning($"Invalid level index: {levelIndex}");
        }
    }

    public void LoadNextLevel()
    {
        LoadLevel(currentLevelIndex + 1);
    }

    public void LoadPreviousLevel()
    {
        LoadLevel(currentLevelIndex - 1);
    }

    public void ReloadCurrentLevel()
    {
        LoadLevel(currentLevelIndex);
    }

    public bool HasNextLevel()
    {
        return currentLevelIndex + 1 < levelScenes.Length;
    }

    public bool HasPreviousLevel()
    {
        return currentLevelIndex > 0;
    }

    #endregion

    #region Input Handling

    void HandleGlobalInput()
    {
        if (!isGameActive) return;

        // Debug keys (only in development builds)
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }

        if (Input.GetKeyDown(KeyCode.N) && HasNextLevel())
        {
            LoadNextLevel();
        }

        if (Input.GetKeyDown(KeyCode.P) && HasPreviousLevel())
        {
            LoadPreviousLevel();
        }
        #endif
    }

    #endregion

    #region Public Getters

    public bool IsPaused => isPaused;
    public bool IsGameActive => isGameActive;
    public FilterSystemManager FilterSystem => filterSystem;
    public PlayerController Player => player;
    public int CurrentLevel => currentLevelIndex;
    public int TotalLevels => levelScenes.Length;

    #endregion

    #region Scene Management Events

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-initialize systems when a new scene loads
        InitializeGame();
    }

    #endregion

    #region Debug

    void OnGUI()
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (!isGameActive) return;

        GUILayout.BeginArea(new Rect(10, 10, 200, 150));
        GUILayout.Label($"Level: {currentLevelIndex + 1}/{levelScenes.Length}");
        GUILayout.Label($"Paused: {isPaused}");
        GUILayout.Label($"Time Scale: {Time.timeScale}");

        if (filterSystem != null)
        {
            GUILayout.Label($"Active Filters: {filterSystem.GetActiveFilters().Count}");
        }

        GUILayout.Label("\nDebug Controls:");
        GUILayout.Label("R - Restart Level");
        GUILayout.Label("N - Next Level");
        GUILayout.Label("P - Previous Level");
        GUILayout.Label($"{pauseKey} - Pause/Resume");
        GUILayout.EndArea();
        #endif
    }

    #endregion
}