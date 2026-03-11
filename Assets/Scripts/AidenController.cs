using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class AidenController : MonoBehaviour
{
    public enum AidenState { NONE, IDLE, WALK, RUN, CROUCH, SCARED };

    [Header("Maquina de estados")]
    [SerializeField] private AidenState currentState;
    [SerializeField] private Transform player;
    private NavMeshAgent agent;

    [Header("Movimiento")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;

    private bool isInDialogue = false;
    [SerializeField] private LayerMask playerLayer;


    [Header("UI")]
    [SerializeField] private GameObject canvasWorld;


    private void OnEnable()
    {
        PlayerController.OnPlayerStateChanged += HandlePlayerStateChanged;
        PlayerController.OnPlayerEnteredDialogueTrigger += HandleDialogueTrigger;
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerStateChanged -= HandlePlayerStateChanged;
        PlayerController.OnPlayerEnteredDialogueTrigger -= HandleDialogueTrigger;
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        InitState(AidenState.WALK);
        if (canvasWorld != null) canvasWorld.SetActive(false);

    }

    void Update()
    {
        UpdateState();
    }


    private void HandleDialogueTrigger()
    {
        isInDialogue = true;
        agent.isStopped = true;
        ChangeState(AidenState.IDLE);
        MostrarUI();

    }

    public void EndDialogue()
    {
        isInDialogue = false;
        agent.isStopped = false;
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

    #region State Machine
    private void InitState(AidenState state)
    {
        currentState = state;
    }

    private void UpdateState()
    {
        switch (currentState)
        {
            case AidenState.IDLE: break;
            case AidenState.WALK: FollowPlayer(); break;
            case AidenState.RUN: FollowPlayer(); break;
            case AidenState.CROUCH: break;
            case AidenState.SCARED: break;
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
            case AidenState.IDLE: break;
            case AidenState.WALK: break;
            case AidenState.RUN: break;
            case AidenState.CROUCH: break;
            case AidenState.SCARED: break;
        }
    }
    #endregion

    private void FollowPlayer()
    {
        if (isInDialogue) return;

        agent.stoppingDistance = 1f;
        agent.acceleration = 8f;
        agent.autoBraking = false;
        agent.SetDestination(player.transform.position);

        switch (currentState)
        {
            case AidenState.WALK: agent.speed = walkSpeed; break;
            case AidenState.RUN: agent.speed = runSpeed; break;
        }
    }

    private void CheckInteractInput()
    {
    }

    public void MostrarUI()
    {
        if (canvasWorld != null)
            canvasWorld.SetActive(true);
    }

    public void OcultarUI()
    {
        if (canvasWorld != null)
            canvasWorld.SetActive(false);
    }



}