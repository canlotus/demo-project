using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class ButtonSfx : MonoBehaviour
{
    [SerializeField] private AudioClip overrideClip;

    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_button == null)
        {

            enabled = false;
            return;
        }

        _button.onClick.AddListener(Play);
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(Play);
    }

    private void Play()
    {
        if (overrideClip != null)
        {
            if (AudioManager.I != null)
            {
                AudioManager.I.Play(overrideClip);
            }
            else
            {
                var src = GetOrCreateLocalSource();
                src.PlayOneShot(overrideClip, volume);
            }

            return;
        }

        if (AudioManager.I != null)
            AudioManager.I.PlayButton();
    }

    private AudioSource GetOrCreateLocalSource()
    {
        var src = GetComponent<AudioSource>();
        if (src == null) src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
        return src;
    }
}
