using System.Collections.Generic;
using System.Linq;
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
    // Primeiro, verifica se esta instância de PlayerInput já foi configurada.
    // Isso previne que um evento duplicado (ex: clique do mouse) destrua um jogador existente.
    if (PlayerConfigs.Any(p => p.Input == pi))
    {
        Debug.Log($"Ignorando evento de entrada para jogador já configurado (índice: {pi.playerIndex}).");
        return;
    }

    // --- Validação para NOVOS jogadores ---

    // Verifica se já existe um jogador com teclado/mouse
    bool isUsingKeyboard = pi.devices.Any(d => d is Keyboard || d is Mouse);
    if (isUsingKeyboard && PlayerConfigs.Any(p => p.Input.devices.Any(d => d is Keyboard || d is Mouse)))
    {
        Debug.LogWarning("Um segundo jogador com teclado/mouse tentou entrar. Ação ignorada.");
        // Destroy(pi.gameObject); // Destrói apenas o NOVO objeto de jogador
        return;
    }

    // Verifica se o número máximo de jogadores foi atingido
    if (PlayerConfigs.Count >= 2)
    {
        Debug.LogWarning("O número máximo de jogadores (2) foi atingido. Novo jogador ignorado.");
        // Destroy(pi.gameObject); // Destrói apenas o NOVO objeto de jogador
        return;
    }

    // --- Se a validação passar, configura o novo jogador ---

    Debug.Log("PlayerConfigurationManager: Novo jogador entrou com índice: " + pi.playerIndex);

    // Impede que o GameObject do Input seja destruído na troca de cena.
    pi.transform.SetParent(transform);

    // Associa o PlayerInput com o prefab do personagem correto.
    if (pi.playerIndex < playerPrefabs.Length)
    {
        PlayerConfigs.Add(new PlayerConfiguration(pi, playerPrefabs[pi.playerIndex]));

        // Atualiza a UI do Lobby para mostrar que o jogador conectou.
        Lobby lobby = FindObjectOfType<Lobby>();
        if (lobby != null)
        {
            lobby.UpdatePlayerUI();
        }
        if (PlayerConfigs.Count >= 2)
        {
            PlayerInputManager.instance.DisableJoining(); // Desativa a entrada de novos jogadores
            Debug.Log("Dois jogadores conectados. Entrada de novos jogadores desativada.");
        }
    }
    else
    {
        Debug.LogError($"[PlayerConfigurationManager] PlayerIndex {pi.playerIndex} está fora do alcance da lista de prefabs. Destruindo jogador.");
        Destroy(pi.gameObject);
    }
}

    public void BeginMatch()
    {
        Lobby lobby = FindObjectOfType<Lobby>();
        if (lobby != null)
        {
            lobby.StartGame();
        }
    }
}

// Classe de dados simples para armazenar a configuração de cada jogador.
public class PlayerConfiguration
{
    public PlayerInput Input { get; private set; }
    public int PlayerIndex { get; private set; }
    public GameObject PlayerPrefab { get; private set; }
    public GameObject PlayerInstance { get; set; }

    public PlayerConfiguration(PlayerInput pi, GameObject prefab)
    {
        Input = pi;
        PlayerIndex = pi.playerIndex;
        PlayerPrefab = prefab;
    }
}
