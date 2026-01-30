using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class UIPanel
{
    public string panelName;
    public GameObject panelObject;
    public bool startActive = false;
    public bool pauseGameWhenActive = false;
    public KeyCode toggleKey = KeyCode.None;

    [System.NonSerialized]
    public bool isActive = false;
}

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public List<UIPanel> uiPanels = new List<UIPanel>();

    [Header("HUD Elements")]
    public Text levelText;
    public Text scoreText;
    public Text timeText;
    public Slider healthBar;
    public Text filterCountText;

    [Header("Menu Buttons")]
    public Button pauseButton;
    public Button resumeButton;
    public Button restartButton;
    public Button mainMenuButton;
    public Button nextLevelButton;
    public Button previousLevelButton;

    [Header("Settings UI")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle fullscreenToggle;
    public Dropdown qualityDropdown;

    [Header("Level Selection")]
    public Transform levelButtonContainer;
    public GameObject levelButtonPrefab;

    [Header("Transition Settings")]
    public float panelTransitionSpeed = 5f;
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // Singleton
    public static UIManager Instance { get; private set; }

    // Current state
    private string currentActivePanel = "";
    private Dictionary<string, UIPanel> panelDictionary = new Dictionary<string, UIPanel>();
    private float gameTime = 0f;
    private int currentScore = 0;

    // Events
    public System.Action<string> OnPanelChanged;
    public System.Action<int> OnScoreChanged;

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
        InitializeUI();
        SetupEventListeners();
    }

    void Update()
    {
        // Update game time
        if (GameManager.Instance != null && !GameManager.Instance.IsPaused)
        {
            gameTime += Time.deltaTime;
            UpdateTimeDisplay();
        }

        // Handle panel toggle keys
        HandlePanelToggleKeys();
    }

    void InitializeUI()
    {
        // Build panel dictionary
        panelDictionary.Clear();
        foreach (UIPanel panel in uiPanels)
        {
            panelDictionary[panel.panelName] = panel;

            // Set initial state
            if (panel.panelObject != null)
            {
                panel.panelObject.SetActive(panel.startActive);
                panel.isActive = panel.startActive;

                if (panel.startActive)
                {
                    currentActivePanel = panel.panelName;
                }
            }
        }

        // Initialize HUD
        UpdateHUD();

        // Initialize settings UI
        InitializeSettingsUI();

        // Generate level selection buttons
        GenerateLevelSelectionButtons();

        Debug.Log("UI system initialized");
    }

    void SetupEventListeners()
    {
        // Menu buttons
        if (pauseButton != null)
            pauseButton.onClick.AddListener(() => ShowPanel("PauseMenu"));

        if (resumeButton != null)
            resumeButton.onClick.AddListener(() => HidePanel("PauseMenu"));

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartLevel);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);

        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(LoadNextLevel);

        if (previousLevelButton != null)
            previousLevelButton.onClick.AddListener(LoadPreviousLevel);

        // Settings sliders
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);

        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.AddListener(SetQualityLevel);

        // Game events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGamePaused += OnGamePausedChanged;
            GameManager.Instance.OnLevelChanged += OnLevelChanged;
        }

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.OnLevelCompleted += OnLevelCompleted;
        }
    }

    #region Panel Management

    public void ShowPanel(string panelName)
    {
        if (!panelDictionary.ContainsKey(panelName)) return;

        UIPanel panel = panelDictionary[panelName];

        if (panel.panelObject != null && !panel.isActive)
        {
            // Hide current active panel
            if (!string.IsNullOrEmpty(currentActivePanel) && currentActivePanel != panelName)
            {
                HidePanel(currentActivePanel);
            }

            // Show new panel
            panel.panelObject.SetActive(true);
            panel.isActive = true;
            currentActivePanel = panelName;

            // Handle game pause
            if (panel.pauseGameWhenActive && GameManager.Instance != null)
            {
                GameManager.Instance.PauseGame();
            }

            OnPanelChanged?.Invoke(panelName);
            Debug.Log($"Showing panel: {panelName}");
        }
    }

    public void HidePanel(string panelName)
    {
        if (!panelDictionary.ContainsKey(panelName)) return;

        UIPanel panel = panelDictionary[panelName];

        if (panel.panelObject != null && panel.isActive)
        {
            panel.panelObject.SetActive(false);
            panel.isActive = false;

            if (currentActivePanel == panelName)
            {
                currentActivePanel = "";
            }

            // Handle game resume
            if (panel.pauseGameWhenActive && GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
            }

            OnPanelChanged?.Invoke("");
            Debug.Log($"Hiding panel: {panelName}");
        }
    }

    public void TogglePanel(string panelName)
    {
        if (!panelDictionary.ContainsKey(panelName)) return;

        UIPanel panel = panelDictionary[panelName];

        if (panel.isActive)
        {
            HidePanel(panelName);
        }
        else
        {
            ShowPanel(panelName);
        }
    }

    void HandlePanelToggleKeys()
    {
        foreach (UIPanel panel in uiPanels)
        {
            if (panel.toggleKey != KeyCode.None && Input.GetKeyDown(panel.toggleKey))
            {
                TogglePanel(panel.panelName);
            }
        }
    }

    #endregion

    #region HUD Management

    void UpdateHUD()
    {
        // Update level display
        if (levelText != null && GameManager.Instance != null)
        {
            levelText.text = $"Level {GameManager.Instance.CurrentLevel + 1}";
        }

        // Update score display
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }

        // Update filter count
        if (filterCountText != null && FilterSystemManager.Instance != null)
        {
            int activeFilters = FilterSystemManager.Instance.GetActiveFilters().Count;
            filterCountText.text = $"Filters: {activeFilters}";
        }
    }

    void UpdateTimeDisplay()
    {
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(gameTime / 60f);
            int seconds = Mathf.FloorToInt(gameTime % 60f);
            timeText.text = $"Time: {minutes:00}:{seconds:00}";
        }
    }

    public void SetScore(int score)
    {
        currentScore = score;
        OnScoreChanged?.Invoke(score);
        UpdateHUD();
    }

    public void AddScore(int points)
    {
        SetScore(currentScore + points);
    }

    public void SetHealth(float health, float maxHealth)
    {
        if (healthBar != null)
        {
            healthBar.value = health / maxHealth;
        }
    }

    #endregion

    #region Settings Management

    void InitializeSettingsUI()
    {
        if (SaveManager.Instance == null) return;

        // Initialize volume sliders
        if (masterVolumeSlider != null)
            masterVolumeSlider.value = SaveManager.Instance.GetMasterVolume();

        if (musicVolumeSlider != null)
            musicVolumeSlider.value = SaveManager.Instance.GetMusicVolume();

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = SaveManager.Instance.GetSFXVolume();

        // Initialize fullscreen toggle
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = SaveManager.Instance.GetFullscreen();

        // Initialize quality dropdown
        if (qualityDropdown != null)
            qualityDropdown.value = SaveManager.Instance.GetQualityLevel();
    }

    void SetMasterVolume(float volume)
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SetMasterVolume(volume);
        }
    }

    void SetMusicVolume(float volume)
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SetMusicVolume(volume);
        }
    }

    void SetSFXVolume(float volume)
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SetSFXVolume(volume);
        }
    }

    void SetFullscreen(bool fullscreen)
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SetFullscreen(fullscreen);
        }
    }

    void SetQualityLevel(int qualityLevel)
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SetQualityLevel(qualityLevel);
        }
    }

    #endregion

    #region Level Selection

    void GenerateLevelSelectionButtons()
    {
        if (levelButtonContainer == null || levelButtonPrefab == null) return;
        if (GameManager.Instance == null || SaveManager.Instance == null) return;

        // Clear existing buttons
        foreach (Transform child in levelButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // Create buttons for each level
        for (int i = 0; i < GameManager.Instance.TotalLevels; i++)
        {
            GameObject buttonObj = Instantiate(levelButtonPrefab, levelButtonContainer);
            Button button = buttonObj.GetComponent<Button>();
            Text buttonText = buttonObj.GetComponentInChildren<Text>();

            if (button != null && buttonText != null)
            {
                int levelIndex = i; // Capture for closure
                buttonText.text = $"Level {i + 1}";

                // Check if level is unlocked
                bool isUnlocked = SaveManager.Instance.IsLevelUnlocked(i);
                bool isCompleted = SaveManager.Instance.IsLevelCompleted(i);

                button.interactable = isUnlocked;

                // Set button color based on state
                ColorBlock colors = button.colors;
                if (isCompleted)
                {
                    colors.normalColor = Color.green;
                }
                else if (isUnlocked)
                {
                    colors.normalColor = Color.white;
                }
                else
                {
                    colors.normalColor = Color.gray;
                }
                button.colors = colors;

                // Add click listener
                button.onClick.AddListener(() => LoadLevel(levelIndex));
            }
        }
    }

    #endregion

    #region Game Flow

    void RestartLevel()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartLevel();
        }
    }

    void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Assuming main menu is scene 0
    }

    void LoadNextLevel()
    {
        if (GameManager.Instance != null && GameManager.Instance.HasNextLevel())
        {
            GameManager.Instance.LoadNextLevel();
        }
    }

    void LoadPreviousLevel()
    {
        if (GameManager.Instance != null && GameManager.Instance.HasPreviousLevel())
        {
            GameManager.Instance.LoadPreviousLevel();
        }
    }

    void LoadLevel(int levelIndex)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadLevel(levelIndex);
        }
    }

    #endregion

    #region Event Handlers

    void OnGamePausedChanged(bool isPaused)
    {
        // Update pause button state
        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(!isPaused);
        }
    }

    void OnLevelChanged(int levelIndex)
    {
        // Reset game time
        gameTime = 0f;

        // Update HUD
        UpdateHUD();

        // Show level start message
        ShowLevelStartMessage(levelIndex);
    }

    void OnLevelCompleted(int levelIndex)
    {
        // Show victory screen
        ShowPanel("VictoryScreen");

        // Update level selection buttons
        GenerateLevelSelectionButtons();
    }

    void ShowLevelStartMessage(int levelIndex)
    {
        // This could show a temporary message or animation
        Debug.Log($"Level {levelIndex + 1} started!");
    }

    #endregion

    #region Public Methods

    public void ShowGameOverScreen()
    {
        ShowPanel("GameOverScreen");
    }

    public void ShowVictoryScreen()
    {
        ShowPanel("VictoryScreen");
    }

    public bool IsPanelActive(string panelName)
    {
        return panelDictionary.ContainsKey(panelName) && panelDictionary[panelName].isActive;
    }

    public string GetCurrentActivePanel()
    {
        return currentActivePanel;
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    public float GetGameTime()
    {
        return gameTime;
    }

    #endregion

    #region Cleanup

    void OnDestroy()
    {
        // Remove event listeners
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGamePaused -= OnGamePausedChanged;
            GameManager.Instance.OnLevelChanged -= OnLevelChanged;
        }

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.OnLevelCompleted -= OnLevelCompleted;
        }
    }

    #endregion
}