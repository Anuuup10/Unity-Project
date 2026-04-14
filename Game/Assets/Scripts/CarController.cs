using UnityEngine;

public class CarController : MonoBehaviour
{
    public float speed = 5f;
    public Transform stopPoint; // where car should stop
    public TrafficLightController trafficLight;

    private bool shouldStop = false;

    void Update()
    {
        CheckTrafficLight();

        if (!shouldStop)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }

    void CheckTrafficLight()
    {
        float distance = Vector3.Distance(transform.position, stopPoint.position);

        if (distance < 5f)
        {
            if (trafficLight.currentState == TrafficLightController.LightState.Red ||
                trafficLight.currentState == TrafficLightController.LightState.Yellow)
            {
                shouldStop = true;
            }
            else
            {
                shouldStop = false;
            }
        }
        else
        {
            shouldStop = false;
        }
    }
}