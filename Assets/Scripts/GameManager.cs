using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using PinPon; // Namespace do PinPonPlayerController

public class GameManager : MonoBehaviour
{
    [Header("Setup da Partida")]
    public GameObject pinPrefab; // Prefab do jogador 1
    public GameObject ponPrefab; // Prefab do jogador 2
    public GameObject icebergPinPrefab;
    public GameObject icebergPonPrefab;
    public Transform player1SpawnPoint;
    public Transform iceberg1SpawnPoint;
    public Transform player2SpawnPoint;
    public Transform iceberg2SpawnPoint;

    [Header("Controle de UI e Jogo")]
    public GameObject pauseMenu;
    public GameObject victoryScreen;
    public GameObject RoundScreen;
    public Score score;
    public Image fadeImage;
    public float fadeDuration = 0.4f;
    public GameObject snowExplosionEffect;
    
    [Header("Outros Sistemas")]
    public AudioSource ostMatch;
    public Transform spawnSeagullPin;
    public Transform spawnSeagullPon;
    public GameObject seagullPinPrefab;
    public GameObject seagullPonPrefab;
    public AudioSource sfxSource;
    public AudioClip seagullHitSound;
    public snowman snowman;

    private bool isPaused = false;
    private List<GameObject> activePlayers = new List<GameObject>();
    private List<Vector2> initialPositions = new List<Vector2>();
    private List<GameObject> spawnedIcebergs = new List<GameObject>();
    private static bool isFirstRound = true;

    void Start()
    {
        // Garante que o estado inicial está limpo
        isFirstRound = true;
        pauseMenu.SetActive(false);
        victoryScreen.SetActive(false);
        Time.timeScale = 1;

        // Carrega scores salvos
        score.ScorePin = PlayerPrefs.GetInt("ScorePin", 0);
        score.ScorePon = PlayerPrefs.GetInt("ScorePon", 0);
        score.UpdateScoreTextPin();
        score.UpdateScoreTextPon();

        // Orquestra a criação dos jogadores e da partida
        SpawnAndSetupPlayers();
        BeginMatch();
    }
    
    private void SpawnAndSetupPlayers()
    {
        activePlayers.Clear();
        initialPositions.Clear();
        
        if (PlayerConfigurationManager.Instance == null)
        {
            Debug.LogError("PlayerConfigurationManager.Instance não encontrado!");
            return;
        }

        var playerConfigs = PlayerConfigurationManager.Instance.PlayerConfigs;
        Debug.Log($"Encontrado {playerConfigs.Count} jogador(es) configurado(s).");

        foreach (var config in playerConfigs)
        {
            GameObject playerPrefab;
            GameObject icebergPrefab;
            Transform playerSpawn;
            Transform icebergSpawn;

            if (config.PlayerIndex == 0)
            {
                playerPrefab = pinPrefab;
                icebergPrefab = icebergPinPrefab;
                playerSpawn = player1SpawnPoint;
                icebergSpawn = iceberg1SpawnPoint;
            }
            else
            {
                playerPrefab = ponPrefab;
                icebergPrefab = icebergPonPrefab;
                playerSpawn = player2SpawnPoint;
                icebergSpawn = iceberg2SpawnPoint;
            }

            // --- Instanciação e Conexão ---
            GameObject icebergGO = Instantiate(icebergPrefab, icebergSpawn.position, icebergSpawn.rotation);
            spawnedIcebergs.Add(icebergGO);    
            iceberg currentIceberg = icebergGO.GetComponent<iceberg>();

            PinPonPlayerController playerController;

            // Se o jogador já foi instanciado antes (em uma rodada anterior), apenas o reposicionamos.
            if (config.PlayerInstance != null)
            {
                Debug.Log($"Jogador {config.PlayerIndex} já existe. Reposicionando.");
                config.PlayerInstance.transform.position = playerSpawn.position;
                playerController = config.PlayerInstance.GetComponent<PinPonPlayerController>();
            }
            // Se for a primeira vez, instanciamos o jogador.
            else
            {
                Debug.Log($"Instanciando novo jogador para o índice {config.PlayerIndex}.");
                PlayerInput newPlayer = PlayerInput.Instantiate(
                    prefab: playerPrefab,
                    playerIndex: config.PlayerIndex,
                    controlScheme: config.Input.currentControlScheme,
                    splitScreenIndex: -1,
                    pairWithDevice: config.Input.devices[0]
                );
                newPlayer.gameObject.transform.position = playerSpawn.position;
                playerController = newPlayer.gameObject.GetComponent<PinPonPlayerController>();
                
                // Armazena a instância no objeto de configuração para que não seja criada novamente
                config.PlayerInstance = newPlayer.gameObject;
            }

            // Conecta as referências entre o jogador (novo ou existente) e o novo iceberg.
            if (playerController != null && currentIceberg != null)
            {
                currentIceberg.AssignPlayer(playerController, this, score);
                playerController.assignedIceberg = currentIceberg;
                
                activePlayers.Add(playerController.gameObject);
                initialPositions.Add(playerSpawn.position);
                
                Debug.Log($"Jogador {playerController.PlayerIndex} ({playerPrefab.name}) e seu iceberg ({icebergPrefab.name}) foram configurados.");
            }
        }
    }

    public void BeginMatch()
    {
        Round();
    }

    public void Round()
    {
        Debug.Log("Round!");
        RoundScreen.SetActive(true);

        DisablePlayerInputs();
        Time.timeScale = 0f;
        StartCoroutine(WaitForSpace());
            
        if (!isFirstRound)
        {
            if (RoundScreen.TryGetComponent(out AudioSource audioSource)) audioSource.Play();
        }
        else
        {
            isFirstRound = false;
        }
    }

    private void DisablePlayerInputs()
    {
        var playerConfigs = PlayerConfigurationManager.Instance.PlayerConfigs;
        foreach (var config in playerConfigs)
        {
            if (config.PlayerInstance != null)
            {
                var playerInput = config.PlayerInstance.GetComponent<PlayerInput>();
                if (playerInput != null)
                {
                    // Usando "Player", que é o nome padrão do mapa de ações.
                    playerInput.actions.FindActionMap("Player").Disable();
                    Debug.Log($"Inputs desabilitados para o jogador {config.PlayerIndex}");
                }
            }
        }
    }

    private void EnablePlayerInputs()
    {
        var playerConfigs = PlayerConfigurationManager.Instance.PlayerConfigs;
        foreach (var config in playerConfigs)
        {
            if (config.PlayerInstance != null)
            {
                var playerInput = config.PlayerInstance.GetComponent<PlayerInput>();
                if (playerInput != null)
                {
                    playerInput.actions.FindActionMap("Player").Enable();
                    Debug.Log($"Inputs habilitados para o jogador {config.PlayerIndex}");
                }
            }
        }
    }

    private IEnumerator WaitForSpace()
    {
        yield return null; 
        while (!Input.GetButtonDown("Submit") && !Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }
        
        EnablePlayerInputs();
        if(ostMatch != null) ostMatch.Play();
        Debug.Log("Round Começou!");
        Time.timeScale = 1f;
        RoundScreen.SetActive(false);
    }

    // public void RestartGame()
    // {
    //     if(snowman != null && snowman.TryGetComponent(out Animator anim))
    //     {
    //         anim.SetBool("Throw", true);
    //     }
        
    //     for (int i = 0; i < activePlayers.Count; i++)
    //     {
    //         if(activePlayers[i] != null)
    //         {
    //             activePlayers[i].transform.position = initialPositions[i];
    //         }
    //     }
    //     Time.timeScale = 1f;
    //     if(snowman != null) snowman.RestartSnowman();
    // }
    public void RestartGame()
    {

    StartCoroutine(Fade(1f));
    
    // Reposiciona os jogadores para suas posições iniciais
    for (int i = 0; i < activePlayers.Count; i++)
    {
        if(activePlayers[i] != null)
        {
            activePlayers[i].transform.position = initialPositions[i];
        }
    }

    var balls = GameObject.FindGameObjectsWithTag("bola"); 
    foreach (var ball in balls)
    {
        Destroy(ball);
    }

    score.ballCounterValue = 1;
    
    Time.timeScale = 1f;

    // Reseta o snowman para ele poder lançar a bola de novo
    if(snowman != null) 
    {
        snowman.RestartSnowman();
    }

    }
    
    // public void ScorePoint()
    // {
    //     Debug.Log("ScorePoint! Scene Reload!");
    //     PlayerPrefs.SetInt("ScorePin", score.ScorePin);
    //     PlayerPrefs.SetInt("ScorePon", score.ScorePon);
        
    //     SceneManager.LoadScene(SceneManager.GetActiveScene().name);    
    // }
    public void ScorePoint()
{
    Debug.Log("ScorePoint! Resetting round.");
    PlayerPrefs.SetInt("ScorePin", score.ScorePin);
    PlayerPrefs.SetInt("ScorePon", score.ScorePon);

    // Verifica se alguém ganhou o jogo
    if (score.ScorePin >= 3 || score.ScorePon >= 3) // Supondo que 3 pontos vencem
    {
        GameOver();
    }
    else
    {
        ResetRound();
    }
}

public void ResetRound()
{
    // 1. Destrói os icebergs da rodada anterior
    foreach (var iceberg in spawnedIcebergs)
    {
        if (iceberg != null)
        {
            Destroy(iceberg);
        }
    }
    spawnedIcebergs.Clear();
    
    // 3. Recria os icebergs e reposiciona os jogadores
    // Como a cena não foi recarregada, a referência a 'config.PlayerInstance' ainda é válida.
    SpawnAndSetupPlayers();
    ResetSeagulls();
    // 4. Reinicia os outros elementos e começa a próxima rodada
    RestartGame(); // Este seu método já reposiciona os players e o snowman
    BeginMatch();  // Este seu método já mostra a tela "Round" e espera o jogador
}
    
    #region Métodos de Gerenciamento de Jogo (Pause, Game Over, etc.)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !victoryScreen.activeSelf && !RoundScreen.activeSelf)
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }
    public IEnumerator Fade(float targetAlpha)
    {
        snowExplosionEffect.SetActive(true);

        float startAlpha = fadeImage.color.a;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, newAlpha);
            yield return null;
        }

        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, targetAlpha);

        if (targetAlpha == 1f)
        {
            StartCoroutine(Fade(0f));
        }
        snowExplosionEffect.SetActive(false);
    }
    
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
    }

    public void GameOver()
    {
        RoundScreen.SetActive(false);
        if(pauseMenu != null) Destroy(pauseMenu);
        Time.timeScale = 0f;
        victoryScreen.SetActive(true);
    }

    public void MainMenu ()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    #endregion
    
    #region Lógica da Gaivota (Seagull)
    public void SeagullKilled(GameObject seagullInstance)
    {
        if (seagullInstance != null)
        {
            sfxSource.PlayOneShot(seagullHitSound);        
            StartCoroutine(DestroySeagull(seagullInstance));
        }
    }

    private IEnumerator DestroySeagull(GameObject seagullInstance)
    {
        if (seagullInstance != null)
        {
            string seagullTag = seagullInstance.tag;
            Destroy(seagullInstance);
            yield return new WaitForSeconds(5f);
            if (seagullTag == "seagullPin")
            {
                Instantiate(seagullPinPrefab, spawnSeagullPin.position, spawnSeagullPin.rotation);
            }
            else if (seagullTag == "seagullPon")
            {
                Instantiate(seagullPonPrefab, spawnSeagullPon.position, spawnSeagullPon.rotation);
            }
        }
    }
    public void ResetSeagulls()
    {
        // Encontra todas as gaivotas ativas na cena e as destrói
        var seagulls = GameObject.FindGameObjectsWithTag("seagullPin");
        foreach (var seagull in seagulls)
        {
            Destroy(seagull);
        }
        var seagullsPon = GameObject.FindGameObjectsWithTag("seagullPon");
        foreach (var seagull in seagullsPon)
        {
            Destroy(seagull);
        }

        // Reinstancia as gaivotas para a nova rodada
        Instantiate(seagullPinPrefab, spawnSeagullPin.position, spawnSeagullPin.rotation);
        Instantiate(seagullPonPrefab, spawnSeagullPon.position, spawnSeagullPon.rotation);
     }
    #endregion
}

