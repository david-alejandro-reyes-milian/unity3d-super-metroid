using UnityEngine;
using System.Collections;

public class CharacterMovement : MonoBehaviour
{
    // Movement
    public float maxSpeed = 2.0f;
    public bool facingRight = true;
    public float moveSpeedX;
    public float moveSpeedY;

    // Character State
    public int bodyState = 0;
    public int bodyStateAir = 0;
    private const int playerStandingConst = 0;
    private const int playerOnKneesConst = 1;
    private const int playerAsBallConst = 2;
    private const float playerStandingColliderheigthConst = .43f;
    private const float playerOnKneesColliderheigthConst = .33f;
    private const float playerAsBallColliderheigthConst = .0f;

    // Jumps
    public int maxJumpCount = 5;
    public int jumpCount = 0;
    public float jumpSpeed = 300;

    // Physics
    private Rigidbody rigidbody;
    private CapsuleCollider collider;
    public Transform groundCheck, obstacleCheck;
    public float groundRadius = 0.0001f;
    public LayerMask whatIsGround;
    public LayerMask whatIsObstacle;
    public bool grounded = false;
    public bool touchingObstacle = false;
    public bool goingUp = false;


    // Turning
    public float turnWaitTime = .2f;
    public float turnTime = 0;
    public bool turning = false;

    // Shotting
    public float shotSpeed = 200;
    public float shotWaitTime = .5f;
    public float lastShotTime = 0;
    public bool shootingOnAir = false;
    private GameObject currentShotSpawn;
    public Rigidbody shotPrefab;
    public Rigidbody bombPrefab;
    public Rigidbody currentWeaponPrefab;
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
    GameObject canonIdleSpawnAir, canonAimingFrontSpawnAir, canonAimingUpFrontSpawnAir,
       canonAimingDownFrontSpawnAir, canonAimingUpAir, canonAimingDownSpawnAir;
    GameObject canonAimingFrontSpawnKnees, canonAimingUpFrontSpawnKnees,
           canonAimingDownFrontSpawnKnees, canonAimingUpSpawnKnees;
    //Sounds 
    AudioClip baseShotSound, bombSound;

    private Animator anim;

    void Awake()
    {
        collider = GetComponent<CapsuleCollider>();

        rigidbody = GetComponent<Rigidbody>();
        // Para que el rigidbody no deje de recibir eventos mientras esta inmovil
        rigidbody.sleepThreshold = 0.0f;

        // Para comprobar cuando se toca el suelo
        groundCheck = GameObject.Find("/Character/GroundCheck").transform;

        // Para comprobar si se toca un obstaculo
        obstacleCheck = GameObject.Find("/Character/ObstacleCheck").transform;

        // Objeto que manipula las animaciones
        anim = GameObject.Find("/Character/CharacterSprite").GetComponent<Animator>();

        currentWeaponPrefab = shotPrefab;
        // Inicializando posiciones de cañon
        canonIdleSpawn = GameObject.Find("/Character/CanonAiming/CanonIdleSpawn");
        canonAimingFrontSpawn = GameObject.Find("/Character/CanonAiming/CanonAimingFrontSpawn");
        canonAimingUpFrontSpawn = GameObject.Find("/Character/CanonAiming/CanonAimingUpFrontSpawn");
        canonAimingDownFrontSpawn = GameObject.Find("/Character/CanonAiming/CanonAimingDownFrontSpawn");
        canonAimingUp = GameObject.Find("/Character/CanonAiming/CanonAimingUp");

        canonAimingFrontSpawnAir = GameObject.Find("/Character/CanonAimingAir/CanonAimingFrontSpawn");
        canonAimingUpFrontSpawnAir = GameObject.Find("/Character/CanonAimingAir/CanonAimingUpFrontSpawn");
        canonAimingDownFrontSpawnAir = GameObject.Find("/Character/CanonAimingAir/CanonAimingDownFrontSpawn");
        canonAimingUpAir = GameObject.Find("/Character/CanonAimingAir/CanonAimingUp");
        canonAimingDownSpawnAir = GameObject.Find("/Character/CanonAimingAir/CanonAimingDownSpawn");

        canonAimingFrontSpawnKnees = GameObject.Find("/Character/CanonAimingKnees/CanonAimingFrontSpawn");
        canonAimingUpFrontSpawnKnees = GameObject.Find("/Character/CanonAimingKnees/CanonAimingUpFrontSpawn");
        canonAimingDownFrontSpawnKnees = GameObject.Find("/Character/CanonAimingKnees/CanonAimingDownFrontSpawn");
        canonAimingUpSpawnKnees = GameObject.Find("/Character/CanonAimingKnees/CanonAimingUpSpawn");

        // Cargando sonidos
        baseShotSound = Resources.Load("Sounds/BaseShot_clean", typeof(AudioClip)) as AudioClip;
        bombSound = Resources.Load("Sounds/Bomb", typeof(AudioClip)) as AudioClip;
    }

    void FixedUpdate()
    {
        anim.SetFloat("MoveSpeedX", moveSpeedX);
        anim.SetFloat("MoveSpeedY", moveSpeedY);
        anim.SetBool("GoingUp", goingUp);
        anim.SetInteger("BodyState", bodyState);

        // Se ajusta el cuerpo del caracter segun el bodyState
        AdjustBody();

        // Se gestiona la puntería
        HandleAimingDirection();

        // Se chekea si se toca el suelo y se actualizan los estados de la animacion
        grounded =
            Physics2D.OverlapCircle(groundCheck.position, groundRadius, whatIsGround);
        anim.SetBool("Grounded", grounded);
        // Si se esta sobre el suelo se inicializan los estados de salto y disparos en el aire
        if (grounded)
        {
            jumpCount = 0;
            shootingOnAir = false;
            bodyStateAir = playerStandingConst;
        }
        anim.SetBool("ShootingOnAir", shootingOnAir);

        // Se chekea si se toca algun obstaculo
        touchingObstacle =
            Physics2D.OverlapCircle(obstacleCheck.position, groundRadius, whatIsObstacle);

        // En funcion del movimiento se cambia la orientacion del personaje
        if ((moveSpeedX > 0.0f && !facingRight) || (moveSpeedX < 0.0f && facingRight)) { Flip(); }

        // Mientras ocurre el giro del personaje no ocurre movimiento
        if (!turning)
        {
            anim.SetBool("Turning", turning);
            // Si se toca un obstaculo se para el movimiento del cuerpo
            if (!touchingObstacle)
                rigidbody.velocity =
                new Vector2(moveSpeedX * maxSpeed, rigidbody.velocity.y);
        }
    }

    void AdjustBody()
    {
        // Se ajusta el collider del cuerpo y el punto de checkeo del suelo de acuerdo al tamanno del cuerpo
        if (bodyState == playerStandingConst && collider.height != playerStandingColliderheigthConst)
        {
            collider.height = playerStandingColliderheigthConst;
            groundCheck.transform.localPosition = new Vector2(0, -0.2158f);
        }
        else if (bodyState == playerOnKneesConst && collider.height != playerOnKneesColliderheigthConst)
        {
            collider.height = playerOnKneesColliderheigthConst;
            groundCheck.transform.localPosition = new Vector2(0, -0.1598f);
        }
        else if (bodyState == playerAsBallConst && collider.height != playerAsBallColliderheigthConst)
        {
            collider.height = playerAsBallColliderheigthConst;
            groundCheck.transform.localPosition = new Vector2(0, -0.0808f);
        }
        else if (bodyStateAir == playerAsBallConst && collider.height != playerAsBallColliderheigthConst)
        {
            collider.height = playerAsBallColliderheigthConst;
            groundCheck.transform.localPosition = new Vector2(0, -0.0808f);
            bodyState = bodyStateAir;
        }
    }
    void HandleAimingDirection()
    {
        if (moveSpeedX == 0 && moveSpeedY == 0)
        {
            aimingDirection = aimingIdleConst;//idle
            // Si el caracter esta de rodillas, usa el spawn especial de esa posicion
            currentShotSpawn = bodyState == playerOnKneesConst ? canonAimingFrontSpawnKnees : canonIdleSpawn;
        }
        else if (moveSpeedX == 0 && moveSpeedY > 0)
        {
            aimingDirection = aimingUpConst;//up
            currentShotSpawn = shootingOnAir ? canonAimingUpAir : bodyState == playerOnKneesConst ? canonAimingUpSpawnKnees : canonAimingUp;
        }
        else if (Mathf.Abs(moveSpeedX) > 0 && moveSpeedY > 0)
        {
            aimingDirection = aimingUpFrontConst;//up-right 45 grados
            currentShotSpawn = shootingOnAir ? canonAimingUpFrontSpawnAir : bodyState == playerOnKneesConst ? canonAimingUpFrontSpawnKnees : canonAimingUpFrontSpawn;
        }
        else if (Mathf.Abs(moveSpeedX) > 0 && moveSpeedY == 0)
        {
            aimingDirection = aimingFrontConst;//front
            currentShotSpawn = shootingOnAir ? canonAimingFrontSpawnAir : canonAimingFrontSpawn;
        }
        else if (Mathf.Abs(moveSpeedX) > 0 && moveSpeedY < 0)
        {
            aimingDirection = aimingDownFrontConst;//down-right 315 grados 
            currentShotSpawn = shootingOnAir ? canonAimingDownFrontSpawnAir : bodyState == playerOnKneesConst ? canonAimingDownFrontSpawnKnees : canonAimingDownFrontSpawn;
        }
        else if (moveSpeedX == 0 && moveSpeedY < 0)
        {
            aimingDirection = aimingDownConst;//down 270 grados
            currentShotSpawn = canonAimingDownSpawnAir;
        }

        // Apuntando arriba-delante
        if (Input.GetKey(KeyCode.R))
        {
            aimingDirection = aimingUpFrontConst;
            currentShotSpawn = shootingOnAir || bodyState == playerOnKneesConst ?
                canonAimingUpFrontSpawnKnees : canonAimingUpFrontSpawn;

        }
        // Apuntando debajo-delante
        if (Input.GetKey(KeyCode.F))
        {
            aimingDirection = aimingDownFrontConst;
            currentShotSpawn = shootingOnAir || bodyState == playerOnKneesConst ?
                canonAimingDownFrontSpawnKnees : canonAimingDownFrontSpawn;
        }

        anim.SetInteger("AimingDirection", aimingDirection);
        // Una copia como float usada en arbol de mezclas
        anim.SetFloat("AimingDirectionF", aimingDirection);

        // Si cambia el objetivo de disparo o se salta se deshabilita el disparo al frente
        if (aimingDirection != aimingFrontConst || !grounded) { anim.SetBool("ShootingFront", false); }
    }

    void Update()
    {
        // Se captura la direccion y velocidad del movimiento horizontal
        //moveSpeedX = Input.GetAxis("Horizontal");
        moveSpeedX = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
        moveSpeedY = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;

        HandleBodyState();
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

        // Se controla la frecuencia de disparos
        if (lastShotTime >= shotWaitTime && Input.GetButtonDown("Fire1"))
        {
            lastShotTime = 0;
            Attack();
        }
        else { lastShotTime += Time.deltaTime; }

        if (Input.GetKeyDown(KeyCode.C))
        {
            ChangeWeapon();
        }

    }

    void HandleBodyState()
    {
        if (grounded)
        {
            if (Input.GetKeyDown(KeyCode.S) && moveSpeedX == 0) { bodyState = bodyState < 2 ? bodyState + 1 : 2; }
            else if (Input.GetKeyDown(KeyCode.W)) { bodyState = bodyState > 0 ? bodyState - 1 : 0; }
            if (moveSpeedX != 0 && bodyState == playerOnKneesConst) bodyState = playerStandingConst;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.S) && moveSpeedX == 0) { bodyStateAir = bodyStateAir < 2 ? bodyStateAir + 1 : 2; }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                bodyStateAir = bodyStateAir > 0 ? bodyStateAir - 1 : 0;
                // Mientras se encuentra en el aire si se presiona arriba mientras esta en forma de bola se transforma
                if (bodyState == playerAsBallConst && !grounded)
                {
                    bodyStateAir = playerStandingConst;
                    bodyState = playerStandingConst;
                }
            }
        }
    }

    void HandleJump()
    {
        // Se captura la direccion del salto para reaccionar en caida libre y animaciones
        if (rigidbody.velocity.y >= 0.5f) goingUp = true; else goingUp = false;

        if (Input.GetButtonDown("Jump"))
        {
            // Si modo bola, se actualiza el estado del cuerpo a de rodillas
            if (bodyState == playerAsBallConst)
            {
                if (grounded) { bodyState = playerOnKneesConst; }
                else { bodyState = playerStandingConst; bodyStateAir = playerStandingConst; }
            }

            // Si el personaje esta en el suelo o si kedan saltos permisibles aun y se presiona saltar
            else if ((grounded || jumpCount < maxJumpCount))
            {
                // Se annade una fuerza vertical (Salto)
                rigidbody.AddForce(new Vector2(0.0f, jumpSpeed));
                // Se actualiza la cantidad de saltos
                if (jumpCount < maxJumpCount && !grounded) jumpCount++;
                // Al saltar se deshabilita el disparo hacia el frente
                anim.SetBool("ShootingFront", false);

                // Al saltar se reinicia el estado del jugador en el suelo
                bodyState = playerStandingConst;
                bodyStateAir = playerStandingConst;
            }
        }
        if (Input.GetButtonUp("Jump") && goingUp) { rigidbody.velocity = Vector3.zero; }
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
        anim.SetBool("ShootingFront", false);
    }

    void Attack()
    {
        if (bodyState == playerAsBallConst) { Bomb(); }
        else { Shot(); }
    }
    void Shot()
    {
        // En el suelo no se puede disparar hacia abajo
        if (aimingDirection == aimingDownConst && grounded) return;

        // Sonido del disparo
        Camera.main.GetComponent<AudioSource>().PlayOneShot(baseShotSound, .3f);

        Vector3 newPosition = currentShotSpawn.transform.position;
        //newPosition.x += .05f;
        Quaternion newRotation = currentShotSpawn.transform.rotation;
        //newRotation.z += 2;

        clone =
            Instantiate(currentWeaponPrefab, newPosition, newRotation) as Rigidbody;
        // Se otorga inicialmente la velocidad actual del jugador a la bala
        clone.velocity = rigidbody.velocity;
        // Luego se annade la velocidad del disparo
        clone.AddForce(currentShotSpawn.transform.right * shotSpeed);

        // Si al atacar se apunta al frente se habilita la bandera
        if (aimingDirection == aimingFrontConst)
            anim.SetBool("ShootingFront", true);

        if (!grounded) { shootingOnAir = true; }
    }
    void Bomb()
    {
        // Sonido de bomb
        //Camera.main.GetComponent<AudioSource>().PlayOneShot(baseShotSound, .5f);

        clone =
            Instantiate(bombPrefab, transform.position, transform.rotation) as Rigidbody;

        // Ignorar colisiones del caracter con cada instancia de la bomba(otra forma es a traves de capas):
        //Physics.IgnoreCollision(collider, clone.GetComponent<Collider>());
    }
    void ChangeWeapon()
    {
        if (currentWeaponPrefab == shotPrefab) currentWeaponPrefab = bombPrefab;
        else currentWeaponPrefab = shotPrefab;
    }

}
