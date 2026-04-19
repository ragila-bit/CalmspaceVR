using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEditor.SceneManagement;

public class MusicPlayerSetup : Editor
{
    // ── Colour palette ──────────────────────────────────────────────────────
    static readonly Color BgPanel      = new Color(0.08f, 0.07f, 0.14f, 0.96f);
    static readonly Color BgHeader     = new Color(0.13f, 0.11f, 0.22f, 1.00f);
    static readonly Color AccentColor  = new Color(0.47f, 0.35f, 0.85f, 1.00f);
    static readonly Color BtnNormal    = new Color(0.18f, 0.15f, 0.30f, 1.00f);
    static readonly Color BtnHighlight = new Color(0.35f, 0.28f, 0.55f, 1.00f);
    static readonly Color BtnPressed   = new Color(0.55f, 0.45f, 0.80f, 1.00f);
    static readonly Color TextPrimary  = new Color(0.95f, 0.93f, 1.00f, 1.00f);
    static readonly Color TextSubtle   = new Color(0.65f, 0.60f, 0.80f, 1.00f);
    static readonly Color SliderBg     = new Color(0.25f, 0.20f, 0.40f, 1.00f);

    [MenuItem("CalmspaceVR/Setup Music Player")]
    public static void SetupMusicPlayer()
    {
        // --- EventSystem ---
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<XRUIInputModule>();
        }
        else
        {
            EventSystem es = Object.FindFirstObjectByType<EventSystem>();
            if (es.GetComponent<XRUIInputModule>() == null)
                es.gameObject.AddComponent<XRUIInputModule>();
        }

        // --- MusicPlayer ---
        GameObject musicPlayerGO = new GameObject("MusicPlayer");
        AudioSource audioSource = musicPlayerGO.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = 1f;
        MusicPlayer musicPlayer = musicPlayerGO.AddComponent<MusicPlayer>();

        // --- Canvas (starts hidden via MusicPlayer.Start) ---
        GameObject canvasGO = new GameObject("MusicPlayerCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<TrackedDeviceGraphicRaycaster>();
        RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(480, 360);
        canvasGO.transform.localScale = Vector3.one * 0.001f;
        canvasGO.transform.position   = new Vector3(0, 1.5f, 1.5f);

        // --- Panel ---
        GameObject panelGO = new GameObject("MusicPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        panelGO.AddComponent<Image>().color = BgPanel;
        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.sizeDelta        = new Vector2(480, 360);
        panelRect.anchoredPosition = Vector2.zero;

        // Header bar
        GameObject headerGO = new GameObject("Header");
        headerGO.transform.SetParent(panelGO.transform, false);
        headerGO.AddComponent<Image>().color = BgHeader;
        RectTransform headerRect = headerGO.GetComponent<RectTransform>();
        headerRect.anchorMin        = new Vector2(0, 1);
        headerRect.anchorMax        = new Vector2(1, 1);
        headerRect.pivot            = new Vector2(0.5f, 1);
        headerRect.sizeDelta        = new Vector2(0, 50);
        headerRect.anchoredPosition = Vector2.zero;

        // "NOW PLAYING" in header
        CreateText(headerGO.transform, "NowPlaying", "NOW PLAYING",
            new Vector2(0, -25), new Vector2(440, 50), 11, TextSubtle, FontStyles.Bold);

        // Accent line
        GameObject accentGO = new GameObject("AccentLine");
        accentGO.transform.SetParent(panelGO.transform, false);
        accentGO.AddComponent<Image>().color = AccentColor;
        RectTransform accentRect = accentGO.GetComponent<RectTransform>();
        accentRect.anchorMin        = new Vector2(0, 1);
        accentRect.anchorMax        = new Vector2(1, 1);
        accentRect.pivot            = new Vector2(0.5f, 1);
        accentRect.sizeDelta        = new Vector2(0, 2);
        accentRect.anchoredPosition = new Vector2(0, -50);

        // Track name
        TextMeshProUGUI trackText = CreateText(panelGO.transform, "TrackName", "No Track Loaded",
            new Vector2(0, 60), new Vector2(440, 40), 20, TextPrimary, FontStyles.Bold)
            .GetComponent<TextMeshProUGUI>();

        // Track counter
        TextMeshProUGUI counterText = CreateText(panelGO.transform, "TrackCounter", "—",
            new Vector2(0, 28), new Vector2(440, 28), 13, TextSubtle, FontStyles.Normal)
            .GetComponent<TextMeshProUGUI>();

        // Playback buttons
        GameObject prevBtnGO = CreateButton(panelGO.transform, "PreviousButton", "◄◄",
            new Vector2(-130, -28), new Vector2(80, 56), BtnNormal, 20);
        GameObject playBtnGO = CreateButton(panelGO.transform, "PlayPauseButton", "Pause",
            new Vector2(0, -28), new Vector2(90, 56), AccentColor, 22);
        GameObject nextBtnGO = CreateButton(panelGO.transform, "NextButton", "►►",
            new Vector2(130, -28), new Vector2(80, 56), BtnNormal, 20);

        UnityEventTools.AddVoidPersistentListener(prevBtnGO.GetComponent<Button>().onClick, musicPlayer.PreviousTrack);
        UnityEventTools.AddVoidPersistentListener(playBtnGO.GetComponent<Button>().onClick, musicPlayer.PlayPause);
        UnityEventTools.AddVoidPersistentListener(nextBtnGO.GetComponent<Button>().onClick, musicPlayer.NextTrack);

        // Poke effect on playback buttons
        foreach (GameObject btn in new[] { prevBtnGO, playBtnGO, nextBtnGO })
            btn.AddComponent<ButtonPokeEffect>();

        // Volume label
        CreateText(panelGO.transform, "VolumeLabel", "VOLUME",
            new Vector2(0, -98), new Vector2(440, 22), 10, TextSubtle, FontStyles.Bold);

        // Volume slider
        GameObject sliderGO = CreateSlider(panelGO.transform, new Vector2(0, -122), new Vector2(380, 28));
        Slider slider = sliderGO.GetComponent<Slider>();

        // Close button (top-right X)
        GameObject closeBtnGO = CreateButton(panelGO.transform, "CloseButton", "X",
            new Vector2(210, 135), new Vector2(36, 36), BgHeader, 14);
        UnityEventTools.AddVoidPersistentListener(closeBtnGO.GetComponent<Button>().onClick, musicPlayer.TogglePanel);
        closeBtnGO.AddComponent<ButtonPokeEffect>();

        // Exit button
        GameObject exitBtnGO = CreateButton(panelGO.transform, "ExitButton", "Quit Game",
            new Vector2(0, -158), new Vector2(140, 36), new Color(0.45f, 0.10f, 0.10f, 1f), 14);
        UnityEventTools.AddVoidPersistentListener(exitBtnGO.GetComponent<Button>().onClick, musicPlayer.QuitGame);
        exitBtnGO.AddComponent<ButtonPokeEffect>();

        // --- Wire up MusicPlayer ---
        SerializedObject so = new SerializedObject(musicPlayer);
        so.FindProperty("audioSource").objectReferenceValue         = audioSource;
        so.FindProperty("musicCanvas").objectReferenceValue         = canvas;
        so.FindProperty("trackNameText").objectReferenceValue       = trackText;
        so.FindProperty("trackCounterText").objectReferenceValue    = counterText;
        so.FindProperty("playPauseButton").objectReferenceValue     = playBtnGO.GetComponent<Button>();
        so.FindProperty("playPauseButtonText").objectReferenceValue = playBtnGO.GetComponentInChildren<TextMeshProUGUI>();
        so.FindProperty("volumeSlider").objectReferenceValue        = slider;
        so.ApplyModifiedProperties();

        // Auto-load audio clips + fix import settings to prevent lag on track switch
        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Audio" });

        // Sort so Campfire is first
        System.Array.Sort(guids, (a, b) =>
        {
            string nameA = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(a));
            string nameB = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(b));
            if (nameA == "Campfire") return -1;
            if (nameB == "Campfire") return  1;
            return string.Compare(nameA, nameB);
        });

        SerializedProperty tracksProp = so.FindProperty("tracks");
        tracksProp.arraySize = guids.Length;
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            // Decompress on load = no stutter when switching tracks
            AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer != null)
            {
                AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                importer.defaultSampleSettings = settings;
                importer.SaveAndReimport();
            }
            tracksProp.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = musicPlayerGO;
        Debug.Log($"Music Player setup complete! {guids.Length} tracks loaded. Assign XR Camera in Inspector.");
    }

    [MenuItem("CalmspaceVR/Setup Start Screen")]
    public static void SetupStartScreen()
    {
        // --- Canvas ---
        GameObject canvasGO = new GameObject("StartScreenCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<TrackedDeviceGraphicRaycaster>();
        canvasGO.AddComponent<CanvasGroup>();
        RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1100, 700);
        canvasGO.transform.localScale = Vector3.one * 0.001f;
        canvasGO.transform.position   = new Vector3(0, 1.5f, 1.2f);

        // Background panel
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform, false);
        bgGO.AddComponent<Image>().color = new Color(0.05f, 0.04f, 0.10f, 0.97f);
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.sizeDelta        = new Vector2(1100, 700);
        bgRect.anchoredPosition = Vector2.zero;

        // Accent top bar
        GameObject topBarGO = new GameObject("TopBar");
        topBarGO.transform.SetParent(bgGO.transform, false);
        topBarGO.AddComponent<Image>().color = new Color(0.47f, 0.35f, 0.85f, 1f);
        RectTransform topBarRect = topBarGO.GetComponent<RectTransform>();
        topBarRect.anchorMin        = new Vector2(0, 1);
        topBarRect.anchorMax        = new Vector2(1, 1);
        topBarRect.pivot            = new Vector2(0.5f, 1);
        topBarRect.sizeDelta        = new Vector2(0, 5);
        topBarRect.anchoredPosition = Vector2.zero;

        // Title
        CreateText(bgGO.transform, "Title", "CalmspaceVR",
            new Vector2(0, 150), new Vector2(860, 110), 68, new Color(0.95f, 0.93f, 1f), FontStyles.Bold);

        // Tagline
        CreateText(bgGO.transform, "Tagline", "Your peaceful VR space",
            new Vector2(0, 90), new Vector2(860, 40), 22, new Color(0.65f, 0.60f, 0.80f), FontStyles.Normal);

        // Divider line
        GameObject dividerGO = new GameObject("Divider");
        dividerGO.transform.SetParent(bgGO.transform, false);
        dividerGO.AddComponent<Image>().color = new Color(0.47f, 0.35f, 0.85f, 0.4f);
        RectTransform divRect = dividerGO.GetComponent<RectTransform>();
        divRect.sizeDelta        = new Vector2(700, 1);
        divRect.anchoredPosition = new Vector2(0, 48);

        // Controls heading
        CreateText(bgGO.transform, "ControlsHeading", "CONTROLS",
            new Vector2(0, 18), new Vector2(1040, 36), 16, new Color(0.47f, 0.35f, 0.85f, 1f), FontStyles.Bold);

        // Controls text
        CreateText(bgGO.transform, "Controls",
            "Press  B  —  Open / Close music player",
            new Vector2(0, -40), new Vector2(1020, 60), 28, new Color(0.75f, 0.72f, 0.88f), FontStyles.Normal);

        // Start button
        GameObject startBtnGO = CreateButton(bgGO.transform, "StartButton", "Begin",
            new Vector2(0, -160), new Vector2(220, 68), new Color(0.47f, 0.35f, 0.85f, 1f), 26);
        startBtnGO.AddComponent<ButtonPokeEffect>();

        // StartScreen script
        GameObject screenGO = new GameObject("StartScreen");
        StartScreen startScreen = screenGO.AddComponent<StartScreen>();

        SerializedObject so = new SerializedObject(startScreen);
        so.FindProperty("startCanvas").objectReferenceValue  = canvas;
        so.FindProperty("canvasGroup").objectReferenceValue  = canvasGO.GetComponent<CanvasGroup>();
        so.ApplyModifiedProperties();

        UnityEventTools.AddVoidPersistentListener(startBtnGO.GetComponent<Button>().onClick, startScreen.OnStartClicked);

        // EventSystem (in case Setup Music Player wasn't run first)
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<XRUIInputModule>();
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = screenGO;
        Debug.Log("Start Screen setup complete! Assign XR Camera to the StartScreen component in the Inspector.");
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    static GameObject CreateButton(Transform parent, string name, string label,
        Vector2 position, Vector2 size, Color bgColor, int fontSize)
    {
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent, false);
        btnGO.AddComponent<Image>().color = bgColor;

        Button btn = btnGO.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor      = bgColor;
        cb.highlightedColor = BtnHighlight;
        cb.pressedColor     = BtnPressed;
        cb.selectedColor    = bgColor;
        btn.colors = cb;

        RectTransform rect = btnGO.GetComponent<RectTransform>();
        rect.sizeDelta        = size;
        rect.anchoredPosition = position;

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(btnGO.transform, false);
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = TextPrimary;
        RectTransform tr = textGO.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.sizeDelta = Vector2.zero; tr.anchoredPosition = Vector2.zero;

        return btnGO;
    }

    static GameObject CreateText(Transform parent, string name, string content,
        Vector2 position, Vector2 size, int fontSize, Color color, FontStyles style)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = content;
        tmp.fontSize  = fontSize;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = color;
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta        = size;
        rect.anchoredPosition = position;
        return go;
    }

    static GameObject CreateSlider(Transform parent, Vector2 position, Vector2 size)
    {
        GameObject sliderGO = new GameObject("VolumeSlider");
        sliderGO.transform.SetParent(parent, false);
        RectTransform sliderRect = sliderGO.AddComponent<RectTransform>();
        sliderRect.sizeDelta        = size;
        sliderRect.anchoredPosition = position;

        Slider slider = sliderGO.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value    = 1f;

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderGO.transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = SliderBg;
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin        = new Vector2(0, 0.25f);
        bgRect.anchorMax        = new Vector2(1, 0.75f);
        bgRect.sizeDelta        = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;

        // Fill area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGO.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin        = new Vector2(0, 0.25f);
        fillAreaRect.anchorMax        = new Vector2(1, 0.75f);
        fillAreaRect.sizeDelta        = new Vector2(-20, 0);
        fillAreaRect.anchoredPosition = new Vector2(-5, 0);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.47f, 0.35f, 0.85f, 1f);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.sizeDelta        = Vector2.zero;
        fillRect.anchorMin        = Vector2.zero;
        fillRect.anchorMax        = new Vector2(1, 1);
        fillRect.anchoredPosition = Vector2.zero;

        // Handle
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderGO.transform, false);
        RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin        = Vector2.zero;
        handleAreaRect.anchorMax        = Vector2.one;
        handleAreaRect.sizeDelta        = new Vector2(-20, 0);
        handleAreaRect.anchoredPosition = Vector2.zero;

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = TextPrimary;
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 20);

        slider.fillRect   = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImg;

        return sliderGO;
    }
}
