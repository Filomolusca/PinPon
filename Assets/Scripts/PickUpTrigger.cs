using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PinPon;

public class PickUpTrigger : MonoBehaviour
{
    public PinPonPlayerController playerController;

    private void Awake()
    {
        Debug.Log("Awake do filho trigger");
        playerController = GetComponentInParent<PinPonPlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PickUpTrigger não encontrou o PinPonPlayerController no pai.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bomb"))
        {
            BombController bomb = other.GetComponent<BombController>();
            if (bomb != null && playerController != null && !bomb.isThrown && bomb.bombLanded)
            {
                Debug.Log("Jogador entrou na área de coleta da bomba!");
                playerController.AddCollectible(bomb);

            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Bomb"))
        {
            BombController bomb = other.GetComponent<BombController>();
            if (bomb != null && playerController != null)
            {
                Debug.Log("Jogador saiu da área de coleta da bomba!");
                playerController.RemoveCollectible(bomb);
            }
        }
    }
}
