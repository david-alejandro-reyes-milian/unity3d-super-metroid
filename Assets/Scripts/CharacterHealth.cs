using UnityEngine;
using System.Collections;

public class CharacterHealth : MonoBehaviour
{
    public int health = 100;
    CharacterMovement characterMovement;
    Animator anim;
    void Awake()
    {
        characterMovement = GetComponent<CharacterMovement>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (health <= 0) { PlayerDied(); }
    }
    void PlayerDied()
    {
        // Animacion de muerte
        print("Player is dead");
        // Se deshabilita el movimiento del jugador
        characterMovement.enabled = false;
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
