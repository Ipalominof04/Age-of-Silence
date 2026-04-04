using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    private SoundEmitter soundEmitter;

    public enum PlayerState { NONE, IDLE, WALK, RUN, CROUCH, INTERACT, DIALOGUE, THROW }

    [Header("Maquina de estados")]
    [SerializeField] private PlayerState currentState;

    [Header("Movimiento")]
    [SerializeField] private InputActionAsset myInputActionAsset;
    private InputActionAsset inputInstance;
    private InputAction walkAction;
    private InputAction runAction;
    private InputAction interactAction;
    private InputAction crouchAction;
    private Vector2 inputMoviment;
    private float gravity = -9.81f;
    private Vector3 verticalVelocity = Vector3.zero;

    [Header("Aiden")]
    [SerializeField] private GameObject aiden;
    [SerializeField] private float interactRange;

    [Header("Crouch")]
    [SerializeField] private Vector3 standCenter;
    [SerializeField] private Vector3 crouchCenter;
    [SerializeField] private float standRadius;
    [SerializeField] private float crouchRadius;
    [SerializeField] private float standHeight;
    [SerializeField] private float crouchHeight;
    [SerializeField] private CapsuleCollider standCollider;
    [SerializeField] private CapsuleCollider crouchCollider;

    [Header("Objetos")]
    [SerializeField] private ObjetoAgarrado objetoMano;

    [Header("Eventos")]
    [SerializeField] private GameEvent OnDialogueStarted;
    [SerializeField] private GameEvent OnDialogueEnded;
    [SerializeField] private GameEvent OnAidenStopped;
    public static event Action<PlayerState> OnPlayerStateChanged;
    public static event Action OnPlayerEnteredDialogueTrigger;

    [Header("Lanzamiento")]
    [SerializeField] private ParabolaRenderer parabolaRenderer;
    [SerializeField] private Transform puntoLanzamiento;
    [SerializeField] private Camera camara;
    private ObjetoLanzable objetoLanzableEnMano;
    private InputAction throwAction;

    private bool InDialogue;
    private bool canInteract = false;
    private GameObject objectInRange = null;
    private GameObject objectLanzableInRange = null;
    private TriggerPuzle puzleInRange = null;
    private TriggerDialogue dialogueActivo = null;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        soundEmitter = GetComponent<SoundEmitter>();
        inputInstance = Instantiate(myInputActionAsset);
        inputInstance.FindActionMap("Player").Enable();
        walkAction = inputInstance.FindActionMap("Player").FindAction("Move");
        runAction = inputInstance.FindActionMap("Player").FindAction("Sprint");
        interactAction = inputInstance.FindActionMap("Player").FindAction("Interact");
        crouchAction = inputInstance.FindActionMap("Player").FindAction("Crouch");
        throwAction = inputInstance.FindActionMap("Player").FindAction("Throw");
     //   lookAction = inputInstance.FindActionMap("Player").FindAction("Look");
    }

    void Start()
    {
        InitState(PlayerState.IDLE);
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
    }

    private void OnDisable()
    {
        OnDialogueStarted.Unsubscribe(() => InDialogue = true);
        OnDialogueEnded.Unsubscribe(() => { InDialogue = false; canInteract = false; dialogueActivo = null; });
        OnAidenStopped.Unsubscribe(() => canInteract = true);
    }

    #region State Machine
    private void InitState(PlayerState state)
    {
        currentState = state;
        switch (currentState)
        {
            case PlayerState.IDLE:
                break;
            case PlayerState.WALK:
                break;
            case PlayerState.RUN:
                break;
            case PlayerState.CROUCH:
                characterController.center = crouchCenter;
                characterController.radius = crouchRadius;
                characterController.height = crouchHeight;
                standCollider.enabled = false;
                crouchCollider.enabled = true;
                break;
            case PlayerState.INTERACT:
                break;
            case PlayerState.THROW:
                if (objetoLanzableEnMano == null)
                {
                    if (objectLanzableInRange != null)
                    {
                        objetoLanzableEnMano = objectLanzableInRange.GetComponent<ObjetoLanzable>();
                        objetoLanzableEnMano?.Recoger();
                        objectLanzableInRange = null;
                    }
                    else
                    {
                        ChangeState(PlayerState.IDLE);
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

                if (interactAction.triggered && (objectInRange != null || puzleInRange != null))
                    ChangeState(PlayerState.INTERACT);

                if (interactAction.triggered && canInteract && objectInRange == null)
                    ChangeState(PlayerState.DIALOGUE);

                if (interactAction.triggered && objectLanzableInRange != null)
                    ChangeState(PlayerState.THROW);

                if (objetoLanzableEnMano != null && throwAction.IsPressed())
                    ChangeState(PlayerState.THROW);
                break;

            case PlayerState.WALK:
                Walk();
                if (inputMoviment.magnitude < 0.01f) ChangeState(PlayerState.IDLE);
                if (runAction.IsPressed() && characterController.isGrounded) ChangeState(PlayerState.RUN);
                if (crouchAction.IsPressed()) ChangeState(PlayerState.CROUCH);

                if (objetoLanzableEnMano != null && throwAction.IsPressed())
                    ChangeState(PlayerState.THROW);
                break;

            case PlayerState.RUN:
                Run();
                if (!runAction.IsPressed()) ChangeState(PlayerState.WALK);
                if (inputMoviment.magnitude < 0.01f) ChangeState(PlayerState.IDLE);
                break;

            case PlayerState.CROUCH:
                Crouch();
                if (!crouchAction.IsPressed()) ChangeState(PlayerState.IDLE);
                break;

            case PlayerState.INTERACT:
                Interact();
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
                characterController.center = standCenter;
                characterController.radius = standRadius;
                characterController.height = standHeight;
                standCollider.enabled = true;
                crouchCollider.enabled = false;
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
            ChangeState(PlayerState.IDLE);
            return;
        }

        if (throwAction.IsPressed())
        {
            Vector3 puntoObjetivo = parabolaRenderer.ObtenerPuntoObjetivo(puntoLanzamiento.position, camara);

            // Rotar player hacia el punto objetivo
            Vector3 direccion = puntoObjetivo - transform.position;
            direccion.y = 0;
            if (direccion.magnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(direccion);

            parabolaRenderer.MostrarParabola(puntoLanzamiento.position, puntoObjetivo);
        }
        else if (throwAction.WasReleasedThisFrame())
        {
            Vector3 puntoObjetivo = parabolaRenderer.ObtenerPuntoObjetivo(puntoLanzamiento.position, camara);
            Vector3 velocidad = parabolaRenderer.CalcularVelocidadInicial(puntoLanzamiento.position, puntoObjetivo);
            objetoLanzableEnMano.Lanzar(velocidad, GetComponent<CapsuleCollider>());
            objetoLanzableEnMano = null;
            ChangeState(PlayerState.IDLE);
        }
        else
        {
            parabolaRenderer.OcultarParabola();
            ChangeState(PlayerState.IDLE);
        }
    }

    private void ApplyGravity()
    {
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
        Move(2f);
        soundEmitter.EmitSound(ESound.WALK);
    }

    private void Run()
    {
        Move(3f);
        soundEmitter.EmitSound(ESound.WALK);
    }

    private void Crouch()
    {
        Move(1f);
        soundEmitter.EmitSound(ESound.CROUCH);
    }

    private void Interact()
    {
        if (objectInRange != null)
        {
            if (string.IsNullOrEmpty(objetoMano.nombreObjeto))
            {
                objetoMano.GuardarObjeto(objectInRange);
                objectInRange.SetActive(false);
                objectInRange = null;
                Debug.Log("Objeto recogido: " + objetoMano.nombreObjeto);
            }
            else
            {
                Debug.Log("Mano ocupada");
            }
        }
        else if (puzleInRange != null)
        {
            puzleInRange.TryOpen(objetoMano);
        }

        ChangeState(PlayerState.IDLE);
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("TriggerAction"))
        {
            if (other.tag == "TriggerDialogue")
            {
                OnPlayerEnteredDialogueTrigger?.Invoke();

                TriggerDialogue dialogue = other.GetComponent<TriggerDialogue>();
                if (dialogue != null)
                    dialogueActivo = dialogue;
            }

            if (other.tag == "TriggerPuzle")
            {
                TriggerPuzle puzle = other.GetComponent<TriggerPuzle>();
                if (puzle != null)
                {
                    puzleInRange = puzle;
                    if (!puzle.infoMostrada)
                    {
                        puzle.infoMostrada = true;
                        DialogueManager.Instance.StartInfo(puzle.graphInfo);
                    }
                }
            }

            if (other.tag == "TriggerInfo")
            {
                TriggerInfo info = other.GetComponent<TriggerInfo>();
                if (info != null)
                    DialogueManager.Instance.StartInfo(info.graphInfo);
            }
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Objetos"))
        {
            if (other.tag == "Rama" || other.tag == "Palanca" || other.tag == "Cizallas")
            {
                objectInRange = other.gameObject;
                print("objeto " + other.gameObject.name + " en rango");
            }

            if (other.tag == "Botella")
            {
                objectLanzableInRange = other.gameObject;

                ObjetoLanzable lanzable = other.GetComponent<ObjetoLanzable>();
                if (lanzable != null)
                    lanzable.MostrarUI();

                print("objeto " + other.gameObject.name + " en rango");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == objectInRange)
            objectInRange = null;

        if (other.gameObject == puzleInRange?.gameObject)
            puzleInRange = null;

        if (other.gameObject == objectLanzableInRange?.gameObject)
        {
            objectLanzableInRange = null;

            ObjetoLanzable lanzable = other.GetComponent<ObjetoLanzable>();
            if (lanzable != null)
                lanzable.OcultarUI();
        }
    }
}