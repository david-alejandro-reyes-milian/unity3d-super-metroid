using UnityEngine;
using System.Collections;

public class ZoomerHealth : MonoBehaviour
{

    public int health = 3;
    public bool isAlive = true;
    Animator anim;
    ZoomerMovement movement;
    void Awake() { anim = GetComponent<Animator>(); movement = GetComponent<ZoomerMovement>(); }

    void OnTriggerEnter(Collider other)
    {
        // Si entra un disparo al area de colisiones y aun esta vivo el enemigo se actualiza el estado
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
            // Se para el movimiento del zoomer
            movement.enabled = false;
            // Se cambia el tag a cualquier otro para que los tiros no colisionen mas con el objeto
            gameObject.tag = "Shot";
            // Se reproduce la animacion de muerte
            anim.SetTrigger("IsDead");
            // Se destruye el objeto pasado un tiempo
            Destroy(gameObject, .8f);
        }
    }
}
