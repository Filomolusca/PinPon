using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class Lobby : MonoBehaviour
{
    public GameObject beginButton;
    public GameObject player1; // UI para indicar que o Player 1 conectou
    public GameObject player2; // UI para indicar que o Player 2 conectou

    void Start()
    {
        // Garante que a UI comece no estado correto
        player1.SetActive(true);
        player2.SetActive(true);
        beginButton.SetActive(false);
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
            beginButton.SetActive(p1Ready && p2Ready);
        }
    }
    
    public void StartGame()
    {
        PlayerPrefs.SetInt("ScorePin", 0);
        PlayerPrefs.SetInt("ScorePon", 0);
        SceneManager.LoadScene("Match");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

