using UnityEngine;
using System.Collections;
using PinPon;

public class ball_movement : MonoBehaviour
{
    [Header("Movimento")]
    public float initialSpeed;
    public float speed;
    public float speedIncrement;

    [Header("Referências")]
    public Rigidbody2D rb;
    public GameManager gameManager; // Injetado pelo criador (Snowman ou GameManager)
    public Score score; // Injetado pelo criador
    public Transform spawnPoint; // Injetado pelo criador

    [Header("Efeitos")]
    private AudioSource audioSource;
    public AudioClip SnowballHit;
    public AudioClip IcebergCrack;
    private SpriteRenderer spriteRenderer;
    public TrailRenderer trailRenderer;

    void Awake()
    {
        // Inicializa componentes locais no Awake para garantir que estejam prontos.
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        // Garante que a velocidade da bola seja consistente, mesmo após colisões.
        if (rb.velocity.magnitude > 0)
        {
            rb.velocity = rb.velocity.normalized * speed;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        bool validHit = false;

        if (collision.gameObject.CompareTag("pin") || collision.gameObject.CompareTag("pon"))
        {
            PinPonPlayerController playerController = collision.gameObject.GetComponent<PinPonPlayerController>();
            if (playerController != null)
            {
                Vector2 knockbackDirection = collision.transform.position - transform.position;

                float knockbackForce = 5f; // Ajuste este valor para controlar a força do knockback
                playerController.ApplyKnockback(knockbackDirection, knockbackForce);
                Debug.Log("Bola empurrou um jogador!");

                validHit = true;
            }
        }
        
        if(SnowballHit != null) audioSource.PlayOneShot(SnowballHit);

        if (collision.gameObject.GetComponent<BouncesAndSpeedsUpBall>() != null)
        {
            validHit = true;
        }

        if (validHit)
        {
            IncreaseSpeed();
        }


        // Lógica de pontuação com GetComponentInParent
        if (collision.gameObject.CompareTag("pontos") || collision.gameObject.CompareTag("pontos2"))
        {
            if (collision.gameObject.CompareTag("pontos"))
            {
                gameManager.snowExplosionEffectPin.SetActive(true);
            }
            else if (collision.gameObject.CompareTag("pontos2"))
            {
                gameManager.snowExplosionEffectPon.SetActive(true);
            }
            iceberg hitIceberg = collision.gameObject.GetComponentInParent<iceberg>();
            if (hitIceberg != null)
            {
                if(IcebergCrack != null) audioSource.PlayOneShot(IcebergCrack);
                hitIceberg.Stages--;
                Restart();

            }
        }

        Vector2 direction = rb.velocity.normalized;
        direction.x += Random.Range(-0.1f, 0.1f);
        direction.y += Random.Range(-0.1f, 0.1f);
        if (direction != Vector2.zero) rb.velocity = direction.normalized * speed;
    }

    public void Launch()
    {
        StartCoroutine(LaunchWithDelay());
    }

    private IEnumerator LaunchWithDelay()
    {
        Debug.Log("Lançando bola...");
        // transform.position = spawnPoint.position;
        // rb.velocity = Vector2.zero;
        spriteRenderer.enabled = false;

        // var balls = GameObject.FindGameObjectsWithTag("bola");
        // foreach (var ball in balls)
        // {
        //     ball_movement ballScript = ball.GetComponent<ball_movement>();
        //     if (ballScript != null)
        //     {
        //         speed = ballScript.speed;
        //     }
        // }

        yield return new WaitForSeconds(1f); // Pequena pausa antes de lançar a bola

        // Para bolas extras, a velocidade é definida pelo Snowman.
        // Como um failsafe, se a velocidade for 0, usamos a inicial.
        if (speed <= 0)
        {
            Debug.LogWarning($"Velocidade da bola era {speed}. Usando a velocidade inicial como fallback.");
            speed = initialSpeed;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("Cannot launch ball, spawn point is not set!", this.gameObject);
            yield break;
        }

        spriteRenderer.enabled = true;


        float x = Random.Range(0, 2) == 0 ? -1 : 1;
        float y = Random.Range(0.2f, 0.4f);

        Vector2 direction = new Vector2(x, y).normalized;
        rb.velocity = direction * speed;
    }

    private void IncreaseSpeed()
    {
        if (speed >= initialSpeed*3) return;

        speed += speedIncrement;
        // if(score != null) score.ballCounterValue += 0.1f;
        rb.velocity = rb.velocity.normalized * speed;

        // SetColorBasedOnSpeed();
    }

    // private Gradient SetColorBasedOnSpeed()
    // {
    //     if (score == null) return Gradient.white;

    //     if (score.ballCounterValue >= 1f)
    //         return trailRenderer.colorGradient = Gradient.white;
    //     else if (score.ballCounterValue >= 2f)
    //         return trailRenderer.colorGradient = Gradient.yellow;
    //     else if (score.ballCounterValue >= 3f)
    //         return trailRenderer.colorGradient = Gradient.red;
    //     else
    //     return trailRenderer.colorGradient = Gradient.white;
    // }

    public void Restart()
    {
        speed = initialSpeed;
        if(gameManager != null) gameManager.RestartGame();
    }
}

