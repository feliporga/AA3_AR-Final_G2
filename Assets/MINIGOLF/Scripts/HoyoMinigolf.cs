using UnityEngine;
using TMPro;

public class HoyoMinigolf : MonoBehaviour
{
    public TextMeshProUGUI textoVictoria;
    private Collider miCollider;

    private void Start()
    {
        GameObject objetoTexto = GameObject.Find("WonText");

        if (objetoTexto != null)
        {
            textoVictoria = objetoTexto.GetComponent<TextMeshProUGUI>();
        }

        miCollider = GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (miCollider != null)
            {
                Vector3 posicionDentro = miCollider.ClosestPoint(other.transform.position);
                other.transform.position = posicionDentro;
            }

            Rigidbody rbBola = other.GetComponent<Rigidbody>();
            if (rbBola != null)
            {
                rbBola.linearVelocity = Vector3.zero;
                rbBola.angularVelocity = Vector3.zero;
                rbBola.isKinematic = true;
            }

            if (textoVictoria != null)
            {
                textoVictoria.text = "YOU WON";
                textoVictoria.gameObject.SetActive(true);
            }
        }
    }
}