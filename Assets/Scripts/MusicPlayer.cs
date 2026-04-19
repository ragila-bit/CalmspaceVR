using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using TMPro;

public class MusicPlayer : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public List<AudioClip> tracks = new List<AudioClip>();

    [Header("UI")]
    public Canvas musicCanvas;
    public TextMeshProUGUI trackNameText;
    public TextMeshProUGUI trackCounterText;
    public Button playPauseButton;
    public TextMeshProUGUI playPauseButtonText;
    public Slider volumeSlider;

    [Header("Panel Position")]
    public float distanceFromHead = 1.2f;
    public float heightOffset = 0f;

    [Header("XR Camera")]
    public Transform xrCamera;

    private const float PalmHoldTime = 0.5f;
    private int currentIndex = 0;
    private bool bWasPressed = false;
    private XRHandSubsystem handSubsystem;
    private float palmTimer = 0f;
    private bool palmTriggered = false;

    void Start()
    {
        if (musicCanvas != null)
            musicCanvas.gameObject.SetActive(false);

        if (xrCamera == null && Camera.main != null)
            xrCamera = Camera.main.transform;

        if (volumeSlider != null)
        {
            volumeSlider.value = audioSource != null ? audioSource.volume : 1f;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        if (tracks.Count > 0)
            LoadTrack(0);

        StartCoroutine(AutoAdvance());
    }

    IEnumerator AutoAdvance()
    {
        while (true)
        {
            // Wait until something is playing
            yield return new WaitUntil(() => audioSource != null && audioSource.isPlaying);
            // Wait until it stops
            yield return new WaitWhile(() => audioSource != null && audioSource.isPlaying);
            // Small buffer to avoid false triggers (e.g. pausing)
            yield return new WaitForSeconds(0.1f);
            if (audioSource != null && !audioSource.isPlaying)
                NextTrack();
        }
    }

    void Update()
    {
        CheckBButton();
        CheckPalmGesture();
    }

    // ── B button ─────────────────────────────────────────────────────────────

    void CheckBButton()
    {
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);
        if (devices.Count == 0) return;

        devices[0].TryGetFeatureValue(CommonUsages.secondaryButton, out bool bPressed);
        if (bPressed && !bWasPressed)
            TogglePanelInFront();
        bWasPressed = bPressed;
    }

    // ── Palm wrist gesture ───────────────────────────────────────────────────

    void CheckPalmGesture()
    {
        // Find subsystem lazily — hand tracking initialises after Start()
        if (handSubsystem == null || !handSubsystem.running)
        {
            var subsystems = new List<XRHandSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);
            handSubsystem = subsystems.Count > 0 ? subsystems[0] : null;
            if (handSubsystem == null) return;
        }

        XRHand rightHand = handSubsystem.rightHand;
        if (!rightHand.isTracked) { ResetPalmTimer(); return; }

        // Try palm joint, fall back to wrist
        bool hasPose = rightHand.GetJoint(XRHandJointID.Palm).TryGetPose(out Pose jointPose);
        if (!hasPose)
            hasPose = rightHand.GetJoint(XRHandJointID.Wrist).TryGetPose(out jointPose);
        if (!hasPose) { ResetPalmTimer(); return; }

        Transform cam = xrCamera != null ? xrCamera : Camera.main?.transform;
        if (cam == null) return;

        Vector3 toCamera   = (cam.position - jointPose.position).normalized;
        Vector3 palmNormal = jointPose.rotation * Vector3.up;
        bool palmFacingCamera = Vector3.Dot(palmNormal, toCamera) > 0.5f;

        if (palmFacingCamera && !palmTriggered)
        {
            palmTimer += Time.deltaTime;
            if (palmTimer >= PalmHoldTime)
            {
                palmTriggered = true;
                palmTimer = 0f;
                TogglePanelAtWrist(jointPose);
            }
        }
        else if (!palmFacingCamera)
        {
            ResetPalmTimer();
        }
    }

    void ResetPalmTimer()
    {
        palmTimer = 0f;
        palmTriggered = false;
    }

    // ── Panel positioning ────────────────────────────────────────────────────

    public void TogglePanelInFront()
    {
        if (musicCanvas == null) return;
        bool show = !musicCanvas.gameObject.activeSelf;
        musicCanvas.gameObject.SetActive(show);

        if (show)
        {
            Transform cam = xrCamera != null ? xrCamera : Camera.main?.transform;
            if (cam == null) return;

            Vector3 forward = cam.forward;
            forward.y = 0;
            if (forward.magnitude < 0.01f) forward = cam.forward;
            forward.Normalize();

            musicCanvas.transform.position = cam.position
                + forward * distanceFromHead
                + Vector3.up * heightOffset;
            musicCanvas.transform.rotation = Quaternion.LookRotation(forward);
        }
    }

    void TogglePanelAtWrist(Pose palmPose)
    {
        if (musicCanvas == null) return;
        bool show = !musicCanvas.gameObject.activeSelf;
        musicCanvas.gameObject.SetActive(show);

        if (show)
        {
            Transform cam = xrCamera != null ? xrCamera : Camera.main?.transform;
            Vector3 pos = palmPose.position + Vector3.up * 0.12f;
            Vector3 lookDir = cam != null ? (cam.position - pos).normalized : Vector3.forward;

            musicCanvas.transform.position = pos;
            musicCanvas.transform.rotation = Quaternion.LookRotation(lookDir);
        }
    }

    // Close button calls this
    public void TogglePanel()
    {
        if (musicCanvas == null) return;
        musicCanvas.gameObject.SetActive(!musicCanvas.gameObject.activeSelf);
    }

    // ── Audio ────────────────────────────────────────────────────────────────

    public void SetVolume(float value)
    {
        if (audioSource != null) audioSource.volume = value;
    }

    public void PlayPause()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            if (playPauseButtonText != null) playPauseButtonText.text = "▶";
        }
        else
        {
            audioSource.Play();
            if (playPauseButtonText != null) playPauseButtonText.text = "▐▐";
        }
    }

    public void NextTrack()
    {
        if (tracks.Count == 0) return;
        currentIndex = (currentIndex + 1) % tracks.Count;
        LoadTrack(currentIndex);
        audioSource.Play();
        if (playPauseButtonText != null) playPauseButtonText.text = "▐▐";
    }

    public void PreviousTrack()
    {
        if (tracks.Count == 0) return;
        currentIndex = (currentIndex - 1 + tracks.Count) % tracks.Count;
        LoadTrack(currentIndex);
        audioSource.Play();
        if (playPauseButtonText != null) playPauseButtonText.text = "▐▐";
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void LoadTrack(int index)
    {
        if (index < 0 || index >= tracks.Count) return;
        audioSource.clip = tracks[index];
        if (trackNameText != null)
            trackNameText.text = tracks[index].name;
        if (trackCounterText != null)
            trackCounterText.text = $"{index + 1}  /  {tracks.Count}";
    }
}
