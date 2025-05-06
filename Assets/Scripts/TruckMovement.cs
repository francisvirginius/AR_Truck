using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TruckMovement : MonoBehaviour
{
    [Header("Points de déplacement")]
    public Transform pointA;
    public Transform pointB;
    
    [Header("Paramètres de mouvement")]
    public float speed = 1.0f;
    public float waitTimeAtPoints = 2.0f;
    public bool loopMovement = true;
    
    [Header("État actuel")]
    public bool isMoving = true;
    private bool movingToB = true;
    private float waitTimer = 0f;
    
    [Header("Simulation API")]
    public bool simulateApiCalls = true;
    public float apiCallDelay = 0.5f;
    
    // Variables pour suivre si nous avons déjà envoyé une notification pour ce point
    private bool notifiedPointA = false;
    private bool notifiedPointB = false;
    
    void Update()
    {
        if (!pointA || !pointB)
        {
            Debug.LogWarning("Points de déplacement non définis sur " + gameObject.name);
            return;
        }
        
        if (isMoving)
        {
            // Déterminer la destination actuelle
            Transform targetPoint = movingToB ? pointB : pointA;
            Transform startPoint = movingToB ? pointA : pointB;
            
            // Calculer la direction et déplacer le camion
            Vector3 direction = (targetPoint.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
            
            // Orienter le camion dans la direction du mouvement
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
            }
            
            // Vérifier si le camion est arrivé à destination
            float distanceToTarget = Vector3.Distance(transform.position, targetPoint.position);
            if (distanceToTarget < 0.1f)
            {
                // Arrivé à destination, attendre avant de repartir
                isMoving = false;
                waitTimer = waitTimeAtPoints;
                
                // Simuler l'envoi à l'API
                if (simulateApiCalls)
                {
                    if (movingToB && !notifiedPointB)
                    {
                        StartCoroutine(SimulateApiCall("pointB", "Le camion est arrivé au point B"));
                        notifiedPointB = true;
                        notifiedPointA = false; // Réinitialiser pour le prochain passage
                    }
                    else if (!movingToB && !notifiedPointA)
                    {
                        StartCoroutine(SimulateApiCall("pointA", "Le camion est arrivé au point A"));
                        notifiedPointA = true;
                        notifiedPointB = false; // Réinitialiser pour le prochain passage
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
                if (loopMovement)
                {
                    // Inverser la direction et recommencer à bouger
                    movingToB = !movingToB;
                    isMoving = true;
                }
                else if (movingToB)
                {
                    // Si on ne boucle pas, on s'arrête après avoir atteint le point B
                    movingToB = false;
                    isMoving = true;
                }
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
        
        // Vous pourriez ajouter ici un événement que d'autres scripts pourraient écouter
        // par exemple: OnApiCallCompleted?.Invoke(pointName, true);
    }
    
    // Fonction pour visualiser le chemin dans l'éditeur
    private void OnDrawGizmos()
    {
        if (pointA && pointB)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(pointA.position, pointB.position);
            
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(pointA.position, 0.2f);
            
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(pointB.position, 0.2f);
        }
    }
}