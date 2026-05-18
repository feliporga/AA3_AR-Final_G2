using UnityEngine;
using TMPro;

public class HoyoMinigolf : MonoBehaviour
{
    public TextMeshProUGUI textoVictoria;

    

    private void Start()
    {
        GameObject objetoTexto = GameObject.Find("WonText");

        if (objetoTexto != null)
        {
            textoVictoria = objetoTexto.GetComponent<TextMeshProUGUI>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (textoVictoria != null)
            {
                textoVictoria.text = "YOU WON";
                textoVictoria.gameObject.SetActive(true);
            }

            Rigidbody rbBola = other.GetComponent<Rigidbody>();
            if (rbBola != null)
            {
                rbBola.linearVelocity = Vector3.zero;
            }
        }
    }
}