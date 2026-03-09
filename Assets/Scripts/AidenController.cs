using UnityEngine;
using UnityEngine.AI;
using static PlayerController;

public class AidenController : MonoBehaviour
{
    public enum AidenState { NONE, IDLE, WALK, RUN, CROUCH, SCARED };

    [Header("Maquina de estados")]
    [SerializeField] private AidenState currentState;

    [SerializeField] private PlayerController player;

    private NavMeshAgent agent;

    [SerializeField] private float followDistance;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        InitState(AidenState.WALK);
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateState();
    }

    #region State Machine
    private void InitState(AidenState state)
    {
        currentState = state;
        switch (currentState)
        {
            case AidenState.IDLE:
                break;
            case AidenState.WALK:
                FollowPlayer();
                break;
            case AidenState.RUN:
                break;
            case AidenState.CROUCH:
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
                break;
            case AidenState.RUN:
                break;
            case AidenState.CROUCH:
                break;
            case AidenState.SCARED:
                break;
        }
    }

    private void ChangeState(AidenState newState)
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
            case AidenState.IDLE:
                break;
            case AidenState.WALK:
                break;
            case AidenState.RUN:
                break;
            case AidenState.CROUCH:
                break;
            case AidenState.SCARED:
                break;
        }
    }

    #endregion


    private void FollowPlayer()
    {
        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;

        // Punto detrás del jugador
        Vector3 targetPosition = player.transform.position - directionToPlayer * followDistance;

        agent.SetDestination(targetPosition);
    }
}
