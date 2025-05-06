using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class ApiManager : MonoBehaviour
{
    // Singleton pattern
    private static ApiManager _instance;
    public static ApiManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("ApiManager");
                _instance = go.AddComponent<ApiManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    [Header("Configuration API")]
    public string apiBaseUrl = "https://votre-api.com/api";
    public float requestTimeout = 10f;
    
    // Événement pour notifier les autres scripts des résultats des appels API
    public event Action<string, bool, string> OnApiResponse;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    // Méthode pour envoyer des données de position du camion
    public void SendTruckPositionData(string truckId, string pointName, Vector3 position)
    {
        StartCoroutine(SendTruckPositionDataCoroutine(truckId, pointName, position));
    }
    
    private IEnumerator SendTruckPositionDataCoroutine(string truckId, string pointName, Vector3 position)
    {
        // Créer le payload JSON
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string payload = $"{{ \"truckId\": \"{truckId}\", \"point\": \"{pointName}\", \"position\": {{ \"x\": {position.x}, \"y\": {position.y}, \"z\": {position.z} }}, \"timestamp\": \"{timestamp}\" }}";
        
        // Pour l'instant, simulons juste l'appel
        Debug.Log($"[API] Envoi des données pour {truckId} au point {pointName}");
        Debug.Log($"[API] Payload: {payload}");
        
        // Simuler un délai réseau
        yield return new WaitForSeconds(0.5f);
        
        // Simuler une réponse
        bool success = true;
        string response = $"{{ \"status\": \"success\", \"message\": \"Position enregistrée pour {truckId}\" }}";
        
        // Notifier les écouteurs
        OnApiResponse?.Invoke(pointName, success, response);
        
        Debug.Log($"[API] Réponse: {response}");
        
        /* 
        // Code pour une implémentation réelle avec UnityWebRequest
        using (UnityWebRequest request = new UnityWebRequest($"{apiBaseUrl}/truck-positions", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(payload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = Mathf.RoundToInt(requestTimeout);
            
            yield return request.SendWebRequest();
            
            bool success = !request.isNetworkError && !request.isHttpError;
            string response = request.downloadHandler.text;
            
            OnApiResponse?.Invoke(pointName, success, response);
            
            if (success)
            {
                Debug.Log($"[API] Succès: {response}");
            }
            else
            {
                Debug.LogError($"[API] Erreur: {request.error} - {response}");
            }
        }
        */
    }
}