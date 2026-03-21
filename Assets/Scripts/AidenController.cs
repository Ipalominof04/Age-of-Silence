using UnityEngine;
using UnityEngine.AI;

public class AidenController : MonoBehaviour
{
    public enum AidenState { NONE, IDLE, WALK, RUN, CROUCH, SCARED };

    [Header("Maquina de estados")]
    [SerializeField] private AidenState currentState;
    [SerializeField] private Transform player;
    private NavMeshAgent agent;

    [Header("Movimiento")]
    [SerializeField] private float Velocity;
    [SerializeField] private LayerMask playerLayer;

    [Header("UI")]
    [SerializeField] private GameObject canvasWorld;

    [Header("Crouch")]
    [SerializeField] private CapsuleCollider standCollider;
    [SerializeField] private CapsuleCollider crouchCollider;

    public GameEvent OnDialogueStarted;
    public GameEvent OnDialogueEnded;
    public GameEvent OnAidenStopped;
    private bool InDialogue;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        InitState(AidenState.WALK);
        SetUI(false);

    }

    void Update()
    {
        UpdateState();
    }

    private void OnEnable()
    {
        PlayerController.OnPlayerStateChanged += HandlePlayerStateChanged;
        PlayerController.OnPlayerEnteredDialogueTrigger += HandleDialogueTrigger;
        OnDialogueStarted.Subscribe(HandleDialogueStarted);
        OnDialogueEnded.Subscribe(HandleDialogueEnded);
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerStateChanged -= HandlePlayerStateChanged;
        PlayerController.OnPlayerEnteredDialogueTrigger -= HandleDialogueTrigger;
        OnDialogueStarted.Unsubscribe(HandleDialogueStarted);
        OnDialogueEnded.Unsubscribe(HandleDialogueEnded);
    }
    private void HandlePlayerStateChanged(PlayerController.PlayerState playerState)
    {
        switch (playerState)
        {
            case PlayerController.PlayerState.WALK: ChangeState(AidenState.WALK); break;
            case PlayerController.PlayerState.RUN: ChangeState(AidenState.RUN); break;
            case PlayerController.PlayerState.CROUCH: ChangeState(AidenState.CROUCH); break;
            case PlayerController.PlayerState.IDLE: ChangeState(AidenState.IDLE); break;
        }
    }

    private void HandleDialogueTrigger()
    {
        agent.isStopped = true;
        ChangeState(AidenState.IDLE);
        SetUI(true);
        OnAidenStopped.Raise();
    }

    private void HandleDialogueStarted()
    {
        InDialogue = true;
    }

    private void HandleDialogueEnded()
    {
        if (agent == null) return;
        InDialogue = false;
        agent.isStopped = false;
        SetUI(false);
        ChangeState(AidenState.WALK);
    }

    private void SetUI(bool active)
    {
        if (canvasWorld != null)
            canvasWorld.SetActive(active);
    }

    #region State Machine
    private void InitState(AidenState state)
    {
        currentState = state;
        currentState = state;
        switch (currentState)
        {
            case AidenState.IDLE:
                break;
            case AidenState.WALK:
                break;
            case AidenState.RUN:
                break;
            case AidenState.CROUCH:
                standCollider.enabled = true;
                crouchCollider.enabled = false;
                break;
            case AidenState.SCARED:
                break;
        }
    }

    private void UpdateState()
    {
        switch (currentState)
        {
            case AidenState.IDLE:
                break;
            case AidenState.WALK:
                FollowPlayer();
                break;
            case AidenState.RUN:
                FollowPlayer();
                break;
            case AidenState.CROUCH:
                Crouch();
                FollowPlayer();
                break;
            case AidenState.SCARED:
                break;
        }
    }

    private void ChangeState(AidenState newState)
    {
        if (newState == currentState) return;
        ExitState();
        InitState(newState);
    }

    private void ExitState()
    {
        switch (currentState)
        {
            case AidenState.IDLE:
                break;
            case AidenState.WALK:
                break;
            case AidenState.RUN:
                break;
            case AidenState.CROUCH:
                standCollider.enabled = true;
                crouchCollider.enabled = false;
                break;
            case AidenState.SCARED:
                break;
        }
    }
    #endregion

    private void Crouch()
    {
        Velocity = 1.5f;

        standCollider.enabled = false;
        crouchCollider.enabled = true;
    }

    private void FollowPlayer()
    {
        if (InDialogue) return;

        agent.stoppingDistance = 1f;
        agent.acceleration = 8f;
        agent.autoBraking = false;
        agent.SetDestination(player.transform.position);

        switch (currentState)
        {
            case AidenState.WALK:
                agent.speed = Velocity * 2;
                break;
            case AidenState.RUN:
                agent.speed = Velocity * 3;
                break;
        }
    }
  
}