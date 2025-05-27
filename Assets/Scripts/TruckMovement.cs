using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
    
    [Header("État actuel")]
    public bool isMoving = true;
    private float waitTimer = 0f;
    
    [Header("Simulation API")]
    public bool simulateApiCalls = true;
    public float apiCallDelay = 0.5f;
    
    // Variables pour suivre si nous avons déjà envoyé une notification pour ce point
    private bool[] waypointNotified;
    
    void Start()
    {
        // Initialiser le tableau de notifications
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
            // Obtenir le point de passage actuel
            Transform currentWaypoint = waypoints[currentWaypointIndex];
            
            if (currentWaypoint == null)
            {
                Debug.LogWarning("Point de passage " + currentWaypointIndex + " est null");
                return;
            }
            
            // Calculer la direction et déplacer le camion
            Vector3 direction = (currentWaypoint.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
            
            // Orienter le camion dans la direction du mouvement
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
            }
            
            // Vérifier si le camion est arrivé à destination
            float distanceToTarget = Vector3.Distance(transform.position, currentWaypoint.position);
            if (distanceToTarget < waypointReachedDistance)
            {
                // Arrivé à destination, attendre avant de repartir
                isMoving = false;
                waitTimer = waitTimeAtPoints;
                
                // Simuler l'envoi à l'API
                if (simulateApiCalls && !waypointNotified[currentWaypointIndex])
                {
                    StartCoroutine(SimulateApiCall("waypoint" + currentWaypointIndex, 
                        "Le camion est arrivé au point " + currentWaypointIndex));
                    waypointNotified[currentWaypointIndex] = true;
                }
                
                // Passer au point suivant
                currentWaypointIndex++;
                
                // Si on a atteint le dernier point, revenir au début si loopPath est activé
                if (currentWaypointIndex >= waypoints.Count)
                {
                    if (loopPath)
                    {
                        currentWaypointIndex = 0;
                        // Réinitialiser les notifications
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
            // Attendre au point d'arrivée
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isMoving = true;
            }
        }
    }
    
    // Fonction pour simuler un appel API
    private IEnumerator SimulateApiCall(string pointName, string message)
    {
        Debug.Log($"Préparation de l'envoi des données à l'API pour {pointName}...");
        
        // Simuler un délai réseau
        yield return new WaitForSeconds(apiCallDelay);
        
        // Simuler la création d'un payload JSON
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string payload = $"{{ \"truckId\": \"{gameObject.name}\", \"point\": \"{pointName}\", \"timestamp\": \"{timestamp}\" }}";
        
        // Simuler l'envoi et la réponse
        Debug.Log($"API CALL: {message}");
        Debug.Log($"Payload envoyé: {payload}");
        Debug.Log($"Réponse de l'API: Succès - Données enregistrées pour {gameObject.name} à {timestamp}");
    }
    
    // Fonction pour visualiser le chemin dans l'éditeur
    private void OnDrawGizmos()
    {
        if (waypoints.Count == 0) return;
        
        // Dessiner des lignes entre les points de passage
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i+1] != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(waypoints[i].position, waypoints[i+1].position);
            }
        }
        
        // Si le chemin est en boucle, relier le dernier point au premier
        if (loopPath && waypoints.Count > 1 && waypoints[0] != null && waypoints[waypoints.Count-1] != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(waypoints[waypoints.Count-1].position, waypoints[0].position);
        }
        
        // Dessiner des sphères à chaque point de passage
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] != null)
            {
                // Le point actuel est en jaune
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
    }
    
    // Méthode pour ajouter un nouveau point de passage
    public void AddWaypoint(Transform waypoint)
    {
        if (waypoint != null)
        {
            waypoints.Add(waypoint);
            
            // Mettre à jour le tableau de notifications
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