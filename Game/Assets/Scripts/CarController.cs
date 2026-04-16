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

    void Update()
    {
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

        // Move
        transform.position += direction * speed * Time.deltaTime;

        // Rotate smoothly
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);

        // Switch waypoint
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
}