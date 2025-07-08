using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TruckMovement : MonoBehaviour
{
    [Header("Points de déplacement")]
    public List<Transform> waypoints = new List<Transform>();
    private int currentWaypointIndex = 0;

    [Header("Paramètres de mouvement")]
    public float speed = 1.0f;
    public float waitTimeAtPoints = 2.0f;
    public bool loopPath = true;
    public float waypointReachedDistance = 0.1f;

    [Header("Paramètres de suivi de route")]
    public List<Transform> roadPoints = new List<Transform>();  
    public float steeringSpeed = 3.0f; 
    public bool followRoad = true;  

    [Header("État actuel")]
    public bool isMoving = true;
    private float waitTimer = 0f;
    private int currentRoadPointIndex = 0; 

    [Header("Simulation API")]
    public bool simulateApiCalls = true;
    public float apiCallDelay = 0.5f;

    private bool[] waypointNotified;

    [Header("UI")]
    public GameObject popupPrefab;  
    private GameObject currentPopup; 

    [Header("Données de la poubelle")]
    public Dictionary<string, string> trashData = new Dictionary<string, string>()
    {
        { "Poids", "25 kg" },
        { "Type", "Déchets ménagers" },
        { "Dernière collecte", "15/06/2023" },
        { "État", "75% pleine" }
    };

    void Start()
    {
        if (waypoints.Count > 0)
        {
            waypointNotified = new bool[waypoints.Count];
        }
    }

    void Update()
    {
        if (waypoints.Count == 0)
        {
            Debug.LogWarning("Aucun point de passage défini pour " + gameObject.name);
            return;
        }

        if (isMoving)
        {
            Transform currentWaypoint = waypoints[currentWaypointIndex];

            if (currentWaypoint == null)
            {
                Debug.LogWarning("Point de passage " + currentWaypointIndex + " est null");
                return;
            }

            Vector3 targetPosition;

            if (followRoad && roadPoints.Count > 0)
            {
                targetPosition = GetCurrentTargetPosition();
            }
            else
            {
                targetPosition = currentWaypoint.position;
            }

            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, steeringSpeed * Time.deltaTime);
            }

            if (followRoad && roadPoints.Count > 0)
            {
                float distanceToRoadPoint = Vector3.Distance(transform.position, targetPosition);
                if (distanceToRoadPoint < waypointReachedDistance)
                {
                    UpdateRoadPointIndex();
                }
            }

            float distanceToTarget = Vector3.Distance(transform.position, currentWaypoint.position);
            if (distanceToTarget < waypointReachedDistance)
            {
                isMoving = false;
                waitTimer = waitTimeAtPoints;

                if (simulateApiCalls && !waypointNotified[currentWaypointIndex])
                {
                    StartCoroutine(SimulateApiCall("waypoint" + currentWaypointIndex,
                        "Le camion est arrivé au point " + currentWaypointIndex));
                    waypointNotified[currentWaypointIndex] = true;
                }

                currentWaypointIndex++;

                ResetRoadPointsForNewSegment();

                if (currentWaypointIndex >= waypoints.Count)
                {
                    if (loopPath)
                    {
                        currentWaypointIndex = 0;
                        for (int i = 0; i < waypointNotified.Length; i++)
                        {
                            waypointNotified[i] = false;
                        }
                    }
                    else
                    {
                        currentWaypointIndex = waypoints.Count - 1;
                        Debug.Log("Fin du parcours atteinte");
                    }
                }
            }
        }
        else
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isMoving = true;
            }
        }
    }

    private Vector3 GetCurrentTargetPosition()
    {
        if (followRoad && roadPoints.Count > 0 && currentRoadPointIndex < roadPoints.Count)
        {
            return roadPoints[currentRoadPointIndex].position;
        }

        return waypoints[currentWaypointIndex].position;
    }

    // Modifier la méthode UpdateRoadPointIndex pour ajouter l'affichage de la popup
    private void UpdateRoadPointIndex()
    {
        if (roadPoints.Count > 0)
        {
            ShowPopup();
            currentRoadPointIndex++;

            // Si on a dépassé le dernier point de route, passer au waypoint
            if (currentRoadPointIndex >= roadPoints.Count)
            {
                currentRoadPointIndex = 0;
            }
        }
    }

    // Nouvelle méthode pour afficher la popup
    private void ShowPopup()
    {
        if (currentPopup != null)
        {
            Destroy(currentPopup);
        }

        // Créer une nouvelle popup
        currentPopup = Instantiate(popupPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);

        // Configurer le contenu de la popup
        TextMeshProUGUI[] texts = currentPopup.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length >= 2)  // On suppose qu'il y a au moins un titre et un contenu
        {
            // Configurer le titre
            texts[0].text = "Information Poubelle";

            // Configurer le contenu
            string content = "";
            foreach (var data in trashData)
            {
                content += $"{data.Key}: {data.Value}\n";
            }
            texts[1].text = content;
        }

        // Faire disparaître la popup après quelques secondes
        StartCoroutine(HidePopupAfterDelay(3f));
    }

    private IEnumerator HidePopupAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (currentPopup != null)
        {
            Destroy(currentPopup);
            currentPopup = null;
        }
    }

    // Réinitialiser les points de route pour un nouveau segment
    private void ResetRoadPointsForNewSegment()
    {
        currentRoadPointIndex = 0;
    }

    // Fonction pour simuler un appel API
    // Dans la méthode SimulateApiCall, ajoutez :
    private IEnumerator SimulateApiCall(string pointName, string message)
    {
        // ... existing code ...

        // Envoyer les données via MQTT
        await ApiManager.Instance.SendMqttMessage(payload);

        Debug.Log($"API CALL: {message}");
        Debug.Log($"Payload envoyé: {payload}");
        Debug.Log($"Réponse de l'API: Succès - Données enregistrées pour {gameObject.name} à {timestamp}");
    }

    private void OnDrawGizmos()
    {
        if (waypoints.Count == 0) return;

        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }

        if (loopPath && waypoints.Count > 1 && waypoints[0] != null && waypoints[waypoints.Count - 1] != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(waypoints[waypoints.Count - 1].position, waypoints[0].position);
        }

        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] != null)
            {
                if (i == currentWaypointIndex)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(waypoints[i].position, 0.3f);
                }
                else
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(waypoints[i].position, 0.2f);
                }
            }
        }

        if (followRoad && roadPoints.Count > 0)
        {
            foreach (Transform roadPoint in roadPoints)
            {
                if (roadPoint != null)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(roadPoint.position, 0.15f);
                }
            }
        }
    }

    // Méthode pour ajouter un nouveau point de passage
    public void AddWaypoint(Transform waypoint)
    {
        if (waypoint != null)
        {
            waypoints.Add(waypoint);

            if (waypointNotified == null || waypointNotified.Length != waypoints.Count)
            {
                bool[] newNotified = new bool[waypoints.Count];
                if (waypointNotified != null)
                {
                    for (int i = 0; i < waypointNotified.Length && i < newNotified.Length; i++)
                    {
                        newNotified[i] = waypointNotified[i];
                    }
                }
                waypointNotified = newNotified;
            }
        }
    }
}