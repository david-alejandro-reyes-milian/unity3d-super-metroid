using UnityEngine;
using System.Collections;

public class CameraTargetAutoMove : MonoBehaviour
{

    private CharacterMovement characterMovement;
    private Transform parentTransform;
    public float maxDistanceOnX = 1f;
    void Awake()
    {
        characterMovement = GameObject.Find("Character").GetComponent<CharacterMovement>();
        parentTransform = transform.parent.transform;
    }

    void Update()
    {
        transform.position =
            new Vector3(
                parentTransform.position.x + characterMovement.moveSpeedX * maxDistanceOnX,
                transform.position.y, transform.position.z);
    }
}
