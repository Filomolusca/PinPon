using System.Collections;
using UnityEngine;

public class snowman : MonoBehaviour
{
    [Header("Configuração")]
    public int initialHP = 3; 
    public int maxBalls = 2; // Máximo de bolas extras em cena
    public GameObject ballPrefab; 
    public Transform spawnPoint; 
    
    [Header("Referências")]
    public GameManager gameManager; // Arraste o GameManager da cena aqui
    public Score score; // Arraste o Score da cena aqui

    [Header("Efeitos")]
    public Animator animator;

    public int hp; 
    private int currentBalls;
    private bool hasCollided = false;

    void Start()
    {
        hp = initialHP;
        if(animator == null) animator = GetComponent<Animator>();
        // RestartSnowman();
    }

    void Update()
    {
        animator.SetInteger("HP", hp);
        animator.SetBool("hasCollided", hasCollided);

    }

    public void RestartSnowman()
    {
        currentBalls = 0;
        hp = initialHP;
        hasCollided = false;

        LaunchBall();
    }

    public void GetHit()
    {
        if (currentBalls < maxBalls)
        {
            SoundManager.Instance.PlaySFX("SnowmanHit");
            hp--;
            hasCollided = true; 
            StartCoroutine(ResetCollision()); 

            if (hp <= 0)
            LaunchBall();
        }
        else
        {
                Debug.Log("Snowman atingiu o limite de bolas em cena. Não pode ser atingido.");
        }
    }
    private void LaunchBall()
    {
        if (currentBalls >= maxBalls) return;

        hp = initialHP; 
        animator.Play("Snowman_Throw_Ball");
        SoundManager.Instance.PlaySFX("SnowmanThrow");
        
        GameObject newBallGO = Instantiate(ballPrefab, spawnPoint.position, spawnPoint.rotation);
        ball_movement newBallScript = newBallGO.GetComponent<ball_movement>();

        // Injeção de Dependência!
        newBallScript.gameManager = this.gameManager;
        newBallScript.score = this.score;
        newBallScript.spawnPoint = this.spawnPoint;

        // A velocidade da bola será definida por ela mesma no seu método Launch.
        
        newBallScript.Launch();
        
        currentBalls++;
        Debug.Log("Nova bola instanciada pelo Snowman.");

        animator.SetBool("Throw", false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasCollided && collision.gameObject.CompareTag("bola"))
        {
            GetHit();
        }

    }

    private IEnumerator ResetCollision()
    {
        yield return new WaitForSeconds(0.5f); 
        hasCollided = false; 
    }
}
