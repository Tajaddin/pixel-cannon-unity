using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Helper script to set up the game scene with all required objects.
/// Attach to an empty GameObject and click "Setup Scene" in the inspector.
/// </summary>
public class SceneSetup : MonoBehaviour
{
    [Header("Setup Options")]
    public bool setupGameScene = true;
    public bool setupMainMenu = false;

    [Header("References (Auto-filled)")]
    public GameManager gameManager;
    public PixelGrid pixelGrid;
    public CannonManager cannonManager;
    public UIManager uiManager;

#if UNITY_EDITOR
    [ContextMenu("Setup Game Scene")]
    public void SetupGameScene()
    {
        if (setupGameScene)
            CreateGameScene();
        else if (setupMainMenu)
            CreateMainMenuScene();
    }

    private void CreateGameScene()
    {
        // Create Managers
        GameObject managers = new GameObject("--- MANAGERS ---");
        
        // Game Manager
        GameObject gmObj = new GameObject("GameManager");
        gmObj.transform.parent = managers.transform;
        gameManager = gmObj.AddComponent<GameManager>();

        // Audio Manager
        GameObject audioObj = new GameObject("AudioManager");
        audioObj.transform.parent = managers.transform;
        audioObj.AddComponent<AudioManager>();

        // Ad Manager
        GameObject adObj = new GameObject("AdManager");
        adObj.transform.parent = managers.transform;
        adObj.AddComponent<AdManager>();

        // Create Game Objects
        GameObject gameObjects = new GameObject("--- GAME OBJECTS ---");

        // Pixel Grid
        GameObject gridObj = new GameObject("PixelGrid");
        gridObj.transform.parent = gameObjects.transform;
        gridObj.transform.position = new Vector3(0, 2, 0);
        pixelGrid = gridObj.AddComponent<PixelGrid>();
        gameManager.pixelGrid = pixelGrid;

        // Cannon Manager
        GameObject cannonMgrObj = new GameObject("CannonManager");
        cannonMgrObj.transform.parent = gameObjects.transform;
        cannonMgrObj.transform.position = new Vector3(0, -3, 0);
        cannonManager = cannonMgrObj.AddComponent<CannonManager>();
        gameManager.cannonManager = cannonManager;

        // Cannon Spawn Area
        GameObject spawnArea = new GameObject("CannonSpawnArea");
        spawnArea.transform.parent = cannonMgrObj.transform;
        spawnArea.transform.localPosition = Vector3.zero;
        cannonManager.spawnArea = spawnArea.transform;

        // Cannon Slots
        GameObject slotsContainer = new GameObject("CannonSlots");
        slotsContainer.transform.parent = gameObjects.transform;
        slotsContainer.transform.position = new Vector3(0, -1, 0);

        gameManager.cannonSlots = new Transform[5];
        for (int i = 0; i < 5; i++)
        {
            GameObject slot = new GameObject($"Slot_{i}");
            slot.transform.parent = slotsContainer.transform;
            slot.transform.localPosition = new Vector3(-2 + i, 0, 0);
            gameManager.cannonSlots[i] = slot.transform;

            // Add visual indicator
            GameObject slotVisual = GameObject.CreatePrimitive(PrimitiveType.Quad);
            slotVisual.name = "Visual";
            slotVisual.transform.parent = slot.transform;
            slotVisual.transform.localPosition = Vector3.zero;
            slotVisual.transform.localScale = new Vector3(0.8f, 0.8f, 1);
            
            MeshRenderer mr = slotVisual.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.color = new Color(1, 1, 1, 0.2f);
                mr.material = mat;
            }

            DestroyImmediate(slotVisual.GetComponent<Collider>());
        }

        // Create UI
        CreateGameUI();

        // Create Camera
        SetupCamera();

        // Create Background
        CreateBackground();

        Debug.Log("Game Scene Setup Complete!");
    }

    private void CreateGameUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        uiManager = canvasObj.AddComponent<UIManager>();
        gameManager.uiManager = uiManager;

        // HUD Panel
        GameObject hudPanel = CreatePanel("HUD", canvasObj.transform);
        RectTransform hudRect = hudPanel.GetComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0, 0.9f);
        hudRect.anchorMax = Vector2.one;
        hudRect.offsetMin = Vector2.zero;
        hudRect.offsetMax = Vector2.zero;

        // Pause Panel
        GameObject pausePanel = CreatePanel("PausePanel", canvasObj.transform);
        pausePanel.SetActive(false);
        uiManager.pausePanel = pausePanel;

        // Level Complete Panel
        GameObject completePanel = CreatePanel("LevelCompletePanel", canvasObj.transform);
        completePanel.SetActive(false);
        uiManager.levelCompletePanel = completePanel;

        // Event System
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    private GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        UnityEngine.UI.Image img = panel.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0, 0, 0, 0.5f);

        return panel;
    }

    private void SetupCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            mainCam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            camObj.AddComponent<AudioListener>();
        }

        mainCam.transform.position = new Vector3(0, 0, -10);
        mainCam.orthographic = true;
        mainCam.orthographicSize = 7;
        mainCam.backgroundColor = new Color(0.1f, 0.1f, 0.2f);
    }

    private void CreateBackground()
    {
        GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bg.name = "Background";
        bg.transform.position = new Vector3(0, 0, 5);
        bg.transform.localScale = new Vector3(20, 15, 1);
        
        MeshRenderer mr = bg.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = new Color(0.15f, 0.15f, 0.25f);
            mr.material = mat;
        }

        DestroyImmediate(bg.GetComponent<Collider>());
    }

    private void CreateMainMenuScene()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        MainMenu mainMenu = canvasObj.AddComponent<MainMenu>();

        // Main Panel
        GameObject mainPanel = CreateMenuPanel("MainPanel", canvasObj.transform);
        mainMenu.mainPanel = mainPanel;

        // Title
        CreateText("PIXEL CANNON", mainPanel.transform, new Vector2(0.5f, 0.8f));

        // Buttons
        CreateButton("PLAY", mainPanel.transform, new Vector2(0.5f, 0.5f));
        CreateButton("Settings", mainPanel.transform, new Vector2(0.5f, 0.4f));

        // Difficulty Panel
        GameObject diffPanel = CreateMenuPanel("DifficultyPanel", canvasObj.transform);
        diffPanel.SetActive(false);
        mainMenu.difficultyPanel = diffPanel;

        // Level Select Panel
        GameObject levelPanel = CreateMenuPanel("LevelSelectPanel", canvasObj.transform);
        levelPanel.SetActive(false);
        mainMenu.levelSelectPanel = levelPanel;

        // Event System
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // Camera
        SetupCamera();

        Debug.Log("Main Menu Scene Setup Complete!");
    }

    private GameObject CreateMenuPanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        return panel;
    }

    private void CreateText(string text, Transform parent, Vector2 anchor)
    {
        GameObject textObj = new GameObject("Text_" + text);
        textObj.transform.SetParent(parent, false);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.sizeDelta = new Vector2(600, 100);

        UnityEngine.UI.Text uiText = textObj.AddComponent<UnityEngine.UI.Text>();
        uiText.text = text;
        uiText.fontSize = 60;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.color = Color.white;
    }

    private void CreateButton(string text, Transform parent, Vector2 anchor)
    {
        GameObject btnObj = new GameObject("Button_" + text);
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.sizeDelta = new Vector2(400, 80);

        UnityEngine.UI.Image img = btnObj.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.3f, 0.6f, 1f);

        btnObj.AddComponent<UnityEngine.UI.Button>();

        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        UnityEngine.UI.Text uiText = textObj.AddComponent<UnityEngine.UI.Text>();
        uiText.text = text;
        uiText.fontSize = 36;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.color = Color.white;
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(SceneSetup))]
public class SceneSetupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SceneSetup setup = (SceneSetup)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Setup Game Scene", GUILayout.Height(40)))
        {
            setup.setupGameScene = true;
            setup.setupMainMenu = false;
            setup.SetupGameScene();
        }

        if (GUILayout.Button("Setup Main Menu Scene", GUILayout.Height(40)))
        {
            setup.setupGameScene = false;
            setup.setupMainMenu = true;
            setup.SetupGameScene();
        }
    }
}
#endif
