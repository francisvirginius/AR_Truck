using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using MQTTnet;                                    // Ajouter cette ligne
using MQTTnet.Client;                             // Ajouter cette ligne
using System.Threading.Tasks;                      // Ajouter cette ligne
using MQTTnet.Protocol;                           // Ajouter cette ligne

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

    [Header("Configuration MQTT")]
    public string mqttBrokerAddress = "localhost";
    public int mqttBrokerPort = 1883;
    public string mqttTopic = "arduino/coucou";
    private IMqttClient mqttClient;

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

    private async void Start()
    {
        try
        {
            // Initialiser le client MQTT
            var factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(mqttBrokerAddress, mqttBrokerPort)
                .WithClientId($"unity-client-{System.Guid.NewGuid()}")
                .WithCleanSession(true)
                .Build();

            // Se connecter au broker MQTT
            var result = await mqttClient.ConnectAsync(options);

            if (result.ResultCode == MqttClientConnectResultCode.Success)
            {
                Debug.Log("[MQTT] Connexion réussie au broker MQTT");

                // S'abonner au topic
                var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                    .WithTopicFilter(f => f.WithTopic(mqttTopic))
                    .Build();

                await mqttClient.SubscribeAsync(subscribeOptions);
                Debug.Log($"[MQTT] Abonnement au topic: {mqttTopic}");
            }
            else
            {
                Debug.LogError($"[MQTT] Échec de la connexion: {result.ResultCode}");
            }

            // Gérer les messages reçus
            mqttClient.ApplicationMessageReceivedAsync += HandleMqttMessage;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[MQTT] Erreur lors de l'initialisation: {ex.Message}");
        }
    }

    private Task HandleMqttMessage(MqttApplicationMessageReceivedEventArgs args)
    {
        try
        {
            var payload = System.Text.Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
            Debug.Log($"[MQTT] Message reçu sur {args.ApplicationMessage.Topic}: {payload}");

            // Traiter le message reçu ici
            // Vous pouvez ajouter votre logique de traitement ici

        }
        catch (Exception ex)
        {
            Debug.LogError($"[MQTT] Erreur lors du traitement du message: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    // Méthode pour envoyer un message MQTT
    public async Task SendMqttMessage(string message)
    {
        try
        {
            if (mqttClient == null || !mqttClient.IsConnected)
            {
                Debug.LogWarning("[MQTT] Client MQTT non connecté");
                return;
            }

            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(mqttTopic)
                .WithPayload(message)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
                .Build();

            await mqttClient.PublishAsync(applicationMessage);
            Debug.Log($"[MQTT] Message envoyé: {message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[MQTT] Erreur lors de l'envoi du message: {ex.Message}");
        }
    }

    private async void OnDestroy()
    {
        try
        {
            if (mqttClient != null && mqttClient.IsConnected)
            {
                await mqttClient.DisconnectAsync();
                mqttClient.Dispose();
                Debug.Log("[MQTT] Client MQTT déconnecté et libéré");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[MQTT] Erreur lors de la déconnexion: {ex.Message}");
        }
    }
}