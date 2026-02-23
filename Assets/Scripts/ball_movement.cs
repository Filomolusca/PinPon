using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UIElements;
using System.Collections;

public class ball_movement : MonoBehaviour
{
    public float speed; // Velocidade inicial da bola
    public float speedIncrement;
    public float speedDecrement; 
    public Rigidbody2D rb;
    public iceberg iceberg;
    public GameObject Pin;
    public GameObject Pon;
    public GameManager gameManager;
    public Score score;
    private AudioSource audioSource;
    public AudioClip SnowballHit;
    public AudioClip IcebergCrack;
    public Transform spawnPoint;
    private SpriteRenderer spriteRenderer;
    private bool canIncreaseSpeed = true;
    // private float lastCollisionTime = 0f;
    // private float collisionCooldown = 0.1f;


    void Start()
    {  
        // spawnPoint = GameObject.FindGameObjectWithTag("spawnPoint").transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();          
        Launch();
    }
    void Update()
    {

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
            // if (Time.time - lastCollisionTime < collisionCooldown)
            //     {
            //         return; // Ignora colisões muito próximas
            //     }
            // lastCollisionTime = Time.time;
            audioSource.PlayOneShot(SnowballHit);

            Vector2 direction = rb.velocity.normalized;
            direction.x += Random.Range(-0.1f, 0.1f);
            direction.y += Random.Range(-0.1f, 0.1f);

            direction = direction.normalized; // Normaliza a direção para manter a velocidade constante
            rb.velocity = direction * speed; // Aplica a nova direção e velocidade

        if (collision.gameObject.CompareTag("pin"))
        {   
            if (canIncreaseSpeed)
            {
                // pinHitAnimation
                IncreaseSpeed();
                StartCoroutine(ResetIncreaseSpeed());
            }
        }
        if (collision.gameObject.CompareTag("pon"))
        {
            if (canIncreaseSpeed)
            {
                IncreaseSpeed();
                StartCoroutine(ResetIncreaseSpeed());
            }
        }

        rb.velocity = rb.velocity.normalized * speed;

        if (collision.gameObject.CompareTag("pontos"))
        {
            audioSource.PlayOneShot(IcebergCrack);
            GameObject pinObject = GameObject.FindWithTag("pin");

            if (pinObject != null)
            {
                iceberg pinIceberg = pinObject.GetComponent<iceberg>();

                if (pinIceberg != null)
                {
                    pinIceberg.Stages--;
                    Restart();
                }
            }
        }

        if (collision.gameObject.CompareTag("pontos2"))
        {
            audioSource.PlayOneShot(IcebergCrack);
            GameObject ponObject = GameObject.FindWithTag("pon");

            if (ponObject != null)
            {
                iceberg ponIceberg = ponObject.GetComponent<iceberg>();
                if (ponIceberg != null)
                {
                    ponIceberg.Stages--;
                    Restart();
                }
            }
        }
        // if (collision.gameObject.CompareTag("seagullPin"))
        // {
        //     Debug.Log("bateu na gaivota do Pin");
        //     DecreaseSpeed();
        // }
        // if (collision.gameObject.CompareTag("seagullPon"))
        // {
        //     Debug.Log("bateu na gaivota do Pon");
        //     DecreaseSpeed();
        // }
    }
    private IEnumerator ResetIncreaseSpeed()
    {
        canIncreaseSpeed = false;
        yield return new WaitForSeconds(0.3f); // Define o tempo de espera desejado
        canIncreaseSpeed = true;
    }

    // public void Launch()
    // {
    //     WaitForSeconds wait = new WaitForSeconds(1f);
    //     if (gameObject.activeSelf == false)
    //     {
    //         gameObject.SetActive(true);
    //     }
    //     Debug.Log("bola lançada");
    //     transform.position = spawnPoint.position;
    //     // Define a direção inicial da bola aleatoriamente
    //     float x = Random.Range(0, 2) == 0 ? -1 : 1;
    //     float y = Random.Range(0, 2) == 0 ? 0 : 1;

    //     x += Random.Range(-0.4f, 0.4f);
    //     y += Random.Range(0.1f, 0.4f);

    //     // Normaliza a direção para manter a velocidade constante
    //     Vector2 direction = new Vector2(x, y).normalized;

    //     // Define a velocidade inicial
    //     rb.velocity = direction * speed;
    // }
    public void Launch()
{
    StartCoroutine(LaunchWithDelay());
}

private IEnumerator LaunchWithDelay()
{
    // Espera 1 segundo
    yield return new WaitForSeconds(1f);


    spriteRenderer.enabled = true;

    Debug.Log("bola lançada");
    transform.position = spawnPoint.position;

    // Define a direção inicial da bola aleatoriamente
    float x = Random.Range(0, 2) == 0 ? -1 : 1;
    float y = Random.Range(0, 2) == 0 ? 0 : 1;

    x += Random.Range(-0.4f, 0.4f);
    y += Random.Range(0.1f, 0.4f);

    // Normaliza a direção para manter a velocidade constante
    Vector2 direction = new Vector2(x, y).normalized;

    // Define a velocidade inicial
    rb.velocity = direction * speed;
}

    private void IncreaseSpeed()
    {
        if (score.ballCounterValue >= 3)
        {
            return;
        }
        Debug.Log("Speed: " + speed);
        speed += speedIncrement;
        score.ballCounterValue += 0.1f;

        rb.velocity = rb.velocity.normalized * speed;
    }

    // public void UpdateBallSpeed()
    // {
    //     rb.velocity = rb.velocity.normalized * speed;
    // }
    // private void DecreaseSpeed()
    // {
    //     if (score.ballCounterValue <= 1)
    //     {
    //         return;
    //     }
    //     Debug.Log("Speed: " + speed);
    //     speed -= speedDecrement;
    //     score.ballCounterValue -= 0.1f;
    //     rb.velocity = rb.velocity.normalized * speed;
    // }
    public void Restart()
    {
        Debug.Log("bola entrou no restart");
        speed = 3f;
        rb.velocity = Vector2.zero;
        score.ballCounterValue = 1;
        DestroyExcessBalls();
        
        if (gameObject.CompareTag("bolaOriginal"))
        {
            spriteRenderer.enabled = false;
            gameManager.RestartGame();
            // Time.timeScale = 0f;
            Launch();
        }

    }
    public void DestroyExcessBalls()
    {
        GameObject originalBall = GameObject.FindGameObjectWithTag("bolaOriginal");
        GameObject[] allBalls = GameObject.FindGameObjectsWithTag("bola");

        foreach (GameObject ball in allBalls)
        {
            if (ball != originalBall)
            {
                Destroy(ball);
                Debug.Log("Bola extra destruída: " + ball.name);;
            }
        }
    }
}

