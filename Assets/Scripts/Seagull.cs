using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seagull : MonoBehaviour
{
    public float speed;
    public bool SeagullPinHit = false;
    public bool SeagullPonHit = false;
    public Rigidbody2D rb;

    public GameManager gameManager;

    // public AudioClip seagullHit;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        SeagullPinHit = false;
        SeagullPonHit = false;
        
    }
    void OnEnable()
    {
        gameManager = FindObjectOfType<GameManager>();   
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("limiteSeagull"))
        {
            Debug.Log("bateu");
            speed = -speed;
        }
        else if (collision.gameObject.CompareTag("bola"))    
        {

            Debug.Log("bola bateu no seagull");
            if (this.CompareTag("seagullPin"))
            {
                Debug.Log("gaivota morreu e gritou");
                SeagullPinHit = true;
            }
            else if (this.CompareTag("seagullPon"))
            {
                Debug.Log("gaivota morreu e gritou");
                SeagullPonHit = true;
            }
        }
    }
}
