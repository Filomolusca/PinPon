using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seagull : MonoBehaviour
{
    public float speed;
    public bool SeagullPinHit = false;
    public bool SeagullPonHit = false;
    public Rigidbody2D rb;
    // public Transform spawnSeagullPin;
    // public Transform spawnSeagullPon;
    // public GameObject seagullPinPrefab;
    // public GameObject seagullPonPrefab;
    public GameManager gameManager;
    public AudioSource audioSource;
    public AudioClip seagullHit;
    // public AudioClip seagullHit;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // spawnPinSeagull = gameManager.spawnSeagull;
        // seagullPinPrefab = gameManager.seagullPinPrefab;
        // seagullPonPrefab = gameManager.seagullPonPrefab;
        SeagullPinHit = false;
        SeagullPonHit = false;
        
                if (!audioSource.enabled)
            {
                audioSource.enabled = true;
            }
    }

    // Update is called once per frame
    void Update()
    {
        rb.velocity = new Vector2(0, speed);
        if (SeagullPinHit)
        {
            speed = 0;
            gameManager.SeagullKilled(this.gameObject);
            SeagullPinHit = false;           
        }
        if (SeagullPonHit)
        {
            speed = 0;
            gameManager.SeagullKilled(this.gameObject);
            SeagullPonHit = false;
        }

    }
    // public void SeagullKilled()
    // {
    //     if (gameObject != null)
    //     {
    //         StartCoroutine(DestroyAndRespawn());
    //     }
    // }

    // private IEnumerator DestroyAndRespawn()
    // {
    //     if (gameObject != null)
    //     {
    //         Debug.Log("Seagull killed!");
    //         Destroy(gameObject);
    //         yield return new WaitForSeconds(5f); // Espera 2 segundos
    //         Instantiate(seagullPrefab, spawnSeagull.position, spawnSeagull.rotation);
    //     }
    // }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("limiteSeagull"))
        {
            Debug.Log("bateu");
            speed = -speed;
        }
        else if (collision.gameObject.CompareTag("bola") || collision.gameObject.CompareTag("bolaOriginal"))    
        {

            audioSource.PlayOneShot(seagullHit);
            Debug.Log("bola bateu no seagull");
            if (this.CompareTag("seagullPin"))
            {
                audioSource.PlayOneShot(seagullHit);
                Debug.Log("gaivota morreu e gritou");
                SeagullPinHit = true;
            }
            else if (this.CompareTag("seagullPon"))
            {
                audioSource.PlayOneShot(seagullHit);
                Debug.Log("gaivota morreu e gritou");
                SeagullPonHit = true;
            }
        }
    }
}
