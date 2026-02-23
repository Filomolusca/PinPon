using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    public int ScorePin;
    public int ScorePon;
    public int ScoreGoal;
    public int Round;
    public TextMeshProUGUI round;
    public GameObject scoreCounter;
    public TextMeshProUGUI scoreTextPin;
    public TextMeshProUGUI scoreTextPon;
    public TextMeshProUGUI Winner;
    public GameManager gameManager;
    public GameObject ballCounter;
    public TextMeshProUGUI ballCounterText;
    public float ballCounterValue;
    // private AudioSource audioSource;
    // public AudioClip WinnerSound;
    // Start is called before the first frame update
    void Start()
    {
        Round = ScorePin + ScorePon +1;
        round.text = "" + Round.ToString();
        scoreCounter.SetActive(true);
        UpdateScoreTextPin();
        UpdateScoreTextPon();
    }
    public void SetInt(string KeyName, int Value)
    {
        PlayerPrefs.SetInt(KeyName, Value);
    }

    public int GetInt(string KeyName)
    {
        return PlayerPrefs.GetInt(KeyName, 0); // Retorna 0 se a chave não existir
    }
    

    // Update is called once per frame
    void Update()
    {
        if (ScorePin == ScoreGoal)
        {

            Winner.text = "Pinguin Wins!";
            Winner.color = Color.red;
            gameManager.GameOver();
            // audioSource.PlayOneShot(WinnerSound);
        }
        if (ScorePon == ScoreGoal)
        {           
            Winner.text = "Ponguin Wins!";
            Winner.color =Color.blue;
            gameManager.GameOver();
            // audioSource.PlayOneShot(WinnerSound);
        }
        if (ballCounterValue >= 1)
        {
            ballCounterText.text = ballCounterValue.ToString() + "x";
        }
    }
    public void IncreaseScorePin()
    {
        ScorePin++;
        UpdateScoreTextPin();
    }
    public void UpdateScoreTextPin()
    {
        scoreTextPin.text = "" + ScorePin.ToString();
    }
    public void IncreaseScorePon()
    {
        ScorePon++;
        UpdateScoreTextPon();
    }
    public void UpdateScoreTextPon()
    {
        scoreTextPon.text = "" + ScorePon.ToString();
    }
}
