using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CharacterHealth : MonoBehaviour
{
    public int energy = 99;
    CharacterMovement characterMovement;
    Animator anim;
    Rigidbody rigidbody;
    SpriteRenderer spriteRenderer;
    BackScreenController backScreenController;
    EnergyNumberSpriteRenderer energyNumberSpriteRenderer;

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
        backScreenController = GameObject.Find("BlackScreen").GetComponent<BackScreenController>();
        energyNumberSpriteRenderer = GameObject.Find("/GUI/TopPanel/EnergyNumber").GetComponent<EnergyNumberSpriteRenderer>();
    }
    void Start() { energyNumberSpriteRenderer.UpdateEnergyGui(energy); }

    void Update()
    {
        if (inRecovery && recoveryTime <= recoveryWaitTime)
        {
            recoveryTime += Time.deltaTime;
            RecoveryAnimation();
        }
        else { inRecovery = false; recoveryTime = 0; spriteRenderer.color = normalColor; }
    }
    void PlayerDied()
    {
        // Se deshabilita el script de salud y se para la fisica del caracter
        // Se deshabilita el control de movimiento del jugador
        this.enabled = false;
        rigidbody.isKinematic = true;
        characterMovement.enabled = false;

        // Se mueve el caracter a la posicion de la camara para relizar la animacion de muerte
        transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z + .5f);

        //Se enfoca la camara al caracter para ver la animacion de muerte
        GameObject cTarget = transform.Find("CameraTarget").gameObject;
        cTarget.GetComponent<CameraTargetAutoMove>().enabled = false;
        cTarget.transform.position = transform.position;

        // Se oscurece el resto de la escena:
        backScreenController.turnScreenBlack = true;

        // Animacion de muerte
        anim.SetTrigger("Died");
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "EnemyAttack" && !inRecovery && energy > 0)
        {
            // Actualizando la salud. Si el jugador muere sale de la funcion
            energy = Mathf.Clamp(energy - 15, 0, 99);
            UpdateEnergyOnGui();
            if (energy <= 0) { PlayerDied(); return; }

            // Play attackReceivedAnimation
            anim.SetTrigger("DamageReceived");
            // Fuerza que aleja al caracter del enemigo
            Vector2 force = new Vector2(characterMovement.facingRight ?
                -damageReceivedForceX : damageReceivedForceX, damageReceivedForceY);
            rigidbody.AddForce(force);
            // Mientras esta en recuperacion el personaje no recibe dannos;
            inRecovery = true;
            //Cuando recibe danno siempre se retorna al estado de pie
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
    public void UpdateEnergyOnGui()
    {
        energyNumberSpriteRenderer.UpdateEnergyGui(energy);
    }
}
