using UnityEngine;
using System.Collections;

public class ZoomerMovement : MonoBehaviour
{
    public Vector2 position;

    public float maxWalkingDistance = 10.0f;
    public float currentWalkingDistance = 0.0f;
    public float moveSpeed = 0.1f;
    public bool walkingRight = true;

    void Start()
    {
        position = transform.position;
    }

    void Update()
    {
        if (walkingRight && currentWalkingDistance <= maxWalkingDistance)
            currentWalkingDistance += Time.deltaTime * moveSpeed;
        else if (!walkingRight && currentWalkingDistance >= 0)
            currentWalkingDistance -= Time.deltaTime * moveSpeed;
        else
            walkingRight = !walkingRight;
        transform.position = new Vector3(position.x + currentWalkingDistance, transform.position.y);
    }
}
