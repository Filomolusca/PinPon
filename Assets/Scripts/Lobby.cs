using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.InputSystem;

public class Lobby : MonoBehaviour
{
    public GameObject beginButton;
    public GameObject player1; // UI para indicar que o Player 1 conectou
    public GameObject player2; // UI para indicar que o Player 2 conectou
    public AudioSource lobbyMusic;

    private InputAction startGameAction;
    private bool canStartGame = false;

    void Awake()
    {
        // Cria a ação de input para iniciar o jogo
        startGameAction = new InputAction("StartGame");
        startGameAction.AddBinding("<Keyboard>/space");
        startGameAction.AddBinding("<Gamepad>/buttonSouth"); // Botão A no Xbox, X no PlayStation
    }

    void Start()
    {
        // Garante que a UI comece no estado correto
        player1.SetActive(true);
        player2.SetActive(true);
        beginButton.SetActive(false);
        lobbyMusic.Play();
        
        // Garante que a ação de start esteja desabilitada no início
        startGameAction.Disable();
    }
    
    private void OnEnable()
    {
        // É uma boa prática habilitar a ação aqui caso o objeto seja desativado/reativado
        if (canStartGame)
        {
            startGameAction.Enable();
        }
    }

    private void OnDisable()
    {
        // Desabilita a ação para evitar execuções indesejadas
        startGameAction.Disable();
    }

    void Update()
    {
        // Se a ação de start foi pressionada e o jogo pode começar
        if (canStartGame && startGameAction.triggered)
        {
            StartGame();
        }
    }

    // Este método será chamado pelo PlayerConfigurationManager quando um jogador entrar
    public void UpdatePlayerUI()
    {
        if (PlayerConfigurationManager.Instance != null)
        {
            var configs = PlayerConfigurationManager.Instance.PlayerConfigs;
            
            // Verifica se o jogador 1 (playerIndex 0) está na lista
            bool p1Ready = configs.Any(p => p.PlayerIndex == 0);
            player1.SetActive(!p1Ready);

            // Verifica se o jogador 2 (playerIndex 1) está na lista
            bool p2Ready = configs.Any(p => p.PlayerIndex == 1);
            player2.SetActive(!p2Ready);

            // O botão para começar só aparece se ambos estiverem prontos
            canStartGame = p1Ready && p2Ready;
            beginButton.SetActive(canStartGame);

            if (canStartGame)
            {
                // Habilita a ação para que o jogo possa ser iniciado
                startGameAction.Enable();
            }
            else
            {
                // Desabilita para não iniciar acidentalmente
                startGameAction.Disable();
            }
        }
    }
    
    public void StartGame()
    {
        // Desabilita a ação para prevenir múltiplos cliques
        startGameAction.Disable();
        canStartGame = false;

        PlayerPrefs.SetInt("ScorePin", 0);
        PlayerPrefs.SetInt("ScorePon", 0);
        SceneManager.LoadScene("Match");
        lobbyMusic.Stop();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

