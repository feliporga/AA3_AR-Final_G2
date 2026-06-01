using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace ARTools.ProceduralGenerator
{
    [RequireComponent(typeof(ARPlaneManager))]
    [RequireComponent(typeof(ARAnchorManager))]
    public class ARLevelGenerator : MonoBehaviour
    {
        public GameObject holePrefab;
        public GameObject ballPrefab;
        public GameObject obstaclePrefab;
        public GameObject floorPrefab;

        public int maxObstacles = 6;

        public Button actionButton;
        public TextMeshProUGUI buttonText;

        private ARPlaneManager planeManager;
        private bool isScanning = false;
        private bool isLevelGenerated = false;

        void Start()
        {
            planeManager = GetComponent<ARPlaneManager>();
            planeManager.enabled = false;
            buttonText.text = "Start Scan";
            actionButton.onClick.AddListener(OnActionButtonPressed);
        }

        void OnActionButtonPressed()
        {
            if (!isScanning && !isLevelGenerated)
            {
                isScanning = true;
                planeManager.enabled = true;
                SetPlanesVisibility(true);
                buttonText.text = "Generate Map";
            }
            else if (isScanning && !isLevelGenerated)
            {
                TryGenerateLevel();
            }
            else if (isLevelGenerated)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        // busca el plano horizontal más grande y genera el nivel sobre él
        void TryGenerateLevel()
        {
            ARPlane bestPlane = null;
            float largestArea = 0f;

            foreach (var plane in planeManager.trackables)
            {
                if (plane.alignment == PlaneAlignment.HorizontalUp)
                {
                    float area = plane.size.x * plane.size.y;
                    if (area > largestArea)
                    {
                        largestArea = area;
                        bestPlane = plane;
                    }
                }
            }

            if (bestPlane != null && largestArea >= 1.0f)
            {
                GenerateDynamicCourse(bestPlane, largestArea);
                isScanning = false;
                isLevelGenerated = true;
                planeManager.enabled = false;
                SetPlanesVisibility(false);
                buttonText.text = "Restart";
            }
            else
            {
                buttonText.text = "Keep scanning...";
                StartCoroutine(RestoreButtonTextRoutine());
            }
        }

        IEnumerator RestoreButtonTextRoutine()
        {
            yield return new WaitForSeconds(2.0f);
            if (isScanning && !isLevelGenerated) buttonText.text = "Generate Map";
        }

        void GenerateDynamicCourse(ARPlane plane, float area)
        {
            Vector3 center = plane.center;

            // suelo
            GameObject spawnedFloor = Instantiate(floorPrefab, center, plane.transform.rotation);
            spawnedFloor.transform.localScale = new Vector3(plane.size.x, 0.01f, plane.size.y);
            spawnedFloor.AddComponent<ARAnchor>();

            // limitar tamaños
            float usableLength = plane.size.y * 0.4f;
            float usableWidth = plane.size.x * 0.4f;

            // hoyo mas alejado del centro, para que el jugador tenga espacio para moverse
            Vector3 holePosition = center + (plane.transform.forward * usableLength);

            // bola cerca del jugador, pero sin que esté justo en el centro para evitar problemas de spawn
            Vector3 devicePosition = Camera.main.transform.position;
            Vector3 directionToDevice = devicePosition - center;
            directionToDevice.y = 0;
            directionToDevice.Normalize();

            float forwardComponent = Vector3.Dot(directionToDevice, plane.transform.forward);
            float rightComponent = Vector3.Dot(directionToDevice, plane.transform.right);

            // limitamos tamaños para que no se salga del plano
            forwardComponent = Mathf.Clamp(forwardComponent, -usableLength, usableLength);
            rightComponent = Mathf.Clamp(rightComponent, -usableWidth, usableWidth);

            // si estamos en el centro de lo escaneado, ponemos la bola al otro lado del hoyo
            if (Mathf.Approximately(forwardComponent, 0) && Mathf.Approximately(rightComponent, 0))
            {
                forwardComponent = -usableLength;
            }

            // calculamos posición final
            Vector3 ballPosition = center + (plane.transform.forward * forwardComponent) + (plane.transform.right * rightComponent);
            ballPosition.y = center.y + 0.05f;

            // spawn bola y hoyo
            SpawnAnchoredObject(holePrefab, holePosition, Quaternion.identity);
            SpawnAnchoredObject(ballPrefab, ballPosition, Quaternion.identity);

            // generar Obstáculos
            int obstacleCount = Mathf.Clamp(Mathf.FloorToInt(area * 1.5f), 1, maxObstacles);
            List<Vector3> occupiedPositions = new List<Vector3> { holePosition, ballPosition };

            for (int i = 0; i < obstacleCount; i++)
            {
                Vector3 attemptedPos = Vector3.zero;
                bool isSpotFree = false;
                int attempts = 0;

                while (!isSpotFree && attempts < 15)
                {
                    float progress = Random.Range(0.2f, 0.8f);
                    Vector3 pointOnPath = Vector3.Lerp(ballPosition, holePosition, progress);
                    float lateralOffset = Random.Range(-usableWidth, usableWidth);
                    attemptedPos = pointOnPath + (plane.transform.right * lateralOffset);

                    isSpotFree = true;
                    foreach (Vector3 occupiedPos in occupiedPositions)
                    {
                        if (Vector3.Distance(attemptedPos, occupiedPos) < 0.4f)
                        {
                            isSpotFree = false;
                            break;
                        }
                    }
                    attempts++;
                }

                if (isSpotFree)
                {
                    SpawnAnchoredObject(obstaclePrefab, attemptedPos, Quaternion.Euler(0, Random.Range(0, 360), 0));
                    occupiedPositions.Add(attemptedPos);
                }
            }
        }

        void SpawnAnchoredObject(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            GameObject newObj = Instantiate(prefab, position, rotation);
            newObj.AddComponent<ARAnchor>();
        }

        void SetPlanesVisibility(bool isVisible)
        {
            foreach (var plane in planeManager.trackables)
            {
                plane.gameObject.SetActive(isVisible);
            }
        }
    }
}