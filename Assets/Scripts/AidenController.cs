using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using static EnemyController;

public class AidenController : MonoBehaviour
{
    public static AidenController Instance;
    public enum AidenState { NONE, IDLE, WALK, RUN, CROUCH, SCARED, HELP, CATCH_UP }

    [Header("Máquina de estados")]
    [SerializeField] private AidenState currentState;
    [SerializeField] private Transform player;

    [Header("UI")]
    [SerializeField] private GameObject canvasWorld;
    [SerializeField] private TextMeshProUGUI interactText;
    private bool isFirstDialogue = true;

    [Header("Crouch")]
    [SerializeField] private CapsuleCollider aidenCollider;

    [Header("Eventos")]
    [SerializeField] private GameEvent OnDialogueStarted;
    [SerializeField] private GameEvent OnDialogueEnded;
    [SerializeField] private GameEvent OnAidenStopped;
    [SerializeField] private GameEventScareType OnAidenScared;
    [SerializeField] private GameEvent OnAidenReanudar;

    private NavMeshAgent agent;
    private Animator animator;
    private SoundEmitter emitter;

    private bool inDialogue;
    private bool inDialogueTrigger;
    private bool esperarEnDestino;
    private int scaredNum;
    private Coroutine scaredCoroutine;
    private Vector3 destino;
    private GameEvent onAyudaCompletada;

    private bool playerDead = false;

    #region Unity Callbacks

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        //que no se activen en las cinematicas pq los players que ahi hay son falsos
        else if (SceneManager.GetActiveScene().name == "Escena1" || SceneManager.GetActiveScene().name == "EscenaFinal")
        {
            enabled = false;
            return;
        }
        else
        {
            Destroy(this.gameObject);
        }

        agent = GetComponent<NavMeshAgent>();
        emitter = GetComponent<SoundEmitter>();
        animator = GetComponent<Animator>();
        SetColliderNormal();

        //como se cargan desde el menu y cuando le das a play apareces en el menu para poder iniciar en cualquier escena para testear etc,
        //ponerlos en false en el menu pq ahi no hacen falta
        this.gameObject.SetActive(false);

    }

    void Start()
    {
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
        PlayerController.OnPlayerDied += OnPlayerDied;
        OnDialogueStarted?.Subscribe(HandleDialogueStarted);
        OnDialogueEnded?.Subscribe(HandleDialogueEnded);
        OnAidenScared?.Subscribe(Scared);
        OnAidenReanudar?.Subscribe(HandleReanudar);
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerStateChanged -= HandlePlayerStateChanged;
        PlayerController.OnPlayerEnteredDialogueTrigger -= HandleDialogueTrigger;
        PlayerController.OnPlayerDied -= OnPlayerDied;
        OnDialogueStarted?.Unsubscribe(HandleDialogueStarted);
        OnDialogueEnded?.Unsubscribe(HandleDialogueEnded);
        OnAidenScared?.Unsubscribe(Scared);
        OnAidenReanudar?.Unsubscribe(HandleReanudar);
    }
    #endregion
    #region Event Handlers
    private void HandleReanudar()
    {
        esperarEnDestino = false;
        if (currentState == AidenState.IDLE)
            ChangeState(AidenState.RUN);
    }

    private void HandlePlayerStateChanged(PlayerController.PlayerState playerState)
    {
        if (!gameObject.activeInHierarchy) return;
        // para que ignore los cambios del player hasta que el niño llegue al player
        if (currentState == AidenState.CATCH_UP) return;
        //si el niño esta asustado, que ignore lo que hace el player y no le siga hasta que se indique dependiendo el caso 
        if (currentState == AidenState.SCARED) return;
        // para que ignore los cambios del player y se mueva hasta el waypont
        if (currentState == AidenState.HELP) return;
        if (esperarEnDestino && currentState == AidenState.IDLE) return;

        // para que ignore los cambios del player mientras esta esperando a un dialogo o en el propio dialogo
        if (inDialogue) return;
        if (inDialogueTrigger) return;

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
        inDialogueTrigger = true;
        agent.isStopped = true;
        ChangeState(AidenState.IDLE);
        SetUI(true);
        OnAidenStopped.Raise();
    }

    private void HandleDialogueStarted()
    {
        inDialogue = true;
        inDialogueTrigger = false;
        agent.isStopped = true;
        ChangeState(AidenState.IDLE);
    }

    private void HandleDialogueEnded()
    {
        if (agent == null) return;
        inDialogue = false;
        agent.isStopped = false;
        SetUI(false);
        ChangeState(AidenState.IDLE);

        if (isFirstDialogue)
        {
            isFirstDialogue = false;
            interactText.gameObject.SetActive(false);
        }
    }
    #endregion
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
                agent.speed = 1.5f;
                aidenCollider.center = new Vector3(0, 0.3f, 0);
                aidenCollider.radius = 0.2f;
                aidenCollider.height = 0.6f;
                agent.height = 0.45f;
                agent.radius = 0.2f;
                break;
            case AidenState.SCARED:
                InitScared();
                break;
            case AidenState.HELP:
                animator.SetFloat("Speed", 1f);
                agent.stoppingDistance = 0f;
                agent.SetDestination(destino);
                break;
            case AidenState.CATCH_UP:
                animator.SetFloat("Speed", 1f);
                agent.speed = 3f;
                break;
        }
    }

    private void UpdateState()
    {
        if (playerDead) return;
        switch (currentState)
        {
            case AidenState.IDLE:
                if (!inDialogueTrigger && !esperarEnDestino && Vector3.Distance(transform.position, player.position) > 4f)
                    ChangeState(AidenState.CATCH_UP);
                break;
            case AidenState.WALK:
                if (!inDialogueTrigger && Vector3.Distance(transform.position, player.position) > 4f)
                    ChangeState(AidenState.CATCH_UP);
                FollowPlayer();
                break;
            case AidenState.RUN:
                if (!inDialogueTrigger && Vector3.Distance(transform.position, player.position) > 4f)
                    ChangeState(AidenState.CATCH_UP);
                FollowPlayer();
                break;
            case AidenState.CROUCH:
                if (!inDialogueTrigger && Vector3.Distance(transform.position, player.position) > 4f)
                    ChangeState(AidenState.CATCH_UP);
                //si el agente se esta moviendo, animación de crouch walking (0.35), si no, crouch idle (0.25)
                animator.SetFloat("Speed", agent.velocity.magnitude > 0.1f ? 0.35f : 0.25f);
                FollowPlayer();
                break;
            case AidenState.SCARED:
                UpdateScared();
                break;
            case AidenState.HELP:
                if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
                {
                    onAyudaCompletada?.Raise();
                    ChangeState(esperarEnDestino ? AidenState.IDLE : AidenState.RUN);
                }
                break;
            case AidenState.CATCH_UP:
                FollowPlayer();
                if (!inDialogueTrigger && Vector3.Distance(transform.position, player.position) <= agent.stoppingDistance)
                    ChangeState(AidenState.IDLE);
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
            case AidenState.HELP:
                agent.stoppingDistance = 1f;
                break;
            case AidenState.CATCH_UP:
                break;
        }
    }
    #endregion
    #region Scared
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
        else if (type == ScareType.Shoot)
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

    private void InitScared()
    {
        if (scaredNum == 0)
        {
            animator.SetTrigger("scaredScream");
            emitter.EmitSound(ESound.A_SCARE);
            SetUI(true);
            StartCoroutine(ScreamRoutine());
        }
        else if (scaredNum == 1)
        {
            animator.SetTrigger("scaredRunning");
            emitter.EmitSound(ESound.A_RUN);
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
            scaredCoroutine = StartCoroutine(scaredRunRoutine());
        }
    }

    private void UpdateScared()
    {
        float distancia = Vector3.Distance(transform.position, player.position);

        //cuando llegue al destination que salte la otra animacion
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            animator.SetFloat("Speed", 0.75f);
            emitter.StopSound();
        }

        //uso el getkeydown para poder leer el input de forma facil, tambien puede hacerse por evento (por si queremos cambiarlo)
        if (distancia <= 2f && Input.GetKeyDown(KeyCode.E))
        {
            //si la corrutina esta activa, parala y ponla a null
            if (scaredCoroutine != null) StopCoroutine(scaredCoroutine);
            scaredCoroutine = null;
            SetUI(false);
            GameManager.Instance.SumValor(10);
            ChangeState(AidenState.CATCH_UP);
        }
    }

    private IEnumerator scaredRunRoutine()
    {
        yield return new WaitForSeconds(20f);
        scaredCoroutine = null;
        SetUI(false);
        GameManager.Instance.RestValor(10);
        ChangeState(AidenState.CATCH_UP);
    }

    private IEnumerator ScreamRoutine()
    {
        agent.isStopped = true;

        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName("scaredScream"));

        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);

        agent.isStopped = false;
        SetUI(false);
        ChangeState(AidenState.CATCH_UP);
    }
    #endregion 
    private void FollowPlayer()
    {
        if (inDialogue) return;
        if (playerDead) return;

        agent.stoppingDistance = 1f;
        agent.autoBraking = false;
        agent.SetDestination(player.transform.position);

        // Rotar hacia el player cuando está cerca y el agente está parado
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            RotarHaciaPlayer();
        }
    }

    private void RotarHaciaPlayer()
    {
        Vector3 direccion = player.position - transform.position;
        direccion.y = 0f;
        if (direccion.sqrMagnitude > 0.01f)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotacionObjetivo, 180f * Time.deltaTime);
        }
    }

    public void Help(Vector3 nuevoDest, GameEvent onLlego, bool esperar = false)
    {
        destino = nuevoDest;
        onAyudaCompletada = onLlego;
        esperarEnDestino = esperar;
        ChangeState(AidenState.HELP);
    }

    private void SetUI(bool active)
    {
        if (canvasWorld != null)
            canvasWorld.SetActive(active);
    }

    private void SetColliderNormal()
    {
        aidenCollider.center = new Vector3(0, 0.5f, 0);
        aidenCollider.radius = 0.2f;
        aidenCollider.height = 1f;
        agent.height = 0.8f;
        agent.radius = 0.2f;
        agent.baseOffset = -0.07f;
    }

    public void RespawnReset()
    {
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        if (scaredCoroutine != null)
        {
            StopCoroutine(scaredCoroutine);
            scaredCoroutine = null;
        }

        emitter.StopSound();

        inDialogue = false;
        inDialogueTrigger = false;
        esperarEnDestino = false;
        playerDead = false;
        scaredNum = -1;

        if (agent != null)
        {
            agent.isStopped = false;
            agent.speed = 3f;
            agent.stoppingDistance = 1f;
            agent.ResetPath();
        }

        SetColliderNormal();
        SetUI(false);

        currentState = AidenState.NONE;
        ChangeState(AidenState.IDLE);
    }
    public void ResetUI()
    {
        isFirstDialogue = true;
        interactText.gameObject.SetActive(true);
    }
    private void OnPlayerDied()
    {
        playerDead = true;
        emitter.StopSound();

        if (scaredCoroutine != null)
        {
            StopCoroutine(scaredCoroutine);
            scaredCoroutine = null;
        }

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        ChangeState(AidenState.IDLE);
    }
}