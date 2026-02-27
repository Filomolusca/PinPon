using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerConfigurationManager : MonoBehaviour
{
    public static PlayerConfigurationManager Instance { get; private set; }

    // A lista de configurações de jogadores que o GameManager irá ler
    public List<PlayerConfiguration> PlayerConfigs { get; private set; }
    
    // Array para os prefabs dos personagens (Ex: Elemento 0 = Pin, Elemento 1 = Pon)
    [SerializeField]
    private GameObject[] playerPrefabs; 

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

    // Este método é chamado pelo 'PlayerInputManager' quando um novo jogador entra no lobby.
    public void HandlePlayerJoin(PlayerInput pi)
    {
        Debug.Log("PlayerConfigurationManager: Jogador entrou com index: " + pi.playerIndex);

        // Impede que o GameObject do Input seja destruído na troca de cena.
        pi.transform.SetParent(transform);

        // Adiciona a configuração se for um jogador novo.
        if (!PlayerConfigs.Exists(p => p.PlayerIndex == pi.playerIndex))
        {
            if (pi.playerIndex < playerPrefabs.Length)
            {
                // Associa o PlayerInput (que contém o device) com o prefab do personagem correto.
                PlayerConfigs.Add(new PlayerConfiguration(pi, playerPrefabs[pi.playerIndex]));
            }
            else
            {
                Debug.LogError($"[PlayerConfigurationManager] PlayerIndex {pi.playerIndex} está fora do alcance da lista de prefabs. Verifique o array 'playerPrefabs' no Inspector.");
            }
        }

        // Atualiza a UI do Lobby para mostrar que o jogador conectou.
        Lobby lobby = FindObjectOfType<Lobby>();
        if (lobby != null)
        {
            lobby.UpdatePlayerUI();
        }
    }
}

// Classe de dados simples para armazenar a configuração de cada jogador.
public class PlayerConfiguration
{
    public PlayerInput Input { get; private set; }
    public int PlayerIndex { get; private set; }
    public GameObject PlayerPrefab { get; private set; }

    public PlayerConfiguration(PlayerInput pi, GameObject prefab)
    {
        Input = pi;
        PlayerIndex = pi.playerIndex;
        PlayerPrefab = prefab;
    }
}
