using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Movement")]
    public Transform[] waypoints;
    public float speed = 5f;
    public float rotationSpeed = 5f;

    private int currentIndex = 0;

    [Header("Traffic System")]
    public TrafficLightController[] trafficLights;
    public Transform[] stopPoints;
    public float stopDistance = 5f;

    private bool shouldStop = false;
    private bool isCrashed = false;

    void Update()
    {
        if (isCrashed) return;

        CheckTrafficLights();

        if (!shouldStop)
        {
            MoveCar();
        }
    }

    void MoveCar()
    {
        if (waypoints.Length == 0) return;

        Transform target = waypoints[currentIndex];

        Vector3 direction = (target.position - transform.position).normalized;

        transform.position += direction * speed * Time.deltaTime;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 1f)
        {
            currentIndex = (currentIndex + 1) % waypoints.Length;
        }
    }

    void CheckTrafficLights()
    {
        shouldStop = false;

        for (int i = 0; i < trafficLights.Length; i++)
        {
            if (trafficLights[i] == null || stopPoints[i] == null) continue;

            float distance = Vector3.Distance(transform.position, stopPoints[i].position);

            if (distance < stopDistance)
            {
                if (trafficLights[i].currentState == TrafficLightController.LightState.Red ||
                    trafficLights[i].currentState == TrafficLightController.LightState.Yellow)
                {
                    shouldStop = true;
                    return;
                }
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth player = collision.gameObject.GetComponent<PlayerHealth>();

            if (player != null)
            {
                player.TakeDamage(100); // full damage (instant death)
                TriggerAccident(collision.gameObject);
            }
        }
    }

    void TriggerAccident(GameObject player)
    {
        isCrashed = true;
        speed = 0f;

        Rigidbody carRigidbody = GetComponent<Rigidbody>();
        if (carRigidbody != null)
        {
            carRigidbody.linearVelocity = Vector3.zero;
            carRigidbody.angularVelocity = Vector3.zero;
        }

        // Stop player movement (CharacterController case)
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
        }

        // Stop Rigidbody movement (if used)
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log("Player has been hit by car!");
    }
}
