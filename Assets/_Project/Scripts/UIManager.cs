using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; protected set; }

    [Header("Panels")] [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject collectPanel;
    [SerializeField] private GameObject fadePanel;
    [SerializeField] private GameObject playPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    public Image slider;
    public Image hand;
    public Slider speedSlider;
    [Header("Texts")] public TextMeshProUGUI levelText;
    [Header("Texts")] public TextMeshProUGUI levelCollectText;
    public TextMeshProUGUI levelPlayText;
    public TextMeshProUGUI nextLevelText;
    public TextMeshProUGUI rewardText;
    public TextMeshProUGUI rewardWinText;
    [Header("Buttons")] public Button startButton;
    private float initialDistance;
    private Vector3 finishPosition;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        levelText.text = "LEVEL " + GameManager.Instance.level;
        levelCollectText.text = "LEVEL " + GameManager.Instance.level;
        levelPlayText.text = GameManager.Instance.level.ToString();
        nextLevelText.text = (GameManager.Instance.level + 1).ToString();
        slider.fillAmount = 0f;
        speedSlider.maxValue = GameManager.Instance.speedRushMax;
        speedSlider.value = 0;
        finishPosition = GameManager.Instance.finishLine.position + new Vector3(0, 1.5f, 0f);
        initialDistance = Vector3.Distance(finishPosition, Player.Instance.transform.position);
        HidePanels();
    }

    private void Update()
    {
        if (GameManager.gameState is GameState.Play or GameState.Win) UpdateSlider();
        if(GameManager.gameState == GameState.Play && Input.GetMouseButtonDown(0))
            hand.gameObject.SetActive(false);
    }

    public void SetPanel(GameState state = GameState.Start)
    {
        startPanel.SetActive(false);
        playPanel.SetActive(false);
        winPanel.SetActive(false);
        losePanel.SetActive(false);
        collectPanel.SetActive(false);
        fadePanel.SetActive(false);

        switch (state)
        {
            case GameState.Start:
                startPanel.SetActive(true);
                break;
            case GameState.Play:
                playPanel.SetActive(true);
                break; 
            case GameState.Rotate:
                collectPanel.SetActive(true);
                break;
            case GameState.Win:
                winPanel.SetActive(true);
                rewardWinText.text = Player.Instance.reward.ToString();
                break;
            case GameState.Lose:
                losePanel.SetActive(true);
                break;
        }

        GameManager.gameState = state;
    }

    public void ActivateStartButton(bool type)
    {
        startButton.gameObject.SetActive(type);
    }

    public void ActivateFadePanel()
    {
        fadePanel.SetActive(true);
        Invoke(nameof(CloseFadePanel), 0.85f);
    }

    public void CloseFadePanel()
    {
        GameManager.Instance.SwitchCams();
        SetPanel(GameState.Play);
    }

    public void UpdateReward()
    {
        Player.Instance.reward++;
        rewardText.text = Player.Instance.reward.ToString();
    }

    private void UpdateSlider()
    {
        var dis = Vector3.Distance(finishPosition, Player.Instance.transform.position);
        var progress = (initialDistance - dis) / initialDistance;
        slider.fillAmount = slider.fillAmount > progress ? slider.fillAmount : progress;
    }

    public void UpdateSpeedSlider()
    {
        if (GameManager.gameState == GameState.SpeedRush) return;

        GameManager.Instance.speedCounter++;
        speedSlider.value = GameManager.Instance.speedCounter;
        if (speedSlider.value >= GameManager.Instance.speedRushMax)
            GameManager.Instance.SetSpeedRushMode();
    }

    public void ResetSpeedSlider()
    {
        GameManager.Instance.speedCounter = 0;
        speedSlider.value = GameManager.Instance.speedCounter;
    }

    private void HidePanels()
    {
        fadePanel.transform.position -= new Vector3(Screen.width * 2, 0, 0);
        winPanel.transform.position -= new Vector3(Screen.width, 0, 0);
        losePanel.transform.position -= new Vector3(Screen.width, 0, 0);
    }
}