using System.Collections;
using UnityEngine;

public class TruckController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Position de départ du camion")]
    public Vector3 startPosition = new Vector3(0, 0, 0);
    
    [Tooltip("Position d'arrivée du camion")]
    public Vector3 endPosition = new Vector3(0, 0, 10);
    
    [Tooltip("Vitesse de déplacement du camion")]
    public float speed = 1.0f;
    
    [Tooltip("Délai avant le démarrage du mouvement")]
    public float startDelay = 2.0f;
    
    [Tooltip("Le camion doit-il se déplacer automatiquement au démarrage?")]
    public bool autoStart = false;
    
    private bool isMoving = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (autoStart)
        {
            StartMovement();
        }
    }
    
    // Méthode publique pour démarrer le mouvement du camion
    public void StartMovement()
    {
        if (!isMoving)
        {
            StartCoroutine(MoveFromTo(startPosition, endPosition));
        }
        else
        {
            Debug.LogWarning("Le camion est déjà en mouvement.");
        }
    }
    
    // Coroutine qui déplace le camion du point A au point B
    private IEnumerator MoveFromTo(Vector3 startPos, Vector3 endPos)
    {
        isMoving = true;
        
        // Positionne le camion au point de départ
        transform.position = startPos;
        
        // Oriente le camion vers le point d'arrivée
        Vector3 direction = endPos - startPos;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
        
        // Attend le délai de démarrage
        yield return new WaitForSeconds(startDelay);
        
        float journeyLength = Vector3.Distance(startPos, endPos);
        
        // Déplace progressivement le camion
        float distanceCovered = 0;
        while (distanceCovered < journeyLength)
        {
            float distanceToMove = speed * Time.deltaTime;
            distanceCovered += distanceToMove;
            
            float journeyFraction = distanceCovered / journeyLength;
            transform.position = Vector3.Lerp(startPos, endPos, journeyFraction);
            
            yield return null;
        }
        
        // Assure que le camion arrive exactement au point d'arrivée
        transform.position = endPos;
        isMoving = false;
        
        Debug.Log("Le camion est arrivé à destination!");
    }
    
    // Méthode pour faire revenir le camion à son point de départ
    public void ReturnToStart()
    {
        if (!isMoving)
        {
            StartCoroutine(MoveFromTo(endPosition, startPosition));
        }
    }
}