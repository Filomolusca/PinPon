using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PinPon;

public class BombController : MonoBehaviour
{
    public float explosionTime = 5f;
    public GameObject explosionEffect;
    public float speed;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    public GameManager gameManager;
    public bool isThrown = false;
    public bool bombLanded = false;
    public bool isExploding = false;
    public float spawnWaitTime;
    private Color highlightColor = new Color(1f, 1f, 0.5f);
    private Color originalColor;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        gameManager = FindObjectOfType<GameManager>();
        originalColor = spriteRenderer.color;  
        explosionEffect.SetActive(false);
        spriteRenderer.enabled = true;
        StartCoroutine(WaitForLanding());
    }

    private IEnumerator WaitForLanding()
    {
        rb.simulated = false;
        
        yield return new WaitForSeconds(spawnWaitTime);

        rb.simulated = true;

        while (!bombLanded)
        {

            if (rb.velocity.magnitude < 0.1f && isThrown)
            {
                rb.velocity = Vector2.zero;
                rb.isKinematic = true;
            }
            yield return null;
        }

        StartCoroutine(ExplosionCountdownAnimation());
    }

    #region Interaction Events
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (!bombLanded)
        {
            if (collision.gameObject.CompareTag("tile") || collision.gameObject.CompareTag("base"))
            {
                bombLanded = true;
                rb.velocity = Vector2.zero;
                rb.isKinematic = true;
                Debug.Log("Bomba aterrissou!");
            }
        }
        if (isThrown)
        {

            if (collision.gameObject.CompareTag("pontos") || collision.gameObject.CompareTag("pontos2"))
            {
                gameManager.IcebergCrack(collision.gameObject);
                rb.velocity = Vector2.zero;
                StartCoroutine(ExplosionAnimCoroutine());
            }

            if (collision.gameObject.CompareTag("pin") || collision.gameObject.CompareTag("pon"))
            {
                PinPonPlayerController playerController = collision.gameObject.GetComponent<PinPonPlayerController>();
                if (playerController != null)
                {
                    playerController.Stun(2f);
                    rb.velocity = Vector2.zero;
                    StartCoroutine(ExplosionAnimCoroutine());
                }
            }
                if (collision.gameObject.CompareTag("snowman"))
                {
                    snowman snowman = collision.gameObject.GetComponent<snowman>();
                    if (snowman != null)
                    {
                        snowman.hp = 0;
                        snowman.GetHit();
                        rb.velocity = Vector2.zero;
                        StartCoroutine(ExplosionAnimCoroutine());
                    }
                }

        }

    }
    public void ShowHighlight()
    {
        if (spriteRenderer != null)
        {
            Debug.Log("Mostrando highlight da bomba!");
            spriteRenderer.color = highlightColor;
        }
    }
    public void HideHighlight()
    {
        if (spriteRenderer != null)
        {
            Debug.Log("Escondendo highlight da bomba...");
            spriteRenderer.color = originalColor;
        }
    }
    
    public void Throw(Vector2 direction)
    {
        HideHighlight();
        rb.velocity = direction.normalized * speed;
        isThrown = true;

        int thrownBombLayer = LayerMask.NameToLayer("ThrownBomb");

        if (thrownBombLayer == -1)
        {
            Debug.LogError("Layer 'ThrownBomb' não encontrada. Certifique-se de que a camada existe e está atribuída corretamente.");
        }

        gameObject.layer = thrownBombLayer;

        foreach (Transform child in transform)
        {
            child.gameObject.layer = thrownBombLayer;
        }

            // int currentLayer = gameObject.layer;
            // int[] layersToIgnore = {LayerMask.NameToLayer("tile"), LayerMask.NameToLayer("base")};

            // foreach (int layer in layersToIgnore)
            // {
            //     Physics2D.IgnoreLayerCollision(currentLayer, layer, true);
            // }

    }
    #endregion

    #region Explosion
    private IEnumerator ExplosionCountdownAnimation()
    {
        Debug.Log("Iniciando contagem regressiva da bomba...");
        float initialExplosionTime = explosionTime;

        // Store original visual properties to return to after each blink
        Vector3 originalScale = transform.localScale;
        Color originalColor = spriteRenderer.color;

        // --- Define Animation Limits for a smooth and capped progression ---
        float maxTickInterval = 1.0f; // Longest interval at the beginning
        float minTickInterval = 0.05f; // Shortest interval at the end
        float tickScale = 1.2f; // Max size increase at the end

        while (explosionTime > 0)
        {
            // Calculate normalized progress (0 at start, 1 at the end)
            float progress = 1f - (explosionTime / initialExplosionTime);
            progress = Mathf.Clamp01(progress);

            // --- Calculate interpolated values based on progress ---
            // Interval gets shorter as progress -> 1
            float tickInterval = Mathf.Lerp(maxTickInterval, minTickInterval, progress);
            
            // Wait for the calculated interval
            yield return new WaitForSeconds(tickInterval);
            explosionTime -= tickInterval;

            // Recalculate progress after waiting, for the blink itself
            if (explosionTime < 0) explosionTime = 0;
            progress = 1f - (explosionTime / initialExplosionTime);
            progress = Mathf.Clamp01(progress);

            if (spriteRenderer != null)
            {

                // --- Blink Effect ---
                // Scale multiplier gets larger as progress -> 1
                transform.localScale = originalScale * tickScale;

                // Color gets redder as progress -> 1 (by reducing green and blue channels)
                if (spriteRenderer.color != highlightColor)
                {
                    Debug.Log("Bomba não está destacada, aplicando efeito de piscar...");
                    float greenBlueValue = Mathf.Lerp(1f, 0f, progress);
                    spriteRenderer.color = new Color(1f, greenBlueValue, greenBlueValue);
                }


                // Wait for a short duration for the "blink" to be visible
                float blinkDuration = 0.1f;
                yield return new WaitForSeconds(blinkDuration);
                explosionTime -= blinkDuration; // Account for the time spent blinking

                // Revert to original visuals
                transform.localScale = originalScale;
                if (spriteRenderer.color != highlightColor)
                {
                    spriteRenderer.color = originalColor;
                }
            }
        }

        Explode();
    }

    public void Explode()
    {
        rb.velocity = Vector2.zero;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 2f);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("pontos") || hitCollider.CompareTag("pontos2"))
            {
                gameManager.IcebergCrack(hitCollider.gameObject);
                StartCoroutine(ExplosionAnimCoroutine());
                return;
            }

            if (hitCollider.CompareTag("pin") || hitCollider.CompareTag("pon"))
            {
                PinPonPlayerController playerController = hitCollider.GetComponent<PinPonPlayerController>();
                if (playerController != null)
                {
                    playerController.Stun(2f); 
                }
            }
                if (hitCollider.CompareTag("snowman"))
                {
                    snowman snowman = hitCollider.GetComponent<snowman>();
                    if (snowman != null)
                    {
                        snowman.hp = 0;
                        snowman.GetHit();
                    }
                }

        }
        StartCoroutine(ExplosionAnimCoroutine());
    }
    public IEnumerator ExplosionAnimCoroutine()
    {
        isExploding = true;
        explosionEffect.SetActive(true);
        yield return new WaitForSeconds(0.07f);
        spriteRenderer.enabled = false;
        yield return new WaitForSeconds(0.27f);

        Destroy(gameObject);
        
    }
    #endregion
}