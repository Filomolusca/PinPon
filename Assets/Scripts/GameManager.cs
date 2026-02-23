using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    public Transform spawnPoint;
    public Transform spawnSeagullPin;
    public Transform spawnSeagullPon;
    public GameObject seagullPinPrefab;
    public GameObject seagullPonPrefab;
    public Animator snowmanAnimator;
    public snowman snowman;
    // public pin_movement pin;
    // public pin_movement pon;
    public void StartGame()
    {
        isFirstRound = true;
        Round();
        PlayerPrefs.SetInt("ScorePin", 0);
        PlayerPrefs.SetInt("ScorePon", 0);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        SceneManager.LoadScene("Match");
    }
public void Round()
{
    Debug.Log("Round!");
    RoundScreen.SetActive(true);
    Time.timeScale = 0f;
    StartCoroutine(WaitForSpace());
            
            if (!isFirstRound)
        {
            // Tocar componentes de áudio da tela de round
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
    while (!Input.GetKeyDown(KeyCode.Space))
    {
        yield return null;
    }
    ostMatch.Play();
    Debug.Log("Round Começou!");
    Time.timeScale = 1f;
    RoundScreen.SetActive(false);
}
    void Start()
    {
        isFirstRound = true;
        Round();
        pauseMenu.SetActive(false);
        victoryScreen.SetActive(false);
        foreach (GameObject actor in actors)
        {
            initialPositions.Add(actor.transform.position);
        }

        // Carrega os scores salvos
        score.ScorePin = score.GetInt("ScorePin");
        score.ScorePon = score.GetInt("ScorePon");
        score.UpdateScoreTextPin();
        score.UpdateScoreTextPon();    
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
        if (Input.GetKeyDown(KeyCode.Space) && !victoryScreen.activeSelf && !RoundScreen.activeSelf)
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
        // snowman.SetInteger("HP", 3);
        // pin.isJumping = false;
        // pon.isJumping = false; 
        
        for (int i = 0; i < actors.Count; i++)
        {
            actors[i].transform.position = initialPositions[i];
        }
        Time.timeScale = 1f;
        snowman.RestartSnowman();
        
    }
    public void ScorePoint()
    {
        Debug.Log("ScorePoint! Scene Reload!");
        // audioSource.PlayOneShot(Point);
        PlayerPrefs.SetInt("ScorePin", score.ScorePin);
        PlayerPrefs.SetInt("ScorePon", score.ScorePon);
        

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);    
    }

    public void GameOver()
    {
        RoundScreen.SetActive(false);
        Destroy(pauseMenu);
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
    // void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    // {
    //     RestartGame(); // Chama RestartGame quando a cena é carregada
    // }

    // void OnEnable()
    // {
    //     SceneManager.sceneLoaded += OnSceneLoaded;
    // }

    // void OnDisable()
    // {
    //     SceneManager.sceneLoaded -= OnSceneLoaded;
    // }
    //     void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    // {
    //     icebergManager.ResetScoreUpdate(); // Reseta a variável de controle quando a cena é carregada
    // }
//     void OnEnable()
//     {
//         SceneManager.sceneLoaded += OnSceneLoaded;
//     }

//     void OnDisable()
//     {
//         SceneManager.sceneLoaded -= OnSceneLoaded;
//     }
}
