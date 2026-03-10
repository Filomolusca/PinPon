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
    public AudioSource audioSource;
    public AudioClip snowmanHit; 
    public AudioClip snowmanThrowBall; 

    private int hp; 
    private int currentBalls;
    private bool hasCollided = false;

    void Start()
    {
        hp = initialHP;
        if(animator == null) animator = GetComponent<Animator>();
        if(audioSource == null) audioSource = GetComponent<AudioSource>();
        RestartSnowman();
    }

    void Update()
    {
        animator.SetInteger("HP", hp);
        animator.SetBool("hasCollided", hasCollided);

        if (hp <= 0)
        {
            animator.Play("Snowman_Throw_Ball");
            LaunchBall();
            hp = initialHP; // Reseta a vida após jogar a bola
        }
    }

    public void RestartSnowman()
    {
        currentBalls = 0;
        hp = initialHP;
        hasCollided = false;
        animator.Play("Snowman_Throw_Ball");
        if(snowmanThrowBall != null) audioSource.PlayOneShot(snowmanThrowBall);
        LaunchBall();
    }

    private void LaunchBall()
    {
        if (currentBalls >= maxBalls) return;
        
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
            if(snowmanHit != null) audioSource.PlayOneShot(snowmanHit);
            hp--;
            hasCollided = true; 
            StartCoroutine(ResetCollision()); 
        }
    }

    private IEnumerator ResetCollision()
    {
        yield return new WaitForSeconds(0.5f); 
        hasCollided = false; 
    }
}
