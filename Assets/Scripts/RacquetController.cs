using UnityEngine;

public class BatController : MonoBehaviour
{
    // Esta função será chamada por um Animation Event no final da animação da raquete.
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
