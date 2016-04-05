using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{

    public int health = 3;
    Animator anim;
    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Shot")
        {
            health--;
            anim.SetTrigger("AttackReceived");
        }
    }
    void Update()
    {
        if (health == 0)
        {
            //Play enemy death animation
            anim.SetTrigger("IsDead");
            Destroy(gameObject, .8f);
        }
    }
}
