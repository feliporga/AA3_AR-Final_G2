using UnityEngine;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class HoyoMinigolf : MonoBehaviour
{
    [Header("UI Victoria")]
    public TextMeshProUGUI textoVictoria;

    [Header("Sonido")]
    [SerializeField] private AudioClip sonidoEntradaHoyo;
    [SerializeField, Range(0f, 1f)] private float volumenSonido = 1f;
    [SerializeField] private bool sonido3D = true;

    private Collider miCollider;
    private AudioSource audioSource;
    private bool partidaGanada = false;

    private void Start()
    {
        GameObject objetoTexto = GameObject.Find("WonText");

        if (objetoTexto != null)
        {
            textoVictoria = objetoTexto.GetComponent<TextMeshProUGUI>();
        }

        miCollider = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();

        ConfigurarAudioSource();
    }

    private void ConfigurarAudioSource()
    {
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        if (sonido3D)
        {
            audioSource.spatialBlend = 1f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = 0.2f;
            audioSource.maxDistance = 8f;
        }
        else
        {
            audioSource.spatialBlend = 0f;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (partidaGanada) return;

        if (other.CompareTag("Player"))
        {
            partidaGanada = true;

            ReproducirSonidoEntradaHoyo();

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

    private void ReproducirSonidoEntradaHoyo()
    {
        if (sonidoEntradaHoyo == null || audioSource == null) return;

        audioSource.PlayOneShot(sonidoEntradaHoyo, volumenSonido);
    }
}