using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioClip bgMusic;
    public AudioClip coinSound;
    public float musicStartTime = 16f;
    public float coinStartOffset = 0f;
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float coinVolume = 1f;

    AudioSource musicSource;
    AudioSource[] coinSources;
    int coinIndex;
    bool musicStarted;
    AudioClip trimmedCoinClip;

    void Awake()
    {
        Instance = this;

        // Background music source
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.clip = bgMusic;
        musicSource.volume = musicVolume;
        musicSource.loop = false;

        PrepareCoinAudio();

        // Ready-made audio sources for snappy, zero-latency playback
        AudioClip clipToAssign = trimmedCoinClip != null ? trimmedCoinClip : coinSound;
        coinSources = new AudioSource[8];
        for (int i = 0; i < coinSources.Length; i++)
        {
            coinSources[i] = gameObject.AddComponent<AudioSource>();
            coinSources[i].playOnAwake = false;
            coinSources[i].clip = clipToAssign;
            coinSources[i].volume = coinVolume;
        }
    }

    void Start()
    {
        // Start background music promptly
        if (bgMusic != null && !musicStarted)
        {
            StartMusic();
        }
    }

    void Update()
    {
        if (bgMusic == null) return;

        musicSource.volume = musicVolume;

        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
        {
            if (musicSource.isPlaying) musicSource.Stop();
            return;
        }

        if (!musicSource.isPlaying)
            StartMusic();
    }

    public void StartMusic()
    {
        if (bgMusic == null) return;

        musicStarted = true;
        musicSource.clip = bgMusic;
        if (bgMusic.length > 0f)
            musicSource.time = Mathf.Clamp(musicStartTime, 0f, Mathf.Max(0f, bgMusic.length - 0.5f));
        musicSource.Play();
    }

    void PrepareCoinAudio()
    {
        if (coinSound == null) return;

        try
        {
            coinSound.LoadAudioData();
            int totalSamples = coinSound.samples * coinSound.channels;
            float[] samples = new float[totalSamples];

            if (coinSound.GetData(samples, 0))
            {
                // Find first audible sample (threshold)
                int startIndex = -1;
                float threshold = 0.02f;
                for (int i = 0; i < samples.Length; i++)
                {
                    if (Mathf.Abs(samples[i]) > threshold)
                    {
                        startIndex = i - (i % coinSound.channels);
                        break;
                    }
                }

                // If clip has leading silence (> 0.05s), auto-trim it
                int minSilenceSamples = (int)(coinSound.frequency * coinSound.channels * 0.05f);
                if (startIndex >= minSilenceSamples)
                {
                    int endIndex = samples.Length;
                    for (int i = samples.Length - 1; i >= startIndex; i--)
                    {
                        if (Mathf.Abs(samples[i]) > 0.005f)
                        {
                            int tailSamples = (int)(coinSound.frequency * coinSound.channels * 0.1f);
                            endIndex = Mathf.Min(samples.Length, i + tailSamples);
                            break;
                        }
                    }

                    int trimmedLength = endIndex - startIndex;
                    int sampleCount = trimmedLength / coinSound.channels;

                    if (sampleCount > 0)
                    {
                        float[] trimmedData = new float[trimmedLength];
                        System.Array.Copy(samples, startIndex, trimmedData, 0, trimmedLength);

                        trimmedCoinClip = AudioClip.Create(coinSound.name + "_Trimmed", sampleCount, coinSound.channels, coinSound.frequency, false);
                        trimmedCoinClip.SetData(trimmedData, 0);
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Auto-trim coin audio skipped: " + e.Message);
        }

        if (trimmedCoinClip == null)
            trimmedCoinClip = coinSound;
    }

    public void PlayCoin()
    {
        AudioClip clipToPlay = trimmedCoinClip != null ? trimmedCoinClip : coinSound;
        if (clipToPlay == null) return;

        AudioSource s = coinSources[coinIndex];
        coinIndex = (coinIndex + 1) % coinSources.Length;

        s.volume = coinVolume;

        // If manual offset is specified on untrimmed clip, seek before playing
        if (coinStartOffset > 0.01f && clipToPlay == coinSound)
        {
            s.Stop();
            s.time = Mathf.Clamp(coinStartOffset, 0f, clipToPlay.length - 0.01f);
            s.Play();
        }
        else
        {
            s.PlayOneShot(clipToPlay, coinVolume);
        }
    }
}
