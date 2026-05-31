using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace HerramientasAR.GeneradorProcedural
{
    [RequireComponent(typeof(ARPlaneManager))]
    [RequireComponent(typeof(ARAnchorManager))]
    public class GeneradorNivelAR : MonoBehaviour
    {
        public GameObject prefabFinal;
        public GameObject prefabPrincipio;
        public GameObject prefabObstaculo;
        public GameObject prefabSuelo;

        public int ObstaculosMax = 6;

        public Button botonAccion;
        public TextMeshProUGUI textoBoton;

        private ARPlaneManager planeManager;
        private bool estaEscaneando = false;
        private bool nivelGenerado = false;

        void Start()
        {
            planeManager = GetComponent<ARPlaneManager>();
            planeManager.enabled = false;
            textoBoton.text = "Start Scan";
            botonAccion.onClick.AddListener(AlPulsarBoton);
        }

        void AlPulsarBoton()
        {
            if (!estaEscaneando && !nivelGenerado)
            {
                estaEscaneando = true;
                planeManager.enabled = true;
                CambiarVisibilidadPlanos(true);
                textoBoton.text = "Generar Mapa";
            }
            else if (estaEscaneando && !nivelGenerado)
            {
                IntentarGenerarNivel();
            }
            else if (nivelGenerado)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        void IntentarGenerarNivel()
        {
            ARPlane mejorPlano = null;
            float mayorArea = 0f;

            foreach (var plano in planeManager.trackables)
            {
                if (plano.alignment == PlaneAlignment.HorizontalUp)
                {
                    float area = plano.size.x * plano.size.y;
                    if (area > mayorArea)
                    {
                        mayorArea = area;
                        mejorPlano = plano;
                    }
                }
            }

            if (mejorPlano != null && mayorArea >= 1.0f)
            {
                GenerarMinigolfDinamico(mejorPlano, mayorArea);
                estaEscaneando = false;
                nivelGenerado = true;
                planeManager.enabled = false;
                CambiarVisibilidadPlanos(false);
                textoBoton.text = "Restart";
            }
            else
            {
                textoBoton.text = "Sigue escaneando...";
                StartCoroutine(RestaurarTextoBotonCoroutine());
            }
        }

        IEnumerator RestaurarTextoBotonCoroutine()
        {
            yield return new WaitForSeconds(2.0f);
            if (estaEscaneando && !nivelGenerado) textoBoton.text = "Generar Mapa";
        }

        void GenerarMinigolfDinamico(ARPlane plano, float area)
        {
            Vector3 centro = plano.center;

            // 1. Instanciar el suelo
            GameObject sueloInstanciado = Instantiate(prefabSuelo, centro, plano.transform.rotation);
            sueloInstanciado.transform.localScale = new Vector3(plano.size.x, 0.01f, plano.size.y);
            sueloInstanciado.AddComponent<ARAnchor>();

            // Límites seguros (40% del tamaño total desde el centro)
            float largoUtil = plano.size.y * 0.4f;
            float anchoUtil = plano.size.x * 0.4f;

            // El hoyo se queda en el extremo "frontal" del plano
            Vector3 posicionHoyo = centro + (plano.transform.forward * largoUtil);

            // --- ¡AQUÍ ESTÁ LA CORRECCIÓN DEL BUG! ---
            // 2. Calcular la posición de la bola de forma segura dentro del plano
            Vector3 posicionDispositivo = Camera.main.transform.position;

            // Conseguimos la dirección horizontal desde el centro del plano hacia el móvil
            Vector3 direccionHaciaMovil = posicionDispositivo - centro;
            direccionHaciaMovil.y = 0; // Aplanamos el vector en el eje vertical
            direccionHaciaMovil.Normalize();

            // Proyectamos esa dirección en los ejes locales del plano para no salirnos de los bordes
            float componenteForward = Vector3.Dot(direccionHaciaMovil, plano.transform.forward);
            float componenteRight = Vector3.Dot(direccionHaciaMovil, plano.transform.right);

            // Limitamos los componentes para que no superen los metros útiles de la pista
            componenteForward = Mathf.Clamp(componenteForward, -largoUtil, largoUtil);
            componenteRight = Mathf.Clamp(componenteRight, -anchoUtil, anchoUtil);

            // Si por alguna razón estás justo en el centro o da 0, la ponemos por defecto en el extremo opuesto al hoyo
            if (Mathf.Approximately(componenteForward, 0) && Mathf.Approximately(componenteRight, 0))
            {
                componenteForward = -largoUtil;
            }

            // Calculamos la posición final sumando los desplazamientos seguros al centro del plano
            Vector3 posicionBola = centro + (plano.transform.forward * componenteForward) + (plano.transform.right * componenteRight);
            posicionBola.y = centro.y + 0.05f; // Un pelín por encima del suelo para que caiga bien
                                               // ----------------------------------------

            // 3. Spawnear elementos principales
            CrearObjetoAnclado(prefabFinal, posicionHoyo, Quaternion.identity);
            CrearObjetoAnclado(prefabPrincipio, posicionBola, Quaternion.identity);

            // 4. Generar Obstáculos (El resto de tu lógica se queda igual)
            int numObstaculos = Mathf.Clamp(Mathf.FloorToInt(area * 1.5f), 1, ObstaculosMax);
            List<Vector3> posicionesOcupadas = new List<Vector3> { posicionHoyo, posicionBola };

            for (int i = 0; i < numObstaculos; i++)
            {
                Vector3 posIntentada = Vector3.zero;
                bool sitioLibre = false;
                int intentos = 0;

                while (!sitioLibre && intentos < 15)
                {
                    float avance = Random.Range(0.2f, 0.8f);
                    Vector3 puntoEnElCamino = Vector3.Lerp(posicionBola, posicionHoyo, avance);
                    float desvioLateral = Random.Range(-anchoUtil, anchoUtil);
                    posIntentada = puntoEnElCamino + (plano.transform.right * desvioLateral);

                    sitioLibre = true;
                    foreach (Vector3 posOcupada in posicionesOcupadas)
                    {
                        if (Vector3.Distance(posIntentada, posOcupada) < 0.4f)
                        {
                            sitioLibre = false;
                            break;
                        }
                    }
                    intentos++;
                }

                if (sitioLibre)
                {
                    CrearObjetoAnclado(prefabObstaculo, posIntentada, Quaternion.Euler(0, Random.Range(0, 360), 0));
                    posicionesOcupadas.Add(posIntentada);
                }
            }
        }

        void CrearObjetoAnclado(GameObject prefab, Vector3 posicion, Quaternion rotacion)
        {
            GameObject nuevoObjeto = Instantiate(prefab, posicion, rotacion);
            nuevoObjeto.AddComponent<ARAnchor>();
        }

        void CambiarVisibilidadPlanos(bool visible)
        {
            foreach (var plano in planeManager.trackables)
            {
                plano.gameObject.SetActive(visible);
            }
        }
    }
}