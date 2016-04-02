using UnityEngine;
using System.Collections;

public class BombDestruction : MonoBehaviour
{

    public float destructionWaitTime = 2.0f;
    public float destructionTime = 0;
    void Update()
    {
        destructionTime += Time.deltaTime;
        if (destructionTime >= destructionWaitTime)
        {
            Destroy(gameObject);
        }
    }
}

