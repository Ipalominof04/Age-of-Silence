using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    public enum PlayerState { NONE, IDLE, WALK, RUN };

    [Header("Maquina de estados")]
    [SerializeField] private PlayerState currentState;

    [Header("Movimiento")]
    [SerializeField] private InputActionAsset myInputActionAsset;
    private InputActionAsset inputInstance;
    private InputAction walkAction;
    private InputAction runAction;
    Vector2 inputMoviment;
    [SerializeField] private float walkVelocity;
    [SerializeField] private float sprintVelocity;
    [SerializeField] private float gravity = -9.81f;
    private Vector3 verticalVelocity = Vector3.zero;
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        inputInstance = Instantiate(myInputActionAsset);

        inputInstance.FindActionMap("Player").Enable();
        walkAction = inputInstance.FindActionMap("Player").FindAction("Move");
        runAction = inputInstance.FindActionMap("Player").FindAction("Sprint");
    }
    void Start()
    {
        InitState(PlayerState.IDLE);
    }

    void Update()
    {
        //movimiento pj
        inputMoviment = walkAction.ReadValue<Vector2>();
        //statemachine
        UpdateState();
        ApplyGravity();


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
        }
    }

    private void UpdateState()
    {
        switch (currentState)
        {
            case PlayerState.IDLE:
                if (inputMoviment.magnitude > 0.01f) ChangeState(PlayerState.WALK);
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
        }
    }

    private void ChangeState(PlayerState newState)
    {
        if (newState == currentState)
            return;
        ExitState();
        InitState(newState);
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
        }
    }

    #endregion

    private void ApplyGravity()
    {
        if (characterController.isGrounded)
        {
            verticalVelocity.y = -2f; // pequeño valor para mantenerlo pegado al suelo
        }
        else
        {
            verticalVelocity.y += gravity * Time.deltaTime;
        }

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

}

