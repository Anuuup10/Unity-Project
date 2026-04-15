using UnityEngine;
using System.Collections;

public class TrafficLight3 : MonoBehaviour
{
    public Renderer redLight;
    public Renderer yellowLight;
    public Renderer greenLight;

    public Material redOn, redOff;
    public Material yellowOn, yellowOff;
    public Material greenOn, greenOff;

    public float redTime = 8f;
    public float greenTime = 6f;
    public float yellowTime = 2f;

    public float startDelay = 0f; 

    public enum LightState { Red, Yellow, Green }
    public LightState currentState;

    void Start()
    {
        StartCoroutine(StartWithDelay());
    }

    IEnumerator StartWithDelay()
    {
        yield return new WaitForSeconds(startDelay);
        StartCoroutine(TrafficCycle());
    }

    IEnumerator TrafficCycle()
    {
        while (true)
        {
            SetLight(LightState.Red);
            yield return new WaitForSeconds(redTime);

            SetLight(LightState.Green);
            yield return new WaitForSeconds(greenTime);

            SetLight(LightState.Yellow);
            yield return new WaitForSeconds(yellowTime);
        }
    }

    void SetLight(LightState state)
    {
        currentState = state;

        redLight.material = redOff;
        yellowLight.material = yellowOff;
        greenLight.material = greenOff;

        if (state == LightState.Red)
            redLight.material = redOn;

        else if (state == LightState.Yellow)
            yellowLight.material = yellowOn;

        else if (state == LightState.Green)
            greenLight.material = greenOn;
    }
}