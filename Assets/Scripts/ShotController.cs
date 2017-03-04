using UnityEngine;
using System.Collections;

public class ShotController : MonoBehaviour
{
    Animator anim;
    public float destructionWaitTime = 2.0f;
    public float destructionTime = 0;
    public GameObject shot_explotion_pfb;
    GameObject explotion;
    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }
    void Update()
    {
        destructionTime += Time.deltaTime;
        if (destructionTime >= destructionWaitTime)
        {
            Destroy(gameObject);
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "ShotCollider" || other.tag == "Enemy")
        {
            Destroy(gameObject);
            explotion = GameObject.Instantiate(shot_explotion_pfb, transform.position, transform.rotation) as GameObject;
            Destroy(explotion, .5f);
        }
    }
}
