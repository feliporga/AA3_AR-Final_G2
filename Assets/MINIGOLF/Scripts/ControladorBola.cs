using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class ControladorBola : MonoBehaviour
{
    [Header("Configuración de Golpe")]
    public float fuerzaGolpe = 2.5f;

    private Rigidbody rb;
    private Camera camaraPrincipal;

    [SerializeField] private InputActionReference space;
    [SerializeField] private Vector3 direction = Vector3.forward;

    [SerializeField] private Transform putterVisual;

    [Header("Sonido de Golpe")]
    [SerializeField] private AudioClip sonidoGolpe;
    [SerializeField, Range(0f, 1f)] private float volumenGolpe = 1f;
    [SerializeField, Range(0f, 0.3f)] private float variacionPitchGolpe = 0.05f;
    [SerializeField] private float tiempoMinimoEntreGolpes = 0.2f;

    [Header("Sonido de Rebote")]
    [SerializeField] private AudioClip sonidoRebote;
    [SerializeField, Range(0f, 1f)] private float volumenRebote = 0.7f;
    [SerializeField, Range(0f, 0.3f)] private float variacionPitchRebote = 0.08f;
    [SerializeField] private float velocidadMinimaRebote = 0.4f;
    [SerializeField] private float tiempoMinimoEntreRebotes = 0.12f;

    [Header("Configuración Audio")]
    [SerializeField] private bool sonido3D = true;

    private AudioSource audioSource;
    private float ultimoGolpe = -999f;
    private float ultimoRebote = -999f;

    private HerramientaGiroscopio.ManagerGiroscopio managerGiro;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        camaraPrincipal = Camera.main;

        ConfigurarAudioSource();

        if (putterVisual == null)
        {
            putterVisual = transform.Find("PivotePutter");
        }

        managerGiro = FindFirstObjectByType<HerramientaGiroscopio.ManagerGiroscopio>();

        if (managerGiro != null)
        {
            managerGiro.AlSaltar.AddListener(GolpearBola);
        }

        if (space != null)
        {
            space.action.performed += TestPush;
            space.action.Enable();
        }
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

    private void TestPush(InputAction.CallbackContext obj)
    {
        AplicarGolpe(direction);
    }

    public void GolpearBola()
    {
        if (camaraPrincipal == null) return;

        Vector3 direccionGolpe = camaraPrincipal.transform.forward;
        direccionGolpe.y = 0f;
        direccionGolpe.Normalize();

        AplicarGolpe(direccionGolpe);
    }

    private void AplicarGolpe(Vector3 direccionGolpe)
    {
        if (Time.time - ultimoGolpe < tiempoMinimoEntreGolpes) return;
        if (direccionGolpe == Vector3.zero) return;

        rb.AddForce(direccionGolpe * fuerzaGolpe, ForceMode.Impulse);

        ReproducirSonidoGolpe();

        ultimoGolpe = Time.time;
    }

    private void ReproducirSonidoGolpe()
    {
        if (sonidoGolpe == null || audioSource == null) return;

        audioSource.pitch = Random.Range(1f - variacionPitchGolpe, 1f + variacionPitchGolpe);
        audioSource.PlayOneShot(sonidoGolpe, volumenGolpe);
    }

    private void ReproducirSonidoRebote(float intensidad)
    {
        if (sonidoRebote == null || audioSource == null) return;

        audioSource.pitch = Random.Range(1f - variacionPitchRebote, 1f + variacionPitchRebote);

        float volumenFinal = Mathf.Clamp01(intensidad) * volumenRebote;

        audioSource.PlayOneShot(sonidoRebote, volumenFinal);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time - ultimoRebote < tiempoMinimoEntreRebotes) return;

        float velocidadImpacto = collision.relativeVelocity.magnitude;

        if (velocidadImpacto < velocidadMinimaRebote) return;

        float intensidad = velocidadImpacto / 4f;

        ReproducirSonidoRebote(intensidad);

        ultimoRebote = Time.time;
    }

    void Update()
    {
        if (putterVisual != null && camaraPrincipal != null)
        {
            Vector3 direccionCamara = camaraPrincipal.transform.forward;
            direccionCamara.y = 0f;

            if (direccionCamara != Vector3.zero)
            {
                Quaternion rotacionObjetivo = Quaternion.LookRotation(direccionCamara);
                putterVisual.rotation = rotacionObjetivo;
            }
        }
    }

    private void OnDestroy()
    {
        if (managerGiro != null)
        {
            managerGiro.AlSaltar.RemoveListener(GolpearBola);
        }

        if (space != null)
        {
            space.action.performed -= TestPush;
            space.action.Disable();
        }
    }
}