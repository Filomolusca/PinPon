using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;

    [Range(0f,1f)]
    public float volume = 1f;

    [Range(.5f,2f)]
    public float pitch = 1f;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Libraries")]
    public Sound[] music;
    public Sound[] sfx;

    [Header("Settings")]
    public int initialPoolSize = 5;
    public bool allowPoolExpansion = true;

    Dictionary<string, Sound> musicLookup;
    Dictionary<string, Sound> sfxLookup;

    AudioSource musicSource;
    List<AudioSource> sfxPool;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildLookups();
        CreateSources();
    }

    void BuildLookups()
    {
        musicLookup = new Dictionary<string, Sound>();
        sfxLookup = new Dictionary<string, Sound>();

        foreach (var s in music)
            musicLookup[s.name] = s;

        foreach (var s in sfx)
            sfxLookup[s.name] = s;
    }

    void CreateSources()
    {
        // music
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.spatialBlend = 0;

        // pool
        sfxPool = new List<AudioSource>();

        for (int i = 0; i < initialPoolSize; i++)
            sfxPool.Add(CreateNewSource());
    }

    AudioSource CreateNewSource()
    {
        AudioSource src = gameObject.AddComponent<AudioSource>();
        src.spatialBlend = 0;
        src.playOnAwake = false;

        return src;
    }

    AudioSource GetFreeSource()
    {
        for (int i = 0; i < sfxPool.Count; i++)
        {
            if (!sfxPool[i].isPlaying)
                return sfxPool[i];
        }

        if (allowPoolExpansion)
        {
            AudioSource src = CreateNewSource();
            sfxPool.Add(src);
            return src;
        }

        return sfxPool[0];
    }

    // MUSIC
    public void PlayMusic(string name)
    {
        if (!musicLookup.TryGetValue(name, out Sound s))
        {
            Debug.LogWarning("Music not found: " + name);
            return;
        }

        musicSource.Stop();
        musicSource.clip = s.clip;
        musicSource.volume = s.volume;
        musicSource.pitch = s.pitch;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    // SFX
    public void PlaySFX(string name)
    {
        if (!sfxLookup.TryGetValue(name, out Sound s))
        {
            Debug.LogWarning("SFX not found: " + name);
            return;
        }

        AudioSource src = GetFreeSource();

        src.Stop();
        src.clip = s.clip;
        src.volume = s.volume;
        src.pitch = s.pitch;
        src.Play();
    }

    // random pitch
    public void PlaySFX(string name, float pitchVariance)
    {
        if (!sfxLookup.TryGetValue(name, out Sound s))
        {
            Debug.LogWarning("SFX not found: " + name);
            return;
        }

        AudioSource src = GetFreeSource();

        src.Stop();
        src.clip = s.clip;
        src.volume = s.volume;
        src.pitch = s.pitch + Random.Range(-pitchVariance, pitchVariance);
        src.Play();
    }
}