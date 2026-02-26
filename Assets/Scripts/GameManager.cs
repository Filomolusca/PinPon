using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject victoryScreen;
    public GameObject RoundScreen;
    public GameObject ballCounter;
    public Score score;
    private bool isPaused = false;
    public List<GameObject> actors = new List<GameObject>();
    private List<Vector2> initialPositions = new List<Vector2>();
    public iceberg icebergManager;
    private static bool isFirstRound = true;
    public AudioSource ostMatch;
    public Transform spawnSeagullPin;
    public Transform spawnSeagullPon;
    public GameObject seagullPinPrefab;
    public GameObject seagullPonPrefab;
    public Animator snowmanAnimator;
    public snowman snowman;
    public GameObject icebergPin;
    public GameObject icebergPon;

    void Start()
    {
        isFirstRound = true;
        pauseMenu.SetActive(false);
        victoryScreen.SetActive(false);
        
        // Limpa as listas para garantir que não haja dados de sessões anteriores
        actors.Clear();
        initialPositions.Clear();

        // Carrega os scores salvos
        score.ScorePin = score.GetInt("ScorePin");
        score.ScorePon = score.GetInt("ScorePon");
        score.UpdateScoreTextPin();
        score.UpdateScoreTextPon();
    }

    /// <summary>
    /// Registra um jogador no GameManager. Chamado pelo PlayerConfigurationManager.
    /// </summary>
    public void RegisterPlayer(GameObject player, Vector2 initialPos)
    {
        if (!actors.Contains(player))
        {
            actors.Add(player);
            initialPositions.Add(initialPos);
            
        }
    }

    /// <summary>
    /// Inicia a partida. Chamado pelo PlayerConfigurationManager após todos os jogadores serem spawnados.
    /// </summary>
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
            AudioSource[] audioSources = RoundScreen.GetComponents<AudioSource>();
            foreach (AudioSource audioSource in audioSources)
            {
                audioSource.Play();
            }
        }
        else
        {
            isFirstRound = false;
        }
    }

    private IEnumerator WaitForSpace()
    {
        // Espera um frame para garantir que os inputs de 'join' não sejam contados
        yield return null; 
        
        // Usa GetButtonDown para ser compatível com controle e teclado
        while (!Input.GetButtonDown("Submit") && !Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }
        
        ostMatch.Play();
        Debug.Log("Round Começou!");
        Time.timeScale = 1f;
        RoundScreen.SetActive(false);
    }
    
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
            Debug.Log("Seagull killed!");
            string seagullTag = seagullInstance.tag;
            Destroy(seagullInstance);
            yield return new WaitForSeconds(5f); // Espera 5 segundos
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !victoryScreen.activeSelf && !RoundScreen.activeSelf)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Freeze the game
        pauseMenu.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Resume the game
        pauseMenu.SetActive(false);
    }
    public void RestartGame()
    {
        snowmanAnimator.SetBool("Throw", true);
        
        for (int i = 0; i < actors.Count; i++)
        {
            if(actors[i] != null)
            {
                actors[i].transform.position = initialPositions[i];
            }
        }
        Time.timeScale = 1f;
        snowman.RestartSnowman();
    }
    public void ScorePoint()
    {
        Debug.Log("ScorePoint! Scene Reload!");
        PlayerPrefs.SetInt("ScorePin", score.ScorePin);
        PlayerPrefs.SetInt("ScorePon", score.ScorePon);
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);    
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
        SceneManager.LoadScene("Menu");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}

