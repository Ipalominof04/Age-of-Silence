using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour, IDamageable
{
    //singleton
    public static PlayerController Instance;

    private CharacterController characterController;
    private SoundEmitter soundEmitter;
    private Animator animator;

    //estados del player
    public enum PlayerState { NONE, IDLE, WALK, RUN, CROUCH, SHOOT, INTERACT, DIALOGUE, THROW }

    [Header("Maquina de estados")]
    [SerializeField] public PlayerState currentState;
    private PlayerState previousState;

    [Header("Movimiento")]
    [SerializeField] private InputActionAsset myInputActionAsset;
    [SerializeField] private float slopeCheckDistance = 0.3f;
    [SerializeField] private float walkStepRate = 0.45f;
    [SerializeField] private float runStepRate = 0.28f;
    private InputAction walkAction;
    private InputAction runAction;
    private InputAction interactAction;
    private InputAction crouchAction;
    private InputAction shootAction;
    private Vector2 inputMoviment;
    private float gravity = -9.81f;
    private Vector3 verticalVelocity = Vector3.zero;
    private bool escaleraInRange = false;
    private Vector3 direccionEscalera;
    private bool isCrouchWalking = false;
    private float nextStepTime = 0f;

    [Header("Vida")]
    [SerializeField] private float vidaMaxima;
    [SerializeField] private GameObject luzBarraVida;
    public float vidaActual;

    [Header("Aiden")]
    [SerializeField] private GameObject aiden;
    [SerializeField] private float interactRange;
    private bool InDialogue;
    private TriggerDialogue dialogueActivo = null;

    [Header("Crouch")]
    [SerializeField] private CapsuleCollider playerCollider;
    [SerializeField] private Transform eyePoint;
    [SerializeField] private float eyeHeightStanding = 1.6f;
    [SerializeField] private float eyeHeightCrouch = 0.8f;

    [Header("Objetos")]
    [SerializeField] private ObjetoAgarrado objetoMano;
    private GameObject objectInRange = null;
    private bool canInteract = false;

    [Header("Eventos")]
    [SerializeField] private GameEvent OnDialogueStarted;
    [SerializeField] private GameEvent OnDialogueEnded;
    [SerializeField] private GameEvent OnInfoStarted;
    [SerializeField] private GameEvent OnInfoEnded;
    [SerializeField] private GameEvent OnAidenStopped;
    [SerializeField] private GameEvent onShoot;
    [SerializeField] private GameEvent onArmaSobrecalentada;
    [SerializeField] private GameEvent onArmaEnfriada;
    [SerializeField] private GameEventFloat onVidaCambiada;
    public static event Action<PlayerState> OnPlayerStateChanged;
    public static event Action OnPlayerEnteredDialogueTrigger;
    public static event Action OnPlayerDied;

    [Header("Lanzamiento")]
    [SerializeField] private ParabolaRenderer parabolaRenderer;
    [SerializeField] private Transform huesoMano;
    [SerializeField] private Camera camara;
    private ObjetoLanzable objetoLanzableEnMano;
    private InputAction throwAction;
    private GameObject objectLanzableInRange = null;

    [Header("Disparo")]
    [SerializeField] private GameObject bala;
    [SerializeField] private GameObject shootingPoint;
    private bool armaSobrecalentada;
    private bool armaEquipada = false;
    private bool isAiming = false;
    private GameObject armaEnMano = null;

    [Header("Ragdoll")]
    [SerializeField] private Transform ragdollRoot;
    private Rigidbody[] ragdollRigidbodies;

    [Header("Sound Waves")]
    [SerializeField] private MeshRenderer wavesRenderer;

    private IPuzle puzleInRange = null;
    private TriggerAyuda ayudaInRange = null;

    private bool isDying = false;
 
    // -

    //aquesta funcio serveix per a que l'enemic detecti al cap del player independenment de que estigui ajupit o de peu
    public Transform GetEyePoint() => eyePoint;
    public void ForceIdle() => ChangeState(PlayerState.IDLE);

    //inicialització del Singleton i dels components del player
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        //cas especial en les escenes de cinematiques, en aquestes escenes hi ha un player fals que no s'ha de reemplacar
        else if (SceneManager.GetActiveScene().name == "Escena1" || SceneManager.GetActiveScene().name == "EscenaFinal")
        {
            return;
        }
        else
        {
            Destroy(this.gameObject);
        }

        //asignar components
        characterController = GetComponent<CharacterController>();
        camara = Camera.main;
        soundEmitter = GetComponent<SoundEmitter>();
        animator = GetComponent<Animator>();

        //obtenir totes les accions del input
        var playerMap = InputManager.Instance.PlayerMap;
        walkAction = playerMap.FindAction("Move");
        runAction = playerMap.FindAction("Sprint");
        interactAction = playerMap.FindAction("Interact");
        crouchAction = playerMap.FindAction("Crouch");
        throwAction = playerMap.FindAction("Throw");
        shootAction = playerMap.FindAction("Attack");
        
        //inicialitzar al player amb el collider gran (collider de peu, en comptes del de crouch)
        SetColliderNormal();
        //init del ragdoll
        InitRagdoll();
    }

    void Start()
    {
        isDying = false;

        //forçar a buscar el shooting_point i la camara en la primera escena on neix el player
        shootingPoint = GameObject.Find("shooting_point");
        if (shootingPoint == null)
            Debug.Log("No hay arma en la escena");
        camara = Camera.main;

        //el player s'inicia en idle
        InitState(PlayerState.IDLE);

        //recuperar la vida guardada, en cas de que no existeixi vida guardada asignar la vida maxima
        float vidaGuardada = GameManager.Instance.datos.PlayerHealth;
        vidaActual = vidaGuardada > 0f ? vidaGuardada : vidaMaxima;
        onVidaCambiada.Raise(vidaActual / vidaMaxima);
        float ratio = vidaActual / vidaMaxima;
        DamageVignette.Instance?.UpdateHealthRatio(ratio);

        //esperar a que el mapa carregui abans de activar el charactercontroller per a que el player no es que caigui al apareixer
        StartCoroutine(WaitingChargeMap());
    }

    //llegeix l'input i l'actualitza cada frame, tambe aplica gravetat
    void Update()
    {
        if (isDying) return;
        inputMoviment = walkAction.ReadValue<Vector2>();
        UpdateState();
        ApplyGravity();
    }

    private void OnEnable()
    {
        OnDialogueStarted.Subscribe(() => InDialogue = true);
        OnDialogueEnded.Subscribe(() => { InDialogue = false; canInteract = false; });
        OnInfoStarted.Subscribe(() => InDialogue = true);
        OnInfoEnded.Subscribe(() => InDialogue = false);
        OnAidenStopped.Subscribe(() => canInteract = true);
        onArmaSobrecalentada.Subscribe(() => armaSobrecalentada = true);
        onArmaEnfriada.Subscribe(() => armaSobrecalentada = false);
        SceneManager.sceneLoaded += OnSceneLoaded;
        UpdateBrilloBarraVida(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnDisable()
    {
        OnDialogueStarted.Unsubscribe(() => InDialogue = true);
        OnDialogueEnded.Unsubscribe(() => { InDialogue = false; canInteract = false; dialogueActivo = null; });
        OnInfoStarted.Unsubscribe(() => InDialogue = true);
        OnInfoEnded.Unsubscribe(() => InDialogue = false);
        OnAidenStopped.Unsubscribe(() => canInteract = true);
        onArmaSobrecalentada.Unsubscribe(() => armaSobrecalentada = true);
        onArmaEnfriada.Unsubscribe(() => armaSobrecalentada = false);
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    //aquesta funcio es crida cada vegada que es carrega una escena nova
    //neteja les referencies de l'escena anterior per evitar bugs i actualiza els components del player als de l'escena
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //netejar referencies que apunten a l'escena anterior
        objectInRange = null;
        objectLanzableInRange = null;
        puzleInRange = null;
        ayudaInRange = null;
        dialogueActivo = null;

        UpdateBrilloBarraVida(scene, mode);

        shootingPoint = GameObject.Find("shooting_point");

        camara = Camera.main;
        if (camara == null)
        {
            camara = FindFirstObjectByType<Camera>();
        }
    }

    //corrutina que espera a que el navmesh i el mapa hagin carregat per activar el charactercontroller
    //evitant que el player es caigui a sota del mapa
    private IEnumerator WaitingChargeMap()
    {
        characterController.enabled = false;
        verticalVelocity = Vector3.zero;

        //espera fins que hi hagui un Navmesh sota el player
        while (!UnityEngine.AI.NavMesh.SamplePosition(transform.position, out _, 2f, UnityEngine.AI.NavMesh.AllAreas))
        {
            yield return null;
        }

        characterController.enabled = true;

        //esperar fins a que toqui el terra fisicament
        while (!IsGroundedOrOnSlope())
        {
            verticalVelocity.y = 0f;
            characterController.Move(Vector3.down * 0.05f);
            yield return null;
        }

        verticalVelocity.y = -2f;
    }
    #region State Machine
    private void InitState(PlayerState state)
    {
        currentState = state;
        switch (currentState)
        {
            case PlayerState.IDLE:
                if (previousState != PlayerState.SHOOT)
                    //ocultar ones de so al estar en idle, a no ser que l'estat anterior sigui SHOOT,
                    //en aquest cas, deixar que acabi l'animacio de les ones
                    SetWavesSprite(-1);
                if (!escaleraInRange)
                {
                    //idle amb arma
                    if (armaEquipada)
                    {
                        //si no hi ha arma
                        if (armaEnMano == null)
                        {
                            armaEquipada = false;
                            animator.Play("idle");
                        }
                        //si hi ha arma
                        else
                        {
                            animator.Play("idleArma");
                            //ajustar la posicio de l'arma a la ma del player
                            armaEnMano.transform.localPosition = new Vector3(0.0153f, 0.0766f, 0.0142f);
                            armaEnMano.transform.localRotation = Quaternion.Euler(33.74f, 137.574f, -44.139f);
                        }
                    }
                    //idle normal
                    else
                        animator.Play("idle");
                }
                break;
            case PlayerState.WALK:
                //cambiar les ones a les ones de WALK
                SetWavesSprite(0);
                if (!escaleraInRange)
                    animator.Play("walk");
                break;
            case PlayerState.RUN:
                //cambiar les ones a les ones de RUN
                SetWavesSprite(1);
                animator.Play("run");
                break;
            case PlayerState.CROUCH:
                //sense ones al estar en crouch perque no s'emet soroll
                SetWavesSprite(-1);
                if (!escaleraInRange)
                {
                    //fer el collider mes petit quan esta en CROUCH per a no ser visible pels enemics
                    characterController.center = new Vector3(0, 0.45f, 0);
                    characterController.radius = 0.4f;
                    characterController.height = 0.7f;
                    playerCollider.center = new Vector3(0, 0.55f, 0);
                    playerCollider.radius = 0.6f;
                    playerCollider.height = 0.7f;
                    eyePoint.localPosition = new Vector3(0, eyeHeightCrouch, 0);

                    if (armaEquipada)
                        animator.Play("crouch");
                    else
                    {
                        animator.SetBool("isCrouching", true);
                    }
                    //si el player es mou mentre esta en crouch activar crouchwalking
                    if (inputMoviment.magnitude > 0.01f)
                    {
                        isCrouchWalking = true;
                        animator.Play("crouchWalking");
                    }
                    else
                    {
                        isCrouchWalking = false;
                    }
                }
                break;
            case PlayerState.SHOOT:
                break;
            case PlayerState.INTERACT:
                break;
            case PlayerState.THROW:
                //si no te cap objecte a la ma (ma fisica del player)
                if (objetoLanzableEnMano == null)
                {
                    //i si hi ha un objecte llencable en range, asignar el objecte a la ma 
                    if (objectLanzableInRange != null)
                    {
                        objetoLanzableEnMano = objectLanzableInRange.GetComponent<ObjetoLanzable>();
                        objetoLanzableEnMano.hand = huesoMano.gameObject;
                        objetoLanzableEnMano?.Recoger();
                        objectLanzableInRange = null;
                    }
                    else
                    {
                        ChangeState(previousState);
                    }
                }
                break;
        }
    }

    //es gestionan tots els canvis d'estat d'epenent de l'input pressionat per el player
    private void UpdateState()
    {
        //si esta en dialogue, bloquejar l'input per a que no es pugui moure
        if (InDialogue) return;

        switch (currentState)
        {
            case PlayerState.IDLE:
                //si es pulsa el boto d'interact es cambia a determinat estat depenent del cas
                if (interactAction.triggered)
                {
            
                    if (objectInRange != null || puzleInRange != null)
                    {
                        ChangeState(PlayerState.INTERACT);
                    }
                    else if (armaEnMano != null && !armaEquipada)
                    {
                        ChangeState(PlayerState.INTERACT);
                    }
                    else if (canInteract && objectInRange == null)
                    {
                        ChangeState(PlayerState.DIALOGUE);
                    }
                    else if (objectLanzableInRange != null)
                    {
                        ChangeState(PlayerState.THROW);
                    }     
                }
                //si el arma esta equipada i el jugador dispara sense que estigui sobreescalfada
                else if (armaEquipada && shootAction.triggered && !armaSobrecalentada)
                {
                    //activar ones de so del tret
                    SetWavesSprite(2);
                    ChangeState(PlayerState.SHOOT);
                }

                if (inputMoviment.magnitude > 0.01f) ChangeState(PlayerState.WALK);
                if (crouchAction.IsPressed()) ChangeState(PlayerState.CROUCH);
                if (objetoLanzableEnMano != null && throwAction.IsPressed())
                    ChangeState(PlayerState.THROW);
                if (interactAction.triggered && ayudaInRange != null)
                    ayudaInRange.PedirAyuda();
                break;
            case PlayerState.WALK:
                if (escaleraInRange) ClimbLadder();
                else Walk();
                if (inputMoviment.magnitude < 0.01f) ChangeState(PlayerState.IDLE);
                if (runAction.IsPressed() && IsGroundedOrOnSlope()) ChangeState(PlayerState.RUN);
                if (crouchAction.IsPressed()) ChangeState(PlayerState.CROUCH);
                if (objetoLanzableEnMano != null && throwAction.IsPressed())
                    ChangeState(PlayerState.THROW);
                break;

            case PlayerState.RUN:
                if (escaleraInRange) ClimbLadder();
                else Run();
                if (!runAction.IsPressed()) ChangeState(PlayerState.WALK);
                if (inputMoviment.magnitude < 0.01f) ChangeState(PlayerState.IDLE);
                break;

            case PlayerState.CROUCH:
                if (escaleraInRange)
                {
                    ClimbLadder();
                }
                else
                {
                    Crouch();
                    //transicio per l'animacio de crouch i crouchwalking depenent de si el player es mou o no mentres esta ajupit
                    if (inputMoviment.magnitude > 0.01f && !isCrouchWalking)
                    {
                        isCrouchWalking = true;
                        animator.Play("crouchWalking");
                    }
                    else if (inputMoviment.magnitude < 0.01f && isCrouchWalking)
                    {
                        isCrouchWalking = false;
                        animator.Play("crouch");
                    }
                }
                if (!crouchAction.IsPressed()) ChangeState(PlayerState.IDLE);
                if (interactAction.triggered && (objectInRange != null || puzleInRange != null) && crouchAction.IsPressed())
                    ChangeState(PlayerState.INTERACT);
                if (interactAction.triggered && armaEnMano != null && crouchAction.IsPressed() && !armaEquipada)
                    ChangeState(PlayerState.INTERACT);
                if (interactAction.triggered && canInteract && objectInRange == null)
                    ChangeState(PlayerState.DIALOGUE);
                if (interactAction.triggered && objectLanzableInRange != null)
                    ChangeState(PlayerState.THROW);
                if (objetoLanzableEnMano != null && throwAction.IsPressed())
                    ChangeState(PlayerState.THROW);
                break;

            case PlayerState.SHOOT:
                Shoot();
                if (!shootAction.IsPressed()) ChangeState(PlayerState.IDLE);
                break;

            case PlayerState.INTERACT:
                Interact();
                //tornar al estat que hi habia abans d'interactuar
                ChangeState(previousState == PlayerState.CROUCH ? PlayerState.CROUCH : PlayerState.IDLE);
                break;

            case PlayerState.DIALOGUE:
                Dialogue();
                break;

            case PlayerState.THROW:
                ThrowObject();
                break;
        }
    }

    private void ChangeState(PlayerState newState)
    {
        if (newState == currentState) return;

        PlayerState leavingState = currentState;
        ExitState();
        previousState = currentState;
        InitState(newState);

        //cas especial: al passar de crouch a throw hi ha que mantenir el collider reduit
        if (leavingState == PlayerState.CROUCH && newState == PlayerState.THROW)
        {
            characterController.center = new Vector3(0, 0.45f, 0);
            characterController.radius = 0.4f;
            characterController.height = 0.7f;
            playerCollider.center = new Vector3(0, 0.55f, 0);
            playerCollider.radius = 0.6f;
            playerCollider.height = 0.7f;
            eyePoint.localPosition = new Vector3(0, eyeHeightCrouch, 0);
        }
        OnPlayerStateChanged?.Invoke(currentState);
    }

    private void ExitState()
    {
        switch (currentState)
        {
            case PlayerState.IDLE:
                soundEmitter.StopSound();
                break;
            case PlayerState.WALK:
                soundEmitter.StopSound();
                break;
            case PlayerState.RUN:
                soundEmitter.StopSound();
                break;
            case PlayerState.CROUCH:
                animator.SetBool("isCrouching", false);
                soundEmitter.StopSound();
                //restaurar el collider de tamany normal 
                SetColliderNormal();
                break;
            case PlayerState.SHOOT:
                soundEmitter.StopSound();
                break;
            case PlayerState.INTERACT:
                soundEmitter.StopSound();
                break;
            case PlayerState.THROW:
                //ocultar la linea de la trayectoria de la parabola
                parabolaRenderer.OcultarParabola();
                soundEmitter.StopSound();
                break;
        }
    }
    #endregion
    //comproba si el player esta en el terra utlitzant tant el charactercontroller com un raycast per a rampes
    private bool IsGroundedOrOnSlope()
    {
        if (characterController.isGrounded) return true;
        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down,
                               slopeCheckDistance, ~LayerMask.GetMask("Player"));
    }
    //activa la fisica del ragdoll al morir, desactiva el charactercontroller, el animator i llibera el rigidbody
    private void ActivateRagdoll()
    {
        characterController.enabled = false;
        if (animator != null) animator.enabled = false;
        foreach (var rb in ragdollRigidbodies)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
    private void InitRagdoll()
    {
        ragdollRigidbodies = ragdollRoot.GetComponentsInChildren<Rigidbody>();
        foreach (var rb in ragdollRigidbodies)
            rb.isKinematic = true;
    }
    //gestiona el sistema de llancament d'objectes, mentres el boto de llancament esta polsat es mostra la
    //parabola i el player rota a la direccio que el player vulgui i al soltar el boto l'objecte es llanca
    private void ThrowObject()
    {
        if (camara == null)
        {
            camara = Camera.main;

            if (camara == null)
            {
                return;
            }

            if (objetoLanzableEnMano == null)
            {
                ChangeState(previousState);
                return;
            }
        }

        if (throwAction.IsPressed())
        {
            //activar l'animacio d'apuntar segons si ve del crouch o no
            if (previousState == PlayerState.CROUCH && !isAiming)
            {
                isAiming = true;
                animator.SetBool("isAiming", true);
            }
            else if (previousState != PlayerState.CROUCH)
                animator.Play("aimThrow");

            //calcular el punt objectiu de caiguda de l'objecte i rotar al player cap a aquesta direcció
            Vector3 puntoObjetivo = parabolaRenderer.ObtenerPuntoObjetivo(huesoMano.position, camara);
            Vector3 direccion = puntoObjetivo - transform.position;
            direccion.y = 0;
            if (direccion.magnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(direccion);

            parabolaRenderer.MostrarParabola(huesoMano.position, puntoObjetivo);
        }
        else if (throwAction.WasReleasedThisFrame())
        {
            //llencar l'objecte
            isAiming = false;
            animator.SetBool("isAiming", false);
            Vector3 puntoObjetivo = parabolaRenderer.ObtenerPuntoObjetivo(huesoMano.position, camara);
            Vector3 velocidad = parabolaRenderer.CalcularVelocidadInicial(huesoMano.position, puntoObjetivo);
            objetoLanzableEnMano.Lanzar(velocidad, playerCollider);
            objetoLanzableEnMano = null;
            ChangeState(previousState);
        }
        else
        {
            isAiming = false;
            animator.SetBool("isAiming", false);
            parabolaRenderer.OcultarParabola();
            ChangeState(previousState);
        }
    }

    //aplicar gravetat manual al charactercontroller, aquesta gravetat es desactiva al pujar i baixar escales 
    private void ApplyGravity()
    {
        if (isDying) return;
        if (!characterController.enabled) return;

        if (escaleraInRange)
        {
            verticalVelocity.y = 0;
            return;
        }
        if (IsGroundedOrOnSlope())
            verticalVelocity.y = -2f;
        else
            verticalVelocity.y += gravity * Time.deltaTime;
        characterController.Move(verticalVelocity * Time.deltaTime);
    }
    //rota al player en la direccio del input i li aplica la velocitat
    private void Move(float velocity)
    {
        Vector3 movement = new Vector3(inputMoviment.x, 0f, inputMoviment.y).normalized;
        if (movement.magnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(movement);
        characterController.Move(movement * velocity * Time.deltaTime);
    }
    //velocitat i so de walk
    private void Walk()
    {
        Move(1.75f);

        if (Time.time >= nextStepTime)
        {
            soundEmitter.EmitSound(ESound.WALK);
            nextStepTime = Time.time + walkStepRate;
        }

    }
    //velocitat i so de run
    private void Run()
    {
        Move(3f);

        if (Time.time >= nextStepTime)
        {
            soundEmitter.EmitSound(ESound.RUN);
            nextStepTime = Time.time + runStepRate;
        }
    }
    //velocitat i so de crouch
    private void Crouch()
    {
        Move(1.5f);

    }
    //moviment especial per a les escales
    private void ClimbLadder()
    {
        transform.rotation = Quaternion.LookRotation(direccionEscalera);
        Vector3 verticalMovement = new Vector3(0, inputMoviment.y, 0);
        Vector3 horizontalMovement = new Vector3(inputMoviment.x, 0, 0);
        characterController.Move((verticalMovement + horizontalMovement) * 1f * Time.deltaTime);
    }
    //gestiona el que fa cada interaccio amb diferents objectes a traves dels tags dels objects
    private void Interact()
    {       
        if (objectInRange != null)
        {          
            if (objectInRange.CompareTag("Pieza"))
            {                
                if (vidaActual >= vidaMaxima)
                {                  
                    return;
                }
                Curar(2);
                objectInRange.SetActive(false);
                objectInRange = null;
                soundEmitter.EmitSound(ESound.PICKUP);
                return; 
            }
            //menjar: suma valor de confiança 
            if (objectInRange.CompareTag("Comida"))
            {
                GameManager.Instance.SumValor(6);
                objectInRange.SetActive(false);
                objectInRange = null;
                soundEmitter.EmitSound(ESound.PICKUP);
                return; 
            }

            //nomes els objectes normals com objectes de puzles pasen per el SO de ObjetoAgarrado
            if (string.IsNullOrEmpty(objetoMano.nombreObjeto))
            {
                objetoMano.SaveObject(objectInRange);
                GameManager.Instance.datos.objetoMano = objetoMano;
                objectInRange.SetActive(false);
                objectInRange = null;             
                soundEmitter.EmitSound(ESound.PICKUP);
            }
            else
            {
                Debug.Log("Mano ocupada");
            }
            return;
        }

        if (puzleInRange != null)
        {
            puzleInRange.Interact(objetoMano);
            return;
        }

        //equipar l'arma al os de la ma, limpiar el material de glow de l'arma i buscar el shooting_point de l'arma
        if (armaEnMano != null && !armaEquipada)
        {
            GameObject arma = armaEnMano;
            arma.transform.SetParent(huesoMano.transform);
            arma.transform.localPosition = new Vector3(-0.046f, 0.052f, -0.006f);
            arma.transform.localRotation = Quaternion.Euler(33.283f, 142.394f, 38.761f);
            arma.transform.localScale = Vector3.one;
            MeshRenderer meshArma = arma.GetComponent<MeshRenderer>();
            ArmaController arma_ctrl = arma.GetComponent<ArmaController>();

            //elimiar el material
            Material[] materials = meshArma.materials;
            Material[] newMaterials = new Material[materials.Length - 1];
            for (int i = 0, j = 0; i < materials.Length; i++)
            {
                if (i == 1) continue;
                newMaterials[j++] = materials[i];
            }
            meshArma.materials = newMaterials;
            armaEquipada = true;
            arma_ctrl?.Equipar();

            //buscar el shooting_point
            shootingPoint = arma.transform.Find("shooting_point")?.gameObject;
            if (shootingPoint == null)
                shootingPoint = GameObject.Find("shooting_point");
            soundEmitter.EmitSound(ESound.PICKUP);
        }
    }
    //sumar vida al jugador, i guardar nou valor (aixo es per al GodMode)
    public void Curar(float cura)
    {
        vidaActual += cura;
        vidaActual = Mathf.Clamp(vidaActual, 0f, vidaMaxima);
        GameManager.Instance.datos.PlayerHealth = vidaActual;
        float ratio = vidaActual / vidaMaxima;
        onVidaCambiada.Raise(ratio);
        DamageVignette.Instance?.UpdateHealthRatio(ratio);
    }
    //comproba la distancia entre el player i l'aiden e inicia un dialog si el player esta dins del rang
    private void Dialogue()
    {
        if (aiden == null)
        {
            ChangeState(PlayerState.IDLE);
            return;
        }

        float distance = Vector3.Distance(transform.position, aiden.transform.position);
        if (distance <= interactRange)
        {
            DialogueManager.Instance.StartDialogue(dialogueActivo.graphDialogue);
        }
      
        ChangeState(PlayerState.IDLE);
    }
    //instancia una bala desde el shooting_point
    private void Shoot()
    {
        if (shootingPoint == null) return;
        if (!armaEquipada || armaEnMano == null)
        {
            armaEquipada = false;
            ChangeState(PlayerState.IDLE);
            return;
        }

        //si no esta sobreescalfada
        if (!armaSobrecalentada)
        {

            GameObject go = Instantiate(bala, shootingPoint.transform.position, shootingPoint.transform.rotation);
            BalaController balaDefault = go.GetComponent<BalaController>();
            if (balaDefault != null)
                balaDefault.SetDamage(1);

            Rigidbody rb = go.GetComponent<Rigidbody>();
            if (rb != null)
                rb.linearVelocity = this.gameObject.transform.forward * 20f;

            onShoot.Raise();
            ChangeState(PlayerState.IDLE);
            soundEmitter.EmitSound(ESound.SHOOT);

            //particula flash arma
            ArmaController arma = armaEnMano.GetComponent<ArmaController>();
            if (arma != null && arma.shootFlash != null)
            {
                arma.shootFlash.Play();

            }
            else
            {
                ChangeState(PlayerState.IDLE);
            }
        }
    }

    //resta vida i guarda el nou valor al gm, notifica a la ui per a la vinyeta i comproba si el jugador ha de morir 
    //si estas en invencible dins del GodMode fa un return
    public void TakeDamage(float damage)
    {
        if (GodMode.Instance.Invencible) return;
        if (isDying) return;
        if (GameManager.Instance.IsChangingScene) return;
        vidaActual -= damage;
        vidaActual = Mathf.Clamp(vidaActual, 0f, vidaMaxima);
        GameManager.Instance.datos.PlayerHealth = vidaActual;

        float ratio = vidaActual / vidaMaxima;
        onVidaCambiada.Raise(ratio);

        // Actualitza l'alpha base + dispara el flash
        DamageVignette.Instance?.UpdateHealthRatio(ratio);
        DamageVignette.Instance?.PlayDamageFlash();

        if (vidaActual <= 0f) Die();
    }
    //gestiona la mort del jugador, activa el ragdoll i restaura les dades del inici de la escena per fer un reinici
    public void Die()
    {
        if (isDying) return;
        isDying = true;
        soundEmitter.StopSound();
        OnPlayerDied?.Invoke();

        SetWavesSprite(-1);
        ClearObjetoLanzable();

        //soltar l'arma si el jugador la te a la ma
        if (armaEnMano != null)
        {
            ArmaController arma_ctrl = armaEnMano.GetComponent<ArmaController>();
            arma_ctrl?.transform.SetParent(null);
            arma_ctrl?.Soltar();
            armaEnMano = null;
            armaEquipada = false;
        }

        ActivateRagdoll();
        //restaurar les dades de guardat del principi de l'escena 
        GameManager.Instance.datos.PlayerHealth = GameManager.Instance.GetHealthAlEntrarEscena();
        GameManager.Instance.datos.confianza = GameManager.Instance.GetConfianzaAlEntrarEscena();
        GameManager.Instance.confianza = GameManager.Instance.GetConfianzaAlEntrarEscena();
        GameManager.Instance.LimpiarSOEscenas();
        GameManager.Instance.datos.puzzlesResueltos.Clear();
        GameManager.Instance.datos.objetoMano.ClearObject();
        InputManager.Instance.PlayerMap.Disable();
        StartCoroutine(RespawnDelay());
        StartCoroutine(RespawnDelay());
    }
    //espera uns segons abans de reiniciar la escena per donar-li temps al ragdoll
    private IEnumerator RespawnDelay()
    {
        yield return new WaitForSeconds(2f);
        DamageVignette.Instance?.SetVignetteActive(false);
        //el respawndelay crida a changescene del gm, el qual es el que crida a ResetRespawn()
        //aixi que morir tambie neteja variables
        GameManager.Instance.IrAScene(SceneManager.GetActiveScene().name);
    }
    //ajusta la brillantor de la barra de vida segons l'escena
    private void UpdateBrilloBarraVida(Scene scene, LoadSceneMode mode)
    {
        if (luzBarraVida == null) return;

        Light luz = luzBarraVida.GetComponent<Light>();
        if (luz == null) return;

        switch (scene.name)
        {
            case "Escena2": luz.intensity = 1f; luz.range = 1f; break;
            case "Escena4": luz.intensity = 1.5f; luz.range = 1f; break;
            case "Entrada": luz.intensity = 0.5f; luz.range = 1f; break;
            case "SalidaFabrica": luz.intensity = 4f; luz.range = 1f; break;
            case "SigiloSalida": luz.intensity = 55f; luz.range = 1f; break;
            case "ExteriorGasolinera": luz.intensity = 100f; luz.range = 1f; break;
            case "InteriorGasolinera": luz.intensity = 12f; luz.range = 1f; break;
            case "Muro": luz.intensity = 65f; luz.range = 1f; break;
            case "SalidaMuro": luz.intensity = 175f; luz.range = 1f; break;
            case "ExteriorTienda": luz.intensity = 250f; luz.range = 1f; break;
            case "InteriorTienda": luz.intensity = 45f; luz.range = 1f; break;
            case "Tienda2": luz.intensity = 0.15f; luz.range = 1f; break;
            case "CabañaNPC": luz.intensity = 500f; luz.range = 1f; break;
            case "InteriorCabaña": luz.intensity = 0.5f; luz.range = 1f; break;
            case "PuenteBosque": luz.intensity = 500f; luz.range = 1f; break;
            case "MaderaBosque": luz.intensity = 150f; luz.range = 1f; break;
            //si l'escena no esta llistada no fa res
        }
    }
    //asigna el nivell de ones que ha de possar segons l'estat, la logica de la animacio esta a SoundWavesController
    private void SetWavesSprite(int index)
    {
        if (wavesRenderer == null) return;
        if (index < 0)
        {
            wavesRenderer.gameObject.SetActive(false);
            return;
        }
        wavesRenderer.gameObject.SetActive(true);
        wavesRenderer.GetComponent<SoundWavesController>()?.SetWaveState(index);
    }

    //assignar el collider normal (el de peu)
    private void SetColliderNormal()
    {
        characterController.center = new Vector3(0, 0.8f, 0);
        characterController.radius = 0.2f;
        characterController.height = 1.5f;
        playerCollider.center = new Vector3(0, 0.9f, 0);
        playerCollider.radius = 0.3f;
        playerCollider.height = 1.6f;
        eyePoint.localPosition = new Vector3(0, eyeHeightStanding, 0);
    }

    //reiniciar al player (Es crida desde el GM al cargar escena nova i netejar referencies de escenes passades)
    public void RespawnReset()
    {
        InputManager.Instance.PlayerMap.Enable();

        //neteja flags i referencies 
        isDying = false;
        escaleraInRange = false;
        objectInRange = null;
        objectLanzableInRange = null;
        puzleInRange = null;
        ayudaInRange = null;
        dialogueActivo = null;
        canInteract = false;
        InDialogue = false;
        armaSobrecalentada = false;

        //treu l'arma de la ma
        ClearArma();

        if (armaEnMano != null)
        {
            ArmaController arma_ctrl = armaEnMano.GetComponent<ArmaController>();
            arma_ctrl?.transform.SetParent(null);
            arma_ctrl?.Soltar();
            armaEnMano = null;
        }
        armaEquipada = false;

        //reinicia el ragdoll
        foreach (var rb in ragdollRigidbodies)
            rb.isKinematic = true;

        //reinicia l'animator
        if (animator != null)
        {
            animator.enabled = true;
            animator.Rebind();
            animator.Update(0f);
            animator.Play("idle");
        }

        verticalVelocity = Vector3.zero;

        //restaura vida i actualitza la ui
        vidaActual = GameManager.Instance.datos.PlayerHealth;
        onVidaCambiada.Raise(vidaActual / vidaMaxima);
        DamageVignette.Instance?.ResetHealthRatio(vidaActual / vidaMaxima);

        ChangeState(PlayerState.IDLE);

        GameManager.Instance.MoverPlayerSpawn();
        StartCoroutine(WaitingChargeMap());
    }

    //gestio de tots el triggers del joc, separa la logica segons layer i tag
    private void OnTriggerEnter(Collider other)
    {
        //layer TriggerAction: triggers de la logica del joc
        if (other.gameObject.layer == LayerMask.NameToLayer("TriggerAction"))
        {
            //guardar referencia del dialeg actiu per por iniciar-ho al interactuar amb l'aiden
            if (other.tag == "TriggerDialogue")
            {
                OnPlayerEnteredDialogueTrigger?.Invoke();
                if (other.TryGetComponent<TriggerDialogue>(out TriggerDialogue dialogue))
                    dialogueActivo = dialogue;
            }
            //registrar puzle en rang
            if (other.tag == "TriggerPuzle")
            {
                IPuzle puzle = other.GetComponent<IPuzle>();
                if (puzle != null)
                {
                    puzleInRange = puzle;
                    puzle.OnEnterRange();
                    ChangeState(PlayerState.IDLE);
                }
            }
            //iniciar info automaticament al entrar al trigger
            if (other.tag == "TriggerInfo")
            {
                TriggerInfo info = other.GetComponent<TriggerInfo>();
                if (info != null)
                {
                    DialogueManager.Instance.StartInfo(info.graphInfo);
                    ChangeState(PlayerState.IDLE);
                }
            }
            //activar mode escalera de ma, fixar la posicio i cambiar la rotacio
            if (other.tag == "TriggerEscalera")
            {
                escaleraInRange = true;
                direccionEscalera = -other.transform.forward;
                direccionEscalera.y = 0;
                direccionEscalera.Normalize();
                transform.rotation = Quaternion.LookRotation(direccionEscalera);
                animator.Play("idle");
            }
            //trigger per pedir ajuda a aiden
            if (other.tag == "TriggerAyuda")
            {
                TriggerAyuda ayuda = other.GetComponent<TriggerAyuda>();
                if (ayuda != null)
                    ayudaInRange = ayuda;
            }
        }
        //layer Objetos: items agafables e interactuables
        if (other.gameObject.layer == LayerMask.NameToLayer("Objetos"))
        {
            //objectes agafables de puzles
            if (other.tag == "Rama" || other.tag == "Palanca" || other.tag == "Cizallas" ||
                other.tag == "Fusible" || other.tag == "Galon" || other.tag == "Cerillas" || other.tag == "MaderasPuente" 
                || other.tag == "PaletPuente" || other.tag == "MaderaPequeñaPuente")
            {
                if (objectInRange == null)
                {
                    objectInRange = other.gameObject;
                }
            }
            //objectes llencables
            if (other.tag == "Botella" || other.tag == "Roca")
            {
                if (objectLanzableInRange == null)
                {
                    objectLanzableInRange = other.gameObject;
                }
            }
            //arma agafable
            if (other.tag == "Arma")
            {
                if (armaEnMano == null)
                {
                    armaEnMano = other.gameObject;
                }
            }
            //consumibles d'aiden
            if (other.tag == "Comida" || other.tag == "Agua")
            {
                if (objectInRange == null)
                {
                    objectInRange = other.gameObject;
                }
            }
            //peces per recuperar vida
            if (other.tag == "Pieza")
            {
                if (objectInRange == null)
                {
                    objectInRange = other.gameObject;
                }
            }
            //objecte especial del puzle npc
            if (other.tag == "ComidaNPC")
            {
                if (objectInRange == null)
                {
                    objectInRange = other.gameObject;
                }
            }
        }
    }
    //neteja les referencies
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == objectInRange)
            objectInRange = null;

        if (puzleInRange != null && other.GetComponent<IPuzle>() == puzleInRange)
            puzleInRange = null;

        if (other.gameObject == objectLanzableInRange?.gameObject)
            objectLanzableInRange = null;

        if (other.tag == "TriggerEscalera")
        {
            escaleraInRange = false;
            if (currentState == PlayerState.WALK)
                animator.Play("walk");
            else if (currentState == PlayerState.RUN)
                animator.Play("run");
            else
                animator.Play("idle");
        }

        if (other.tag == "TriggerAyuda")
            ayudaInRange = null;
    }

    //neteja extra de referencies de puzle i trigger ajuda (es crida desde els scripts de puzles i d'ajuda
    public void ClearTriggerPuzle() { puzleInRange = null; }
    public void ClearTriggerAyuda() { ayudaInRange = null; }

    //neteja arma
    public void ClearArma()
    {
        if (armaEnMano != null)
        {
            ArmaController arma_ctrl = armaEnMano.GetComponent<ArmaController>();
            arma_ctrl?.transform.SetParent(null);
            arma_ctrl?.Soltar();
            armaEnMano = null;
        }
        armaEquipada = false;
    }

    //neteja objecte llancable
    public void ClearObjetoLanzable()
    {
        if (objetoLanzableEnMano == null) return;

        objetoLanzableEnMano.Soltar();
        objetoLanzableEnMano = null;
    }
}