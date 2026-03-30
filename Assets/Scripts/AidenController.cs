using System;
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

    [Header("Eventos")]
    [SerializeField] GameEvent OnDialogueStarted;
    [SerializeField] GameEvent OnDialogueEnded;
    [SerializeField] GameEvent OnAidenStopped;
    [SerializeField] GameEventString OnAidenScared;
    private bool InDialogue;

    private int scaredNum;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        InitState(AidenState.IDLE);
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
        OnAidenScared.Subscribe(Scared);
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerStateChanged -= HandlePlayerStateChanged;
        PlayerController.OnPlayerEnteredDialogueTrigger -= HandleDialogueTrigger;
        OnDialogueStarted.Unsubscribe(HandleDialogueStarted);
        OnDialogueEnded.Unsubscribe(HandleDialogueEnded);
        OnAidenScared.Unsubscribe(Scared);
    }
    private void HandlePlayerStateChanged(PlayerController.PlayerState playerState)
    {
        //si el niño esta asustado, que ignore lo que hace el player y no le siga hasta que se indique dependiendo el caso 
        if (currentState == AidenState.SCARED) return;

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
                if (scaredNum == 0)
                {
                    print("Aiden ha GRITADO");
                    ChangeState(AidenState.WALK);
                }
                if (scaredNum == 1)
                {
                    print("Aiden ha SALIDO CORRIENDO");
                    Vector3 randomDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f)).normalized;
                    Vector3 targetPos = transform.position + randomDirection * 7;

                    if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                        agent.speed = 2.5f;
                        agent.stoppingDistance = 0f;
                        agent.SetDestination(hit.position);
                }
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
    private void Crouch()
    {
        Velocity = 1.5f;

        standCollider.enabled = false;
        crouchCollider.enabled = true;
    }

    private void Scared(ScareType type)
    {
        int confianza = GameManager.Instance.Confianza;

        if (type == ScareType.Body)
        {
            if (confianza <= 20)
            {
                //se va a asustar SI o SI porque tiene poca confianza
                scaredNum = UnityEngine.Random.Range(0, 2);
            }
            else if (confianza <= 60)
            {
                scaredNum = UnityEngine.Random.Range(0, 10);
            }
            else
            {
                scaredNum = UnityEngine.Random.Range(0, 20);
            }

            if (scaredNum == 0 || scaredNum == 1)
                ChangeState(AidenState.SCARED);
            else
                print("Aiden no se ha asustado por probabilidad");
        }

        if (type == ScareType.Shoot)
        {
            print("me ha asustao un TIRO");
        }
    }

}