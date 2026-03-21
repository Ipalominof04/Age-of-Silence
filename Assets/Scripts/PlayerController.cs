using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static event Action<PlayerState> OnPlayerStateChanged;
    public static event Action OnPlayerEnteredDialogueTrigger;

    private CharacterController characterController;
    public enum PlayerState { NONE, IDLE, WALK, RUN, CROUCH, INTERACT, DIALOGUE }

    [Header("Maquina de estados")]
    [SerializeField] private PlayerState currentState;

    [Header("Movimiento")]
    [SerializeField] private InputActionAsset myInputActionAsset;
    private InputActionAsset inputInstance;
    private InputAction walkAction;
    private InputAction runAction;
    private InputAction interactAction;
    private InputAction crouchAction;
    Vector2 inputMoviment;
    private float gravity = -9.81f;
    private Vector3 verticalVelocity = Vector3.zero;


    [SerializeField] private GameObject aiden;
    [SerializeField] private float interactRange;


    public GameEvent OnDialogueStarted;
    public GameEvent OnDialogueEnded;
    public GameEvent OnAidenStopped;
    private bool InDialogue;
    private bool canInteract = false;

    [Header("Crouch")]
    [SerializeField] private Vector3 standCenter = new Vector3(0, 1.0f, 0);
    [SerializeField] private Vector3 crouchCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private float standRadius;
    [SerializeField] private float crouchRadius;
    [SerializeField] private float standHeight;
    [SerializeField] private float crouchHeight;
    [SerializeField] private CapsuleCollider standCollider;
    [SerializeField] private CapsuleCollider crouchCollider;


    private SoundEmitter soundEmitter;


    [SerializeField] ObjetoAgarrado objetoMano;
    private GameObject objectInRange = null;
    private DoorTrigger doorInRange = null;



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
        OnDialogueEnded.Unsubscribe(() => { InDialogue = false; canInteract = false; });
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
                characterController.center = standCenter;
                characterController.radius = standRadius;
                characterController.height = standHeight;
                standCollider.enabled = true;
                crouchCollider.enabled = false;

                break;
            case PlayerState.INTERACT:
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

                // Interact → solo para objetos
                if (interactAction.triggered && (objectInRange != null || doorInRange != null))
                    ChangeState(PlayerState.INTERACT);

                // Dialogue → solo para Aiden
                if (interactAction.triggered && canInteract && objectInRange == null)
                    ChangeState(PlayerState.DIALOGUE);
                break;
            case PlayerState.WALK:
                Move(2);
                if (inputMoviment.magnitude < 0.01f) ChangeState(PlayerState.IDLE);
                if (runAction.IsPressed() && characterController.isGrounded) ChangeState(PlayerState.RUN);
                if (crouchAction.IsPressed()) ChangeState(PlayerState.CROUCH);
                break;
            case PlayerState.RUN:
                Move(3);
                if (!runAction.IsPressed()) ChangeState(PlayerState.WALK);
                if (inputMoviment.magnitude < 0.01f) ChangeState(PlayerState.IDLE);
                break;
            case PlayerState.CROUCH:
                Crouch();
                Move(1);
                if (!crouchAction.IsPressed()) ChangeState(PlayerState.IDLE);
                break;
            case PlayerState.INTERACT:
                Interact();
                break;
            case PlayerState.DIALOGUE:
                Dialogue();
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

        }
    }
    #endregion

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
    private void Crouch()
    {
        characterController.center = crouchCenter;
        characterController.radius = crouchRadius;
        characterController.height = crouchHeight;

        standCollider.enabled = false;
        crouchCollider.enabled = true;


        float volume = soundEmitter.EmitSound(ESound.CROUCH);


    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("TriggerAction"))
        {
            if (other.tag == "TriggerDialogue")
            {
                OnPlayerEnteredDialogueTrigger?.Invoke();
            }
            if (other.tag == "TriggerPuerta")
            {
                DoorTrigger door = other.GetComponent<DoorTrigger>();
                if (door != null)
                    doorInRange = door;
            }

        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Objetos"))
        {
            if (other.tag == "madera")
            {
                objectInRange = other.gameObject;
                print("objeto" + other.gameObject.name + "en rango");
            }
        }


    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == objectInRange)
            objectInRange = null;

        if (other.gameObject == doorInRange)
            doorInRange = null;

    }

    private void Interact()
    {
        if (objectInRange != null)
        {
            objetoMano.GuardarObjeto(objectInRange);
            objectInRange.SetActive(false);
            objectInRange = null;
            Debug.Log("Objeto recogido: " + objetoMano.objeto.name);
        }
        else if (doorInRange != null)
        {
            Debug.Log("Intento abrir puerta con: " + objetoMano.objeto.name);
            doorInRange.TryOpen(objetoMano);
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
            DialogueManager.Instance.StartDialogue();
        }
        else
        {
            Debug.Log("Muy lejos de Aiden");
        }

        ChangeState(PlayerState.IDLE);
    }

}