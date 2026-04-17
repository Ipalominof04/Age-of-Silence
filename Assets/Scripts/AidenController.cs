using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AidenController : MonoBehaviour
{
    public enum AidenState { NONE, IDLE, WALK, RUN, CROUCH, SCARED, HELP };

    [Header("Maquina de estados")]
    [SerializeField] private AidenState currentState;
    [SerializeField] private Transform player;
    private NavMeshAgent agent;

    [Header("UI")]
    [SerializeField] private GameObject canvasWorld;

    [Header("Crouch")]
    [SerializeField] private CapsuleCollider aidenCollider;

    [Header("Eventos")]
    [SerializeField] GameEvent OnDialogueStarted;
    [SerializeField] GameEvent OnDialogueEnded;
    [SerializeField] GameEvent OnAidenStopped;
    [SerializeField] GameEventString OnAidenScared;
    [SerializeField] GameEvent OnAidenReanudar;
    private bool InDialogue;

    private int scaredNum;
    private Coroutine scaredCoroutine;

    private Vector3 _destino;
    private GameEvent _onAyudaCompletada;
    private bool esperarEnDestino;

    private SoundEmitter emitter;

    private Animator animator;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        emitter = GetComponent<SoundEmitter>();
        animator = GetComponent<Animator>();
        SetColliderNormal();
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
        OnAidenReanudar.Subscribe(HandleReanudar);
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerStateChanged -= HandlePlayerStateChanged;
        PlayerController.OnPlayerEnteredDialogueTrigger -= HandleDialogueTrigger;
        OnDialogueStarted.Unsubscribe(HandleDialogueStarted);
        OnDialogueEnded.Unsubscribe(HandleDialogueEnded);
        OnAidenScared.Unsubscribe(Scared);
        OnAidenReanudar.Unsubscribe(HandleReanudar);
    }

    private void HandleReanudar()
    {
        esperarEnDestino = false;
        if (currentState == AidenState.IDLE)
            ChangeState(AidenState.RUN);
    }
    private void HandlePlayerStateChanged(PlayerController.PlayerState playerState)
    {
        //si el niño esta asustado, que ignore lo que hace el player y no le siga hasta que se indique dependiendo el caso 
        if (currentState == AidenState.SCARED) return;
        if (currentState == AidenState.HELP) return; // para que ignore los cambios del player y se mueva hasta el waypont
        if (esperarEnDestino && currentState == AidenState.IDLE) return;

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
                animator.SetFloat("Speed", 0f);
                break;
            case AidenState.WALK:
                animator.SetFloat("Speed", 0.5f);
                agent.speed = 1.75f;
                break;
            case AidenState.RUN:
                animator.SetFloat("Speed", 1f);
                agent.speed = 3f;
                break;
            case AidenState.CROUCH:
                animator.SetFloat("Speed", 0.25f);
                agent.speed = 1f;
                aidenCollider.center = new Vector3(0, 0.3f, 0);
                aidenCollider.radius = 0.2f;
                aidenCollider.height = 0.6f;
                agent.height = 0.45f;
                agent.radius = 0.2f;
                break;
            case AidenState.SCARED:
                if (scaredNum == 0)
                {
                    animator.SetTrigger("scaredScream");
                    print("Aiden ha GRITADO");
                  //  emitter.EmitSound(ESound.A_SCARE);
                    SetUI(true);
                    StartCoroutine(ScreamRoutine());
                    //ChangeState(AidenState.WALK);         
                }
               else if (scaredNum == 1)
                {
                    animator.SetTrigger("scaredRunning");
                    print("Aiden ha SALIDO CORRIENDO");
                  //  emitter.EmitSound(ESound.A_RUN);
                    SetUI(true);
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
            case AidenState.HELP:
                agent.SetDestination(_destino);
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
                    SetUI(false);
                    GameManager.Instance.SumValor(10);
                    ChangeState(AidenState.WALK);
                }
                break;
            case AidenState.HELP:
                if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
                {
                    _onAyudaCompletada?.Raise();
                    ChangeState(esperarEnDestino ? AidenState.IDLE : AidenState.WALK);
                    //ChangeState(AidenState.WALK);
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
                SetColliderNormal();
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

        // Rotar hacia el player cuando está cerca y el agente está parado
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            Vector3 direccion = player.position - transform.position;
            direccion.y = 0f;
            if (direccion.sqrMagnitude > 0.01f)
            {
                Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rotacionObjetivo, 180f * Time.deltaTime);
            }
        }
    }

    private void SetColliderNormal()
    {
        aidenCollider.center = new Vector3(0, 0.5f, 0);
        aidenCollider.radius = 0.2f;
        aidenCollider.height = 1f;
        agent.height = 0.8f;
        agent.radius = 0.2f;
        agent.baseOffset = -0.14f;
    }

    private void Scared(ScareType type)
    {
        //si se ha asustado y a huido, que no se vuelva a asustar mientras esta esperando a que el player vaya a consolarlo
        if (scaredCoroutine != null) return;

        float confianza = GameManager.Instance.confianza;

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
        SetUI(false);
        GameManager.Instance.RestValor(10);
        ChangeState(AidenState.WALK);
    }

    private IEnumerator ScreamRoutine()
    {
        agent.isStopped = true;

        yield return null;
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName("scaredScream"));

        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);

        Debug.Log("ScreamRoutine terminada correctamente");
        agent.isStopped = false;
        SetUI(false);
        ChangeState(AidenState.WALK); // reset para que HandlePlayerStateChanged no ignore el evento
        //HandlePlayerStateChanged(PlayerController.CurrentState);
    }
    public void PedirAyuda(Vector3 destino, GameEvent onLlego, bool esperarEnDestino2 = false)
    {
        _destino = destino;
        _onAyudaCompletada = onLlego;
        esperarEnDestino = esperarEnDestino2;
        ChangeState(AidenState.HELP);
    }

}