using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static event Action<PlayerState> OnPlayerStateChanged;
    public static event Action OnPlayerEnteredDialogueTrigger;

    private CharacterController characterController;
    public enum PlayerState { NONE, IDLE, WALK, RUN, CROUCH, INTERACT };

    [Header("Maquina de estados")]
    [SerializeField] private PlayerState currentState;

    [Header("Movimiento")]
    [SerializeField] private InputActionAsset myInputActionAsset;
    private InputActionAsset inputInstance;
    private InputAction walkAction;
    private InputAction runAction;
    private InputAction interactAction;
    Vector2 inputMoviment;
    [SerializeField] private float walkVelocity;
    [SerializeField] private float sprintVelocity;
    private float gravity = -9.81f;
    private Vector3 verticalVelocity = Vector3.zero;


    [SerializeField] private GameObject aiden;
    [SerializeField] private float interactRange;


    public GameEvent OnDialogueStarted;
    public GameEvent OnDialogueEnded;
    public GameEvent OnAidenStopped;
    private bool InDialogue;
    private bool canInteract = false;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        inputInstance = Instantiate(myInputActionAsset);
        inputInstance.FindActionMap("Player").Enable();
        walkAction = inputInstance.FindActionMap("Player").FindAction("Move");
        runAction = inputInstance.FindActionMap("Player").FindAction("Sprint");
        interactAction = inputInstance.FindActionMap("Player").FindAction("Interact");
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
        OnDialogueEnded.Subscribe(() => InDialogue = false);
        OnAidenStopped.Subscribe(() => canInteract = true);
    }

    private void OnDisable()
    {
        OnDialogueStarted.Unsubscribe(() => InDialogue = true);
        OnDialogueEnded.Unsubscribe(() => InDialogue = false);
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
                if (interactAction.triggered && canInteract) ChangeState(PlayerState.INTERACT);
                break;
            case PlayerState.WALK:
                Move();
                if (inputMoviment.magnitude < 0.01f) ChangeState(PlayerState.IDLE);
                if (runAction.IsPressed() && characterController.isGrounded) ChangeState(PlayerState.RUN);
                break;
            case PlayerState.RUN:
                Sprint();
                if (!runAction.IsPressed()) ChangeState(PlayerState.WALK);
                if (inputMoviment.magnitude < 0.01f) ChangeState(PlayerState.IDLE);
                break;
            case PlayerState.INTERACT:
                Interact();
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
            case PlayerState.IDLE: break;
            case PlayerState.WALK: break;
            case PlayerState.RUN: break;
            case PlayerState.INTERACT: break;

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

    private void Move()
    {
        Vector3 movement = new Vector3(inputMoviment.x, 0f, inputMoviment.y).normalized;
        if (movement.magnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(movement);
        characterController.Move(movement * walkVelocity * Time.deltaTime);
    }

    private void Sprint()
    {
        Vector3 movement = new Vector3(inputMoviment.x, 0f, inputMoviment.y).normalized;
        if (movement.magnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(movement);
        characterController.Move(movement * sprintVelocity * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("TriggerDialogue"))
        {
            OnPlayerEnteredDialogueTrigger?.Invoke();
        }

    }

    private void Interact()
    {
        if (aiden == null)
        {
            Debug.LogError("Aiden no está asignado en el Inspector");
            ChangeState(PlayerState.IDLE);
            return;
        }

        float distance = Vector3.Distance(transform.position, aiden.transform.position);
        print("AAAAAAAA" + distance);
        if (distance <= interactRange)
        {
            print("empieza dialogo");
            DialogueManager.Instance.StartDialogue();
        }
        else
        {
            Debug.Log("Muy lejos de Aiden");
        }

            ChangeState(PlayerState.IDLE);
    }

}