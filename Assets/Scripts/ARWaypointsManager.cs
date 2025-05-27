using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARWaypointsManager : MonoBehaviour
{
    [Header("Configuration des waypoints")]
    public GameObject waypointPrefab;
    public Transform waypointsContainer;
    
    [Header("Positions relatives sur le tapis")]
    [Tooltip("Positions relatives des waypoints par rapport au centre du tapis (x,y,z)")]
    public List<Vector3> waypointRelativePositions = new List<Vector3>();
    
    [Header("Références")]
    public GameObject truckObject;
    
    void Start()
    {
        if (waypointPrefab == null || waypointsContainer == null)
        {
            Debug.LogError("Waypoint prefab ou container non défini!");
            return;
        }
        
        CreateWaypoints();
        AssignWaypointsToTruck();
    }
    
    private void CreateWaypoints()
    {
        // Supprimer les waypoints existants
        foreach (Transform child in waypointsContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Créer de nouveaux waypoints aux positions relatives
        for (int i = 0; i < waypointRelativePositions.Count; i++)
        {
            GameObject waypoint = Instantiate(waypointPrefab, waypointsContainer);
            waypoint.name = "Waypoint_" + i;
            waypoint.transform.localPosition = waypointRelativePositions[i];
        }
    }
    
    private void AssignWaypointsToTruck()
    {
        if (truckObject == null)
        {
            Debug.LogWarning("Objet camion non défini!");
            return;
        }
        
        TruckMovement truckMovement = truckObject.GetComponent<TruckMovement>();
        
        if (truckMovement == null)
        {
            Debug.LogError("Le script TruckMovement n'est pas attaché au camion!");
            return;
        }
        
        // Vider la liste actuelle de waypoints
        truckMovement.waypoints.Clear();
        
        // Ajouter tous les waypoints créés
        foreach (Transform child in waypointsContainer)
        {
            truckMovement.AddWaypoint(child);
        }
        
        Debug.Log($"Assigné {waypointsContainer.childCount} waypoints au camion");
    }
    
    // Méthode pour définir manuellement les positions des waypoints
    public void SetupDefaultWaypoints()
    {
        // Exemple de positions relatives pour un tapis standard
        // Ces valeurs devront être ajustées en fonction de votre tapis spécifique
        waypointRelativePositions.Clear();
        
        // Positions relatives par rapport au centre du tapis
        // Ajustez ces valeurs en fonction de votre tapis
        waypointRelativePositions.Add(new Vector3(-0.4f, 0.01f, -0.3f));  // Coin inférieur gauche
        waypointRelativePositions.Add(new Vector3(-0.4f, 0.01f, 0.0f));   // Milieu gauche
        waypointRelativePositions.Add(new Vector3(-0.2f, 0.01f, 0.3f));   // Haut centre-gauche
        waypointRelativePositions.Add(new Vector3(0.0f, 0.01f, 0.3f));    // Haut centre
        waypointRelativePositions.Add(new Vector3(0.2f, 0.01f, 0.3f));    // Haut centre-droit
        waypointRelativePositions.Add(new Vector3(0.4f, 0.01f, 0.0f));    // Milieu droit
        waypointRelativePositions.Add(new Vector3(0.4f, 0.01f, -0.3f));   // Coin inférieur droit
        waypointRelativePositions.Add(new Vector3(0.0f, 0.01f, -0.3f));   // Bas centre
        
        CreateWaypoints();
        AssignWaypointsToTruck();
    }
}