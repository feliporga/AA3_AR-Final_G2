using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class BallController : MonoBehaviour
{
    [Header("Hit Force")]
    public float hitForce = 2.5f;

    private Rigidbody rb;
    private Camera mainCamera;

    [SerializeField] private Vector3 direction = Vector3.forward;

    private Transform putterVisual;

    [Header("Hit Audio")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private float hitVolume = 1f;
    [SerializeField] private float hitPitchVariance = 0.05f;
    [SerializeField] private float minTimeBetweenHits = 0.2f;

    [Header("Bounce Audio")]
    [SerializeField] private AudioClip bounceSound;
    [SerializeField] private float bounceVolume = 0.7f;
    [SerializeField] private float bouncePitchVariance = 0.08f;
    [SerializeField] private float minBounceVelocity = 0.4f;
    [SerializeField] private float minTimeBetweenBounces = 0.12f;

    [Header("Audio Settings")]
    [SerializeField] private bool useSpatialAudio = true;

    private AudioSource audioSource;
    private float lastHitTime = -999f;
    private float lastBounceTime = -999f;

    [Header("Game Stats")]
    public int hitCount = 0;

    private GyroTool.GyroManager gyroManager;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        mainCamera = Camera.main;

        SetupAudioSource();

        if (putterVisual == null)
        {
            putterVisual = transform.Find("PivotePutter");
        }

        gyroManager = FindFirstObjectByType<GyroTool.GyroManager>();

        if (gyroManager != null)
        {
            gyroManager.OnJump.AddListener(HitBall);
        }
    }

    private void SetupAudioSource()
    {
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        if (useSpatialAudio)
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

    public void HitBall()
    {
        if (mainCamera == null) return;

        Vector3 hitDirection = mainCamera.transform.forward;
        hitDirection.y = 0f;
        hitDirection.Normalize();

        ApplyHit(hitDirection);
    }

    private void ApplyHit(Vector3 hitDirection)
    {
        if (Time.time - lastHitTime < minTimeBetweenHits) return;
        if (hitDirection == Vector3.zero) return;

        rb.AddForce(hitDirection * hitForce, ForceMode.Impulse);

        PlayHitSound();

        lastHitTime = Time.time;

        hitCount++;
    }

    private void PlayHitSound()
    {
        if (hitSound == null || audioSource == null) return;

        audioSource.pitch = Random.Range(1f - hitPitchVariance, 1f + hitPitchVariance);
        audioSource.PlayOneShot(hitSound, hitVolume);
    }

    private void PlayBounceSound(float intensity)
    {
        if (bounceSound == null || audioSource == null) return;

        audioSource.pitch = Random.Range(1f - bouncePitchVariance, 1f + bouncePitchVariance);

        float finalVolume = Mathf.Clamp01(intensity) * bounceVolume;

        audioSource.PlayOneShot(bounceSound, finalVolume);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time - lastBounceTime < minTimeBetweenBounces) return;

        float impactVelocity = collision.relativeVelocity.magnitude;

        if (impactVelocity < minBounceVelocity) return;

        float intensity = impactVelocity / 4f;

        PlayBounceSound(intensity);

        lastBounceTime = Time.time;
    }

    void Update()
    {
        if (putterVisual != null && mainCamera != null)
        {
            Vector3 cameraDirection = mainCamera.transform.forward;
            cameraDirection.y = 0f;

            if (cameraDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(cameraDirection);
                putterVisual.rotation = targetRotation;
            }
        }
    }

    private void OnDestroy()
    {
        if (gyroManager != null)
        {
            gyroManager.OnJump.RemoveListener(HitBall);
        }
    }
}