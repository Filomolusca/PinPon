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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(ExplosionCountdownAnimation());
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (isThrown)
        {
            if (collision.gameObject.CompareTag("pontos") || collision.gameObject.CompareTag("pontos2"))
            {
                rb.velocity = Vector2.zero;
                Explode();
            }
        }
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (isThrown) return;
        if (collision.gameObject.CompareTag("pin") || collision.gameObject.CompareTag("pon"))
        {
            collision.gameObject.GetComponent<PinPonPlayerController>()._isBombPickable = true;
            spriteRenderer.color = new Color(1f, 0.5f, 0.5f);
        }
    }
    public void OnTriggerExit2D(Collider2D collision)
    {
        if (isThrown) return;
        if (collision.gameObject.CompareTag("pin") || collision.gameObject.CompareTag("pon"))
        {
            collision.gameObject.GetComponent<PinPonPlayerController>()._isBombPickable = false;
            spriteRenderer.color = new Color(1f, 1f, 1f);
        }
    }


    private IEnumerator ExplosionCountdownAnimation()
    {
        while (explosionTime > 0)
        {
            // SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)            
            {
                float alpha = Mathf.PingPong(Time.time * 2f, 1f)*explosionTime/5f;
                float scale = 1f + Mathf.PingPong(Time.time, 1f)*explosionTime/5f;
                float redColor = Mathf.PingPong(Time.time * 2f, 1f)*explosionTime/5f;

                transform.localScale = new Vector3(scale, scale, 1f);
                spriteRenderer.color = new Color(redColor, 1f, 1f, alpha);
            }

            yield return new WaitForSeconds(1f);
            explosionTime -= 1f;


        }

        Explode();
    
    }

    public void Explode()
    {
        explosionEffect.SetActive(true);
        rb.velocity = Vector2.zero;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 3f);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("pin") || hitCollider.CompareTag("pon"))
            {
                PinPonPlayerController playerController = hitCollider.GetComponent<PinPonPlayerController>();
                if (playerController != null)
                {
                    playerController.Stun(2f); 
                }
            }
        }

        Destroy(gameObject);
    }
    
    public void Throw(Vector2 direction)
    {
        rb.velocity = direction.normalized * speed;
        isThrown = true;
    }
}
