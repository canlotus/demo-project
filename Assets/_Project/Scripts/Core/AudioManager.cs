using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I { get; private set; }

    [Header("Clips")]
    [SerializeField] private AudioClip sfxMatch;
    [SerializeField] private AudioClip sfxMismatch;
    [SerializeField] private AudioClip sfxWin;
    [SerializeField] private AudioClip sfxButton;

    [Header("Settings")]
    [Range(0f, 1f)][SerializeField] private float volume = 1f;

    private AudioSource _source;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);

        _source = GetComponent<AudioSource>();
        if (_source == null) _source = gameObject.AddComponent<AudioSource>();
        _source.playOnAwake = false;
    }

    public void PlayMatch()   => Play(sfxMatch);
    public void PlayMismatch()=> Play(sfxMismatch);
    public void PlayWin()     => Play(sfxWin);
    public void PlayButton()  => Play(sfxButton);

    public void Play(AudioClip clip)
    {
        if (clip == null) return;
        _source.PlayOneShot(clip, volume);
    }
}
