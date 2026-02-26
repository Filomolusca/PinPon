using PinPon;
using UnityEngine;

public class iceberg : MonoBehaviour
{
    public int Stages;
    public float MoveDistance;
    public GameObject stage1;
    public GameObject stage2;
    public GameObject stage3;
    public GameObject stage4;
    public GameObject goal;
     public GameManager gameManager;
     private GameObject[] stages;
     private Vector2[] initialPositions;
    private PinPonPlayerController _playerController; // Referência será injetada
     public Score score;
     private bool scoreUpdated = false;
     private AudioSource audioSource;
     public AudioClip pointSound;
     public AudioClip icebergDestroySound;

    /// <summary>
    /// Atribui o jogador a este iceberg. Chamado pelo GameManager.
    /// </summary>
    /// <param name="playerController">O controlador do jogador a ser associado.</param>
    public void AssignPlayer(PinPonPlayerController playerController)
    {
        _playerController = playerController;
    }

     void Start()
     {
        // A linha incorreta "GetComponent<PinPonPlayerController>()" foi removida.
         stages = new GameObject[] { stage1, stage2, stage3, stage4 };
         initialPositions = new Vector2[stages.Length];
         for (int i = 0; i < stages.Length; i++)
    


        // Esta verificação agora funcionará, pois _playerController será válido.
        if (Stages <= 0 && !scoreUpdated && _playerController != null)
         {
  
             if (_playerController.PlayerIndex == 0) // Se este é o iceberg do Jogador 1 (Pin)
             {
                 score.IncreaseScorePon();
                 score.IncreaseScorePin();
                 Debug.Log("Pin Scored!");
             }
            scoreUpdated = true;
             gameManager.ScorePoint();
             Debug.Log("Stages Destroyed and score updated!");
         }
 
         if (Stages <= 3)
         {
            MoveDistance = 2.1f;
         }
         if (Stages <= 2)
         {
            MoveDistance = 1.6f;
         }
    } 
         public void ResetScoreUpdate()
     {
        scoreUpdated = false;
     }
}