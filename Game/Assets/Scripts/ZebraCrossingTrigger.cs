using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ZebraCrossingTrigger : MonoBehaviour
{
    private GameManager gameManager;

    private void Awake()
    {
        Collider triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;
        gameManager = FindFirstObjectByType<GameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && gameManager != null)
        {
            gameManager.MarkZebraCrossingUsed();
        }
    }
}
