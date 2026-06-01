using UnityEngine;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class GolfHole : MonoBehaviour
{
    [Header("Victory UI")]
    public TextMeshProUGUI victoryText;

    [Header("Audio")]
    [SerializeField] private AudioClip scoreSound;
    [SerializeField] private float soundVolume = 1f;
    [SerializeField] private bool useSpatialAudio = true;

    private Collider holeCollider;
    private AudioSource audioSource;
    private bool hasWon = false;

    private void Start()
    {
        GameObject textObject = GameObject.Find("WonText");

        if (textObject != null)
        {
            victoryText = textObject.GetComponent<TextMeshProUGUI>();
        }

        holeCollider = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();

        SetupAudioSource();
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

    void OnTriggerEnter(Collider other)
    {
        if (hasWon) return;

        if (other.CompareTag("Player"))
        {
            hasWon = true;

            PlayScoreSound();

            if (holeCollider != null)
            {
                Vector3 insidePosition = holeCollider.ClosestPoint(other.transform.position);
                other.transform.position = insidePosition;
            }

            Rigidbody ballRb = other.GetComponent<Rigidbody>();

            if (ballRb != null)
            {
                ballRb.linearVelocity = Vector3.zero;
                ballRb.angularVelocity = Vector3.zero;
                ballRb.isKinematic = true;
            }

            if (victoryText != null)
            {
                BallController ball = other.GetComponent<BallController>();

                if (ball != null)
                {
                    victoryText.text = $"YOU WON!\nStrokes: {ball.hitCount}";
                    ball.hitCount = 0;
                }
                else
                {
                    victoryText.text = "YOU WON!";
                }

                victoryText.gameObject.SetActive(true);
            }
        }
    }

    private void PlayScoreSound()
    {
        if (scoreSound == null || audioSource == null) return;

        audioSource.PlayOneShot(scoreSound, soundVolume);
    }
}