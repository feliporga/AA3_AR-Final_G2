using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class ControladorBola : MonoBehaviour
{
    [Header("Configuración de Golpe")]
    public float fuerzaGolpe = 2.5f;
    private Rigidbody rb;
    private Camera camaraPrincipal;
    [SerializeField] private InputActionReference space;
    [SerializeField] private Vector3 direction = Vector3.forward;

    [SerializeField] private Transform putterVisual;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        camaraPrincipal = Camera.main;

        if (putterVisual == null)
        {
            putterVisual = transform.Find("PivotePutter");
        }

        var managerGiro = FindFirstObjectByType<HerramientaGiroscopio.ManagerGiroscopio>();
        if (managerGiro != null)
        {
            managerGiro.AlSaltar.AddListener(GolpearBola);
        }

        space.action.performed += TestPush;
        space.action.Enable();
    }

    private void TestPush(InputAction.CallbackContext obj)
    {
        rb.AddForce(direction * fuerzaGolpe, ForceMode.Impulse);
    }

    public void GolpearBola()
    {
        
        Vector3 direccionGolpe = camaraPrincipal.transform.forward;
        direccionGolpe.y = 0;
        direccionGolpe.Normalize();

        rb.AddForce(direccionGolpe * fuerzaGolpe, ForceMode.Impulse);
    }

    void Update()
    {
        // Orientar el putter hacia la dirección de la cámara
        if (putterVisual != null && camaraPrincipal != null)
        {
            Vector3 direccionCamara = camaraPrincipal.transform.forward;

            direccionCamara.y = 0;

            if (direccionCamara != Vector3.zero)
            {
                Quaternion rotacionObjetivo = Quaternion.LookRotation(direccionCamara);

                putterVisual.rotation = rotacionObjetivo;
            }
        }
    }
}