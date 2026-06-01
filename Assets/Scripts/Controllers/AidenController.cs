using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class AidenController : MonoBehaviour
{
    //instancia
    public static AidenController Instance;

    private NavMeshAgent agent;
    private Animator animator;
    private SoundEmitter emitter;

    //posibles estats de l'aiden
    public enum AidenState { NONE, IDLE, WALK, RUN, CROUCH, SCARED, HELP, CATCH_UP }

    [Header("Máquina de estados")]
    [SerializeField] private AidenState currentState;
    [SerializeField] private Transform player;

    [Header("UI")]
    [SerializeField] private GameObject canvasWorld;
    [SerializeField] private TextMeshProUGUI interactText;
    [SerializeField] private TextMeshProUGUI exclamationText;
    [SerializeField] private GameObject canvasImage;
    private bool isFirstDialogue = true;

    [Header("Crouch")]
    [SerializeField] private CapsuleCollider aidenCollider;

    [Header("Eventos")]
    [SerializeField] private GameEvent OnDialogueStarted;
    [SerializeField] private GameEvent OnDialogueEnded;
    [SerializeField] private GameEvent OnAidenStopped;
    [SerializeField] private GameEventScareType OnAidenScared;
    [SerializeField] private GameEvent OnAidenReanudar;

    [Header("Hueso oso")]
    [SerializeField] private GameObject OsoAiden;

    private bool inDialogue;
    private bool inDialogueTrigger;

    private bool esperarEnDestino;
    private Vector3 destino;
    private GameEvent onAyudaCompletada;

    private int scaredNum;
    private Coroutine scaredCoroutine;

    private bool playerDead = false;

    #region Unity Callbacks

    //inialitzacio de la instancia i dels seus components
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        //cas especial en les cinematiques, en aquestes escenes hi ha un Aiden fals que no s'ha de desactivar
        else if (SceneManager.GetActiveScene().name == "Escena1" || SceneManager.GetActiveScene().name == "EscenaFinal")
        {
            enabled = false;
            return;
        }
        else
        {
            Destroy(this.gameObject);
        }

        //asignar components
        agent = GetComponent<NavMeshAgent>();
        emitter = GetComponent<SoundEmitter>();
        animator = GetComponent<Animator>();

        //asignar collider de peu
        SetColliderNormal();

    }
    void Start()
    {
        //aiden s'inicia en idle i amb la ui desactivada
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
        interactText.gameObject.SetActive(isFirstDialogue);
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

    //event per a que aiden reprengui el moviment en el puzle de BRIDGE on aiden pot ajudar
    private void HandleReanudar()
    {
        esperarEnDestino = false;
        if (currentState == AidenState.IDLE)
            ChangeState(AidenState.RUN);
    }

    //event que sincronitza l'estat de aiden amb el del player
    private void HandlePlayerStateChanged(PlayerController.PlayerState playerState)
    {
        if (!gameObject.activeInHierarchy) return;
        
        //per a que ignori els canvis del player fins que el nen arribi al player
        if (currentState == AidenState.CATCH_UP) return;
        //si aiden esta asustat, que ignori el que fa el player
        if (currentState == AidenState.SCARED) return;
        //per a que ignori els canvis del player i es mogui fins al waypoint
        if (currentState == AidenState.HELP) return;
        if (esperarEnDestino && currentState == AidenState.IDLE) return;
        //per a que ignori els canvis del player mentres esta esperant a un dialeg o en el propi dialeg
        if (inDialogue) return;
        if (inDialogueTrigger) return;

        //canvia l'estat de l'aiden per igualar el del jugador gracies a l'event
        switch (playerState)
        {
            case PlayerController.PlayerState.WALK: ChangeState(AidenState.WALK); break;
            case PlayerController.PlayerState.RUN: ChangeState(AidenState.RUN); break;
            case PlayerController.PlayerState.CROUCH: ChangeState(AidenState.CROUCH); break;
            case PlayerController.PlayerState.IDLE: ChangeState(AidenState.IDLE); break;
        }
    }

    //quan el jugador entra a un TriggerDialogue, aiden es para i mostra la ui
    private void HandleDialogueTrigger()
    {
        inDialogueTrigger = true;
        agent.isStopped = true;
        ChangeState(AidenState.IDLE);
        SetUI(true);
        canvasImage.SetActive(true);
        //notifica que l'aiden s'ha parat
        OnAidenStopped.Raise();
    }
    //quan el jugador comença el dialeg
    private void HandleDialogueStarted()
    {
        inDialogue = true;
        inDialogueTrigger = false;
        agent.isStopped = true;
        ChangeState(AidenState.IDLE);
    }
    //quan s'acaba el dialeg aiden repren el moviment
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
                //ajusta el collider al tamany d'aiden ajupit
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
                //stopping distance 0 per a help perque ha d'arriba exactament a sobre del waypoint
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
        //no fa res si el player ha mort
        if (playerDead) return;

        switch (currentState)
        {
            case AidenState.IDLE:
                //si el jugador s'allunya massa, aiden entra en catch up per anar a buscar-lo (aquest if esta possat a cada estat)
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
                //si l'aiden se esta movent, animació de crouch walking (0.35), si no, crouch idle (0.25)
                animator.SetFloat("Speed", agent.velocity.magnitude > 0.1f ? 0.35f : 0.25f);
                FollowPlayer();
                break;
            case AidenState.SCARED:
                UpdateScared();
                break;
            case AidenState.HELP:
                //quan arriba al waypoint on ha de fer la seva accio d'ajuda (simbolica perque realment no fa res, nomes arriba al waypoint)
                //llanca l'event i canvia d'estat
                if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
                {
                    onAyudaCompletada?.Raise();
                    ChangeState(esperarEnDestino ? AidenState.IDLE : AidenState.RUN);
                }
                break;
            case AidenState.CATCH_UP:
                FollowPlayer();
                //aquest cas es diferent als demes de l'aiden, es necessari que aiden es torni a posar en l'estat del player quant arriba al seu costat
                //inclos si el player no canvia d'estat i no en llenca l'event perque si no es queda tota l'estona en catch up fins que el player canvia d'estat
                //llavors aqui es necesari comprovar de forma continua l'estat del player quan aiden esta en catch up, en comptes de només comprobar-ho quan es llanca l'event
                if (!inDialogueTrigger && Vector3.Distance(transform.position, player.position) <= agent.stoppingDistance)
                {
                    switch (PlayerController.Instance.currentState)
                    {
                        case PlayerController.PlayerState.WALK: ChangeState(AidenState.WALK); break;
                        case PlayerController.PlayerState.RUN: ChangeState(AidenState.RUN); break;
                        case PlayerController.PlayerState.CROUCH: ChangeState(AidenState.CROUCH); break;
                        case PlayerController.PlayerState.IDLE: ChangeState(AidenState.IDLE); break;
                    }
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
                //restaura el collider al collider normal de peu
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
    //determina si l'aiden s'asusta, els percentatges de probabilitat que te d'asustar-se
    private void Scared(ScareType type)
    {
        //si s'ha asustat i a fugit, que no torni a asustar-se mentres esta esperant a que el player el consoli
        if (scaredCoroutine != null) return;

        float confianza = GameManager.Instance.confianza;

        if (type == ScareType.Body)
        {
            //com menys confianca, mes probable que aiden s'asusti
            if (confianza <= 30)
            {
                scaredNum = UnityEngine.Random.Range(0, 2);
            }
            else if (confianza > 30 && confianza <= 45)
            {
                scaredNum = UnityEngine.Random.Range(0, 10);
            }
            else if (confianza > 45)
            {
                scaredNum = UnityEngine.Random.Range(0, 20);
            }

            //si en el random surt els numeros 0 o 1, canvia a estat de SCARED
            if (scaredNum == 0 || scaredNum == 1)
                ChangeState(AidenState.SCARED);

        }
        else if (type == ScareType.Shoot)
        {
            //com menys confianca, mes probable que aiden s'asusti
            if (confianza <= 20)
            {
                scaredNum = UnityEngine.Random.Range(0, 10);
            }
            else if (confianza > 20 && confianza <= 45)
            {
                scaredNum = UnityEngine.Random.Range(0, 20);
            }
            else if (confianza > 45)
            {
                scaredNum = UnityEngine.Random.Range(0, 40);
            }

            //si en el random surt els numeros 0 o 1, canvia a estat de SCARED
            if (scaredNum == 0 || scaredNum == 1)
                ChangeState(AidenState.SCARED);
        }
    }

    //inicialitza la reaccio del enseurt: crit (0) o fugida (1)
    private void InitScared()
    {
        if (scaredNum == 0)
        {
            //reaccio de crit: es queda quiet durant el que dura l'animacio (uns 2 o 3 segons aprox) i crida
            animator.SetTrigger("scaredScream");
            emitter.EmitSound(ESound.A_SCARE);
            exclamationText.gameObject.SetActive(true);
            canvasImage.SetActive(false);
            SetUI(true);
            scaredCoroutine = StartCoroutine(ScreamRoutine());
        }
        else if (scaredNum == 1)
        {
            //reaccio de fugida: surt corrents en una direccio aleatoria dins del navmesh, es para i espera 20 segons a que el player el consoli i sumi confianca
            //si el player no interacciona amb l'aiden resta confianca
            animator.SetTrigger("scaredRunning");
            emitter.EmitSound(ESound.A_RUN);

            exclamationText.gameObject.SetActive(true);
            canvasImage.SetActive(false);
            SetUI(true);
            Vector3 randomDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f)).normalized;
            Vector3 targetPos = transform.position + randomDirection * 7;

            //primer limitar que nomes pugui sortir corrent a zones dins del namesh
            if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                agent.speed = 2.5f;
                agent.stoppingDistance = 0f;
                agent.SetDestination(hit.position);
            }

            //si ja hi ha una corrutina que no es comenci una altre per a que no hagui bugs
            scaredCoroutine = StartCoroutine(scaredRunRoutine());
        }
    }

    //update del scared, principalment del cas de fugida
    private void UpdateScared()
    {
        
        float distancia = Vector3.Distance(transform.position, player.position);

        //quan arribi al desti, que salti l'altre animacio
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            animator.SetFloat("Speed", 0.75f);
            emitter.StopSound();

            if (scaredNum == 1)
            {
                exclamationText.gameObject.SetActive(false);
                canvasImage.SetActive(true); 
            }
        }
        //utilitzo el getkeydown per poder lleguir l'input de forma facil, tambe podria fer-se per event
        //comprobo tambe que aiden hagi arribat al desti despres de sortir corrents per no poder interatuar amb ell mentres esta corrent
        //i que nomes puguis fer-ho quan l'aiden estigui esperant al desti
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && distancia <= 2f && Input.GetKeyDown(KeyCode.E))
        {
            if (scaredCoroutine != null) StopCoroutine(scaredCoroutine);
            scaredCoroutine = null;
            SetUI(false);
            //si el calmes, sumes 5 de confianca
            GameManager.Instance.SumValor(5);
            ChangeState(AidenState.CATCH_UP);
        }
    }

    //corutina de fugida: despres de 20 segons (el jugador no l'ha calmat), repren el seguiment del jugador ir esta confianca
    private IEnumerator scaredRunRoutine()
    {
 
        yield return new WaitForSeconds(20f);
        scaredCoroutine = null;
        SetUI(false);
        //baixa la confianca si el player no interactua amb l'aiden
        GameManager.Instance.RestValor(1);
        ChangeState(AidenState.CATCH_UP);
    }

    //corrutina de crit: espera a que s'acabi la animacio per a que aiden reprengui el moviment
    //aqui el jugador no te opcio de pujar o baixar confianca
    private IEnumerator ScreamRoutine()
    {
        agent.isStopped = true;

        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName("scaredScream"));

        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);

        agent.isStopped = false;
        scaredCoroutine = null;
        exclamationText.gameObject.SetActive(false);
        SetUI(false);
        ChangeState(AidenState.CATCH_UP);
    }
    #endregion 

    //fa que l'aiden segueixi al player 
    private void FollowPlayer()
    {
        if (inDialogue) return;
        if (playerDead) return;

        agent.stoppingDistance = 1f;
        agent.autoBraking = false;
        agent.SetDestination(player.transform.position);

        //rotar cap al player quan este a prop i el agent estigui parat
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            RotarHaciaPlayer();
        }
    }
    //rotar cap al player (la funcio es crida a followplayer)
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
    //envia l'Aiden a una posicio concreta (per a missions d'ajuda), amb event de callback en arribar
    public void Help(Vector3 nuevoDest, GameEvent onLlego, bool esperar = false)
    {
        destino = nuevoDest;
        onAyudaCompletada = onLlego;
        esperarEnDestino = esperar;
        ChangeState(AidenState.HELP);
    }
    //funcio per activar i desactivar la ui
    private void SetUI(bool active)
    {
        if (canvasWorld != null)
            canvasWorld.SetActive(active);
    }

    //restaurar el collider amb les mesures de peu
    private void SetColliderNormal()
    {
        aidenCollider.center = new Vector3(0, 0.5f, 0);
        aidenCollider.radius = 0.2f;
        aidenCollider.height = 1f;
        agent.height = 0.8f;
        agent.radius = 0.2f;
        agent.baseOffset = -0.07f;
    }

    //reiniciar i netejar les variables de l'aiden quan es cambia d'escena o es reinicia l'escena (quan el player mor)
    //(funciona igual que el player, la funcio es crida desde el gm)
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
        exclamationText.gameObject.SetActive(false);
        canvasImage.SetActive(true);

        currentState = AidenState.NONE;
        ChangeState(AidenState.IDLE);
    }

    //aixo nomes resetea valors quan es comenca partida nova (es crida desde el mainmenu, al Play)
    public void ResetAidenPlay()
    {
        isFirstDialogue = true;
        interactText.gameObject.SetActive(true);
        SetOso(false);
    }

    //aquesta funcio es crida quan el player mor, per a que aiden es quedi quiet en indle i no es quedi buguejat en l'ultim estat
    //ni els seus sons ni animacions
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

    //posa l'os de peluix a la ma de l'aideb
    public void SetOso(bool activo)
    {
        if(OsoAiden != null)
        {
            OsoAiden.SetActive(activo);
        }
    }
}