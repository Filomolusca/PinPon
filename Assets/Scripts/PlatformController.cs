using UnityEngine;
using System.Collections;

public class PlatformController : MonoBehaviour
{
    private PlatformEffector2D effector;
    public float waitTime = 0.5f;

    void Start()
    {
        effector = GetComponent<PlatformEffector2D>();
    }

    public void Drop()
    {
        StartCoroutine(DisableCollision());
    }

    private IEnumerator DisableCollision()
    {
        // Vira o effector para baixo para o jogador cair
        effector.rotationalOffset = 180f;
        // Espera um pouco
        yield return new WaitForSeconds(waitTime);
        // Volta ao normal para o jogador poder pousar em cima de novo
        effector.rotationalOffset = 0f;
    }
}
