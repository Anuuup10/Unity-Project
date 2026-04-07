using UnityEngine;
public class VehicleMover : MonoBehaviour
{
[SerializeField]
private GameObject waypoint1;
[SerializeField]
private GameObject waypoint2;
[SerializeField]
private GameObject waypoint3;
[SerializeField]
private GameObject waypoint4;
[SerializeField]
private GameObject target;
private const float CLOSE_DISTANCE = 1;
private const float SPEED = 10.0f;
[SerializeField]
private bool flipLookDirection = false;
// Start is called before the first frame update
void Start()
{
}
// Update is called once per frame
void Update()
{
// Determine the direction to the current target waypoint.
Vector3 direction = target.transform.position - transform.position;
direction.y = 0;
// Calculates the length of the relative position vector
float distance = direction.magnitude;
// Face in the right direction.
if (distance > 0)
{
Quaternion rotation;
if (flipLookDirection)
{
rotation = Quaternion.LookRotation(-direction, Vector3.up);
}
else
{
rotation = Quaternion.LookRotation(direction, Vector3.up);
}
transform.rotation = rotation;
}
// Calculate the normalised direction to the target from a game object.
Vector3 normDirection = direction / distance;
// Move the game object.
transform.position = transform.position + normDirection * SPEED * Time.deltaTime;
// Check if close to the current target.
if (distance < CLOSE_DISTANCE)
{
// Change the target.
if (target.Equals(waypoint1))
{
target = waypoint2;
}
else if (target.Equals(waypoint2))
{
target = waypoint3;
}
else if (target.Equals(waypoint3))
{
target = waypoint4;
}
else if (target.Equals(waypoint4))
{
target = waypoint1;
}
}
}
}