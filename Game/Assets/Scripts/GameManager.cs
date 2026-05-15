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
    [SerializeField] private float cycleDuration = 480f;
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
    [SerializeField] private GameObject objectivePanel;
    [SerializeField] private GameObject milestonePanel;
    [SerializeField] private TMP_Text milestoneText;

    private Transform player;
    private PlayerHealth playerHealth;
    private TrafficLightController nearestTrafficLight;
    private Vector3 startPosition;
    private bool gameStarted;
    private bool milestoneShowing;
    private bool gameComplete;
    private int score;
    private int nextStepMilestone = 100;
    private float totalStepDistance;
    private Vector3 lastStepPosition;
    private bool hasStepPosition;
    private Light cycleLight;
    private float cycleTime = 0.25f;
    private string heldStatusMessage;
    private float heldStatusUntil;

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
        CheckStepMilestone();
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
        Vector3 currentPosition = player.position;

        if (!hasStepPosition)
        {
            lastStepPosition = currentPosition;
            hasStepPosition = true;
        }

        Vector3 movement = currentPosition - lastStepPosition;
        movement.y = 0f;
        totalStepDistance += movement.magnitude;
        lastStepPosition = currentPosition;

        float stepsPerUnit = safeCrossingScore / Mathf.Max(1f, distanceToWin);
        score = Mathf.FloorToInt(totalStepDistance * stepsPerUnit);

        if (scoreText != null)
        {
            scoreText.text = $"Steps: {score}";
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
                lightText.text = "Signal: CROSS";
                lightText.color = new Color(0.25f, 0.9f, 0.35f);
                SetStatus("Green signal. Cross carefully and keep watching cars.");
                break;
            case TrafficLightController.LightState.Yellow:
                lightText.text = "Signal: WAIT";
                lightText.color = new Color(1f, 0.8f, 0.25f);
                SetStatus("Yellow signal. Stop and wait for the next safe crossing.");
                break;
            default:
                lightText.text = "Signal: WAIT";
                lightText.color = new Color(1f, 0.3f, 0.3f);
                SetStatus("Red signal. Stay on the footpath.");
                break;
        }
    }

    private void CheckStepMilestone()
    {
        if (milestoneShowing || gameComplete || score < nextStepMilestone)
        {
            return;
        }

        milestoneShowing = true;
        gameStarted = false;
        gameComplete = nextStepMilestone >= 500;

        if (milestoneText != null)
        {
            milestoneText.text = gameComplete
                ? "Congratulation\nYou Won"
                : $"Congratulations!\nYou completed {nextStepMilestone} steps safely.";
        }

        Button milestoneButton = milestonePanel != null ? milestonePanel.GetComponentInChildren<Button>(true) : null;
        TMP_Text milestoneButtonText = milestoneButton != null ? milestoneButton.GetComponentInChildren<TMP_Text>(true) : null;

        if (milestoneButtonText != null)
        {
            milestoneButtonText.text = gameComplete ? "Play Again" : "Continue";
        }

        if (milestoneButton != null)
        {
            milestoneButton.onClick.RemoveAllListeners();
            milestoneButton.onClick.AddListener(gameComplete ? RestartGame : ContinueGame);
        }

        if (milestonePanel != null)
        {
            milestonePanel.SetActive(true);
        }

        Time.timeScale = 0f;
        UnlockCursor();
    }

    private void SetStatus(string message)
    {
        if (!string.IsNullOrEmpty(heldStatusMessage) && Time.time < heldStatusUntil)
        {
            if (statusText != null)
            {
                statusText.text = heldStatusMessage;
            }

            return;
        }

        heldStatusMessage = null;

        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    private void HoldStatus(string message, float duration)
    {
        heldStatusMessage = message;
        heldStatusUntil = Time.time + duration;

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
        if (scoreText != null && statusText != null && lightText != null && hudPanel != null && startPanel != null && objectivePanel != null && milestonePanel != null && milestoneText != null)
        {
            return;
        }

        Canvas canvas = FindOrCreateOverlayCanvas();

        RectTransform hudRoot = CreatePanel(canvas.transform, "SafetyHud", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(36f, -36f), new Vector2(420f, 150f));
        hudPanel = hudRoot.gameObject;
        hudRoot.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.78f);

        TMP_Text titleText = CreateText(hudRoot, "TitleText", gameTitle, 46, FontStyles.Bold, new Vector2(32f, -20f), new Vector2(360f, 58f));
        titleText.color = Color.white;

        scoreText = CreateText(hudRoot, "ScoreText", "Steps: 0", 42, FontStyles.Bold, new Vector2(32f, -86f), new Vector2(340f, 54f));
        lightText = CreateText(hudRoot, "LightText", "", 1, FontStyles.Normal, Vector2.zero, Vector2.zero);
        statusText = CreateText(hudRoot, "StatusText", "", 1, FontStyles.Normal, Vector2.zero, Vector2.zero);
        lightText.gameObject.SetActive(false);
        statusText.gameObject.SetActive(false);

        startPanel = CreatePanel(canvas.transform, "StartPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1420f, 780f)).gameObject;
        startPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.9f);
        TMP_Text startTitle = CreateText(startPanel.transform, "StartTitle", gameTitle, 104, FontStyles.Bold, new Vector2(100f, -70f), new Vector2(1220f, 130f));
        startTitle.alignment = TextAlignmentOptions.Center;
        TMP_Text startMessage = CreateText(startPanel.transform, "StartMessage", "Follow Traffic rules.\nCross the road safely.\nTo win complete 500 steps.", 54, FontStyles.Normal, new Vector2(140f, -240f), new Vector2(1140f, 170f));
        startMessage.alignment = TextAlignmentOptions.Center;
        CreateButton(startPanel.transform, "PlayButton", "Play", new Vector2(520f, -610f), new Vector2(380f, 110f), StartGame);

        objectivePanel = CreatePanel(canvas.transform, "ObjectivePanel", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(1280f, 130f)).gameObject;
        objectivePanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.82f);
        TMP_Text objectiveText = CreateText(objectivePanel.transform, "ObjectiveText", "Objective: Cross safely to the other side.", 46, FontStyles.Bold, new Vector2(40f, -28f), new Vector2(1200f, 72f));
        objectiveText.alignment = TextAlignmentOptions.Center;
        objectivePanel.SetActive(false);

        milestonePanel = CreatePanel(canvas.transform, "MilestonePanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1320f, 560f)).gameObject;
        milestonePanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.92f);
        milestoneText = CreateText(milestonePanel.transform, "MilestoneText", "You completed 100 steps safely.", 72, FontStyles.Bold, new Vector2(100f, -90f), new Vector2(1120f, 210f));
        milestoneText.alignment = TextAlignmentOptions.Center;
        CreateButton(milestonePanel.transform, "ContinueButton", "Continue", new Vector2(470f, -390f), new Vector2(380f, 110f), ContinueGame);
        milestonePanel.SetActive(false);
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

        if (objectivePanel != null)
        {
            objectivePanel.SetActive(false);
        }

        if (milestonePanel != null)
        {
            milestonePanel.SetActive(false);
        }
    }

    public void StartGame()
    {
        gameStarted = true;
        milestoneShowing = false;
        gameComplete = false;
        nextStepMilestone = 100;
        totalStepDistance = 0f;
        hasStepPosition = false;
        Time.timeScale = 1f;

        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }

        if (hudPanel != null)
        {
            hudPanel.SetActive(true);
        }

        if (objectivePanel != null)
        {
            objectivePanel.SetActive(true);
            CancelInvoke(nameof(HideObjectivePanel));
            Invoke(nameof(HideObjectivePanel), 4f);
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

        TMP_Text buttonText = CreateText(buttonObject.transform, "Label", label, 48, FontStyles.Bold, Vector2.zero, size);
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.rectTransform.anchorMin = Vector2.zero;
        buttonText.rectTransform.anchorMax = Vector2.one;
        buttonText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        buttonText.rectTransform.anchoredPosition = Vector2.zero;
        buttonText.rectTransform.sizeDelta = Vector2.zero;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnlockCursor();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ContinueGame()
    {
        nextStepMilestone += 100;
        milestoneShowing = false;
        gameStarted = true;
        hasStepPosition = false;
        Time.timeScale = 1f;

        if (milestonePanel != null)
        {
            milestonePanel.SetActive(false);
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

    private void HideObjectivePanel()
    {
        if (objectivePanel != null)
        {
            objectivePanel.SetActive(false);
        }
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}

