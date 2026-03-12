using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PinPon;

public class BombTrigger : MonoBehaviour
{
    private GameObject bombParentObject;
    private BombController bombParentScript;


    private void Awake()
    {
        bombParentObject = transform.parent.gameObject;
        bombParentScript = bombParentObject.GetComponent<BombController>();
        if (bombParentScript == null)
        {
            Debug.LogError("BombTrigger não encontrou o BombController no pai.");
        }
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("pin") || other.CompareTag("pon"))
        {
            PinPonPlayerController playerController = other.gameObject.GetComponent<PinPonPlayerController>();
                if (playerController != null)
                {
                    playerController.Stun(2f); 
                    bombParentScript.Explode();
                }
        }

    }
}
