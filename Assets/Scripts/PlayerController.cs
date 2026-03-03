using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    public enum PlayerState { NONE, IDLE, WALK };

    [Header("Maquina de estados")]
    [SerializeField] private PlayerState currentState;

    [Header("Movimiento")]
    [SerializeField] private InputActionAsset myInputActionAsset;
    private InputActionAsset inputInstance;
    private InputAction moveAction;
    Vector2 inputMoviment;
    [SerializeField] private float vel;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        inputInstance = Instantiate(myInputActionAsset);

        inputInstance.FindActionMap("Player").Enable();
        moveAction = inputInstance.FindActionMap("Player").FindAction("Move");
    }
    void Start()
    {
        InitState(PlayerState.IDLE);
    }

    void Update()
    {
        //movimiento pj
        inputMoviment = moveAction.ReadValue<Vector2>();
        //statemachine
        UpdateState();
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
        }
    }

    private void UpdateState()
    {
        switch (currentState)
        {
            case PlayerState.IDLE:
                if (inputMoviment.magnitude > 0.01f)
                    ChangeState(PlayerState.WALK);
                break;
            case PlayerState.WALK:
                Move();
                if (inputMoviment.magnitude < 0.01f)
                    ChangeState(PlayerState.IDLE);
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
        }
    }

    #endregion

    private void Move()
    {
        Vector3 movement = new Vector3(inputMoviment.x, 0f, inputMoviment.y).normalized;

        if (movement.magnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(movement);

        characterController.Move(movement * vel * Time.deltaTime);
    }

}

