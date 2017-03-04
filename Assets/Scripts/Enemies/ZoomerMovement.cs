using UnityEngine;
using System.Collections;

public class ZoomerMovement : MonoBehaviour
{
    public Vector2 position;
    Rigidbody rigidBody;

    public float currentMovementTime = 0.0f;
    public float maxMovementTime = 8f;
    public float moveSpeed = .5f;
    public bool walkingRight = true;

    void Awake()
    {
        position = transform.position;
        rigidBody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (currentMovementTime >= maxMovementTime)
        {
            walkingRight = !walkingRight;
            currentMovementTime = 0;
        }
        currentMovementTime += Time.deltaTime;

        rigidBody.velocity =
            new Vector2(walkingRight ? moveSpeed : -moveSpeed, rigidBody.velocity.y);
    }
}
