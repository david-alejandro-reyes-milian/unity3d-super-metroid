using UnityEngine;
using System.Collections;

public class SmothFollow : MonoBehaviour
{
    public float xMargin = 0;
    public float yMargin = 0;

    public float xSmoth = 2;
    public float ySmoth = 5;

    public Vector2 maxXAndY = new Vector2(30, 10);
    public Vector2 minXAndY = new Vector2(-30, -10);

    public Transform cameraTarget;
    void Awake()
    {
        cameraTarget = GameObject.FindGameObjectWithTag("CameraTarget").transform;
    }

    bool CheckXMargin()
    {
        return Mathf.Abs(transform.position.x - cameraTarget.position.x) > xMargin;
    }
    bool CheckYMargin()
    {
        return Mathf.Abs(transform.position.y - cameraTarget.position.y) > yMargin;
    }
    void FixedUpdate()
    {
        TrackPlayer();
    }
    void TrackPlayer()
    {
        float targetX = transform.position.x;
        float targetY = transform.position.y;

        if (CheckXMargin())
        {
            targetX =
                Mathf.Lerp(transform.position.x, cameraTarget.position.x, Time.deltaTime * xSmoth);
        }
        if (CheckYMargin())
        {
            targetY =
                Mathf.Lerp(transform.position.y, cameraTarget.position.y, Time.deltaTime * ySmoth);
        }

        targetX = Mathf.Clamp(targetX, minXAndY.x, maxXAndY.x);
        targetY = Mathf.Clamp(targetY, minXAndY.y, maxXAndY.y);

        transform.position = new Vector3(targetX, targetY, transform.position.z);
    }
}
