using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Simple Game Goal")]
    [SerializeField] private float distanceToWin = 45f;
    [SerializeField] private int safeCrossingScore = 100;
    [SerializeField] private string gameTitle = "Safe Steps";

    [Header("Day And Night Cycle")]
    [SerializeField] private bool dayNightCycle = true;
    [SerializeField] private float cycleDuration = 90f;
    [SerializeField] private float dayLightIntensity = 1.1f;
    [SerializeField] private float nightLightIntensity = 0.18f;
    [SerializeField] private Color dayLightColor = new Color(1f, 0.95f, 0.82f);
    [SerializeField] private Color nightLightColor = new Color(0.45f, 0.55f, 1f);
    [SerializeField] private Color dayAmbientColor = new Color(0.58f, 0.62f, 0.68f);
    [SerializeField] private Color nightAmbientColor = new Color(0.08f, 0.1f, 0.16f);
    [Header("Optional UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text lightText;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject startPanel;

    private Transform player;
    private PlayerHealth playerHealth;
    private TrafficLightController nearestTrafficLight;
    private Vector3 startPosition;
    private bool gameStarted;
    private int score;
    private Light cycleLight;
    private float cycleTime = 0.25f;

    private void Start()
    {
        FindSceneReferences();
        CreateHudIfNeeded();
        ShowStartPopup();
    }

    private void Update()
    {
        if (!gameStarted || player == null)
        {
            return;
        }

        if (playerHealth != null && playerHealth.IsDead)
        {
            return;
        }

        UpdateDayNightCycle();
        UpdateLightHint();
        UpdateProgressScore();
    }

    private void FindSceneReferences()
    {
        playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (playerHealth != null)
        {
            player = playerHealth.transform;
            startPosition = player.position;
        }
        else
        {
            ThirdPersonController controller = FindFirstObjectByType<ThirdPersonController>();
            if (controller != null)
            {
                player = controller.transform;
                startPosition = player.position;
            }
        }

        nearestTrafficLight = FindFirstObjectByType<TrafficLightController>();
        cycleLight = FindCycleLight();
    }

    private void UpdateProgressScore()
    {
        float progress = Mathf.Clamp01(Vector3.Distance(startPosition, player.position) / distanceToWin);
        score = Mathf.RoundToInt(progress * safeCrossingScore);

        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    private void UpdateLightHint()
    {
        if (nearestTrafficLight == null || lightText == null)
        {
            return;
        }

        switch (nearestTrafficLight.currentState)
        {
            case TrafficLightController.LightState.Green:
                lightText.text = "Pedestrian Signal: CROSS";
                lightText.color = new Color(0.25f, 0.9f, 0.35f);
                SetStatus("Green signal. Cross carefully and keep watching cars.");
                break;
            case TrafficLightController.LightState.Yellow:
                lightText.text = "Pedestrian Signal: WAIT";
                lightText.color = new Color(1f, 0.8f, 0.25f);
                SetStatus("Yellow signal. Stop and wait for the next safe crossing.");
                break;
            default:
                lightText.text = "Pedestrian Signal: WAIT";
                lightText.color = new Color(1f, 0.3f, 0.3f);
                SetStatus("Red signal. Stay on the footpath.");
                break;
        }
    }

    private void UpdateHud(string message)
    {
        SetStatus(message);

        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    private void UpdateDayNightCycle()
    {
        if (!dayNightCycle)
        {
            return;
        }

        if (cycleLight == null)
        {
            cycleLight = FindCycleLight();

            if (cycleLight == null)
            {
                return;
            }
        }

        cycleTime = (cycleTime + Time.deltaTime / Mathf.Max(1f, cycleDuration)) % 1f;

        float sunAngle = (cycleTime * 360f) - 90f;
        cycleLight.transform.rotation = Quaternion.Euler(sunAngle, -30f, 0f);

        float dayAmount = Mathf.Clamp01(Mathf.Sin(cycleTime * Mathf.PI));
        cycleLight.intensity = Mathf.Lerp(nightLightIntensity, dayLightIntensity, dayAmount);
        cycleLight.color = Color.Lerp(nightLightColor, dayLightColor, dayAmount);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = Color.Lerp(nightAmbientColor, dayAmbientColor, dayAmount);
        RenderSettings.sun = cycleLight;
    }

    private Light FindCycleLight()
    {
        Light activeSun = RenderSettings.sun;

        if (activeSun != null)
        {
            return activeSun;
        }

        Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);

        foreach (Light sceneLight in lights)
        {
            if (sceneLight.type == LightType.Directional)
            {
                return sceneLight;
            }
        }

        return null;
    }

    private void CreateHudIfNeeded()
    {
        if (scoreText != null && statusText != null && lightText != null && hudPanel != null && startPanel != null)
        {
            return;
        }

        Canvas canvas = FindOrCreateOverlayCanvas();

        RectTransform hudRoot = CreatePanel(canvas.transform, "SafetyHud", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(430f, 120f));
        hudPanel = hudRoot.gameObject;
        hudRoot.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

        TMP_Text titleText = CreateText(hudRoot, "TitleText", gameTitle, 22, FontStyles.Bold, new Vector2(18f, -12f), new Vector2(380f, 28f));
        titleText.color = Color.white;

        scoreText = CreateText(hudRoot, "ScoreText", "Score: 0", 16, FontStyles.Bold, new Vector2(18f, -42f), new Vector2(160f, 24f));
        lightText = CreateText(hudRoot, "LightText", "Signal: WAIT", 16, FontStyles.Bold, new Vector2(18f, -66f), new Vector2(360f, 24f));
        statusText = CreateText(hudRoot, "StatusText", "", 14, FontStyles.Normal, new Vector2(18f, -90f), new Vector2(390f, 24f));

        startPanel = CreatePanel(canvas.transform, "StartPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680f, 340f)).gameObject;
        startPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.82f);
        TMP_Text startTitle = CreateText(startPanel.transform, "StartTitle", gameTitle, 40, FontStyles.Bold, new Vector2(60f, -42f), new Vector2(560f, 56f));
        startTitle.alignment = TextAlignmentOptions.Center;
        TMP_Text startMessage = CreateText(startPanel.transform, "StartMessage", "Cross the road safely.\nWait for the pedestrian signal, stay on the footpath on red, and cross carefully on green.", 22, FontStyles.Normal, new Vector2(70f, -118f), new Vector2(540f, 110f));
        startMessage.alignment = TextAlignmentOptions.Center;
        CreateButton(startPanel.transform, "PlayButton", "Play", new Vector2(240f, -250f), new Vector2(200f, 56f), StartGame);

    }

    private void ShowStartPopup()
    {
        gameStarted = false;
        Time.timeScale = 0f;
        UnlockCursor();

        if (hudPanel != null)
        {
            hudPanel.SetActive(false);
        }

        if (startPanel != null)
        {
            startPanel.SetActive(true);
        }
    }

    public void StartGame()
    {
        gameStarted = true;
        Time.timeScale = 1f;

        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }

        if (hudPanel != null)
        {
            hudPanel.SetActive(false);
        }

        StarterAssetsInputs inputs = FindFirstObjectByType<StarterAssetsInputs>();
        if (inputs != null)
        {
            inputs.cursorLocked = true;
            inputs.cursorInputForLook = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private Canvas FindOrCreateOverlayCanvas()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);

        foreach (Canvas sceneCanvas in canvases)
        {
            if (sceneCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return sceneCanvas;
            }
        }

        GameObject canvasObject = new GameObject("GameHUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        return canvas;
    }

    private RectTransform CreatePanel(Transform parent, string panelName, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject panel = new GameObject(panelName, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = anchorMin;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        return rect;
    }

    private TMP_Text CreateText(Transform parent, string textName, string text, int fontSize, FontStyles fontStyle, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject textObject = new GameObject(textName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        TMP_Text tmpText = textObject.GetComponent<TMP_Text>();
        tmpText.text = text;
        tmpText.fontSize = fontSize;
        tmpText.fontStyle = fontStyle;
        tmpText.enableWordWrapping = true;
        tmpText.color = Color.white;

        return tmpText;
    }

    private void CreateButton(Transform parent, string buttonName, string label, Vector2 anchoredPosition, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = new GameObject(buttonName, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.15f, 0.65f, 0.3f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(onClick);

        TMP_Text buttonText = CreateText(buttonObject.transform, "Label", label, 22, FontStyles.Bold, Vector2.zero, size);
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.rectTransform.anchorMin = Vector2.zero;
        buttonText.rectTransform.anchorMax = Vector2.one;
        buttonText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        buttonText.rectTransform.anchoredPosition = Vector2.zero;
        buttonText.rectTransform.sizeDelta = Vector2.zero;
    }

    public void RestartGame()
    {
        Debug.Log("Restart clicked");

        Time.timeScale = 1f;
        UnlockCursor();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
