using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StartScreen : MonoBehaviour
{
    public Canvas startCanvas;
    public CanvasGroup canvasGroup;
    public float distanceFromHead = 2.0f;
    public float fadeDuration = 0.6f;

    [Header("XR Camera")]
    public Transform xrCamera;

    void Start()
    {
        if (xrCamera == null && Camera.main != null)
            xrCamera = Camera.main.transform;

        if (xrCamera != null && startCanvas != null)
        {
            Vector3 forward = xrCamera.forward;
            forward.y = 0;
            if (forward.magnitude < 0.01f) forward = xrCamera.forward;
            forward.Normalize();

            startCanvas.transform.position = xrCamera.position + forward * distanceFromHead;
            startCanvas.transform.rotation = Quaternion.LookRotation(forward);
        }
    }

    public void OnStartClicked()
    {
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            if (canvasGroup != null) canvasGroup.alpha = 1f - t;
            yield return null;
        }
        startCanvas.gameObject.SetActive(false);
    }
}
