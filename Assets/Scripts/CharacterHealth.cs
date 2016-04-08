using UnityEngine;
using System.Collections;

public class CharacterHealth : MonoBehaviour
{
    public int health = 100;
    CharacterMovement characterMovement;
    Animator anim;
    Rigidbody rigidbody;
    SpriteRenderer spriteRenderer;

    Color transparentColor = new Color(1, 1, 1, 0);
    Color normalColor = new Color(1, 1, 1, 1);

    public float recoveryWaitTime = 2;
    public float recoveryTime;
    public float recoveryAnimationWaitTime = .04f;
    public float recoveryAnimationTime;
    public bool inRecovery = false;

    public float damageReceivedForceX = 200;
    public float damageReceivedForceY = 10;
    void Awake()
    {
        characterMovement = GetComponent<CharacterMovement>();
        anim = GetComponentInChildren<Animator>();
        rigidbody = GetComponent<Rigidbody>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        if (health <= 0) { PlayerDied(); }
        if (inRecovery && recoveryTime <= recoveryWaitTime)
        {
            recoveryTime += Time.deltaTime;
            RecoveryAnimation();
        }
        else { inRecovery = false; recoveryTime = 0; spriteRenderer.color = normalColor; }
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
        if (other.tag == "EnemyAttack" && !inRecovery)
        {
            // Play attackReceivedAnimation
            anim.SetTrigger("DamageReceived");
            // Fuerza que aleja al caracter del enemigo
            Vector2 force = new Vector2(characterMovement.facingRight ?
                -damageReceivedForceX : damageReceivedForceX, damageReceivedForceY);
            rigidbody.AddForce(force);
            // Actualizando la salud
            health -= 15;
            // Mientras esta en recuperacion el personaje no recibe dannos;
            inRecovery = true;

            //Cuando recibe danno siempre se retorna al estado parado
            characterMovement.bodyState = 0;
        }
    }
    void RecoveryAnimation()
    {
        if (recoveryAnimationTime <= recoveryAnimationWaitTime) { recoveryAnimationTime += Time.deltaTime; }
        else
        {
            if (spriteRenderer.color.a == 0) { spriteRenderer.color = normalColor; }
            else { spriteRenderer.color = transparentColor; }
            recoveryAnimationTime = 0;
        }

    }
}
