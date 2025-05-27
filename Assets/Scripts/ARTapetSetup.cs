using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.XR.CoreUtils;  // Ajout pour XROrigin

public class ARTapetSetup : MonoBehaviour
{
    [Header("Références AR")]
    public ARSession arSession;
    public ARSessionOrigin arSessionOrigin;  // Gardé pour compatibilité
    public XROrigin xrOrigin;  // Ajout de XROrigin
    public ARTrackedImageManager trackedImageManager;
    
    [Header("Références du jeu")]
    public GameObject truckPrefab;
    public GameObject waypointsContainer;
    
    private GameObject spawnedTruck;
    private bool tapetDetected = false;
    
    void Awake()
    {
        // Vérifier que tous les composants AR sont présents
        if (arSession == null)
            arSession = FindObjectOfType<ARSession>();
        
        // Chercher soit ARSessionOrigin soit XROrigin
        if (arSessionOrigin == null && xrOrigin == null)
        {
            arSessionOrigin = FindObjectOfType<ARSessionOrigin>();
            xrOrigin = FindObjectOfType<XROrigin>();
        }
        
        if (trackedImageManager == null)
            trackedImageManager = FindObjectOfType<ARTrackedImageManager>();
    }
    
    void OnEnable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }
    
    void OnDisable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }
    
    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            // Vérifier si l'image détectée est notre tapis
            if (trackedImage.referenceImage.name == "TapisTruck")
            {
                HandleTapetDetection(trackedImage);
            }
        }
        
        foreach (var trackedImage in eventArgs.updated)
        {
            // Mettre à jour la position si le tapis bouge
            if (trackedImage.referenceImage.name == "TapisTruck" && tapetDetected)
            {
                UpdateTapetPosition(trackedImage);
            }
        }
    }
    
    private void HandleTapetDetection(ARTrackedImage trackedImage)
    {
        Debug.Log("Tapis détecté!");
        tapetDetected = true;
        
        // Créer le camion s'il n'existe pas déjà
        if (spawnedTruck == null && truckPrefab != null)
        {
            // Positionner le camion au point de départ sur le tapis
            spawnedTruck = Instantiate(truckPrefab, trackedImage.transform);
            
            // Si vous utilisez le système de Splines
            SplinePathManager splineManager = waypointsContainer.GetComponent<SplinePathManager>();
            if (splineManager != null)
            {
                splineManager.truckObject = spawnedTruck;
                splineManager.CreateSplineFromRoads(trackedImage);
                splineManager.StartMoving();
            }
            // Sinon, utiliser le système de waypoints existant
            else
            {
                ARWaypointsManager waypointsManager = waypointsContainer.GetComponent<ARWaypointsManager>();
                if (waypointsManager != null)
                {
                    waypointsManager.truckObject = spawnedTruck;
                    waypointsManager.SetupDefaultWaypoints();
                }
                
                // Positionner le camion au premier waypoint
                PositionTruckAtFirstWaypoint();
            }
        }
        
        // Positionner les waypoints sur le tapis
        PositionWaypointsOnTapet(trackedImage);
    }
    
    private void UpdateTapetPosition(ARTrackedImage trackedImage)
    {
        // Mettre à jour la position des waypoints si le tapis bouge
        if (waypointsContainer != null)
        {
            waypointsContainer.transform.position = trackedImage.transform.position;
            waypointsContainer.transform.rotation = trackedImage.transform.rotation;
        }
    }
    
    private void PositionTruckAtFirstWaypoint()
    {
        if (spawnedTruck != null && waypointsContainer != null)
        {
            TruckMovement truckMovement = spawnedTruck.GetComponent<TruckMovement>();
            
            if (truckMovement != null && truckMovement.waypoints.Count > 0)
            {
                // Positionner le camion au premier waypoint
                spawnedTruck.transform.position = truckMovement.waypoints[0].position;
                
                // Orienter le camion vers le deuxième waypoint s'il existe
                if (truckMovement.waypoints.Count > 1)
                {
                    Vector3 direction = truckMovement.waypoints[1].position - truckMovement.waypoints[0].position;
                    if (direction != Vector3.zero)
                    {
                        spawnedTruck.transform.rotation = Quaternion.LookRotation(direction);
                    }
                }
            }
        }
    }
    
    private void PositionWaypointsOnTapet(ARTrackedImage trackedImage)
    {
        if (waypointsContainer != null)
        {
            // Positionner le conteneur de waypoints sur le tapis
            waypointsContainer.transform.position = trackedImage.transform.position;
            waypointsContainer.transform.rotation = trackedImage.transform.rotation;
            
            // Ajuster l'échelle si nécessaire
            float tapetWidth = trackedImage.size.x;
            float tapetRealWidth = 1.0f; // Largeur réelle du tapis en mètres (à ajuster)
            float scale = tapetWidth / tapetRealWidth;
            waypointsContainer.transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}