using UnityEngine;
using System.Collections;

public class CharacterMovement : MonoBehaviour
{

    public float maxSpeed = 2.0f;
    public bool facingRight = true;
    public float moveSpeedX;
    public float moveSpeedY;

    public bool doubleJump = false;
    public int maxJumpCount = 5;
    public int jumpCount = 0;
    public float jumpSpeed = 200;
    private Rigidbody rigidbody;
    public Transform groundCheck;
    public float groundRadius = 0.001f;
    public LayerMask whatIsGround;
    public bool grounded = false;

    public float turnWaitTime = .2f;
    public float turnTime = 0;
    public bool turning = false;

    private Animator anim;

    public float shotSpeed = 600.0f;
    private GameObject currentShotSpawn;
    public Rigidbody shotPrefab;
    Rigidbody clone;

    public int aimingDirection = 0;
    // Estados en que puede estar el cañon
    private const int aimingIdleConst = 0;
    private const int aimingUpConst = 1;
    private const int aimingUpFrontConst = 2;
    private const int aimingFrontConst = 3;
    private const int aimingDownFrontConst = 4;
    private const int aimingDownConst = 5;

    // Weapon spawns:
    GameObject canonIdleSpawn, canonAimingFrontSpawn, canonAimingUpFrontSpawn,
        canonAimingDownFrontSpawn, canonAimingUp;


    //Sounds 
    AudioClip baseShotSound;
    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        // Para que el rigidbody no deje de recibir eventos mientras esta inmovil
        rigidbody.sleepThreshold = 0.0f;

        // Para comprobar cuando se toca el suelo
        groundCheck = GameObject.Find("/Character/GroundCheck").transform;

        // Objeto que manipula las animaciones
        anim = GameObject.Find("/Character/CharacterSprite").GetComponent<Animator>();

        // Inicializando posiciones de cañon
        canonIdleSpawn = GameObject.Find("/Character/Weapon/CanonIdleSpawn");
        canonAimingFrontSpawn = GameObject.Find("/Character/Weapon/CanonAimingFrontSpawn");
        canonAimingUpFrontSpawn = GameObject.Find("/Character/Weapon/CanonAimingUpFrontSpawn");
        canonAimingDownFrontSpawn = GameObject.Find("/Character/Weapon/CanonAimingDownFrontSpawn");
        canonAimingUp = GameObject.Find("/Character/Weapon/CanonAimingUp");

        // Cargando sonidos
        baseShotSound = Resources.Load("Sounds/BaseShot", typeof(AudioClip)) as AudioClip;
    }

    void FixedUpdate()
    {
        anim.SetFloat("MoveSpeedX", moveSpeedX);
        anim.SetFloat("MoveSpeedY", moveSpeedY);

        HandleAimingState();

        // Se chekea si se toca el suelo y se actualizan los estados de la animacion
        grounded =
            Physics2D.OverlapCircle(groundCheck.position, groundRadius, whatIsGround);
        anim.SetBool("Grounded", grounded);
        // Si se esta sobre el suelo se inicializan los estados de salto
        if (grounded)
        {
            doubleJump = false;
            jumpCount = 0;
        }

        // En funcion del movimiento se cambia la orientacion del personaje
        if (moveSpeedX > 0.0f && !facingRight)
        {
            Flip();
        }
        else if (moveSpeedX < 0.0f && facingRight)
        {
            Flip();
        }

        // Mientras ocurre el giro del personaje no ocurre movimiento
        if (!turning)
        {
            anim.SetBool("Turning", turning);
            rigidbody.velocity =
            new Vector2(moveSpeedX * maxSpeed, rigidbody.velocity.y);
        }
    }


    void HandleAimingState()
    {
        GetAimingDirection();
        if (aimingDirection == aimingIdleConst)
        {
            currentShotSpawn = canonIdleSpawn;
        }
        if (aimingDirection == aimingUpFrontConst)
        {
            currentShotSpawn = canonAimingUpFrontSpawn;
        }
        else if (aimingDirection == aimingFrontConst)
        {
            currentShotSpawn = canonAimingFrontSpawn;
        }
        else if (aimingDirection == aimingDownFrontConst)
        {
            currentShotSpawn = canonAimingDownFrontSpawn;
        }
        else if (aimingDirection == aimingUpConst)
        {
            currentShotSpawn = canonAimingUp;
        }
    }

    void GetAimingDirection()
    {
        if (moveSpeedX == 0 && moveSpeedY == 0) aimingDirection = 0;//idle
        else if (moveSpeedX == 0 && moveSpeedY > 0) aimingDirection = 1;//up
        else if (Mathf.Abs(moveSpeedX) > 0 && moveSpeedY > 0) aimingDirection = 2;//up-right 45 grados
        else if (Mathf.Abs(moveSpeedX) > 0 && moveSpeedY == 0) aimingDirection = 3;//front
        else if (Mathf.Abs(moveSpeedX) > 0 && moveSpeedY < 0) aimingDirection = 4;//down-right 315 grados 
        else if (moveSpeedX == 0 && moveSpeedY < 0) aimingDirection = 5;//down 270 grados

        // Apuntando arriba-delante
        if (Input.GetKey(KeyCode.R))
            aimingDirection = 2;
        // Apuntando debajo-delante
        if (Input.GetKey(KeyCode.F))
            aimingDirection = 4;

        anim.SetInteger("AimingDirection", aimingDirection);
    }

    void Update()
    {
        // Se captura la direccion y velocidad del movimiento horizontal
        moveSpeedX = Input.GetAxis("Horizontal");
        moveSpeedY = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;

        HandleJump();

        // Se actualiza el estado del giro
        if (turning)
        {
            turnTime += Time.deltaTime;
            if (turnTime >= turnWaitTime)
            {
                turning = false;
                turnTime = 0;
            }
        }

        if (Input.GetButtonDown("Fire1"))
        {
            Attack();
        }


    }
    void HandleJump()
    {
        // Si el personaje esta en el suelo o si kedan saltos permisibles aun y se presiona saltar
        if ((grounded || jumpCount < maxJumpCount) && Input.GetButtonDown("Jump"))
        {
            // Se annade una fuerza vertical (Salto)
            rigidbody.AddForce(new Vector2(0.0f, jumpSpeed));
            // Se actualiza la cantidad de saltos
            if (jumpCount < maxJumpCount && !grounded) jumpCount++;

        }
    }

    void Flip()
    {
        // Gira el pesonaje 180 grados y actualiza el estado de las animaciones
        turning = true;
        transform.Rotate(Vector3.up, 180.0f, Space.World);
        // Se ignora el giro para el CharacterSprite porque la animacion es asimetrica
        anim.transform.Rotate(Vector3.up, 180.0f, Space.World);

        facingRight = !facingRight;
        anim.SetBool("FacingRight", facingRight);
        anim.SetBool("Turning", turning);
    }
    void Attack()
    {
        // Sonido del disparo
        Camera.main.GetComponent<AudioSource>().PlayOneShot(baseShotSound, .1f);

        clone =
            Instantiate(shotPrefab, currentShotSpawn.transform.position, currentShotSpawn.transform.rotation) as Rigidbody;
        // Se otorga inicialmente la velocidad actual del jugador a la bala
        clone.velocity = rigidbody.velocity;
        // Luego se annade la velocidad del disparo
        clone.AddForce(currentShotSpawn.transform.right * shotSpeed);
    }

}
