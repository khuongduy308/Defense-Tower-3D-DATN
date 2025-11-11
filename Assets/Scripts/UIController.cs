using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;

// using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text resourcesText;
    [SerializeField] private TMP_Text warningText;

    [SerializeField] private GameObject towerPanel;
    [SerializeField] private GameObject towerCardPrefab;
    [SerializeField] private Transform towerCardContainer;

    [SerializeField] private TowerData[] towers;
    private List<GameObject> activeCards = new List<GameObject>();

    private Platform _currentPlatform;

    [SerializeField] private Button speed1Button;
    [SerializeField] private Button speed2Button;
    [SerializeField] private Button speed3Button;

    [SerializeField] private Color normalButtonColor = Color.white;
    [SerializeField] private Color selectedButtonColor = Color.green;
    [SerializeField] private Color normalTextColor = Color.black;
    [SerializeField] private Color selectedTextColor = Color.white;

    [SerializeField] private GameObject pausePanel;
    private bool _isGamePaused = false;

    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private GameObject missionCompletePanel;

    [Header("Upgrade Panel")]
    [SerializeField] private GameObject upgradePanel; // Kéo Panel Nâng Cấp vào đây
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button sellButton;
    // [SerializeField] private TMP_Text upgradeCostText;
    // [SerializeField] private TMP_Text sellValueText;
    [SerializeField] private float sellReturnPercent = 0.7f; // Bán được 70% giá
    private BaseTower _currentSelectedTower;

    private void OnEnable()
    {
        Spawner.OnWaveChanged += UpdateWaveText;
        GameManager.OnLivesChanged += UpdateLivesText;
        GameManager.OnResourcesChanged += UpdateResourcesText;
        Platform.OnEmptyPlatformClicked += HandleEmptyPlatformClicked; // Mới
        Platform.OnTowerClicked += HandleTowerClicked; // Mới
        TowerCard.OnTowerSelected += HandleTowerSelected;
        SceneManager.sceneLoaded += OnSceneLoaded;
        Spawner.OnMissionComplete += ShowMissionComplete;
    }

    private void OnDisable()
    {
        Spawner.OnWaveChanged -= UpdateWaveText;
        GameManager.OnLivesChanged -= UpdateLivesText;
        GameManager.OnResourcesChanged -= UpdateResourcesText;
        Platform.OnEmptyPlatformClicked -= HandleEmptyPlatformClicked;
        Platform.OnTowerClicked -= HandleTowerClicked;
        TowerCard.OnTowerSelected -= HandleTowerSelected;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Spawner.OnMissionComplete -= ShowMissionComplete;
    }

    private void Start()
    {
        speed1Button.onClick.AddListener(() => SetGameSpeed(0.5f));
        speed2Button.onClick.AddListener(() => SetGameSpeed(1f));
        speed3Button.onClick.AddListener(() => SetGameSpeed(2f));

        upgradeButton.onClick.AddListener(UpgradeSelectedTower);
        sellButton.onClick.AddListener(SellSelectedTower);

        HighlightSelectedSpeedButton(GameManager.Instance.GameSpeed);
    }

    private void UpdateWaveText(int currentWave)
    {
        waveText.text = $"Wave: {currentWave + 1}";
    }

    private void UpdateLivesText(int currentLives)
    {
        livesText.text = $"Lives: {currentLives}";

        if(currentLives <= 0)
        {
            ShowGameOver();
        }
    }

    private void UpdateResourcesText(int currentResources)
    {
        resourcesText.text = $"Resources: {currentResources}";
    }

    private void HandleEmptyPlatformClicked(Platform platform)
    {
        if (towerPanel.activeSelf && _currentPlatform == platform)
        {
            HideTowerPanel(); // Nếu đúng, chỉ cần đóng nó lại
            return;
        }

        HideTowerPanel();
        HideUpgradePanel();

        _currentPlatform = platform;
        ShowTowerPanel();
    }

    public void ShowTowerPanel()
    {
        towerPanel.SetActive(true);
        // Platform.IsModalPanelOpen = true;
        GameManager.Instance.setTimeScale(0f);
        PopulateTowerCards();
    }

    public void HideTowerPanel()
    {
        if (!towerPanel.activeSelf) return;
        towerPanel.SetActive(false);
        Platform.IsModalPanelOpen = false;
        GameManager.Instance.setTimeScale(GameManager.Instance.GameSpeed);
    }

    private void HandleTowerSelected(TowerData towerData)
    {
        if (_currentPlatform.transform.childCount > 0)
        {
            HideTowerPanel();
            StartCoroutine(ShowWarningMessage("This platform already has a tower!"));
            return;
        }
        
        if (GameManager.Instance.Resources >= towerData.cost)
        {
            GameManager.Instance.SpendResources(towerData.cost);
            _currentPlatform.PlaceTower(towerData);

        } else
        {
            StartCoroutine(ShowWarningMessage("No Resource Enough!"));
        }
        
        HideTowerPanel();
    }

    private void PopulateTowerCards()
    {

        foreach (var card in activeCards)
        {
            Destroy(card);
        }
        activeCards.Clear();

        foreach (var data in towers)
        {
            GameObject cardGameObject = Instantiate(towerCardPrefab, towerCardContainer);
            TowerCard card = cardGameObject.GetComponent<TowerCard>();
            card.Initialize(data);
            activeCards.Add(cardGameObject);
        }

    }

    private IEnumerator ShowWarningMessage(string message)
    {
        if (warningText == null)
        {
            Debug.LogError("LỖI: Biến warningText CHƯA ĐƯỢC GÁN trong Inspector của UIController!");
            yield break; // Dừng Coroutine ngay lập tức
        }
    
        warningText.text = message;
        warningText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(2f);
        warningText.gameObject.SetActive(false);
    }

    private void SetGameSpeed(float timeScale)
    {
        HighlightSelectedSpeedButton(timeScale);
        GameManager.Instance.setTimeScale(timeScale);
    }

    private void UpdateButtonVisual(Button button, bool isSelected)
    {
        // ColorBlock colors = button.colors;
        // if (isSelected)
        // {
        //     colors.normalColor = selectedButtonColor;
        //     colors.highlightedColor = selectedButtonColor;
        //     button.GetComponentInChildren<TMP_Text>().color = selectedTextColor;
        // }
        // else
        // {
        //     colors.normalColor = normalButtonColor;
        //     colors.highlightedColor = normalButtonColor;
        //     button.GetComponentInChildren<TMP_Text>().color = normalTextColor;
        // }
        // button.colors = colors;

        button.image.color = isSelected ? selectedButtonColor : normalButtonColor;

        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.color = isSelected ? selectedTextColor : normalTextColor;
        }
    }

    private void HighlightSelectedSpeedButton(float selectedSpeed)
    {
        UpdateButtonVisual(speed1Button, selectedSpeed == 0.5f);
        UpdateButtonVisual(speed2Button, selectedSpeed == 1f);
        UpdateButtonVisual(speed3Button, selectedSpeed == 2f);
    }

    public void TogglePause()
    {
        if (towerPanel.activeSelf)
        {
            return;
        }

        if (_isGamePaused)
        {
            pausePanel.SetActive(false);
            _isGamePaused = false;
            Platform.IsModalPanelOpen = false;
            GameManager.Instance.setTimeScale(GameManager.Instance.GameSpeed);
        }
        else
        {
            pausePanel.SetActive(true);
            _isGamePaused = true;
            Platform.IsModalPanelOpen = true;
            GameManager.Instance.setTimeScale(0f);
        }
    }

    public void RestartLevel()
    {
        LevelManager.Instance.LoadLevel(LevelManager.Instance.CurrentLevel);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void GoToMainMenu()
    {
        GameManager.Instance.setTimeScale(0.5f);
        SceneManager.LoadScene("MainMenu");
    }

    private void ShowGameOver()
    {
        GameManager.Instance.setTimeScale(0f);
        gameOverPanel.SetActive(true);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ShowObjective());
    }

    private IEnumerator ShowObjective()
    {
        objectiveText.text = $"Survive {LevelManager.Instance.CurrentLevel.wavesToWin} waves!";
        objectiveText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        objectiveText.gameObject.SetActive(false);
    }

    private void ShowMissionComplete()
    {
        missionCompletePanel.SetActive(true);
        GameManager.Instance.setTimeScale(0f);
    }

    public void EnterEndlessMode()
    {
        missionCompletePanel.SetActive(false);
        GameManager.Instance.setTimeScale(GameManager.Instance.GameSpeed);
        Spawner.Instance.EnableEndlessMode();
    }

    public void GoToNextLevel()
    {
        // Gọi hàm đã tạo trong LevelManager
        LevelManager.Instance.LoadNextLevel();
    }

    private void HandleTowerClicked(BaseTower tower)
    {
        // 1. Kiểm tra xem có phải click lại chính tháp đang mở không
        if (upgradePanel.activeSelf && _currentSelectedTower == tower)
        {
            HideUpgradePanel(); // Nếu đúng, chỉ cần đóng nó lại
            return;
        }
        
        // 2. Nếu không, đóng tất cả các panel (cả mua và nâng cấp) lại
        HideTowerPanel();
        HideUpgradePanel();

        // 3. Mở panel NÂNG CẤP của tháp mới
        _currentSelectedTower = tower;
        ShowUpgradePanel();
    }
    
    public void ShowUpgradePanel()
    {
        upgradePanel.SetActive(true);
        // Platform.IsModalPanelOpen = true; // Dùng chung biến cờ
        GameManager.Instance.setTimeScale(0f);
        PopulateUpgradePanel();
    }

    public void HideUpgradePanel()
    {
        if (!upgradePanel.activeSelf) return;
        upgradePanel.SetActive(false);
        Platform.IsModalPanelOpen = false;
        GameManager.Instance.setTimeScale(GameManager.Instance.GameSpeed);
        _currentSelectedTower = null; // Quên tháp đi
    }

    private void PopulateUpgradePanel()
    {
        TowerData currentData = _currentSelectedTower.GetData();

        // 1. Tính tiền bán
        int sellValue = Mathf.RoundToInt(currentData.cost * sellReturnPercent);
        // sellValueText.text = sellValue.ToString();

        // 2. Kiểm tra xem có nâng cấp được không
        if (currentData.nextUpgrade != null)
        {
            // upgradeButton.gameObject.SetActive(true);
            upgradeButton.interactable = true;
            // upgradeCostText.text = currentData.nextUpgrade.cost.ToString();
        }
        else
        {
            // upgradeButton.gameObject.SetActive(false);
            upgradeButton.interactable = false;
            // upgradeCostText.text = "MAX";
        }
    }
    
    private void UpgradeSelectedTower()
    {
        TowerData currentData = _currentSelectedTower.GetData();
        TowerData upgradeData = currentData.nextUpgrade;

        if (upgradeData == null)
        {
            Debug.LogError("Lỗi: Cố gắng nâng cấp tháp đã max level.");
            HideUpgradePanel();
            return;
        }
        
        Platform platform = _currentSelectedTower.GetPlatform();
        if (platform == null)
        {
            Debug.LogError("Lỗi: Không tìm thấy Platform của tháp!");
            HideUpgradePanel();
            return;
        }
    
        // 1. Kiểm tra tiền
        if (GameManager.Instance.Resources >= upgradeData.cost)
        {
            // 2. Trừ tiền
            GameManager.Instance.SpendResources(upgradeData.cost);
            _currentSelectedTower.DestroyForUpgrade();
            // 3. Ra lệnh cho tháp tự nâng cấp
            platform.PlaceTower(upgradeData);
        }
        else
        {
            StartCoroutine(ShowWarningMessage("No Resource Enough!"));
        }
        
        // 4. Luôn đóng panel
        HideUpgradePanel();
    }

    private void SellSelectedTower()
    {
        // 1. Tính tiền bán
        int sellValue = Mathf.RoundToInt(_currentSelectedTower.GetData().cost * sellReturnPercent);
        
        // 2. Cộng tiền
        GameManager.Instance.AddResources(sellValue); // Dùng hàm public đã tạo
        
        // 3. Ra lệnh cho tháp tự bán
        _currentSelectedTower.SellTower();
        
        // 4. Đóng panel
        HideUpgradePanel();
    }
}
