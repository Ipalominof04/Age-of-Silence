using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AidenController : MonoBehaviour
{
    public enum AidenState { NONE, IDLE, WALK, RUN, CROUCH, SCARED };

    [Header("Maquina de estados")]
    [SerializeField] private AidenState currentState;
    [SerializeField] private Transform player;
    private NavMeshAgent agent;

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
    private Coroutine scaredCoroutine;


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
        switch (currentState)
        {
            case AidenState.IDLE:
                break;
            case AidenState.WALK:
                agent.speed = 2f;
                break;
            case AidenState.RUN:
                agent.speed = 3f;
                break;
            case AidenState.CROUCH:
                agent.speed = 1f;
                standCollider.enabled = false;
                crouchCollider.enabled = true;
                break;
            case AidenState.SCARED:
                if (scaredNum == 0)
                {
                    print("Aiden ha GRITADO");
                    ChangeState(AidenState.WALK);
                }
               else if (scaredNum == 1)
                {
                    print("Aiden ha SALIDO CORRIENDO");
                    Vector3 randomDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f)).normalized;
                    Vector3 targetPos = transform.position + randomDirection * 7;

                    //primero limitar que solamente pueda salir corriendo a zonas dentro del navmesh
                    if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                    {
                        agent.speed = 2.5f;
                        agent.stoppingDistance = 0f;
                        agent.SetDestination(hit.position);
                    }

                    //si ya hay una corrutina que no se empiece otra, para q no haya bugs
                    if (scaredCoroutine != null) StopCoroutine(scaredCoroutine);
                    scaredCoroutine = StartCoroutine(ScaredRoutine());
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
                FollowPlayer();
                break;
            case AidenState.SCARED:
                float distancia = Vector3.Distance(transform.position, player.position);
                //uso el getkeydown para poder leer el input de forma sencilla, tambien puede hacerse por evento (por si queremos cambiarlo)
                if (distancia <= 2f && Input.GetKeyDown(KeyCode.E))
                {
                    //si la corrutina esta activa, parala y ponla a null
                    if (scaredCoroutine != null) StopCoroutine(scaredCoroutine);
                    scaredCoroutine = null;
                    GameManager.Instance.SumValor(10);
                    ChangeState(AidenState.WALK);
                }
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
        agent.autoBraking = false;
        agent.SetDestination(player.transform.position);
    }

    private void Scared(ScareType type)
    {
        //si se ha asustado y a huido, que no se vuelva a asustar mientras esta esperando a que el player vaya a consolarlo
        if (scaredCoroutine != null) return;

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
            if (confianza <= 20)
            {
                scaredNum = UnityEngine.Random.Range(0, 10);
            }
            else if (confianza <= 60)
            {
                scaredNum = UnityEngine.Random.Range(0, 20);
            }
            else
            {
                scaredNum = UnityEngine.Random.Range(0, 40);
            }

            if (scaredNum == 0 || scaredNum == 1)
                ChangeState(AidenState.SCARED);
            else
                print("Aiden no se ha asustado por probabilidad");
        }
    }

    private IEnumerator ScaredRoutine()
    {
        yield return new WaitForSeconds(20f);
        scaredCoroutine = null;
        GameManager.Instance.RestValor(10);
        ChangeState(AidenState.WALK);
    }

}