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

    [Header("Flip")]
    [SerializeField] private float flipDuration = 0.14f;

    public int CardId { get; private set; }
    public bool IsFaceUp { get; private set; }
    public bool IsMatched { get; private set; }

    public event Action<CardView> Clicked;

    private bool _isFlipping;

    public void Init(int cardId)
    {
        CardId = cardId;
        SetFaceUp(false, instant: true);
        IsMatched = false;
    }

    public void SetMatched(bool matched)
    {
        IsMatched = matched;
        // matched olunca açık kalsın
        if (matched) SetFaceUp(true, instant: true);
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

    public void Flip()
    {
        if (_isFlipping) return;
        StartCoroutine(FlipRoutine());
    }

    private IEnumerator FlipRoutine()
    {
        _isFlipping = true;

        float half = flipDuration * 0.5f;

        // 1 -> 0
        yield return ScaleX(1f, 0f, half);

        // swap
        SetFaceUp(!IsFaceUp, instant: true);

        // 0 -> 1
        yield return ScaleX(0f, 1f, half);

        _isFlipping = false;
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
        Clicked?.Invoke(this);
    }
}
