using System.Collections.Generic;
using UnityEngine;

public class AIInputManager : MonoBehaviour, IInputProvider
{
    public float VerticalInput { get; private set; }
    public float HorizontalInput { get; private set; }
    public bool HandbrakeInput => false; 

    public float acceleration = 0.5f;
    public float steerForce = 1f;
    public int distanceOffset = 1;

    public wayPointsScript wayPointsScript;
    public List<Transform> nodes = new List<Transform>();
    public Transform currentWaypoint;
    public int currentNode;

    private void Start()
    {
        wayPointsScript = GameObject.FindGameObjectWithTag("path")?.GetComponent<wayPointsScript>();

        if (wayPointsScript == null)
        {
            Debug.LogError("No 'wayPointsScript' found with tag 'path'.");
            enabled = false;
            return;
        }

        nodes = wayPointsScript.nodes;
        if (nodes.Count > 0)
            currentWaypoint = nodes[0];
    }

    private void FixedUpdate()
    {
        if (nodes == null || nodes.Count == 0) return;

        CalculateDistanceOfWaypoints();
        SteerTowardsWaypoint();
        VerticalInput = acceleration;
    }

    private void SteerTowardsWaypoint()
    {
        if (currentWaypoint == null) return;

        Vector3 relative = transform.InverseTransformPoint(currentWaypoint.position);
        relative.Normalize();
        HorizontalInput = relative.x * steerForce;
    }

    private void CalculateDistanceOfWaypoints()
    {
        Vector3 position = transform.position;
        float minDistance = Mathf.Infinity;

        for (int i = 0; i < nodes.Count; i++)
        {
            float currentDistance = Vector3.Distance(position, nodes[i].position);
            if (currentDistance < minDistance)
            {
                int targetIndex = Mathf.Min(i + distanceOffset, nodes.Count - 1);
                currentWaypoint = nodes[targetIndex];
                currentNode = i;
                minDistance = currentDistance;
            }
        }
    }
}
