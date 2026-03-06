using PinPon;
using UnityEngine;

public class iceberg : MonoBehaviour
{
    [Header("Configuração dos Estágios")]
    public int Stages = 4;
    public float MoveDistance = 2.0f; // Distância padrão que o iceberg desce
    public GameObject stage1;
    public GameObject stage2;
    public GameObject stage3;
    public GameObject stage4;
    public GameObject goal; // O objeto do gol que também deve descer

    [Header("Efeitos")]
    public AudioClip icebergDestroySound;

    // --- Referências Injetadas ---
    private GameManager gameManager;
    private Score score;
    
    // --- Controle Interno ---
    private PinPonPlayerController _playerController;
    private GameObject[] _stageObjects;
    private int _previousStages;
    private bool _scoreUpdated = false;
    private AudioSource audioSource;

    void Start()
    {
        // Configura o array de estágios para fácil acesso
        _stageObjects = new GameObject[] { stage1, stage2, stage3, stage4 };
        _previousStages = Stages;
        audioSource = GetComponent<AudioSource>();
        if(audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    /// <summary>
    /// Atribui as referências essenciais quando o iceberg é instanciado.
    /// </summary>
    public void AssignPlayer(PinPonPlayerController playerController, GameManager gm, Score sc)
    {
        _playerController = playerController;
        gameManager = gm;
        score = sc;
    }

    void Update()
    {
        // 1. Detecta se um estágio foi destruído (quando 'Stages' diminui)
        if (Stages < _previousStages)
        {
            Debug.Log($"Estágio destruído! Estágios restantes: {Stages}");
            if (icebergDestroySound != null) audioSource.PlayOneShot(icebergDestroySound);

            UpdateVisuals();
            MoveRemainingStagesDown();
            MoveGoalDown();
            
            _previousStages = Stages; // Atualiza o contador para a próxima frame
        }

        // 2. Verifica se o jogo acabou para este iceberg (0 estágios restantes)
        if (Stages <= 0 && !_scoreUpdated && _playerController != null)
        {
            _scoreUpdated = true;
            
            // Se o dono deste iceberg é o P1 (Pin), o P2 (Pon) pontua. E vice-versa.
            if (_playerController.PlayerIndex == 0) 
            {
                score.IncreaseScorePon();
            }
            else 
            {
                score.IncreaseScorePin();
            }

            // Chama o GameManager para reiniciar a rodada
            gameManager.ScorePoint();
        }
    }
    
    /// <summary>
    /// Esconde o GameObject do estágio que foi destruído.
    /// </summary>
    private void UpdateVisuals()
    {
        // 'Stages' agora representa o número de estágios restantes.
        // O estágio a ser escondido está no índice correspondente a esse número.
        // Ex: Se Stages virou 3, esconde o estágio no índice 3 (o 4º da lista).
        if (Stages >= 0 && Stages < _stageObjects.Length)
        {
            if (_stageObjects[Stages] != null)
            {
                _stageObjects[Stages].SetActive(false);
            }
        }
    }

    /// <summary>
    /// Move todos os estágios restantes para baixo.
    /// </summary>
    private void MoveRemainingStagesDown()
    {
        Debug.Log($"Movendo os estágios restantes para baixo. Estágios restantes: {Stages}");
        // Itera apenas sobre os estágios que ainda estão visíveis.
        for (int i = 0; i < Stages; i++)
        {
            if (_stageObjects[i] != null)
            {
                _stageObjects[i].transform.position -= new Vector3(0, MoveDistance, 0);
            }
        }
    }

    /// <summary>
    /// Move o objeto do gol para baixo junto com o resto do iceberg.
    /// </summary>
    private void MoveGoalDown()
    {
        if (goal != null)
        {
            goal.transform.position -= new Vector3(0, MoveDistance, 0);
        }
    }

    public void ResetScoreUpdate()
    {
        _scoreUpdated = false;
    }
}