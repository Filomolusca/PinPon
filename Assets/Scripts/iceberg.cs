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
    private Vector2 BaseinitialPosition;
    public PinPonPlayerController pin;
    public Score score;
    private bool scoreUpdated = false;
    private AudioSource audioSource;
    public AudioClip pointSound;
    public AudioClip icebergDestroySound;
    void Start()
    {
        stages = new GameObject[] { stage1, stage2, stage3, stage4 };
        initialPositions = new Vector2[stages.Length];
        for (int i = 0; i < stages.Length; i++)
        {
            if (stages[i] != null)
            {
                initialPositions[i] = stages[i].transform.position;
            }
        }
        if (stage1 != null)
        {
            BaseinitialPosition = stage1.transform.position;
        }
    }

    void Update()
    {
        if (Stages < stages.Length)
        {
            for (int i = stages.Length - 1; i >= Stages; i--)
            {
                if (stages[i] != null)
                {
                    Destroy(stages[i]);
                    stages[i] = null;
                    MoveRemainingStagesDown(i);
                    MoveGoalDown();
                }
            }
        }
        if (Stages <= 0 && !scoreUpdated) // Verifica se o score já foi atualizado
        {
            
            
            if (pin.Pin == true)
            {
                score.IncreaseScorePon();
                Debug.Log("Pon Scored!");
            }
            else
            {
                score.IncreaseScorePin();
                Debug.Log("Pin Scored!");
            }
            scoreUpdated = true; // Marca que o score foi atualizado
            gameManager.ScorePoint();
            Debug.Log("Stages Destroyed and score updated!");
        }

        if (Stages <= 3)
        {
            MoveDistance = 2.1f; // Ajuste a distância de movimento para baixo
        }
        if (Stages <= 2)
        {
            MoveDistance = 1.6f; // Ajuste a distância de movimento para baixo
        }
    }

    void MoveRemainingStagesDown(int destroyedIndex)
    {
        for (int i = destroyedIndex - 1; i >= 0; i--)
        {
            if (stages[i] != null)
            {
                stages[i].transform.position = new Vector2(stages[i].transform.position.x, stages[i].transform.position.y - MoveDistance);
            }
        }
    }

    void MoveGoalDown()
    {
        if (goal != null)
        {
            goal.transform.position = new Vector2(goal.transform.position.x, goal.transform.position.y - MoveDistance);
        }
    }
        public void ResetScoreUpdate()
    {
        scoreUpdated = false; // Reseta a variável de controle
    }
}