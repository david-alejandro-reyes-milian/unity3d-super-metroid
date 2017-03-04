using UnityEngine;
using System.Collections;

public class ZoomerHealth : MonoBehaviour
{

    public int health = 3;
    public bool isAlive = true;
    Animator anim;
    ZoomerMovement movement;
    public GameObject[] bonusItems;
    public GameObject randomBonusItem;
    void Awake()
    {
        anim = GetComponent<Animator>(); movement = GetComponent<ZoomerMovement>();
        randomBonusItem = bonusItems[Random.Range(0, bonusItems.Length)];
    }

    void OnTriggerEnter(Collider other)
    {
        // Si entra un disparo proveniente de algun arma del caracter 
        // al area de colisiones y aun esta vivo el enemigo se actualiza el estado
        if (other.tag == "Weapon" && isAlive)
        {
            health--;
            anim.SetTrigger("AttackReceived");
        }
    }
    void Update()
    {
        if (health <= 0)
        {
            if (isAlive) GameObject.Instantiate(randomBonusItem, transform.position, transform.rotation);

            isAlive = false;
            // Se deshabilita el area de ataque para no afectar al caracter
            transform.Find("AttackArea").gameObject.SetActive(false);
            // Se para el movimiento del zoomer
            movement.enabled = false;
            // Se cambia el tag a cualquier otro para que los tiros no colisionen mas con el objeto
            gameObject.tag = "Weapon";
            // Se reproduce la animacion de muerte
            anim.SetTrigger("IsDead");
            // Se destruye el objeto pasado un tiempo
            Destroy(gameObject, .8f);


        }
    }
}
