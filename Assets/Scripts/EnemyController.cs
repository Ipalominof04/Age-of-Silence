using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class EnemyController : MonoBehaviour, ISoundListener, IDamageable
{
    public enum EnemyState { NONE, IDLE, PATROL, CHASE, ATTACK, SEARCHING, LOOK };
    public enum EnemyType { NORMAL, FUERTE, VELOZ, VISIONS }

    [Header("Maquina de estados")]
    [SerializeField] private EnemyState currentState;
    private NavMeshAgent agent;

    [Header("Config de valores")]
    [SerializeField] float vida;
    [SerializeField] float speedPatrol = 0.7f;
    [SerializeField] float speedChase = 2f;
    [SerializeField] EnemyType enemyType;

    [Header("Disparo")]
    [SerializeField] private GameObject arma;
    [SerializeField] private GameObject bala;
    [SerializeField] private GameObject shootingPoint;
    [SerializeField] public float damageAmount = 1f;
    [SerializeField] public float bulleSpeed = 20f;
    [SerializeField] public float shootCooldown = 1f;
    [SerializeField] private ParticleSystem shootFlash;

    [Header("Eventos")]
    [SerializeField] private GameEventScareType OnAidenScared;
    [SerializeField] private GameEventString onMusicChange;

    [Header("Config Areas y Layers")]
    [SerializeField] private Transform headTransform;
    [SerializeField] private float radiusChase;
    [SerializeField] private float radiusLook;
    [SerializeField] private float radiusAttack;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;

    // FIX: guardamos siempre el transform raíz del jugador, no el del collider hijo
    private Transform playerTransform;

    [Header("Puntos de patrulla")]
    [SerializeField] private Transform[] destination;
    [SerializeField] int currentIndex = 0;

    [Header("UI")]
    [SerializeField] private GameObject canvasWorld;
    [SerializeField] private TextMeshProUGUI stateText;

    [Header("Detecció de so")]
    [SerializeField] private float listenRadius = 15f;
    [SerializeField][Range(0f, 1f)] private float minDetectionLevel = 0.2f;

    [Header("Ragdoll")]
    [SerializeField] Transform root;
    private Rigidbody[] rigidbodies;

    private float gravity = -9.81f;
    private Vector3 verticalVelocity = Vector3.zero;

    public float GetListenRadius() => listenRadius;
    public float GetMinDetectionLevel() => minDetectionLevel;

    private float searchDuration = 5f;
    private bool hasShot = false;
    private bool playerDead = false;
    private Coroutine lookCoroutine;
    private Coroutine searchCoroutine;

    private Animator animator;
    private SoundEmitter soundEmitter;

    private void OnEnable()
    {
        SoundManager.Instance.Subscribe(this);
        PlayerController.OnPlayerDied += OnPlayerDied;
    }

    private void OnDisable()
    {
        SoundManager.Instance.Unsubscribe(this);
        PlayerController.OnPlayerDied -= OnPlayerDied;
    }

    public void OnHearSound(Vector3 soundPosition, float volume)
    {
        if (volume < minDetectionLevel) return;
        if (currentState == EnemyState.CHASE || currentState == EnemyState.ATTACK) return;

        // FIX: comprobamos que el punto de sonido sea alcanzable en el NavMesh antes de ir
        NavMeshHit navHit;
        if (!NavMesh.SamplePosition(soundPosition, out navHit, 3f, NavMesh.AllAreas)) return;
        Vector3 validDestination = navHit.position;
        /*
        Collider[] enemiesNearSound = Physics.OverlapSphere(soundPosition, 1f, LayerMask.GetMask("Enemies"));
        if (enemiesNearSound.Length > 0) return;
        */

        agent.SetDestination(validDestination);

        if (currentState == EnemyState.SEARCHING)
        {
            if (searchCoroutine != null) StopCoroutine(searchCoroutine);
            agent.isStopped = false;
            animator.SetBool("onPatrol", true);
            animator.SetBool("onSearching", false);
            searchCoroutine = StartCoroutine(SearchRoutine());
        }
        else
        {
            ChangeState(EnemyState.SEARCHING);
        }
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    private void SetUI(bool active, string text = "", string hexColor = "#FFFFFF")
    {
        if (canvasWorld != null)
        {
            canvasWorld.SetActive(active);
            if (stateText != null)
            {
                if (stateText.text != text)
                {
                    stateText.GetComponent<Animator>().SetTrigger("Play");
                }
                stateText.text = text;
                ColorUtility.TryParseHtmlString(hexColor, out Color color);
                stateText.color = color;
            }
        }
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        soundEmitter = GetComponent<SoundEmitter>();
    }

    void Start()
    {
        InitState(EnemyState.PATROL);
        ActivateKinematic(true);
        SetUI(false);
    }

    void Update()
    {
        UpdateState();
        ApplyGravity();
    }

    private void ApplyGravity()
    {
        if (agent == null || !agent.enabled) return;

        // FIX: resetear la velocidad vertical cuando el agente está sobre el suelo
        if (agent.isOnNavMesh && agent.isOnOffMeshLink == false)
        {
            verticalVelocity.y = Mathf.Max(verticalVelocity.y + gravity * Time.deltaTime, -2f);
        }
        else
        {
            verticalVelocity.y = 0f;
        }

        agent.Move(verticalVelocity * Time.deltaTime);
    }

    private void ChangeState(EnemyState newState)
    {
        if (newState == currentState)
            return;
        ExitState();
        InitState(newState);
    }

    private void InitState(EnemyState state)
    {
        currentState = state;
        switch (currentState)
        {
            case EnemyState.IDLE:
                agent.isStopped = true;
                SetUI(false);
                break;

            case EnemyState.PATROL:
                agent.isStopped = false;
                animator.SetBool("onPatrol", true);
                if (enemyType == EnemyType.FUERTE)
                {
                    arma.transform.localPosition = new Vector3(0.046f, -0.352f, -0.055f);
                    arma.transform.localRotation = Quaternion.Euler(-107.384f, -72.8330f, 27.522f);
                }
                else
                {
                    arma.transform.localPosition = new Vector3(-0.048f, -0.122f, 0.009f);
                    arma.transform.localRotation = Quaternion.Euler(-73.674f, 115.541f, -128.345f);
                }
                SetUI(false);
                agent.speed = speedPatrol;
                agent.stoppingDistance = 0.2f;
                agent.SetDestination(destination[currentIndex].position);
                onMusicChange.Raise("musicStop");
                break;

            case EnemyState.CHASE:
                agent.isStopped = false;
                animator.SetBool("onChase", true);
                if (enemyType == EnemyType.FUERTE)
                {
                    arma.transform.localPosition = new Vector3(0.033f, -0.348f, -0.074f);
                    arma.transform.localRotation = Quaternion.Euler(-98.997f, -85.295f, 55.136f);
                }
                else
                {
                    arma.transform.localPosition = new Vector3(-0.05697f, -0.131036f, -0.00457f);
                    arma.transform.localRotation = Quaternion.Euler(-100.667f, -73.7940f, 71.358f);
                }
                onMusicChange.Raise("musicChase");
                SetUI(true, "!", "#F11200");
                agent.speed = speedChase;
                agent.stoppingDistance = radiusAttack;
                break;

            case EnemyState.ATTACK:
                animator.SetBool("onShoot", true);
                if (enemyType == EnemyType.FUERTE)
                {
                    arma.transform.localPosition = new Vector3(0.052f, -0.324f, -0.061f);
                    arma.transform.localRotation = Quaternion.Euler(-97.789f, -68.198f, 37.141f);
                }
                else
                {
                    arma.transform.localPosition = new Vector3(-0.03414f, -0.13247f, -0.007156f);
                    arma.transform.localRotation = Quaternion.Euler(-96.586f, -34.216f, 31.978f);
                }
                break;

            case EnemyState.SEARCHING:
                agent.isStopped = false;
                animator.SetBool("onPatrol", true);
                animator.SetBool("onSearching", false);
                SetUI(true, "?", "#FFC300");
                searchCoroutine = StartCoroutine(SearchRoutine());
                break;

            case EnemyState.LOOK:
                animator.SetBool("onLook", true);
                agent.isStopped = true;
                SetUI(true, "?", "#FFC300");
                lookCoroutine = StartCoroutine(LookRoutine());
                break;
        }
    }

    private void UpdateState()
    {
        switch (currentState)
        {
            // FIX: IDLE ahora tiene lógica correcta sin condiciones contradictorias
            case EnemyState.IDLE:
                if (PlayerInArea(radiusChase))
                    ChangeState(EnemyState.CHASE);
                else if (PlayerInArea(radiusLook))
                    ChangeState(EnemyState.LOOK);
                else
                    ChangeState(EnemyState.PATROL);
                break;

            case EnemyState.PATROL:
                Patrol();
                if (PlayerInArea(radiusChase))
                    ChangeState(EnemyState.CHASE);
                else if (PlayerInArea(radiusLook))
                    ChangeState(EnemyState.LOOK);
                break;

            case EnemyState.CHASE:
                ChasePlayer();
                if (PlayerInAttackArea())
                    ChangeState(EnemyState.ATTACK);
                // FIX: usamos una distancia "dura" sin raycast para no perder al jugador
                // por oclusión momentánea; solo pasamos a SEARCHING si está realmente lejos
                else if (!PlayerInRawRadius(radiusAttack))
                    ChangeState(EnemyState.SEARCHING);
                break;

            case EnemyState.ATTACK:
                LookToPlayer();
                Shoot();
                if (!PlayerInAttackArea())
                {
                    if (PlayerInRawRadius(radiusAttack))
                        ChangeState(EnemyState.CHASE);
                    else
                        ChangeState(EnemyState.SEARCHING);
                }
                break;

            case EnemyState.SEARCHING:
                animator.SetBool("onPatrol", !agent.isStopped);
                animator.SetBool("onSearching", agent.isStopped);
                if (PlayerInArea(radiusChase))
                    ChangeState(EnemyState.CHASE);
                else if (PlayerInArea(radiusLook))
                    ChangeState(EnemyState.CHASE);
                break;

            case EnemyState.LOOK:
                // FIX: siempre miramos al jugador si tenemos referencia
                if (playerTransform != null)
                    LookToPlayer();

                if (PlayerInAttackArea())
                    ChangeState(EnemyState.ATTACK);
                else if (PlayerInArea(radiusChase))
                    ChangeState(EnemyState.CHASE);
                break;
        }
    }

    private void ExitState()
    {
        switch (currentState)
        {
            case EnemyState.PATROL:
                animator.SetBool("onPatrol", false);
                soundEmitter.StopSound();
                break;
            case EnemyState.CHASE:
                animator.SetBool("onChase", false);
                soundEmitter.StopSound();
                break;
            case EnemyState.ATTACK:
                animator.SetBool("onShoot", false);
                soundEmitter.StopSound();
                break;
            case EnemyState.SEARCHING:
                animator.SetBool("onSearching", false);
                animator.SetBool("onPatrol", false);
                if (searchCoroutine != null) StopCoroutine(searchCoroutine);
                break;
            case EnemyState.LOOK:
                animator.SetBool("onLook", false);
                if (lookCoroutine != null) StopCoroutine(lookCoroutine);
                break;
        }
    }

    private void ActivateKinematic(bool activate)
    {
        foreach (var rb in rigidbodies)
        {
            rb.isKinematic = activate;
        }
        animator.enabled = activate;
    }

    private IEnumerator SearchRoutine()
    {
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            yield return null;

        agent.isStopped = true;
        animator.SetBool("onPatrol", false);
        animator.SetBool("onSearching", true);
        if (enemyType == EnemyType.FUERTE)
        {
            arma.transform.localPosition = new Vector3(0.083f, -0.315f, -0.06f);
            arma.transform.localRotation = Quaternion.Euler(-93.849f, -44.531f, 10.404f);
        }
        else
        {
            arma.transform.localPosition = new Vector3(-0.022f, -0.132f, 0f);
            arma.transform.localRotation = Quaternion.Euler(-93.801f, -29.005f, 22.375f);
        }

        yield return new WaitForSeconds(searchDuration);

        if (currentState == EnemyState.SEARCHING)
            ChangeState(EnemyState.PATROL);
    }

    private IEnumerator LookRoutine()
    {
        yield return new WaitForSeconds(searchDuration);

        if (currentState == EnemyState.LOOK)
        {
            if (PlayerInArea(radiusLook))
                ChangeState(EnemyState.CHASE);
            else
                ChangeState(EnemyState.PATROL);
        }
    }

    private void Patrol()
    {
        if (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            return;

        currentIndex = (currentIndex + 1) % destination.Length;
        agent.SetDestination(destination[currentIndex].position);
        soundEmitter.EmitSound(ESound.ENEMY_PATROL);
    }

    // FIX: guardamos el transform RAÍZ del jugador (donde está PlayerController),
    // no el del collider hijo, para que GetComponent<PlayerController> siempre funcione
    private bool PlayerInArea(float radius)
    {
        if (playerDead) return false;
        Collider hit = Physics.OverlapSphere(transform.position, radius, playerLayer).FirstOrDefault();
        if (hit == null) return false;

        PlayerController player = hit.GetComponentInParent<PlayerController>();
        if (player == null) return false;

        Vector3 targetForAngle = player.GetEyePoint().position;
        float angle = Vector3.Angle(transform.forward,
                      (targetForAngle - transform.position).normalized);

        if (angle <= 60f)
        {
            Vector3 origin = headTransform != null ? headTransform.position : transform.position + Vector3.up;
            Vector3 direction = (targetForAngle - origin).normalized;
            float distance = Vector3.Distance(origin, targetForAngle);
            bool blocked = Physics.Raycast(origin, direction, distance, obstacleLayer);
            Debug.DrawRay(origin, direction * distance, blocked ? Color.red : Color.green);

            if (!blocked)
            {
                // FIX: guardamos el transform del objeto raíz del jugador
                playerTransform = player.transform;
                return true;
            }
        }
        return false;
    }

    private bool PlayerInAttackArea()
    {
        if (playerDead) return false;
        Collider hit = Physics.OverlapSphere(transform.position, radiusAttack, playerLayer).FirstOrDefault();
        if (hit == null) return false;

        PlayerController player = hit.GetComponentInParent<PlayerController>();
        if (player == null) return false;

        // FIX: guardamos el transform raíz
        playerTransform = player.transform;

        Vector3 origin = headTransform != null ? headTransform.position : transform.position + Vector3.up;
        Vector3 targetPoint = player.GetEyePoint().position;
        Vector3 direction = (targetPoint - origin).normalized;
        float distance = Vector3.Distance(origin, targetPoint);
        bool blocked = Physics.Raycast(origin, direction, distance, obstacleLayer);

        Debug.DrawRay(origin, direction * distance, blocked ? Color.red : Color.green);
        return !blocked;
    }

    // FIX: nueva función que comprueba solo distancia, sin ángulo ni raycast,
    // para no perder al jugador por oclusión durante la persecución
    private bool PlayerInRawRadius(float radius)
    {
        if (playerDead) return false;
        Collider hit = Physics.OverlapSphere(transform.position, radius, playerLayer).FirstOrDefault();
        return hit != null;
    }

    private void LookToPlayer()
    {
        if (playerTransform == null) return;

        PlayerController player = playerTransform.GetComponent<PlayerController>();
        Vector3 targetPos = player != null ? player.GetEyePoint().position : playerTransform.position;

        Vector3 direction = (targetPos - transform.position);
        direction.y = 0;
        if (direction == Vector3.zero) return;

        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void ChasePlayer()
    {
        if (playerTransform == null) return;
        agent.SetDestination(playerTransform.position);
        soundEmitter.EmitSound(ESound.ENEMY_CHASE);
    }

    private void Shoot()
    {
        if (hasShot) return;
        // FIX: se elimina el "hasShot = true" duplicado que había aquí;
        // solo se pone dentro de la corrutina para evitar redundancia

        if (shootingPoint != null && bala != null)
        {
            Vector3 shootDirection = transform.forward;
            if (playerTransform != null)
            {
                PlayerController player = playerTransform.GetComponent<PlayerController>();
                Vector3 targetPoint = player != null
                    ? player.GetEyePoint().position : playerTransform.position;
                shootDirection = (targetPoint - shootingPoint.transform.position).normalized;
            }

            switch (enemyType)
            {
                case EnemyType.FUERTE:
                    int perdigones = 3;
                    float spreadAngle = 15f;
                    for (int i = 0; i < perdigones; i++)
                    {
                        Quaternion spread = Quaternion.Euler(
                            Random.Range(-spreadAngle, spreadAngle),
                            Random.Range(-spreadAngle, spreadAngle), 0);
                        Vector3 direction = spread * shootDirection;
                        GameObject go = Instantiate(bala, shootingPoint.transform.position, shootingPoint.transform.rotation);
                        BalaController balaController = go.GetComponent<BalaController>();
                        if (balaController != null) balaController.SetDamage(damageAmount);
                        Rigidbody rb = go.GetComponent<Rigidbody>();
                        if (rb != null) rb.linearVelocity = direction * bulleSpeed;
                    }
                    soundEmitter.EmitSound(ESound.ENEMY_SHOOT2);
                    if (shootFlash != null) shootFlash.Play();
                    break;

                default:
                    GameObject goDefault = Instantiate(bala, shootingPoint.transform.position, shootingPoint.transform.rotation);
                    BalaController balaDefault = goDefault.GetComponent<BalaController>();
                    if (balaDefault != null) balaDefault.SetDamage(damageAmount);
                    Rigidbody rbDefault = goDefault.GetComponent<Rigidbody>();
                    if (rbDefault != null) rbDefault.linearVelocity = shootDirection * bulleSpeed;
                    soundEmitter.EmitSound(ESound.ENEMY_SHOOT);
                    if (shootFlash != null) shootFlash.Play();
                    break;
            }
        }

        if (SceneManager.GetActiveScene().name != "Escena2")
            OnAidenScared?.Raise(ScareType.Shoot);

        StartCoroutine(ShootCooldown());
    }

    private IEnumerator ShootCooldown()
    {
        hasShot = true;
        yield return new WaitForSeconds(shootCooldown);
        hasShot = false;
    }

    private void OnPlayerDied()
    {
        playerDead = true;
        ChangeState(EnemyState.PATROL);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Objetos"))
        {
            string tag = collision.gameObject.tag;
            if (tag == "Botella" || tag == "Piedra")
            {
                Vector3 hitDirection = (transform.position - collision.transform.position).normalized;
                hitDirection.y = 0;

                if (hitDirection != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(hitDirection);

                // FIX: al entrar en LOOK por colisión no tenemos playerTransform,
                // pero eso está bien; el LookRoutine esperará y luego patrullará
                ChangeState(EnemyState.LOOK);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;
        float halfAngle = 60f;
        int arcSegments = 12;

        Gizmos.color = Color.green;
        DrawCone(origin, forward, radiusChase, halfAngle, arcSegments);

        Gizmos.color = Color.blue;
        DrawCone(origin, forward, radiusLook, halfAngle, arcSegments);
    }

    void DrawCone(Vector3 origin, Vector3 forward, float radius, float halfAngle, int segments)
    {
        Vector3 leftDir = Quaternion.Euler(0, -halfAngle, 0) * forward;
        Vector3 rightDir = Quaternion.Euler(0, halfAngle, 0) * forward;

        Gizmos.DrawLine(origin, origin + leftDir * radius);
        Gizmos.DrawLine(origin, origin + rightDir * radius);

        Vector3 prevPoint = origin + leftDir * radius;
        for (int i = 1; i <= segments; i++)
        {
            float angle = -halfAngle + (halfAngle * 2f / segments) * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * forward;
            Vector3 point = origin + dir * radius;
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radiusAttack);
    }

    public void TakeDamage(float damage)
    {
        this.vida -= damage;
        if (this.vida <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        onMusicChange?.Raise("musicStop");
        soundEmitter?.StopSound();

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;
        }

        if (animator != null) animator.enabled = false;
        ActivateKinematic(false);
        CapsuleCollider col = GetComponent<CapsuleCollider>();
        if (col != null) col.enabled = false;

        this.enabled = false;
    }
}