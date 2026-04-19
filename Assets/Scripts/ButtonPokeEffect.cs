using UnityEngine;
using UnityEngine.EventSystems;

// Attach to any UI button to give a physical press-down feel when poked with a finger
public class ButtonPokeEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        LeanScale(originalScale * 1.06f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        LeanScale(originalScale);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        LeanScale(originalScale * 0.90f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        LeanScale(originalScale);
    }

    void LeanScale(Vector3 target)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(target, 0.07f));
    }

    System.Collections.IEnumerator ScaleTo(Vector3 target, float duration)
    {
        Vector3 start = transform.localScale;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.localScale = Vector3.Lerp(start, target, t);
            yield return null;
        }
        transform.localScale = target;
    }
}
