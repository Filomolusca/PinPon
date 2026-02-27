using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    
    [Header("Outros Sistemas")]
    public AudioSource ostMatch;
    public Transform spawnSeagullPin;
    public Transform spawnSeagullPon;
    public GameObject seagullPinPrefab;
    public GameObject seagullPonPrefab;
    public snowman snowman;

    private bool isPaused = false;
    private List<GameObject> activePlayers = new List<GameObject>();
    private List<Vector2> initialPositions = new List<Vector2>();
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

            // Determina quais prefabs e spawn points usar baseado no PlayerIndex
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
            
            // 1. Instanciar Iceberg
            GameObject icebergGO = Instantiate(icebergPrefab, icebergSpawn.position, icebergSpawn.rotation);
            iceberg currentIceberg = icebergGO.GetComponent<iceberg>();

            // 2. Instanciar Jogador, garantindo que o dispositivo de entrada correto seja usado
            PlayerInput newPlayer = PlayerInput.Instantiate(
                prefab: playerPrefab,
                playerIndex: config.PlayerIndex,
                controlScheme: config.Input.currentControlScheme,
                splitScreenIndex: -1,
                pairWithDevice: config.Input.devices[0]
            );
            newPlayer.gameObject.transform.position = playerSpawn.position;
            PinPonPlayerController playerController = newPlayer.gameObject.GetComponent<PinPonPlayerController>();

            // 3. Conectar as referências entre eles
            if (playerController != null && currentIceberg != null)
            {
                currentIceberg.AssignPlayer(playerController, this, score);
                playerController.assignedIceberg = currentIceberg;
                
                activePlayers.Add(newPlayer.gameObject);
                initialPositions.Add(playerSpawn.position);
                
                Debug.Log($"Jogador {playerController.PlayerIndex} ({playerPrefab.name}) e seu iceberg ({icebergPrefab.name}) foram configurados.");
            }

            // 4. Destruir o GameObject "marcador" que veio do lobby
            Destroy(config.Input.gameObject);
        }

        // Limpa a lista de configurações pois os jogadores já foram criados
        playerConfigs.Clear();
    }

    public void BeginMatch()
    {
        Round();
    }

    public void Round()
    {
        Debug.Log("Round!");
        RoundScreen.SetActive(true);
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

    private IEnumerator WaitForSpace()
    {
        yield return null; 
        while (!Input.GetButtonDown("Submit") && !Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }
        
        if(ostMatch != null) ostMatch.Play();
        Debug.Log("Round Começou!");
        Time.timeScale = 1f;
        RoundScreen.SetActive(false);
    }

    public void RestartGame()
    {
        if(snowman != null && snowman.TryGetComponent(out Animator anim))
        {
            anim.SetBool("Throw", true);
        }
        
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if(activePlayers[i] != null)
            {
                activePlayers[i].transform.position = initialPositions[i];
            }
        }
        Time.timeScale = 1f;
        if(snowman != null) snowman.RestartSnowman();
    }
    
    public void ScorePoint()
    {
        Debug.Log("ScorePoint! Scene Reload!");
        PlayerPrefs.SetInt("ScorePin", score.ScorePin);
        PlayerPrefs.SetInt("ScorePon", score.ScorePon);
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);    
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
    #endregion
}

