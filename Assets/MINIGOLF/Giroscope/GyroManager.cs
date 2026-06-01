using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.Events;

namespace GyroTool
{
    public class GyroManager : MonoBehaviour
    {
        public enum RotationState { Idle, Slow, Fast }

        [Header("Debug UI")]
        public TextMeshProUGUI debugText;

        [Header("Rotation Events")]
        public UnityEvent OnFastSpin;
        public UnityEvent OnSlowSpin;
        public UnityEvent OnIdle;

        [Header("Special Events")]
        public UnityEvent OnShake;
        public UnityEvent OnJump;

        [Header("Gyro & Shake Sensitivity")]
        public float minThreshold = 0.2f;
        public float fastThreshold = 1.5f;
        public float requiredShakeTime = 0.4f;

        [Header("Jump Sensitivity (Accelerometer)")]
        public float jumpThreshold = 0.8f;

        private RotationState currentState = RotationState.Idle;
        private RotationState previousState = RotationState.Idle;

        private float shakeTimer = 0f;
        private float shakeCooldown = 0f;
        private float jumpCooldown = 0f;

        private Vector3 previousAcceleration;

        private UnityEngine.InputSystem.Gyroscope gyro;
        private Accelerometer accelerometer;

        void Start()
        {
            gyro = UnityEngine.InputSystem.Gyroscope.current;
            if (gyro != null) InputSystem.EnableDevice(gyro);

            accelerometer = Accelerometer.current;
            if (accelerometer != null) InputSystem.EnableDevice(accelerometer);
        }

        void Update()
        {
            float maxRotationForce = 0f;
            float jerkMagnitude = 0f;
            string spinVisualState = "Idle";
            string shakeText = "Stable";
            string jumpText = "Grounded";

            // giroscopio - Detección de rotación y sacudidas
            if (gyro != null)
            {
                Vector3 spinForce = gyro.angularVelocity.ReadValue();
                maxRotationForce = Mathf.Max(Mathf.Abs(spinForce.x), Mathf.Abs(spinForce.y), Mathf.Abs(spinForce.z));

                if (maxRotationForce >= fastThreshold)
                {
                    spinVisualState = "<color=red>FAST SPIN</color>";
                    currentState = RotationState.Fast;
                    shakeTimer += Time.deltaTime;
                }
                else
                {
                    if (maxRotationForce >= minThreshold)
                    {
                        spinVisualState = "<color=green>Slow Spin</color>";
                        currentState = RotationState.Slow;
                    }
                    else
                    {
                        currentState = RotationState.Idle;
                    }
                    shakeTimer = Mathf.Max(0, shakeTimer - (Time.deltaTime * 2f));
                }

                if (currentState != previousState)
                {
                    switch (currentState)
                    {
                        case RotationState.Fast: OnFastSpin?.Invoke(); break;
                        case RotationState.Slow: OnSlowSpin?.Invoke(); break;
                        case RotationState.Idle: OnIdle?.Invoke(); break;
                    }
                    previousState = currentState;
                }

                // Shake Detection
                if (shakeCooldown > 0)
                {
                    shakeCooldown -= Time.deltaTime;
                    shakeText = "<color=red>SHAKE DETECTED!</color>";
                }
                else if (shakeTimer >= requiredShakeTime)
                {
                    OnShake?.Invoke();
                    shakeCooldown = 1.5f;
                    shakeTimer = 0f;
                    shakeText = "<color=red>SHAKE DETECTED!</color>";
                }
            }

            // Acelerómetro - Detección de salto
            if (accelerometer != null)
            {
                Vector3 currentAcceleration = accelerometer.acceleration.ReadValue();
                Vector3 jerk = currentAcceleration - previousAcceleration;
                jerkMagnitude = jerk.magnitude;

                if (jumpCooldown > 0)
                {
                    jumpCooldown -= Time.deltaTime;
                    jumpText = "<color=orange>JUMP DETECTED!</color>";
                }
                else if (jerkMagnitude > jumpThreshold)
                {
                    OnJump?.Invoke();
                    jumpCooldown = 1.5f;
                    jumpText = "<color=orange>JUMP DETECTED!</color>";
                }
                previousAcceleration = currentAcceleration;
            }

            if (debugText != null)
            {
                debugText.text =
                    $"ROTATION:\nState: {spinVisualState}\nForce: {maxRotationForce:F2}\n\n" +
                    $"SHAKE:\nState: {shakeText}\nCharge: {shakeTimer:F2} / {requiredShakeTime:F2}\n\n" +
                    $"JUMP:\nState: {jumpText}\nJerk: {jerkMagnitude:F2} / {jumpThreshold:F2}";
            }
        }
    }
}