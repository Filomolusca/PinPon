using PinPon;
using UnityEngine;

public class iceberg : MonoBehaviour
{
    public int Stages = 4; // Começa com 4 vidas
    public float MoveDistance;
    public GameObject stage1;
    public GameObject stage2;
    public GameObject stage3;
    public GameObject stage4;
    public GameManager gameManager;
    public Score score;
    
    private GameObject[] stages;
    private Vector2[] initialPositions;
    private PinPonPlayerController _playerController;
    private bool scoreUpdated = false;
    private AudioSource audioSource;
    public AudioClip pointSound;
    public AudioClip icebergDestroySound;

    void Start()
    {
        // Inicializa as referências que não dependem do jogador
        stages = new GameObject[] { stage1, stage2, stage3, stage4 };
        initialPositions = new Vector2[stages.Length];
        for (int i = 0; i < stages.Length; i++)
        {
            if (stages[i] != null)
            {
                initialPositions[i] = stages[i].transform.position;
            }
        }
        // As referências do gameManager e score serão atribuídas pelo GameManager no spawn
    }

    /// <summary>
    /// Atribui o jogador e as dependências a este iceberg. Chamado pelo GameManager.
    /// </summary>
    public void AssignPlayer(PinPonPlayerController playerController, GameManager gm, Score sc)
    {
        _playerController = playerController;
        gameManager = gm;
        score = sc;

        // Lógica que depende do jogador agora está aqui
        if (Stages <= 3) MoveDistance = 2.1f;
        if (Stages <= 2) MoveDistance = 1.6f;
    }

    void Update()
    {
        // Esta verificação acontece a cada frame.
        if (Stages <= 0 && !scoreUpdated && _playerController != null)
        {
            scoreUpdated = true;
            
            // O jogador que perdeu foi o dono deste iceberg. O outro jogador pontua.
            if (_playerController.PlayerIndex == 0) // Se o dono deste iceberg é o P1 (Pin)
            {
                score.IncreaseScorePon(); // P2 (Pon) pontua
                Debug.Log("Pon Scored!");
            }
            else // Se o dono deste iceberg é o P2 (Pon)
            {
                score.IncreaseScorePin(); // P1 (Pin) pontua
                Debug.Log("Pin Scored!");
            }

            gameManager.ScorePoint();
            Debug.Log("All stages destroyed, score updated!");
        }
    }
    
    public void ResetScoreUpdate()
    {
        scoreUpdated = false;
    }
}