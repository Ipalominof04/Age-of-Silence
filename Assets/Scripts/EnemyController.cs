using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour, ISoundListener
{
    public enum EnemyState { NONE, IDLE, PATROL, CHASE, ATTACK, SEARCHING, LOOK };

    [Header("Maquina de estados")]
    [SerializeField] private EnemyState currentState;
    private NavMeshAgent agent;



    private void OnEnable()
    {
        SoundManager.Instance.Subscribe(this);

    }
    private void OnDisable()
    {
        SoundManager.Instance.Unsubscribe(this);
    }

    void Start()
    {
        
    }

    private void Awake()
    {
        
    }

    void Update()
    {
        
    }

    public void OnHearSound(Vector3 soundPosition, float volume)
    {
        //  Debug.Log($"{name} escuchó un sonido a {volume * 100}% de volumen desde {soundPosition}");
        agent.SetDestination(soundPosition);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    private void ChangeState(EnemyState newState)
    {
        if (newState == currentState)
            return;
        ExitState();
        InitState(newState);
    }

    #region State Machine
    private void InitState(EnemyState state)
    {
        currentState = state;
        switch (currentState)
        {
            case EnemyState.IDLE: 
                break;
            case EnemyState.PATROL:
                break;
            case EnemyState.CHASE:
                break;
            case EnemyState.ATTACK:
                break;
            case EnemyState.SEARCHING:
                break;
            case EnemyState.LOOK: 
                break;
        }
    }

    private void UpdateState()
    {
        switch (currentState)
        {
            case EnemyState.IDLE: 
                break;
            case EnemyState.PATROL:
                break;
            case EnemyState.CHASE:
                break;
            case EnemyState.ATTACK: 
                break;
            case EnemyState.SEARCHING: 
                break;
            case EnemyState.LOOK:
                break;
        }
    }

    private void ExitState()
    {
        switch (currentState)
        {
            case EnemyState.IDLE:
                break;
            case EnemyState.PATROL: 
                break;
            case EnemyState.CHASE: 
                break;
            case EnemyState.ATTACK:
                break;
            case EnemyState.SEARCHING:
                break;
            case EnemyState.LOOK:
                break;
        }
    }
    #endregion


}
