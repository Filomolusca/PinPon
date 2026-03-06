using UnityEngine;
using System.Collections;

public class ball_movement : MonoBehaviour
{
    [Header("Movimento")]
    public float initialSpeed = 8f;
    public float speed;
    public float speedIncrement = 0.5f;

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

    void Awake()
    {
        // Inicializa componentes locais no Awake para garantir que estejam prontos.
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        // As referências externas (gameManager, score, spawnPoint)
        // devem ser injetadas antes do Start() ser chamado.
        // Se a bola for a original na cena, ela lança a si mesma.
        if (gameObject.CompareTag("bolaOriginal"))
        {
             spriteRenderer.enabled = false; // Começa invisível
             Launch();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        bool validHit = false;
        if (collision.gameObject.GetComponent<RacquetController>() != null || collision.gameObject.GetComponent<BouncesAndSpeedsUpBall>() != null)
        {
            validHit = true;
        }

        if (validHit)
        {
            IncreaseSpeed();
            if(SnowballHit != null) audioSource.PlayOneShot(SnowballHit);
        }
        else if (collision.gameObject.CompareTag("Untagged"))
        {
            if(SnowballHit != null) audioSource.PlayOneShot(SnowballHit);
        }
        
        // Lógica de pontuação com GetComponentInParent
        if (collision.gameObject.CompareTag("pontos") || collision.gameObject.CompareTag("pontos2"))
        {
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
        transform.position = spawnPoint.position;
        rb.velocity = Vector2.zero;
        spriteRenderer.enabled = false;

        yield return new WaitForSeconds(1f); // Pequena pausa antes de lançar a bola

        if (this.gameObject.CompareTag("bolaOriginal"))
        { 
            speed = initialSpeed;
        }
        
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
        float y = Random.Range(-0.1f, 0.4f);

        Vector2 direction = new Vector2(x, y).normalized;
        rb.velocity = direction * speed;
    }

    private void IncreaseSpeed()
    {
        if (score != null && score.ballCounterValue >= 3) return;
        
        speed += speedIncrement;
        if(score != null) score.ballCounterValue += 0.1f;
        rb.velocity = rb.velocity.normalized * speed;
    }

    public void Restart()
    {
        speed = 0;
        rb.velocity = Vector2.zero;
        if(score != null) score.ballCounterValue = 1;
        
        if (gameObject.CompareTag("bolaOriginal"))
        {
            DestroyExcessBalls(); // Só a bola original pode destruir as outras
            spriteRenderer.enabled = false;
            if(gameManager != null) gameManager.RestartGame();
            Launch();
        }
        else
        {
            Destroy(gameObject); // Bolas extras se destroem ao invés de reiniciar o jogo
        }
    }

    public void DestroyExcessBalls()
    {
        GameObject[] allBalls = GameObject.FindGameObjectsWithTag("bola");
        foreach (GameObject ball in allBalls)
        {
            // A bola original nunca tem a tag "bola", então não se destruirá.
            Destroy(ball);
        }
    }
}

