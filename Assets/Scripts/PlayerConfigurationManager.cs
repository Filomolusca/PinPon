using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerConfigurationManager : MonoBehaviour
{
    public static PlayerConfigurationManager Instance { get; private set; }

    public List<PlayerConfiguration> PlayerConfigs { get; private set; }
    
    [SerializeField]
    private GameObject[] playerPrefabs; // Atribua os prefabs Pin e Pon aqui no Inspector

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            PlayerConfigs = new List<PlayerConfiguration>();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Match")
        {
            SpawnPlayersAndStartMatch();
        }
    }

    public void HandlePlayerJoin(PlayerInput pi)
    {
        Debug.Log("Player joined with index: " + pi.playerIndex);

        // Impede que o marcador seja destruído na troca de cena e o mantém organizado.
        // Isso é crucial para que o PlayerInputManager saiba que o índice está ocupado.
        pi.transform.SetParent(transform);

        if (!PlayerConfigs.Exists(p => p.PlayerIndex == pi.playerIndex))
        {
            if (pi.playerIndex < playerPrefabs.Length)
            {
                PlayerConfigs.Add(new PlayerConfiguration(pi, playerPrefabs[pi.playerIndex]));
            }
            else
            {
                Debug.LogError($"[PlayerConfigurationManager] PlayerIndex {pi.playerIndex} está fora do alcance da lista de prefabs.");
            }
        }

        Lobby lobby = FindObjectOfType<Lobby>();
        if (lobby != null)
        {
            lobby.UpdatePlayerUI();
        }
    }

    private void SpawnPlayersAndStartMatch()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null) { /* ... error handling ... */ return; }

        Transform[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint")
                                    .Select(go => go.transform)
                                    .OrderBy(t => t.name)
                                    .ToArray();
        if (spawnPoints.Length < PlayerConfigs.Count) { /* ... error handling ... */ }

        foreach (var config in PlayerConfigs)
        {
            Vector3 spawnPosition = spawnPoints.Length > config.PlayerIndex ? spawnPoints[config.PlayerIndex].position : Vector3.zero;

            PlayerInput newPlayer = PlayerInput.Instantiate(
                prefab: config.PlayerPrefab,
                playerIndex: config.PlayerIndex,
                controlScheme: null,
                splitScreenIndex: -1,
                pairWithDevice: config.Input.devices.Count > 0 ? config.Input.devices[0] : null
            );

            newPlayer.gameObject.transform.position = spawnPosition;
            gameManager.RegisterPlayer(newPlayer.gameObject, spawnPosition);

            // Agora que o jogador real foi criado, destruímos o marcador que veio do lobby.
            Destroy(config.Input.gameObject);
        }

        PlayerConfigs.Clear();
        gameManager.BeginMatch();
    }
}

// Classe de configuração volta a ter a referência ao Input do marcador
public class PlayerConfiguration
{
    public PlayerInput Input { get; set; }
    public int PlayerIndex { get; set; }
    public GameObject PlayerPrefab { get; set; }

    public PlayerConfiguration(PlayerInput pi, GameObject prefab)
    {
        Input = pi;
        PlayerIndex = pi.playerIndex;
        PlayerPrefab = prefab;
    }
}




