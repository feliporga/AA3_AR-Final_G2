using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class ControladorBola : MonoBehaviour
{
    [Header("Configuración de Golpe")]
    public float fuerzaGolpe = 2.5f;
    private Rigidbody rb;
    private Camera camaraPrincipal;
    //[SerializeField] private InputActionReference space;
    [SerializeField] private Vector3 direction = Vector3.forward;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        camaraPrincipal = Camera.main;

        
        var managerGiro = FindFirstObjectByType<HerramientaGiroscopio.ManagerGiroscopio>();
        if (managerGiro != null)
        {
            managerGiro.AlSaltar.AddListener(GolpearBola);
        }

        //space.action.performed += TestPush;
        //space.action.Enable();
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
}