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
    public static UIController Instance { get; private set; }
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

    [Header("Input Settings")]
    [SerializeField] private LayerMask clickableLayer;
    private BaseTower _currentSelectedTower;
    public bool IsAnyPanelOpen => towerPanel.activeSelf || upgradePanel.activeSelf;

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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Update()
    {
        // Xử lý Input chuột trái
        if (Input.GetMouseButtonDown(0))
        {
            ProcessClick();
        }
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

    private void ProcessClick()
    {
        // 1. Chặn click xuyên qua UI (Panel)
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // 2. Bắn tia Raycast từ vị trí chuột
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        // QUAN TRỌNG: Chỉ check va chạm với layer nằm trong clickableLayer
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, clickableLayer);

        if (hit.collider != null)
        {
            // Thử lấy component Platform từ object bị bắn trúng
            Platform clickedPlatform = hit.collider.GetComponent<Platform>();
            
            // Trường hợp click trúng Tháp con nằm trên Platform
            if (clickedPlatform == null)
            {
                clickedPlatform = hit.collider.GetComponentInParent<Platform>();
            }

            // Nếu tìm thấy Platform, kích hoạt hàm xử lý click
            if (clickedPlatform != null)
            {
                clickedPlatform.HandleClick();
            }
        }
        else
        {
            // (Tùy chọn) Nếu click ra ngoài khoảng không -> Đóng các panel đang mở
            if (IsAnyPanelOpen)
            {
                HideTowerPanel();
                HideUpgradePanel();
            }
        }
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
        // Nếu đang mở bảng nâng cấp -> Đóng nó lại trước
        if (upgradePanel.activeSelf) HideUpgradePanel();
        
        // Nếu click lại vào chính platform đang mở -> Đóng bảng xây (Toggle)
        if (towerPanel.activeSelf && _currentPlatform == platform)
        {
            HideTowerPanel();
            return;
        }

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
        _currentPlatform = null;
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
            // Platform.IsModalPanelOpen = false;
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
        int currentIndex = LevelManager.Instance.CurrentLevelIndex;
        LevelManager.Instance.LoadLevelFromCloud(currentIndex);
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
        // objectiveText.text = $"Survive {LevelManager.Instance.CurrentLevel.WavesToWin} waves!";
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
        // Nếu đang mở bảng xây -> Đóng nó lại trước
        if (towerPanel.activeSelf) HideTowerPanel();

        // Nếu click lại vào chính tháp đang chọn -> Đóng bảng nâng cấp (Toggle)
        if (upgradePanel.activeSelf && _currentSelectedTower == tower)
        {
            HideUpgradePanel();
            return;
        }

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
        // Platform.IsModalPanelOpen = false;
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

    public void ResetGameUI()
    {
        HideTowerPanel();
        HideUpgradePanel();
        
        missionCompletePanel.SetActive(false);
    }
}
