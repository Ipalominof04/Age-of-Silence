using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour, IDamageable
{
    public static PlayerController Instance;
    private CharacterController characterController;
    private SoundEmitter soundEmitter;

    public enum PlayerState { NONE, IDLE, WALK, RUN, CROUCH, SHOOT, INTERACT, DIALOGUE, THROW }

    [Header("Maquina de estados")]
    [SerializeField] public PlayerState currentState;

    [Header("Movimiento")]
    [SerializeField] private InputActionAsset myInputActionAsset;
    private InputActionAsset inputInstance;
    private InputAction walkAction;
    private InputAction runAction;
    private InputAction interactAction;
    private InputAction crouchAction;
    private InputAction shootAction;
    private Vector2 inputMoviment;
    private float gravity = -9.81f;
    private Vector3 verticalVelocity = Vector3.zero;
    [SerializeField] private float slopeCheckDistance = 0.3f;

    [Header("Vida")]
    [SerializeField] private float vidaMaxima;
    public float vidaActual;
    [SerializeField] private GameObject luzBarraVida;

    [Header("Aiden")]
    [SerializeField] private GameObject aiden;
    [SerializeField] private float interactRange;

    [Header("Crouch")]
    [SerializeField] private CapsuleCollider playerCollider;
    [SerializeField] private Transform eyePoint;
    [SerializeField] private float eyeHeightStanding = 1.6f;
    [SerializeField] private float eyeHeightCrouch = 0.8f;

    [Header("Objetos")]
    [SerializeField] private ObjetoAgarrado objetoMano;

    [Header("Eventos")]
    [SerializeField] private GameEvent OnDialogueStarted;
    [SerializeField] private GameEvent OnDialogueEnded;
    [SerializeField] private GameEvent OnInfoStarted;
    [SerializeField] private GameEvent OnInfoEnded;
    [SerializeField] private GameEvent OnAidenStopped;
    [SerializeField] private GameEvent onShoot;
    [SerializeField] private GameEvent onArmaSobrecalentada;
    [SerializeField] private GameEvent onArmaEnfriada;
    [SerializeField] private GameEventFloat onVidaCambiada;
    public static event Action<PlayerState> OnPlayerStateChanged;
    public static event Action OnPlayerEnteredDialogueTrigger;
    public static event Action OnPlayerDied;

    [Header("Lanzamiento")]
    [SerializeField] private ParabolaRenderer parabolaRenderer;
    [SerializeField] private Transform huesoMano;
    [SerializeField] private Camera camara;
    private ObjetoLanzable objetoLanzableEnMano;
    private InputAction throwAction;

    [Header("Disparo")]
    [SerializeField] private GameObject bala;
    [SerializeField] private GameObject shootingPoint;
    private bool armaSobrecalentada;

    [Header("Ragdoll")]
    [SerializeField] private Transform ragdollRoot;
    private Rigidbody[] ragdollRigidbodies;

    [Header("Sound Waves")]
    [SerializeField] private MeshRenderer wavesRenderer;

    private bool InDialogue;
    private bool canInteract = false;
    private GameObject objectInRange = null;
    private GameObject armaEnMano = null;
    private bool armaEquipada = false;
    private GameObject objectLanzableInRange = null;
    private TriggerDialogue dialogueActivo = null;
    private bool escaleraInRange = false;
    private TriggerAyuda ayudaInRange = null;
    private Vector3 direccionEscalera;
    private bool isDying = false;
    private bool isCrouchWalking = false;
    private bool isAiming = false;
    private IPuzle puzleInRange = null;
    private Animator animator;
    private PlayerState previousState;

    private float nextStepTime = 0f;
    [SerializeField] private float walkStepRate = 0.45f;
    [SerializeField] private float runStepRate = 0.28f;

    public Transform GetEyePoint() => eyePoint;
    public void ForceIdle() => ChangeState(PlayerState.IDLE);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (SceneManager.GetActiveScene().name == "Escena1" || SceneManager.GetActiveScene().name == "EscenaFinal")
        {
            return;
        }
        else
        {
            Destroy(this.gameObject);
        }


        characterController = GetComponent<CharacterController>();
        camara = Camera.main;
        soundEmitter = GetComponent<SoundEmitter>();
        animator = GetComponent<Animator>();

        var playerMap = InputManager.Instance.PlayerMap;
        walkAction = playerMap.FindAction("Move");
        runAction = playerMap.FindAction("Sprint");
        interactAction = playerMap.FindAction("Interact");
        crouchAction = playerMap.FindAction("Crouch");
        throwAction = playerMap.FindAction("Throw");
        shootAction = playerMap.FindAction("Attack");


        SetColliderNormal();
        InitRagdoll();
    }

    void Start()
    {
        isDying = false;
        shootingPoint = GameObject.Find("shooting_point");
        camara = Camera.main;
        if (shootingPoint == null)
            Debug.Log("No hay arma en la escena");

        InitState(PlayerState.IDLE);

        float vidaGuardada = GameManager.Instance.datos.PlayerHealth;
        vidaActual = vidaGuardada > 0f ? vidaGuardada : vidaMaxima;
        onVidaCambiada.Raise(vidaActual / vidaMaxima);
        float ratio = vidaActual / vidaMaxima;
        DamageVignette.Instance?.UpdateHealthRatio(ratio);

        StartCoroutine(WaitingChargeMap());
    }

    void Update()
    {
        if (isDying) return;
        inputMoviment = walkAction.ReadValue<Vector2>();
        UpdateState();
        ApplyGravity();
    }

    private void OnEnable()
    {
        OnDialogueStarted.Subscribe(() => InDialogue = true);
        OnDialogueEnded.Subscribe(() => { InDialogue = false; canInteract = false; });
        OnInfoStarted.Subscribe(() => InDialogue = true);
        OnInfoEnded.Subscribe(() => InDialogue = false);
        OnAidenStopped.Subscribe(() => canInteract = true);
        onArmaSobrecalentada.Subscribe(() => armaSobrecalentada = true);
        onArmaEnfriada.Subscribe(() => armaSobrecalentada = false);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        OnDialogueStarted.Unsubscribe(() => InDialogue = true);
        OnDialogueEnded.Unsubscribe(() => { InDialogue = false; canInteract = false; dialogueActivo = null; });
        OnInfoStarted.Unsubscribe(() => InDialogue = true);
        OnInfoEnded.Unsubscribe(() => InDialogue = false);
        OnAidenStopped.Unsubscribe(() => canInteract = true);
        onArmaSobrecalentada.Unsubscribe(() => armaSobrecalentada = true);
        onArmaEnfriada.Unsubscribe(() => armaSobrecalentada = false);
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateBrilloBarraVida(scene, mode);
        shootingPoint = GameObject.Find("shooting_point");
        camara = Camera.main;
        if (camara == null)
        {
            // Fallback: busca cualquier cámara activa en la escena
            camara = FindFirstObjectByType<Camera>();
        }
        
        //StartCoroutine(ReasignarCamara());
    }

    private IEnumerator ReasignarCamara()
    {
        while (Camera.main == null)
            yield return null;

        camara = Camera.main;

        Debug.Log("Cámara asignada: " + camara);
    }

    private IEnumerator WaitingChargeMap()
    {
        characterController.enabled = false;
        verticalVelocity = Vector3.zero;

        // espera hasta que haya NavMesh bajo el player
        while (!UnityEngine.AI.NavMesh.SamplePosition(transform.position, out _, 2f, UnityEngine.AI.NavMesh.AllAreas))
        {
            yield return null;
        }

        characterController.enabled = true;

        // ahora espera hasta tocar el suelo físicamente
        while (!IsGroundedOrOnSlope())
        {
            verticalVelocity.y = 0f;
            characterController.Move(Vector3.down * 0.05f);
            yield return null;
        }

        verticalVelocity.y = -2f;
    }
    #region State Machine
    private void InitState(PlayerState state)
    {
        currentState = state;
        switch (currentState)
        {
            case PlayerState.IDLE:
                if (previousState != PlayerState.SHOOT)
                    SetWavesSprite(-1);
                if (!escaleraInRange)
                {
                    if (armaEquipada)
                    {
                        if (armaEnMano == null)
                        {
                            armaEquipada = false;
                            animator.Play("idle");
                        }
                        else
                        {
                            animator.Play("idleArma");
                            armaEnMano.transform.localPosition = new Vector3(0.0153f, 0.0766f, 0.0142f);
                            armaEnMano.transform.localRotation = Quaternion.Euler(33.74f, 137.574f, -44.139f);
                        }
                    }
                    else
                        animator.Play("idle");
                }
                break;
            case PlayerState.WALK:
                SetWavesSprite(0);
                if (!escaleraInRange)
                    animator.Play("walk");
                break;
            case PlayerState.RUN:
                SetWavesSprite(1);
                animator.Play("run");
                break;
            case PlayerState.CROUCH:
                SetWavesSprite(-1);
                if (!escaleraInRange)
                {
                    if (armaEquipada)
                        animator.Play("crouch");
                    else
                    {
                        animator.SetBool("isCrouching", true);
                        characterController.center = new Vector3(0, 0.45f, 0);
                        characterController.radius = 0.4f;
                        characterController.height = 0.7f;
                        playerCollider.center = new Vector3(0, 0.55f, 0);
                        playerCollider.radius = 0.6f;
                        playerCollider.height = 0.7f;
                        eyePoint.localPosition = new Vector3(0, eyeHeightCrouch, 0);
                    }

                    if (inputMoviment.magnitude > 0.01f)
                    {
                        isCrouchWalking = true;
                        animator.Play("crouchWalking");
                    }
                    else
                    {
                        isCrouchWalking = false;
                    }
                }
                break;
            case PlayerState.SHOOT:
                // SetWavesSprite(2);
                break;
            case PlayerState.INTERACT:
                break;
            case PlayerState.THROW:
                if (objetoLanzableEnMano == null)
                {
                    if (objectLanzableInRange != null)
                    {
                        objetoLanzableEnMano = objectLanzableInRange.GetComponent<ObjetoLanzable>();
                        objetoLanzableEnMano.hand = huesoMano.gameObject;
                        objetoLanzableEnMano?.Recoger();
                        objectLanzableInRange = null;
                    }
                    else
                    {
                        ChangeState(previousState);
                    }
                }
                break;
        }
    }

    private void UpdateState()
    {
        if (InDialogue) return;

        switch (currentState)
        {
            case PlayerState.IDLE:
                if (inputMoviment.magnitude > 0.01f) ChangeState(PlayerState.WALK);
                if (crouchAction.IsPressed()) ChangeState(PlayerState.CROUCH);
                if (armaEquipada && shootAction.triggered)
                {
                    if (!armaSobrecalentada)
                    {
                        SetWavesSprite(2);
                        ChangeState(PlayerState.SHOOT);
                    }
                }

                if (interactAction.triggered && (objectInRange != null || puzleInRange != null))
                    ChangeState(PlayerState.INTERACT);
                if (interactAction.triggered && armaEnMano != null && !armaEquipada)
                    ChangeState(PlayerState.INTERACT);
                if (interactAction.triggered && canInteract && objectInRange == null)
                    ChangeState(PlayerState.DIALOGUE);
                if (interactAction.triggered && objectLanzableInRange != null)
                    ChangeState(PlayerState.THROW);
                if (objetoLanzableEnMano != null && throwAction.IsPressed())
                    ChangeState(PlayerState.THROW);
                if (interactAction.triggered && ayudaInRange != null)
                    ayudaInRange.PedirAyuda();
                break;

            case PlayerState.WALK:
                if (escaleraInRange) ClimbLadder();
                else Walk();
                if (inputMoviment.magnitude < 0.01f) ChangeState(PlayerState.IDLE);
                if (runAction.IsPressed() && IsGroundedOrOnSlope()) ChangeState(PlayerState.RUN);
                if (crouchAction.IsPressed()) ChangeState(PlayerState.CROUCH);
                if (objetoLanzableEnMano != null && throwAction.IsPressed())
                    ChangeState(PlayerState.THROW);
                break;

            case PlayerState.RUN:
                if (escaleraInRange) ClimbLadder();
                else Run();
                if (!runAction.IsPressed()) ChangeState(PlayerState.WALK);
                if (inputMoviment.magnitude < 0.01f) ChangeState(PlayerState.IDLE);
                break;

            case PlayerState.CROUCH:
                if (escaleraInRange)
                {
                    ClimbLadder();
                }
                else
                {
                    Crouch();
                    if (inputMoviment.magnitude > 0.01f && !isCrouchWalking)
                    {
                        isCrouchWalking = true;
                        animator.Play("crouchWalking");
                    }
                    else if (inputMoviment.magnitude < 0.01f && isCrouchWalking)
                    {
                        isCrouchWalking = false;
                        animator.Play("crouch");
                    }
                }
                if (!crouchAction.IsPressed()) ChangeState(PlayerState.IDLE);
                if (interactAction.triggered && (objectInRange != null || puzleInRange != null) && crouchAction.IsPressed())
                    ChangeState(PlayerState.INTERACT);
                if (interactAction.triggered && armaEnMano != null && crouchAction.IsPressed() && !armaEquipada)
                    ChangeState(PlayerState.INTERACT);
                if (interactAction.triggered && canInteract && objectInRange == null)
                    ChangeState(PlayerState.DIALOGUE);
                if (interactAction.triggered && objectLanzableInRange != null)
                    ChangeState(PlayerState.THROW);
                if (objetoLanzableEnMano != null && throwAction.IsPressed())
                    ChangeState(PlayerState.THROW);
                break;

            case PlayerState.SHOOT:
                Shoot();
                if (!shootAction.IsPressed()) ChangeState(PlayerState.IDLE);
                break;

            case PlayerState.INTERACT:
                Interact();
                ChangeState(PlayerState.IDLE);
                break;

            case PlayerState.DIALOGUE:
                Dialogue();
                break;

            case PlayerState.THROW:
                ThrowObject();
                break;
        }
    }

    private void ChangeState(PlayerState newState)
    {
        if (newState == currentState) return;
        ExitState();
        previousState = currentState;
        InitState(newState);
        OnPlayerStateChanged?.Invoke(currentState);
    }

    private void ExitState()
    {
        switch (currentState)
        {
            case PlayerState.IDLE:
                soundEmitter.StopSound();
                break;
            case PlayerState.WALK:
                soundEmitter.StopSound();
                break;
            case PlayerState.RUN:
                soundEmitter.StopSound();
                break;
            case PlayerState.CROUCH:
                animator.SetBool("isCrouching", false);
                soundEmitter.StopSound();
                SetColliderNormal();
                break;
            case PlayerState.SHOOT:
                soundEmitter.StopSound();
                break;
            case PlayerState.INTERACT:
                soundEmitter.StopSound();
                break;
            case PlayerState.THROW:
                parabolaRenderer.OcultarParabola();
                soundEmitter.StopSound();
                break;
        }
    }
    #endregion

    private bool IsGroundedOrOnSlope()
    {
        if (characterController.isGrounded) return true;
        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down,
                               slopeCheckDistance, ~LayerMask.GetMask("Player"));
    }

    private void ActivateRagdoll()
    {
        characterController.enabled = false;
        if (animator != null) animator.enabled = false;
        foreach (var rb in ragdollRigidbodies)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void InitRagdoll()
    {
        ragdollRigidbodies = ragdollRoot.GetComponentsInChildren<Rigidbody>();
        foreach (var rb in ragdollRigidbodies)
            rb.isKinematic = true;
    }
    private void ThrowObject()
    {
        if (camara == null)
        {
            camara = Camera.main;

            if (camara == null)
            {
                Debug.LogError("No hay cámara disponible");
                return;
            }

            if (objetoLanzableEnMano == null)
            {
                ChangeState(previousState);
                return;
            }
        }

        if (throwAction.IsPressed())
        {
            if (previousState == PlayerState.CROUCH && !isAiming)
            {
                isAiming = true;
                animator.SetBool("isAiming", true);
            }
            else if (previousState != PlayerState.CROUCH)
                animator.Play("aimThrow");

            Vector3 puntoObjetivo = parabolaRenderer.ObtenerPuntoObjetivo(huesoMano.position, camara);
            Vector3 direccion = puntoObjetivo - transform.position;
            direccion.y = 0;
            if (direccion.magnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(direccion);

            parabolaRenderer.MostrarParabola(huesoMano.position, puntoObjetivo);
        }
        else if (throwAction.WasReleasedThisFrame())
        {
            isAiming = false;
            animator.SetBool("isAiming", false);
            Vector3 puntoObjetivo = parabolaRenderer.ObtenerPuntoObjetivo(huesoMano.position, camara);
            Vector3 velocidad = parabolaRenderer.CalcularVelocidadInicial(huesoMano.position, puntoObjetivo);
            objetoLanzableEnMano.Lanzar(velocidad, playerCollider);
            objetoLanzableEnMano = null;
            ChangeState(previousState);
        }
        else
        {
            isAiming = false;
            animator.SetBool("isAiming", false);
            parabolaRenderer.OcultarParabola();
            ChangeState(previousState);
        }
    }

    private void ApplyGravity()
    {
        if (isDying) return;
        if (!characterController.enabled) return;

        if (escaleraInRange)
        {
            verticalVelocity.y = 0;
            return;
        }
        if (IsGroundedOrOnSlope())
            verticalVelocity.y = -2f;
        else
            verticalVelocity.y += gravity * Time.deltaTime;
        characterController.Move(verticalVelocity * Time.deltaTime);
    }

    private void Move(float velocity)
    {
        Vector3 movement = new Vector3(inputMoviment.x, 0f, inputMoviment.y).normalized;
        if (movement.magnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(movement);
        characterController.Move(movement * velocity * Time.deltaTime);
    }

    private void Walk()
    {
        Move(1.75f);

        if (Time.time >= nextStepTime)
        {
            soundEmitter.EmitSound(ESound.WALK);
            nextStepTime = Time.time + walkStepRate;
        }

    }
    private void Run()
    {
        Move(3f);

        if (Time.time >= nextStepTime)
        {
            soundEmitter.EmitSound(ESound.RUN);
            nextStepTime = Time.time + runStepRate;
        }
    }
    private void Crouch()
    {
        Move(1.5f);

    }
    private void ClimbLadder()
    {
        transform.rotation = Quaternion.LookRotation(direccionEscalera);
        Vector3 verticalMovement = new Vector3(0, inputMoviment.y, 0);
        Vector3 horizontalMovement = new Vector3(inputMoviment.x, 0, 0);
        characterController.Move((verticalMovement + horizontalMovement) * 1f * Time.deltaTime);
    }

    private void Interact()
    {
        if (objectInRange != null)
        {
            if (objectInRange.CompareTag("Pieza"))
            {
                if (vidaActual >= vidaMaxima)
                {
                    Debug.Log("Vida al máximo, no necesitas la pieza");
                    return;
                }
                else
                {
                    Curar(2);


                    objectInRange.SetActive(false);
                    objectInRange = null;
                    soundEmitter.EmitSound(ESound.PICKUP);
                    return;
                }
            }
            //Sino tienes nada en la mano comprueba el tag del objetoInRange y hace lo que debe ese objeto 
            if (string.IsNullOrEmpty(objetoMano.nombreObjeto))
            {
                if (objectInRange.CompareTag("Comida"))
                {
                    GameManager.Instance.SumValor(3);
                    objectInRange.SetActive(false);
                    objectInRange = null;
                    soundEmitter.EmitSound(ESound.PICKUP);
                    return;
                }
                else
                {
                    objetoMano.SaveObject(objectInRange);
                    GameManager.Instance.datos.objetoMano = objetoMano;
                    objectInRange.SetActive(false);
                    objectInRange = null;
                    Debug.Log("Objeto recogido: " + objetoMano.nombreObjeto);
                    soundEmitter.EmitSound(ESound.PICKUP);
                }
            }
            else
            {
                Debug.Log("Mano ocupada");
            }
        }
        else if (puzleInRange != null)
        {
            puzleInRange.Interact(objetoMano);
        }
        else if (armaEnMano != null)
        {
            GameObject arma = armaEnMano;
            arma.transform.SetParent(huesoMano.transform);
            arma.transform.localPosition = new Vector3(-0.046f, 0.052f, -0.006f);
            arma.transform.localRotation = Quaternion.Euler(33.283f, 142.394f, 38.761f);
            arma.transform.localScale = Vector3.one;
            MeshRenderer meshArma = arma.GetComponent<MeshRenderer>();
            ArmaController arma_ctrl = arma.GetComponent<ArmaController>();

            Material[] materials = meshArma.materials;
            Material[] newMaterials = new Material[materials.Length - 1];
            for (int i = 0, j = 0; i < materials.Length; i++)
            {
                if (i == 1) continue;
                newMaterials[j++] = materials[i];
            }
            meshArma.materials = newMaterials;
            armaEquipada = true;
            arma_ctrl?.Equipar();
            shootingPoint = arma.transform.Find("shooting_point")?.gameObject;
            if (shootingPoint == null)
                shootingPoint = GameObject.Find("shooting_point");
            soundEmitter.EmitSound(ESound.PICKUP);
        }
    }

    public void Curar(float cura)
    {
        vidaActual += cura;
        vidaActual = Mathf.Clamp(vidaActual, 0f, vidaMaxima);
        GameManager.Instance.datos.PlayerHealth = vidaActual;
        float ratio = vidaActual / vidaMaxima;
        onVidaCambiada.Raise(ratio);
        DamageVignette.Instance?.UpdateHealthRatio(ratio);
    }

    private void Dialogue()
    {
        if (aiden == null)
        {
            Debug.LogError("Aiden no está asignado en el Inspector");
            ChangeState(PlayerState.IDLE);
            return;
        }

        float distance = Vector3.Distance(transform.position, aiden.transform.position);
        if (distance <= interactRange)
        {
            Debug.Log("Empieza diálogo");
            DialogueManager.Instance.StartDialogue(dialogueActivo.graphDialogue);
        }
        else
        {
            Debug.Log("Muy lejos de Aiden");
        }


        ChangeState(PlayerState.IDLE);
    }

    private void Shoot()
    {
        if (shootingPoint == null) return;
        if (!armaEquipada || armaEnMano == null)
        {
            armaEquipada = false;
            ChangeState(PlayerState.IDLE);
            return;
        }

        if (!armaSobrecalentada)
        {

            GameObject go = Instantiate(bala, shootingPoint.transform.position, shootingPoint.transform.rotation);
            BalaController balaDefault = go.GetComponent<BalaController>();
            if (balaDefault != null)
                balaDefault.SetDamage(1);

            Rigidbody rb = go.GetComponent<Rigidbody>();
            if (rb != null)
                rb.linearVelocity = this.gameObject.transform.forward * 20f;

            onShoot.Raise();
            ChangeState(PlayerState.IDLE);
            soundEmitter.EmitSound(ESound.SHOOT);

            //particula flash arma
            ArmaController arma = armaEnMano.GetComponent<ArmaController>();
            if (arma != null && arma.shootFlash != null)
            {
                arma.shootFlash.Play();

            }
            else
            {
                ChangeState(PlayerState.IDLE);
            }
        }
    }


    public void TakeDamage(float damage)
    {
        if (isDying) return;
        if (GameManager.Instance.IsChangingScene) return;
        vidaActual -= damage;
        vidaActual = Mathf.Clamp(vidaActual, 0f, vidaMaxima);
        GameManager.Instance.datos.PlayerHealth = vidaActual;

        float ratio = vidaActual / vidaMaxima;
        onVidaCambiada.Raise(ratio);

        // Actualitza l'alpha base + dispara el flash
        DamageVignette.Instance?.UpdateHealthRatio(ratio);
        DamageVignette.Instance?.PlayDamageFlash();

        if (vidaActual <= 0f) Die();
    }

    public void Die()
    {
        print("SA MUERTO");
        if (isDying) return;
        isDying = true;
        soundEmitter.StopSound();
        OnPlayerDied?.Invoke();

        ClearObjetoLanzable();

        if (armaEnMano != null)
        {
            ArmaController arma_ctrl = armaEnMano.GetComponent<ArmaController>();
            arma_ctrl?.transform.SetParent(null);
            arma_ctrl?.Soltar();
            armaEnMano = null;
            armaEquipada = false;
        }

        ActivateRagdoll();
        GameManager.Instance.datos.PlayerHealth = GameManager.Instance.GetHealthAlEntrarEscena();
        GameManager.Instance.datos.confianza = GameManager.Instance.GetConfianzaAlEntrarEscena();
        GameManager.Instance.confianza = GameManager.Instance.GetConfianzaAlEntrarEscena();
        GameManager.Instance.LimpiarSOEscenas();
        GameManager.Instance.datos.puzzlesResueltos.Clear();
        // esto es para borrar la referencia
        GameManager.Instance.datos.objetoMano.ClearObject();
        InputManager.Instance.PlayerMap.Disable();
        StartCoroutine(RespawnDelay());
    }

    private IEnumerator RespawnDelay()
    {
        yield return new WaitForSeconds(2f); // ajusta el tiempo a tu gusto
        DamageVignette.Instance?.SetVignetteActive(false);
        GameManager.Instance.IrAScene(SceneManager.GetActiveScene().name);
    }

    private void UpdateBrilloBarraVida(Scene scene, LoadSceneMode mode)
    {
        if (luzBarraVida == null) return;

        Light luz = luzBarraVida.GetComponent<Light>();
        if (luz == null) return;

        switch (scene.name)
        {
            case "Escena1":
                luz.intensity = 1f;
                luz.range = 1f;
                break;
            case "SalidaFabrica":
                luz.intensity = 2f;
                luz.range = 1f;
                break;
            case "SalidaSigilo":
                luz.intensity = 35f;
                luz.range = 1f;
                break;
            case "PuenteLevadizo":
                luz.intensity = 40f;
                luz.range = 1f;
                break;
            case "SalidaMuro":
                luz.intensity = 60f;
                luz.range = 1f;
                break;
            case "ExteriorTienda":
                luz.intensity = 100f;
                luz.range = 1f;
                break;
            case "InteriorTienda":
                luz.intensity = 35f;
                luz.range = 1f;
                break;
            case "CabañaNPC":
                luz.intensity = 400f;
                luz.range = 1f;
                break;
            case "InteriorCabaña":
                luz.intensity = 0.3f;
                luz.range = 1f;
                break;
            case "PuenteBosque":
                luz.intensity = 800f;
                luz.range = 1f;
                break;

        }
    }
    private void SetWavesSprite(int index)
    {
        if (wavesRenderer == null) return;
        if (index < 0)
        {
            wavesRenderer.gameObject.SetActive(false);
            return;
        }
        wavesRenderer.gameObject.SetActive(true);
        wavesRenderer.GetComponent<SoundWavesController>()?.SetWaveState(index);
    }

    private void SetColliderNormal()
    {
        characterController.center = new Vector3(0, 0.8f, 0);
        characterController.radius = 0.2f;
        characterController.height = 1.5f;
        playerCollider.center = new Vector3(0, 0.9f, 0);
        playerCollider.radius = 0.3f;
        playerCollider.height = 1.6f;
        eyePoint.localPosition = new Vector3(0, eyeHeightStanding, 0);
    }

    public void RespawnReset()
    {
        InputManager.Instance.PlayerMap.Enable();

        isDying = false;
        escaleraInRange = false;
        objectInRange = null;
        objectLanzableInRange = null;
        puzleInRange = null;
        ayudaInRange = null;
        dialogueActivo = null;
        canInteract = false;
        InDialogue = false;
        armaSobrecalentada = false;

        ClearArma();

        // 1. Primero reactivar ragdoll/animator
        foreach (var rb in ragdollRigidbodies)
            rb.isKinematic = true;

        if (animator != null)
        {
            animator.enabled = true;
            animator.Rebind();
            animator.Update(0f);
            animator.Play("idle");
        }

        // 2. Ahora soltar el arma (hueso ya está en posición correcta)
        if (armaEnMano != null)
        {
            ArmaController arma_ctrl = armaEnMano.GetComponent<ArmaController>();
            arma_ctrl?.transform.SetParent(null);
            arma_ctrl?.Soltar();
            armaEnMano = null;
        }
        armaEquipada = false;

        verticalVelocity = Vector3.zero;

        vidaActual = GameManager.Instance.datos.PlayerHealth;
        onVidaCambiada.Raise(vidaActual / vidaMaxima);


        DamageVignette.Instance?.ResetHealthRatio(vidaActual / vidaMaxima);

        ChangeState(PlayerState.IDLE);

        //    characterController.enabled = true;
        StartCoroutine(WaitingChargeMap());
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("TriggerAction"))
        {
            if (other.tag == "TriggerDialogue")
            {
                OnPlayerEnteredDialogueTrigger?.Invoke();
                if (other.TryGetComponent<TriggerDialogue>(out TriggerDialogue dialogue))
                    dialogueActivo = dialogue;
            }

            if (other.tag == "TriggerPuzle")
            {
                IPuzle puzle = other.GetComponent<IPuzle>();
                if (puzle != null)
                {
                    puzleInRange = puzle;
                    puzle.OnEnterRange();
                    //    ChangeState(PlayerState.IDLE);
                }
            }

            if (other.tag == "TriggerInfo")
            {
                TriggerInfo info = other.GetComponent<TriggerInfo>();
                if (info != null)
                {
                    DialogueManager.Instance.StartInfo(info.graphInfo);
                    ChangeState(PlayerState.IDLE);
                }
            }

            if (other.tag == "TriggerEscalera")
            {
                escaleraInRange = true;
                direccionEscalera = -other.transform.forward;
                direccionEscalera.y = 0;
                direccionEscalera.Normalize();
                transform.rotation = Quaternion.LookRotation(direccionEscalera);
                animator.Play("idle");
            }

            if (other.tag == "TriggerAyuda")
            {
                TriggerAyuda ayuda = other.GetComponent<TriggerAyuda>();
                if (ayuda != null)
                    ayudaInRange = ayuda;
            }
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Objetos"))
        {
            if (other.tag == "Rama" || other.tag == "Palanca" || other.tag == "Cizallas" ||
                other.tag == "Fusible" || other.tag == "Galon" || other.tag == "Cerillas")
            {
                if (objectInRange == null)
                {
                    objectInRange = other.gameObject;
                    print("objeto " + other.gameObject.name + " en rango");
                }
            }

            if (other.tag == "Botella" || other.tag == "Roca")
            {
                if (objectLanzableInRange == null)
                {
                    objectLanzableInRange = other.gameObject;
                    print("objeto " + other.gameObject.name + " en rango");
                }
            }

            if (other.tag == "Arma")
            {
                if (armaEnMano == null)
                {
                    armaEnMano = other.gameObject;
                    print("objeto " + other.gameObject.name + " en rango");
                }
            }

            if (other.tag == "Comida" || other.tag == "Agua")
            {
                if (objectInRange == null)
                {
                    objectInRange = other.gameObject;
                    print("objeto " + other.gameObject.name + " en rango");
                }
            }

            if (other.tag == "Pieza")
            {
                if (objectInRange == null)
                {
                    objectInRange = other.gameObject;
                    print("objeto " + other.gameObject.name + " en rango");
                }
            }

            if (other.tag == "ComidaNPC")
            {
                if (objectInRange == null)
                {
                    objectInRange = other.gameObject;
                    print("objeto " + other.gameObject.name + " en rango");
                }
            }

            if (other.tag == "MaderasPuente")
            {
                if (objectInRange == null)
                {
                    objectInRange = other.gameObject;
                    print("objeto " + other.gameObject.name + " en rango");
                }
            }
            if (other.tag == "PaletPuente")
            {
                if (objectInRange == null)
                {
                    objectInRange = other.gameObject;
                    print("objeto " + other.gameObject.name + " en rango");
                }
            }
            if (other.tag == "MaderaPequeñaPuente")
            {
                if (objectInRange == null)
                {
                    objectInRange = other.gameObject;
                    print("objeto " + other.gameObject.name + " en rango");
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == objectInRange)
            objectInRange = null;
        objectInRange = null;

        if (puzleInRange != null && other.GetComponent<IPuzle>() == puzleInRange)
            puzleInRange = null;

        if (other.gameObject == objectLanzableInRange?.gameObject)
            objectLanzableInRange = null;

        if (other.tag == "TriggerEscalera")
        {
            escaleraInRange = false;
            if (currentState == PlayerState.WALK)
                animator.Play("walk");
            else if (currentState == PlayerState.RUN)
                animator.Play("run");
            else
                animator.Play("idle");
        }

        if (other.tag == "TriggerAyuda")
            ayudaInRange = null;
    }


    public void ClearTriggerPuzle() { puzleInRange = null; }
    public void ClearTriggerAyuda() { ayudaInRange = null; }
    public void ClearArma()
    {
        if (armaEnMano != null)
        {
            ArmaController arma_ctrl = armaEnMano.GetComponent<ArmaController>();
            arma_ctrl?.transform.SetParent(null);
            arma_ctrl?.Soltar();
            armaEnMano = null;
        }
        armaEquipada = false;
    }
    public void ClearObjetoLanzable()
    {
        if (objetoLanzableEnMano == null) return;

        objetoLanzableEnMano.Soltar();
        objetoLanzableEnMano = null;
    }
}