using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class DelayedHoverDescription :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Description")]
    [SerializeField] private GameObject descriptionObject;

    [Header("Hover Delay")]
    [SerializeField] private float hoverDelay = 0.5f;

    private Coroutine hoverCoroutine;
    private bool isPointerInside;

    private void Awake()
    {
        HideDescription();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerInside = true;

        StopHoverCoroutine();

        hoverCoroutine =
            StartCoroutine(ShowDescriptionAfterDelay());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerInside = false;

        StopHoverCoroutine();
        HideDescription();
    }

    private IEnumerator ShowDescriptionAfterDelay()
    {
        yield return new WaitForSecondsRealtime(hoverDelay);

        if (isPointerInside && descriptionObject != null)
        {
            descriptionObject.SetActive(true);
        }

        hoverCoroutine = null;
    }

    private void StopHoverCoroutine()
    {
        if (hoverCoroutine == null)
        {
            return;
        }

        StopCoroutine(hoverCoroutine);
        hoverCoroutine = null;
    }

    private void HideDescription()
    {
        if (descriptionObject != null)
        {
            descriptionObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        isPointerInside = false;

        StopHoverCoroutine();
        HideDescription();
    }
}