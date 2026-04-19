using System.Collections;
using TMPro;
using UnityEngine;

public class BreathingPrompt : MonoBehaviour
{
    public TextMeshProUGUI promptText;

    public float breatheInTime = 6f;
    public float breatheOutTime = 8f;
    public float holdTime = 4f;

    private void Start()
    {
        StartCoroutine(BreathingLoop());
    }

    IEnumerator BreathingLoop()
    {
        while (true)
        {
            promptText.text = "Breathe In";
            yield return new WaitForSeconds(breatheInTime);

            promptText.text = "Hold";
            yield return new WaitForSeconds(holdTime);

            promptText.text = "Breathe Out";
            yield return new WaitForSeconds(breatheOutTime);

            promptText.text = "Hold";
            yield return new WaitForSeconds(holdTime);
        }
    }
}