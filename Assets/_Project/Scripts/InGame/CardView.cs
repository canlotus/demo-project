using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardView : MonoBehaviour, IPointerClickHandler
{
    [Header("UI")]
    [SerializeField] private GameObject frontRoot;
    [SerializeField] private GameObject backRoot;
    [SerializeField] private Image frontImage;
    [SerializeField] private Graphic[] tintTargets;

    [Header("Optional (recommended)")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Flip")]
    [SerializeField] private float flipDuration = 0.14f;

    public int CellIndex { get; private set; }
    public int CardId { get; private set; }
    public bool IsFaceUp { get; private set; }
    public bool IsMatched { get; private set; }
    public bool IsBusy { get; private set; }

    public event Action<CardView> Clicked;

    private Color[] _baseColors;

    private void Awake()
    {
        if (tintTargets != null && tintTargets.Length > 0)
        {
            _baseColors = new Color[tintTargets.Length];
            for (int i = 0; i < tintTargets.Length; i++)
                _baseColors[i] = tintTargets[i] != null ? tintTargets[i].color : Color.white;
        }
        else
        {
            _baseColors = Array.Empty<Color>();
        }
    }

    public void Init(int cellIndex, int cardId, Sprite faceSprite)
    {
        CellIndex = cellIndex;
        CardId = cardId;

        if (frontImage != null)
            frontImage.sprite = faceSprite;

        SetMatched(false);
        SetFaceUp(true, instant: true); 
        ResetTint();

        // ensure visible
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        var btn = GetComponent<Button>();
        if (btn != null) btn.interactable = true;

        gameObject.SetActive(true);
    }

    public void SetMatched(bool matched)
    {
        IsMatched = matched;
    }

    public void SetFaceUp(bool faceUp, bool instant)
    {
        IsFaceUp = faceUp;

        if (frontRoot != null) frontRoot.SetActive(faceUp);
        if (backRoot != null) backRoot.SetActive(!faceUp);

        if (instant)
        {
            var s = transform.localScale;
            s.x = 1f;
            transform.localScale = s;
        }
    }

    public IEnumerator FlipTo(bool faceUp)
    {
        if (IsBusy) yield break;
        if (IsFaceUp == faceUp) yield break;

        IsBusy = true;
        if (AudioManager.I != null) AudioManager.I.PlayFlip();
        float half = flipDuration * 0.5f;

        yield return ScaleX(1f, 0f, half);
        SetFaceUp(faceUp, instant: true);
        yield return ScaleX(0f, 1f, half);

        IsBusy = false;
    }

    public IEnumerator FlashMismatch(float duration)
    {
        if (IsBusy) yield break;
        IsBusy = true;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);

            float pulse = 1f - Mathf.Abs(2f * k - 1f);
            Color c = Color.Lerp(Color.white, new Color(1f, 0.35f, 0.35f, 1f), pulse);

            ApplyTint(c);
            yield return null;
        }

        ResetTint();
        IsBusy = false;
    }

    public void MakeEmptyInstant()
    {
        IsMatched = true;

        if (frontRoot != null) frontRoot.SetActive(false);
        if (backRoot != null) backRoot.SetActive(false);

        ResetTint();

        var btn = GetComponent<Button>();
        if (btn != null) btn.interactable = false;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }

    public IEnumerator VanishToEmpty(float duration)
    {
        if (IsBusy) yield break;
        IsBusy = true;

        float t = 0f;

        Vector3 fromScale = transform.localScale;
        Vector3 toScale = fromScale * 0.0f;

        float fromAlpha = (canvasGroup != null) ? canvasGroup.alpha : 1f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = duration <= 0f ? 1f : Mathf.Clamp01(t / duration);

            transform.localScale = Vector3.Lerp(fromScale, toScale, k);

            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(fromAlpha, 0f, k);

            yield return null;
        }
        transform.localScale = fromScale;

        MakeEmptyInstant();

        IsBusy = false;
    }

    private IEnumerator ScaleX(float from, float to, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = dur <= 0f ? 1f : Mathf.Clamp01(t / dur);

            float x = Mathf.Lerp(from, to, k);
            var s = transform.localScale;
            s.x = x;
            transform.localScale = s;
            yield return null;
        }

        var s2 = transform.localScale;
        s2.x = to;
        transform.localScale = s2;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsMatched) return;
        if (IsBusy) return;
        if (canvasGroup != null && !canvasGroup.blocksRaycasts) return;

        Clicked?.Invoke(this);
    }

    private void ApplyTint(Color c)
    {
        if (tintTargets == null) return;
        for (int i = 0; i < tintTargets.Length; i++)
            if (tintTargets[i] != null) tintTargets[i].color = c;
    }

    private void ResetTint()
    {
        if (tintTargets == null || _baseColors == null) return;
        for (int i = 0; i < tintTargets.Length; i++)
            if (tintTargets[i] != null) tintTargets[i].color = _baseColors[i];
    }
}
