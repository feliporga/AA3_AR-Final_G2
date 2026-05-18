using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.Events;

namespace HerramientaGiroscopio
{
    public class ManagerGiroscopio : MonoBehaviour
    {
        // Usamos un Enum en lugar de strings para mejorar el rendimiento radicalmente
        public enum EstadoRotacion { Quieto, Lento, Rapido }

        [Header("Interfaz de Reporte")]
        public TextMeshProUGUI visualizadorTexto;

        [Header("Eventos de Rotación")]
        public UnityEvent AlGiroRapido;
        public UnityEvent AlGiroLento;
        public UnityEvent AlEstarQuieto;

        [Header("Eventos Especiales")]
        public UnityEvent AlAgitar;
        public UnityEvent AlSaltar;

        [Header("Sensibilidad Giro y Shake (Giroscopio)")]
        public float umbralMinimo = 0.2f;
        public float umbralRapido = 1.5f;
        public float tiempoNecesarioShake = 0.4f;

        [Header("Sensibilidad Salto (Acelerómetro)")]
        public float umbralSaltar = 0.8f;

        private EstadoRotacion estadoActual = EstadoRotacion.Quieto;
        private EstadoRotacion estadoAnterior = EstadoRotacion.Quieto;

        private float tiempoAgitando = 0f;
        private float enfriamientoShake = 0f;
        private float enfriamientoSaltar = 0f;

        private Vector3 aceleracionAnterior;

        // Caché de sensores para no llamar a .current en cada frame (Optimización)
        private UnityEngine.InputSystem.Gyroscope giroscopio;
        private Accelerometer acelerometro;

        void Start()
        {
            giroscopio = UnityEngine.InputSystem.Gyroscope.current;
            if (giroscopio != null) InputSystem.EnableDevice(giroscopio);

            acelerometro = Accelerometer.current;
            if (acelerometro != null) InputSystem.EnableDevice(acelerometro);
        }

        void Update()
        {
            float fuerzaMaxRotacion = 0f;
            float magnitudTiron = 0f;
            string estadoVisualGiro = "Quieto";
            string textoShake = "Estable";
            string textoSalto = "En el suelo";

            // 1. GIROSCOPIO
            if (giroscopio != null)
            {
                Vector3 fuerzaGiro = giroscopio.angularVelocity.ReadValue();
                fuerzaMaxRotacion = Mathf.Max(Mathf.Abs(fuerzaGiro.x), Mathf.Abs(fuerzaGiro.y), Mathf.Abs(fuerzaGiro.z));

                if (fuerzaMaxRotacion >= umbralRapido)
                {
                    estadoVisualGiro = "<color=red>GIRO RÁPIDO</color>";
                    estadoActual = EstadoRotacion.Rapido;
                    tiempoAgitando += Time.deltaTime;
                }
                else
                {
                    if (fuerzaMaxRotacion >= umbralMinimo)
                    {
                        estadoVisualGiro = "<color=green>Giro Lento</color>";
                        estadoActual = EstadoRotacion.Lento;
                    }
                    else
                    {
                        estadoActual = EstadoRotacion.Quieto;
                    }
                    tiempoAgitando = Mathf.Max(0, tiempoAgitando - (Time.deltaTime * 2f));
                }

                // Disparar Eventos
                if (estadoActual != estadoAnterior)
                {
                    switch (estadoActual)
                    {
                        case EstadoRotacion.Rapido: AlGiroRapido?.Invoke(); break;
                        case EstadoRotacion.Lento: AlGiroLento?.Invoke(); break;
                        case EstadoRotacion.Quieto: AlEstarQuieto?.Invoke(); break;
                    }
                    estadoAnterior = estadoActual;
                }

                // Shake
                if (enfriamientoShake > 0)
                {
                    enfriamientoShake -= Time.deltaTime;
                    textoShake = "<color=red>¡SHAKE DETECTADO!</color>";
                }
                else if (tiempoAgitando >= tiempoNecesarioShake)
                {
                    AlAgitar?.Invoke();
                    enfriamientoShake = 1.5f;
                    tiempoAgitando = 0f;
                    textoShake = "<color=red>¡SHAKE DETECTADO!</color>";
                }
            }

            // 2. ACELERÓMETRO
            if (acelerometro != null)
            {
                Vector3 aceleracionActual = acelerometro.acceleration.ReadValue();
                Vector3 tiron = aceleracionActual - aceleracionAnterior;
                magnitudTiron = tiron.magnitude;

                if (enfriamientoSaltar > 0)
                {
                    enfriamientoSaltar -= Time.deltaTime;
                    textoSalto = "<color=orange>¡JUMP DETECTADO!</color>";
                }
                else if (magnitudTiron > umbralSaltar)
                {
                    AlSaltar?.Invoke();
                    enfriamientoSaltar = 1.5f;
                    textoSalto = "<color=orange>¡JUMP DETECTADO!</color>";
                }
                aceleracionAnterior = aceleracionActual;
            }

            // Actualizar UI de una sola vez
            if (visualizadorTexto != null)
            {
                visualizadorTexto.text =
                    $"ROTACIÓN:\nEstado: {estadoVisualGiro}\nFuerza: {fuerzaMaxRotacion:F2}\n\n" +
                    $"AGITAR:\nEstado: {textoShake}\nCarga: {tiempoAgitando:F2} / {tiempoNecesarioShake:F2}\n\n" +
                    $"SALTAR:\nEstado: {textoSalto}\nTirón físico: {magnitudTiron:F2} / {umbralSaltar:F2}";
            }
        }
    }
}