using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class snowman : MonoBehaviour
{
    public int HP; 
    public int initialHP;
    public int maxBalls;
    private int currentBalls;
    public GameObject ballPrefab; 
    public GameObject originalBall;
    public Transform spawnPoint; 
    public bool hasCollided = false;
    public Animator animator;
    public AudioSource audioSource;
    public AudioClip snowmanHit; 
    public AudioClip snowmanThrowBall; 
    void Start()
    {
        HP = initialHP;

    }
    void Update()
    {
        animator.SetInteger("HP", HP);
        animator.SetBool("hasCollided", hasCollided);
        if (HP <= 0)
        {
            // animator.SetBool("Throw", true);
            animator.Play("Snowman_Throw_Ball");
            Debug.Log("Snowman is dead");
            SpawnNewBall();
            HP = initialHP;
        }
    }
    public void RestartSnowman()
{
    HP = initialHP;
    hasCollided = false;
    // animator.SetBool("Throw", false);
    animator.Play("Snowman_Throw_Ball");
    audioSource.PlayOneShot(snowmanThrowBall);
}

    private void SpawnNewBall()
    {
        if (currentBalls >= maxBalls)
        {
            Debug.Log("Max balls reached");
            return;
        }
        if (originalBall != null)
        {
            // Obtém a velocidade da bola original
            float originalSpeed = originalBall.GetComponent<ball_movement>().speed;

            // Cria uma nova instância do prefab da bola no ponto de spawn
            GameObject newBall = Instantiate(ballPrefab, spawnPoint.position, spawnPoint.rotation);

            // Define a velocidade da nova bola para a mesma velocidade da bola original
            newBall.GetComponent<ball_movement>().speed = originalSpeed;

            // Lança a nova bola
            newBall.GetComponent<ball_movement>().Launch();
            currentBalls++;
            Debug.Log("New ball spawned");

            animator.SetBool("Throw", false);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasCollided && (collision.gameObject.CompareTag("bolaOriginal") || collision.gameObject.CompareTag("bola")))
        {
            audioSource.PlayOneShot(snowmanHit);
            HP--;
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
