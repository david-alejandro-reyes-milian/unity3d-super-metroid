using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{

    public int health = 6;
    public bool isAlive = true;
    Animator anim;
    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void OnTriggerEnter(Collider other)
    {

        if (other.tag == "Shot" && isAlive)
        {
            health--;
            anim.SetTrigger("AttackReceived");
        }
    }
    void Update()
    {
        if (health == 0)
        {
            isAlive = false;
            // Se cambia el tag a cualquier otro para que los tiros no colisionen mas con el objeto
            gameObject.tag = "Shot";
            anim.SetTrigger("IsDead");
            Destroy(gameObject, .8f);
        }
    }
}
