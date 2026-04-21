using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour, IDamageable
{
    private CharacterController characterController;
    private SoundEmitter soundEmitter;

    public enum PlayerState { NONE, IDLE, WALK, RUN, CROUCH, SHOOT, INTERACT, DIALOGUE, THROW }


    [Header("Maquina de estados")]
    [SerializeField] public PlayerState currentState;

    [Header("Movimiento")]
    [SerializeField] private InputActionAsset myInputActionAsset;
    private InputActionAsset inputInstance;
    private InputAction walkAction;
    private InputAction runAction;
    private InputAction interactAction;
    private InputAction crouchAction;
    private InputAction shootAction;
    private Vector2 inputMoviment;
    private float gravity = -9.81f;
    private Vector3 verticalVelocity = Vector3.zero;

    [Header("Vida")]
    [SerializeField] private float vidaMaxima;
    public float vidaActual;

    [Header("Aiden")]
    [SerializeField] private GameObject aiden;
    [SerializeField] private float interactRange;

    [Header("Crouch")]
    [SerializeField] private CapsuleCollider playerCollider;
    [SerializeField] private Transform eyePoint;

    [SerializeField] private float eyeHeightStanding = 1.6f;
    [SerializeField] private float eyeHeightCrouch = 0.8f;

    [Header("Objetos")]
    [SerializeField] private ObjetoAgarrado objetoMano;

    [Header("Eventos")]
    [SerializeField] private GameEvent OnDialogueStarted;
    [SerializeField] private GameEvent OnDialogueEnded;
    [SerializeField] private GameEvent OnAidenStopped;
    [SerializeField] private GameEvent onShoot;
    [SerializeField] private GameEvent onArmaSobrecalentada;
    [SerializeField] private GameEvent onArmaEnfriada;
    [SerializeField] private GameEventFloat onVidaCambiada;
    public static event Action<PlayerState> OnPlayerStateChanged;
    public static event Action OnPlayerEnteredDialogueTrigger;

    [Header("Lanzamiento")]
    [SerializeField] private ParabolaRenderer parabolaRenderer;
    [SerializeField] private Transform huesoMano;
    [SerializeField] private Camera camara;
    private ObjetoLanzable objetoLanzableEnMano;
    private InputAction throwAction;

    [Header("Disparo")]
    [SerializeField] private GameObject bala;
    [SerializeField] private GameObject shootingPoint;
    private bool armaSobrecalentada;

    private bool InDialogue;
    private bool canInteract = false;
    private GameObject objectInRange = null;
    private GameObject armaEnMano = null;
    private bool armaEquipada = false;
    private GameObject objectLanzableInRange = null;
    private TriggerDialogue dialogueActivo = null;
    private bool escaleraInRange = false;
    private TriggerAyuda ayudaInRange = null;
    private Vector3 direccionEscalera;
    private bool isDying = false;

    private bool isCrouchWalking = false;
    private bool isClimbing = false;

    private IPuzle puzleInRange = null;

    private Animator animator;

    private PlayerState previousState;
    public Transform GetEyePoint()
    {
        return eyePoint;
    }
    public bool IsCrouching() => currentState == PlayerState.CROUCH;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        soundEmitter = GetComponent<SoundEmitter>();
        animator = GetComponent<Animator>();
        inputInstance = Instantiate(myInputActionAsset);
        inputInstance.FindActionMap("Player").Enable();
        walkAction = inputInstance.FindActionMap("Player").FindAction("Move");
        runAction = inputInstance.FindActionMap("Player").FindAction("Sprint");
        interactAction = inputInstance.FindActionMap("Player").FindAction("Interact");
        crouchAction = inputInstance.FindActionMap("Player").FindAction("Crouch");
        throwAction = inputInstance.FindActionMap("Player").FindAction("Throw");
        shootAction = inputInstance.FindActionMap("Player").FindAction("Attack");
        SetColliderNormal();

    }

    void Start()
    {
        isDying = false;
        shootingPoint = GameObject.Find("shooting_point");
        camara = Camera.main;
        if (shootingPoint == null)
            Debug.Log("No hay arma en la escena");

        InitState(PlayerState.IDLE);

        float vidaGuardada = GameManager.Instance.datos.PlayerHealth; 
        vidaActual = vidaGuardada > 0f ? vidaGuardada : vidaMaxima;
        onVidaCambiada.Raise(vidaActual / vidaMaxima);
    }

    void Update()
    {
        inputMoviment = walkAction.ReadValue<Vector2>();
        UpdateState();
        ApplyGravity();
    }

    private void OnEnable()
    {
        OnDialogueStarted.Subscribe(() => InDialogue = true);
        OnDialogueEnded.Subscribe(() => { InDialogue = false; canInteract = false; });
        OnAidenStopped.Subscribe(() => canInteract = true);
        onArmaSobrecalentada.Subscribe(() => armaSobrecalentada = true);
        onArmaEnfriada.Subscribe(() =>  armaSobrecalentada = false);

    }

    private void OnDisable()
    {
        OnDialogueStarted.Unsubscribe(() => InDialogue = true);
        OnDialogueEnded.Unsubscribe(() => { InDialogue = false; canInteract = false; dialogueActivo = null; });
        OnAidenStopped.Unsubscribe(() => canInteract = true);
        onArmaSobrecalentada.Unsubscribe(() => armaSobrecalentada = true);
        onArmaEnfriada.Unsubscribe(() => armaSobrecalentada = false);
    }
    #region State Machine
    private void InitState(PlayerState state)
    {
        currentState = state;
        switch (currentState)
        {
            case PlayerState.IDLE:
                if (!escaleraInRange)
                {
                    if (armaEquipada)
                    {
                        animator.Play("idle_arma");
                        armaEnMano.transform.localPosition = new Vector3(0.0153f, 0.0766f, 0.0142f);
                        armaEnMano.transform.localRotation = Quaternion.Euler(33.74f, 137.574f, -44.139f);
                    }    
                    else
                        animator.Play("idle");
                }
                break;
                
            case PlayerState.WALK:
                if (!escaleraInRange)
                    animator.Play("walk");
                break;
            case PlayerState.RUN:
                animator.Play("run");
                break;
            case PlayerState.CROUCH:
                animator.Play("crouch");
                characterController.center = new Vector3(0, 0.375f, 0);
                characterController.radius = 0.4f;
                characterController.height = 0.7f;
                playerCollider.center = new Vector3(0, 0.475f, 0);
                playerCollider.radius = 0.6f;
                playerCollider.height = 0.7f;

                eyePoint.localPosition = new Vector3(0, eyeHeightCrouch, 0);

                if (inputMoviment.magnitude > 0.01f)
                {
                    isCrouchWalking = true;
                    animator.Play("crouchWalking");
                }
                else
                {
                    isCrouchWalking = false;
                    animator.Play("crouch");
                }
                break;
            case PlayerState.SHOOT:
                break;
            case PlayerState.INTERACT:
                break;
            case PlayerState.THROW:
                if (objetoLanzableEnMano == null)
                {
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
                        //ChangeState(PlayerState.IDLE);
                    }
                }
                break;
        }
    }

    private void UpdateState()
    {
        if (InDialogue) return;

        switch (currentState)
        {
            case PlayerState.IDLE:
                if (inputMoviment.magnitude > 0.01f) ChangeState(PlayerState.WALK);
                if (crouchAction.IsPressed()) ChangeState(PlayerState.CROUCH);
                if (armaEquipada && shootAction.triggered) ChangeState(PlayerState.SHOOT);

                if (interactAction.triggered && (objectInRange != null || puzleInRange != null))
                    ChangeState(PlayerState.INTERACT);
                if (interactAction.triggered && armaEnMano != null)
                    ChangeState(PlayerState.INTERACT);

                if (interactAction.triggered && canInteract && objectInRange == null)
                    ChangeState(PlayerState.DIALOGUE);

                if (interactAction.triggered && objectLanzableInRange != null)
                    ChangeState(PlayerState.THROW);

                if (objetoLanzableEnMano != null && throwAction.IsPressed())
                    ChangeState(PlayerState.THROW);

                if (interactAction.triggered && ayudaInRange != null)
                    ayudaInRange.PedirAyuda();
                break;

            case PlayerState.WALK:
                if (escaleraInRange)
                    ClimbLadder();
                else
                    Walk();
                if (inputMoviment.magnitude < 0.01f) ChangeState(PlayerState.IDLE);
                if (runAction.IsPressed() && characterController.isGrounded) ChangeState(PlayerState.RUN);
                if (crouchAction.IsPressed()) ChangeState(PlayerState.CROUCH);
                if (objetoLanzableEnMano != null && throwAction.IsPressed())
                    ChangeState(PlayerState.THROW);
                break;

            case PlayerState.RUN:
                if (escaleraInRange)
                    ClimbLadder();
                else
                    Run();
                if (!runAction.IsPressed()) ChangeState(PlayerState.WALK);
                if (inputMoviment.magnitude < 0.01f) ChangeState(PlayerState.IDLE);
                break;

            case PlayerState.CROUCH:
                Crouch();

                //para que se actualice la animacion si el player deja de andar mientras esta agachado y se ponga en crouch idle
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

                if (!crouchAction.IsPressed()) ChangeState(PlayerState.IDLE);

                if (interactAction.triggered && (objectInRange != null || puzleInRange != null) && crouchAction.IsPressed())
                    ChangeState(PlayerState.INTERACT);
                if (interactAction.triggered && armaEnMano != null && crouchAction.IsPressed())
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
                ChangeState(PlayerState.IDLE);
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
        ExitState();
        // currentState = newState;
        previousState = currentState;
        InitState(newState);
        OnPlayerStateChanged?.Invoke(currentState);
    }

    private void ExitState()
    {
        switch (currentState)
        {
            case PlayerState.IDLE:
                break;
            case PlayerState.WALK:
                break;
            case PlayerState.RUN:
                break;
            case PlayerState.CROUCH:
                SetColliderNormal();
                break;
            case PlayerState.SHOOT:
                break;
            case PlayerState.INTERACT:
                break;
            case PlayerState.THROW:
                parabolaRenderer.OcultarParabola();
                break;
        }
    }
    #endregion

    private void ThrowObject()
    {
        if (objetoLanzableEnMano == null)
        {
            ChangeState(previousState);
            //ChangeState(PlayerState.IDLE);
            return;
        }

        if (throwAction.IsPressed())
        {
            Vector3 puntoObjetivo = parabolaRenderer.ObtenerPuntoObjetivo(huesoMano.position, camara);

            // Rotar player hacia el punto objetivo
            Vector3 direccion = puntoObjetivo - transform.position;
            direccion.y = 0;
            if (direccion.magnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(direccion);

            parabolaRenderer.MostrarParabola(huesoMano.position, puntoObjetivo);
        }
        else if (throwAction.WasReleasedThisFrame())
        {
            Vector3 puntoObjetivo = parabolaRenderer.ObtenerPuntoObjetivo(huesoMano.position, camara);
            Vector3 velocidad = parabolaRenderer.CalcularVelocidadInicial(huesoMano.position, puntoObjetivo);
            objetoLanzableEnMano.Lanzar(velocidad, playerCollider);
            objetoLanzableEnMano = null;
            ChangeState(previousState);
            //ChangeState(PlayerState.IDLE);
        }
        else
        {
            parabolaRenderer.OcultarParabola();
            ChangeState(previousState);
            //ChangeState(PlayerState.IDLE);
        }
    }
    private void ApplyGravity()
    {
        if (escaleraInRange)
        {
            verticalVelocity.y = 0;
            return;
        }
        if (characterController.isGrounded)
            verticalVelocity.y = -2f;
        else
            verticalVelocity.y += gravity * Time.deltaTime;

        characterController.Move(verticalVelocity * Time.deltaTime);
    }
    private void Move(float velocity)
    {
        Vector3 movement = new Vector3(inputMoviment.x, 0f, inputMoviment.y).normalized;
        if (movement.magnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(movement);
        characterController.Move(movement * velocity * Time.deltaTime);
    }

    private void Walk()
    {
        Move(1.75f);
       //soundEmitter.EmitSound(ESound.WALK);
    }

    private void Run()
    {
        Move(3f);
        //soundEmitter.EmitSound(ESound.WALK);
    }

    private void Crouch()
    {
        Move(1.5f);
        //soundEmitter.EmitSound(ESound.CROUCH);
    }

    private void ClimbLadder()
    {
        transform.rotation = Quaternion.LookRotation(direccionEscalera);

        Vector3 verticalMovement = new Vector3(0, inputMoviment.y, 0);
        Vector3 horizontalMovement = new Vector3(inputMoviment.x, 0, 0);

        animator.SetFloat("Speed", inputMoviment.magnitude);

        characterController.Move((verticalMovement + horizontalMovement) * 1f * Time.deltaTime);
    }

    private void Interact()
    {
        if (objectInRange != null)
        {
            if (string.IsNullOrEmpty(objetoMano.nombreObjeto))
            {
                //compruebar si es comida antes de guardarla
                if (objectInRange.CompareTag("Comida"))
                {
                    GameManager.Instance.SumValor(3); //sumo confianza
                    objectInRange.SetActive(false);
                    objectInRange = null;
                    return; //salir sin guardar en la mano

                } else if (objectInRange.CompareTag("Pieza"))
                {
                    if (vidaActual >= vidaMaxima)
                    {
                        Debug.Log("Vida al máximo, no necesitas la pieza");
                        return;

                    } else
                    {
                        vidaActual += 2f; // o la cantidad que quieras
                        vidaActual = Mathf.Clamp(vidaActual, 0f, vidaMaxima);
                        GameManager.Instance.datos.PlayerHealth = vidaActual;
                        onVidaCambiada.Raise(vidaActual / vidaMaxima);

                        objectInRange.SetActive(false);
                        objectInRange = null;
                        return;
                    }
                        
                }else
                {
                    objetoMano.GuardarObjeto(objectInRange);
                    GameManager.Instance.datos.objetoMano = objetoMano;
                    objectInRange.SetActive(false);
                    objectInRange = null;
                    Debug.Log("Objeto recogido: " + objetoMano.nombreObjeto);
                }   
            }
            else
            {
                Debug.Log("Mano ocupada");
            }
        }
        else if (puzleInRange != null)
        {
            puzleInRange.Interact(objetoMano);
        }
        else if (armaEnMano != null)
        {
            GameObject arma = armaEnMano;
            arma.transform.SetParent(huesoMano.transform);
            arma.transform.localPosition = new Vector3(-0.046f, 0.052f, -0.006f);     
            arma.transform.localRotation = Quaternion.Euler(33.283f, 142.394f, 38.761f);
            arma.transform.localScale = Vector3.one;
            MeshRenderer meshArma = arma.GetComponent<MeshRenderer>();
            ArmaController arma_ctrl = arma.GetComponent<ArmaController>();

            // Crear array sin el material en índice 1
            Material[] materials = meshArma.materials;
            Material[] newMaterials = new Material[materials.Length - 1];

            for (int i = 0, j = 0; i < materials.Length; i++)
            {
                if (i == 1) continue; // salta el material 1
                newMaterials[j++] = materials[i];
            }

            meshArma.materials = newMaterials;

            armaEquipada = true;
            arma_ctrl?.Equipar();

        }
    }
    private void Dialogue()
    {
        if (aiden == null)
        {
            Debug.LogError("Aiden no está asignado en el Inspector");
            ChangeState(PlayerState.IDLE);
            return;
        }

        float distance = Vector3.Distance(transform.position, aiden.transform.position);
        if (distance <= interactRange)
        {
            Debug.Log("Empieza diálogo");
            DialogueManager.Instance.StartDialogue(dialogueActivo.graphDialogue);
        }
        else
        {
            Debug.Log("Muy lejos de Aiden");
        }

        ChangeState(PlayerState.IDLE);
    }
    private void Shoot()
    {
        if(shootingPoint == null) return;

        if(!armaSobrecalentada)
        {
        GameObject go = Instantiate(bala, shootingPoint.transform.position, shootingPoint.transform.rotation);
            BalaController balaDefault = go.GetComponent<BalaController>();
            if (balaDefault != null)
                balaDefault.SetDamage(1);

            Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = this.gameObject.transform.forward * 20f;

        onShoot.Raise();
            //soundEmitter.EmitSound(ESound.SHOOT);
        }
        ChangeState(PlayerState.IDLE);

    }
    public void TakeDamage(float damage)
    {

        vidaActual -= damage;
        vidaActual = Mathf.Clamp(vidaActual, 0f, vidaMaxima);
        GameManager.Instance.datos.PlayerHealth = vidaActual;
        onVidaCambiada.Raise(vidaActual / vidaMaxima);

        if (vidaActual <= 0f)
            Die();
    }

    public void Die()
    {
        if (isDying) return;
        isDying = true;

        print("player muerto");
        GameManager.Instance.LimpiarSOEscenas();
        GameManager.Instance.IrAScene(SceneManager.GetActiveScene().name);
    }

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
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("TriggerAction"))
        {
            if (other.tag == "TriggerDialogue")
            {
                OnPlayerEnteredDialogueTrigger?.Invoke();

                if (other.TryGetComponent<TriggerDialogue>(out TriggerDialogue dialogue))
                    dialogueActivo = dialogue;
            }

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

            if (other.tag == "TriggerInfo")
            {
                TriggerInfo info = other.GetComponent<TriggerInfo>();
                if (info != null)
                {
                    DialogueManager.Instance.StartInfo(info.graphInfo);
                    ChangeState(PlayerState.IDLE);
                }

            }

            if (other.tag == "TriggerEscalera")
            {
                escaleraInRange = true;
                animator.SetBool("onLadder", true);
                direccionEscalera = (other.transform.position - transform.position).normalized;
                direccionEscalera.y = 0;
                // Fuerza al player a mirar hacia la escalera
                transform.rotation = Quaternion.LookRotation(direccionEscalera);
            }

            if (other.tag == "TriggerAyuda")
            {
                TriggerAyuda ayuda = other.GetComponent<TriggerAyuda>();
                if (ayuda != null)
                    ayudaInRange = ayuda;
            }
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Objetos"))
        {
            if (other.tag == "Rama" || other.tag == "Palanca" || other.tag == "Cizallas" || other.tag == "Fusible" || other.tag == "Galon" || other.tag == "Cerillas")
            {
                objectInRange = other.gameObject;
                print("objeto " + other.gameObject.name + " en rango");
            }

            if (other.tag == "Botella" || other.tag == "Roca")
            {
                objectLanzableInRange = other.gameObject;

                print("objeto " + other.gameObject.name + " en rango");
            }
            if (other.tag == "Arma")
            {
                armaEnMano = other.gameObject;
                print("objeto " + other.gameObject.name + " en rango");

            }
            if (other.tag == "Comida")
            {
                objectInRange = other.gameObject;
                print("objeto " + other.gameObject.name + " en rango");
            }
            if (other.tag == "Pieza")
            {
                objectInRange = other.gameObject;
                print("objeto " + other.gameObject.name + " en rango");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == objectInRange)
            objectInRange = null;

        if (puzleInRange != null && other.GetComponent<IPuzle>() == puzleInRange)
        {
            puzleInRange = null;
        }

        if (other.gameObject == objectLanzableInRange?.gameObject)
        {
            objectLanzableInRange = null;
        }

        if (other.tag == "TriggerEscalera")
        {
            escaleraInRange = false;
            isClimbing = false;
            animator.SetBool("onLadder", false);
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
    public void ClearTriggerPuzle()
    {
        puzleInRange = null;
    }
    public void ClearTriggerAyuda()
    {
        ayudaInRange = null;
    }

}