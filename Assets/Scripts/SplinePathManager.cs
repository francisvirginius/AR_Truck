using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Splines;
using UnityEngine.XR.ARFoundation;  // Ajout de cette ligne pour ARTrackedImage

public class SplinePathManager : MonoBehaviour
{
    public SplineContainer splineContainer;
    public GameObject truckObject;
    public float speed = 5f;
    
    // Ajout du point d'arrêt
    public GameObject stopPoint;
    public float stopDuration = 3f; // Durée de l'arrêt en secondes
    
    private float currentDistance = 0f;
    private float splineLength;
    private bool isMoving = false;
    private bool isStopped = false;
    private float stopTimer = 0f;
    private float stopPointDistance = -1f; // Distance normalisée du point d'arrêt sur la spline
    
    void Start()
    {
        if (splineContainer != null && splineContainer.Spline != null)
        {
            splineLength = splineContainer.CalculateLength();
            Debug.Log("Spline length: " + splineLength);
            
            // Calculer la position du point d'arrêt sur la spline
            if (stopPoint != null)
            {
                CalculateStopPointDistance();
            }
        }
        else
        {
            Debug.LogWarning("Spline container or spline not assigned!");
        }
    }
    
    void CalculateStopPointDistance()
    {
        // Convertir la position du point d'arrêt en float3
        float3 stopPointPosition = new float3(
            stopPoint.transform.position.x,
            stopPoint.transform.position.y,
            stopPoint.transform.position.z
        );
        
        // Variables pour stocker le point le plus proche
        float closestDistance = float.MaxValue;
        float closestT = 0f;
        
        // Nombre d'échantillons à vérifier sur la spline
        int samples = 100;
        
        // Parcourir la spline pour trouver le point le plus proche
        for (int i = 0; i <= samples; i++)
        {
            float t = (float)i / samples;
            
            float3 position;
            float3 tangent;
            float3 up;
            
            // Évaluer la position à ce point de la spline
            splineContainer.Spline.Evaluate(t, out position, out tangent, out up);
            
            // Calculer la distance avec le point d'arrêt
            float distance = math.distance(position, stopPointPosition);
            
            // Si c'est plus proche que ce qu'on a trouvé jusqu'à présent
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestT = t;
            }
        }
        
        stopPointDistance = closestT;
        Debug.Log("Point d'arrêt trouvé à la distance normalisée: " + stopPointDistance);
    }
    
    void Update()
    {
        if (truckObject == null || splineContainer == null)
            return;
            
        if (isStopped)
        {
            // Gérer l'arrêt temporaire
            stopTimer += Time.deltaTime;
            if (stopTimer >= stopDuration)
            {
                isStopped = false;
                isMoving = true;
                Debug.Log("Reprise du mouvement après arrêt");
            }
            return;
        }
            
        if (!isMoving)
            return;
            
        // Avancer le long de la spline
        currentDistance += speed * Time.deltaTime;
        
        // Boucler si on atteint la fin
        if (currentDistance > splineLength)
            currentDistance = 0;
            
        // Calculer la position et la rotation sur la spline
        float normalizedDistance = currentDistance / splineLength;
        
        // Vérifier si on est proche du point d'arrêt
        if (stopPointDistance >= 0 && 
            Mathf.Abs(normalizedDistance - stopPointDistance) < 0.01f && 
            !isStopped)
        {
            isMoving = false;
            isStopped = true;
            stopTimer = 0f;
            Debug.Log("Arrêt au point spécifié");
            return;
        }
        
        // Obtenir la position sur la spline
        float3 position;
        float3 tangent;
        float3 up;
        
        splineContainer.Spline.Evaluate(normalizedDistance, out position, out tangent, out up);
        
        // Appliquer la position et la rotation au camion
        truckObject.transform.position = new Vector3(position.x, position.y, position.z);
        
        // Orienter le camion dans la direction de la tangente
        if (math.lengthsq(tangent) > 0)
        {
            truckObject.transform.rotation = Quaternion.LookRotation(new Vector3(tangent.x, tangent.y, tangent.z));
        }
    }
    
    public void StartMoving()
    {
        isMoving = true;
        isStopped = false;
    }
    
    public void StopMoving()
    {
        isMoving = false;
    }
    
    // Méthode pour créer une spline à partir des routes sur le tapis
    public void CreateSplineFromRoads(ARTrackedImage tapetImage)
    {
        // Cette méthode pourrait être implémentée pour créer automatiquement 
        // une spline basée sur la détection des routes dans l'image du tapis
        Debug.Log("Création de spline à partir de l'image du tapis");
        
        // Pour l'instant, vous pouvez créer manuellement une spline dans l'éditeur
        // et l'assigner au splineContainer
        
        // Recalculer la position du point d'arrêt après création de la spline
        if (stopPoint != null && splineContainer != null && splineContainer.Spline != null)
        {
            CalculateStopPointDistance();
        }
    }
}