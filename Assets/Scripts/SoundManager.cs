using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;

[System.Serializable]
[Preserve]
public class Sound
{
    public string name;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1.0f;
    [Range(.1f, 3f)]
    public float pitch = 1.0f;
}

[Preserve]
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Audio Sources")]
    public AudioSource effectsSource;
    public AudioSource loopingEffectsSource;
    public AudioSource themeSource;

    [Header("Audio Clip Libraries")]
    public Sound[] musicThemes;
    public Sound[] soundEffects;

    [Header("Positional Audio Settings")]
    [Tooltip("Margem extra acima e abaixo da câmera para tocar sons. Aumente se os sons estiverem cortando muito cedo.")]
    public float verticalCullingPadding = 2.0f;

    private Camera mainCamera;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // CORREÇÃO CRÍTICA: Se a instância existente não tiver músicas (veio do Loader vazia)
            // e esta nova instância tiver músicas, substituímos a antiga pela nova.
            if ((instance.musicThemes == null || instance.musicThemes.Length == 0) && (this.musicThemes != null && this.musicThemes.Length > 0))
            {
                Destroy(instance.gameObject);
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return; // Importante: parar a execução aqui para não inicializar o objeto destruído
            }
        }

        // Garante que os arrays não sejam nulos para evitar ArgumentNullException
        if (musicThemes == null) musicThemes = new Sound[0];
        if (soundEffects == null) soundEffects = new Sound[0];

        if (loopingEffectsSource == null)
        {
            Debug.LogWarning("SoundManager: LoopingEffectsSource not assigned. Creating one automatically.");
            loopingEffectsSource = gameObject.AddComponent<AudioSource>();
        }

        mainCamera = Camera.main;
    }

    /// <summary>
    /// Toggles the master volume of the game based on the state in SettingsManager.
    /// This is called by the MuteToggleButton.
    /// </summary>
    public void ToggleMute()
    {
        // SettingsManager.IsSoundMuted has already been updated by the button before this is called.
        // We just need to apply the state.
        // AudioListener.volume = SettingsManager.IsSoundMuted ? 0f : 1f;
    }
    
    public bool IsPositionVisible(Vector3 worldPosition)
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("SoundManager: Main Camera not found for visibility check. Returning true as a fallback.");
                return true; // Retorna true como fallback para não silenciar sons incorretamente
            }
        }

        float camY = mainCamera.transform.position.y;
        float camHeight = mainCamera.orthographicSize;
        float topBound = camY + camHeight + verticalCullingPadding;
        float bottomBound = camY - camHeight - verticalCullingPadding;

        return worldPosition.y >= bottomBound && worldPosition.y <= topBound;
    }

    private Sound FindSound(string name)
    {
        if (soundEffects == null) return null; // Proteção extra
        Sound s = Array.Find(soundEffects, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("SoundManager: Sound '" + name + "' not found in soundEffects library!");
        }
        return s;
    }

    private Sound FindTheme(string name)
    {
        if (musicThemes == null) return null; // Proteção extra contra o erro ArgumentNullException
        Sound s = Array.Find(musicThemes, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("SoundManager: Theme '" + name + "' not found in musicThemes library!");
        }
        return s;
    }

    // --- Play Sound Effects ---

    /// <summary>
    /// Toca um som posicional, que só será ouvido se estiver dentro da visão da câmera.
    /// </summary>
    /// <param name="name">Nome do som</param>
    /// <param name="worldPosition">Posição do objeto que está emitindo o som</param>
    /// <param name="volumeScale">Escala de volume</param>
    public void PlaySound(string name, Vector3 worldPosition, float volumeScale = 1.0f)
    {
        if (mainCamera == null) 
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("SoundManager: Main Camera not found! Playing sound without culling.");
                PlaySound(name, volumeScale); // Toca o som de qualquer maneira para evitar erros silenciosos
                return;
            }
        }

        float camY = mainCamera.transform.position.y;
        float camHeight = mainCamera.orthographicSize;
        float topBound = camY + camHeight + verticalCullingPadding;
        float bottomBound = camY - camHeight - verticalCullingPadding;

        // Checa se a posição do som está dentro dos limites verticais da câmera
        if (worldPosition.y >= bottomBound && worldPosition.y <= topBound)
        {
            PlaySound(name, volumeScale);
        }
        // Se estiver fora dos limites, não faz nada.
    }

    /// <summary>
    /// Toca um som não-posicional (como um som de UI) que será sempre ouvido.
    /// </summary>
    public void PlaySound(string name, float volumeScale = 1.0f)
    {
        Sound s = FindSound(name);
        if (s != null)
        {
            effectsSource.pitch = s.pitch;
            effectsSource.PlayOneShot(s.clip, s.volume * volumeScale);
        }
    }

    public void PlaySound(int index, float volumeScale = 1.0f)
    {
        if (index >= 0 && index < soundEffects.Length)
        {
            Sound s = soundEffects[index];
            effectsSource.pitch = s.pitch;
            effectsSource.PlayOneShot(s.clip, s.volume * volumeScale);
        }
    }

    public void PlayLoopingEffect(string name)
    {
        Sound s = FindSound(name);
        if (s != null)
        {
            if (loopingEffectsSource.clip == s.clip && loopingEffectsSource.isPlaying) return;

            loopingEffectsSource.clip = s.clip;
            loopingEffectsSource.volume = s.volume;
            loopingEffectsSource.pitch = s.pitch;
            loopingEffectsSource.loop = true;
            loopingEffectsSource.Play();
        }
    }

    public void PlayLoopingEffect(int index)
    {
        if (index >= 0 && index < soundEffects.Length)
        {
            PlayLoopingEffect(soundEffects[index].name);
        }
    }

    public void StopLoopingEffect()
    {
        if (loopingEffectsSource.isPlaying)
        {
            loopingEffectsSource.Stop();
            loopingEffectsSource.clip = null;
        }
    }

    // --- Play Music Themes ---

    public void PlayTheme(string name)
    {
        Sound s = FindTheme(name);
        if (s != null)
        {
            if (themeSource.clip == s.clip && themeSource.isPlaying) return;

            themeSource.clip = s.clip;
            // Volume is now handled by the caller (e.g., GameManager) via FadeMusic
            themeSource.pitch = s.pitch;
            themeSource.loop = true;
            themeSource.Play();
        }
    }

    public void PlayTheme(int index)
    {
         if (index >= 0 && index < musicThemes.Length)
        {
            PlayTheme(musicThemes[index].name);
        }
    }

    public void FadeMusic(float targetVolume, float duration)
    {
        StartCoroutine(Fade(targetVolume, duration));
    }

    private IEnumerator Fade(float targetVolume, float duration)
    {
        float startVolume = themeSource.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            themeSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }
        themeSource.volume = targetVolume;
    }
}