using UnityEngine;

public class TrafficLightInfoUI : MonoBehaviour
{
    [Header("References")]
    public GameObject infoPanel;        // Drag your UI panel here
    public Transform trafficLight;      // Drag your traffic light here

    [Header("Settings")]
    public float detectionRadius = 20f;

    private bool isInRange = false;

    void Start()
    {
        // Make sure UI is hidden at start
        if (infoPanel != null)
            infoPanel.SetActive(false);
    }

    void Update()
    {
        if (trafficLight == null || infoPanel == null) return;

        float distance = Vector3.Distance(transform.position, trafficLight.position);

        if (distance <= detectionRadius && !isInRange)
        {
            isInRange = true;
            infoPanel.SetActive(true);   // Show when entering 20m
        }
        else if (distance > detectionRadius && isInRange)
        {
            isInRange = false;
            infoPanel.SetActive(false);  // Hide when leaving 20m
        }
    }
}