using UnityEngine;
using System.Collections;

public class CharacterHealth : MonoBehaviour
{
    public int health = 100;
    CharacterMovement characterMovement;
    void Awake()
    {
        characterMovement = GetComponent<CharacterMovement>();
    }

    void Update()
    {
        if (health <= 0)
        {
            // Se deshabilita el movimiento del jugador
            characterMovement.enabled = false;
            // Animacion de muerte
            print("Player is dead");
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "EnemyAttack")
        {
            // Play attackReceivedAnimation
            health -= 1;
        }
    }
}
