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

    [Header("Eventos")]
    [SerializeField] private GameEventString OnAidenScared;

    [Header("Config Areas y Layers")]
    [SerializeField] private Transform headTransform;
    [SerializeField] private float radiusChase;
    [SerializeField] private float radiusLook;
    [SerializeField] private float radiusAttack;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;
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

    private float gravity = -9.81f;
    private Vector3 verticalVelocity = Vector3.zero;

    public float GetListenRadius() => listenRadius;
    public float GetMinDetectionLevel() => minDetectionLevel;

    private float searchDuration = 5f;
    private bool hasShot = false;
    private Coroutine lookCoroutine;
    private Coroutine searchCoroutine;

    private Animator animator;

    private void OnEnable()
    {
        SoundManager.Instance.Subscribe(this);
    }

    private void OnDisable()
    {
        SoundManager.Instance.Unsubscribe(this);
    }

    public void OnHearSound(Vector3 soundPosition, float volume)
    {
        if (volume < minDetectionLevel) return; // filtre nivell mínim
        if (currentState == EnemyState.CHASE || currentState == EnemyState.ATTACK) return;

        agent.SetDestination(soundPosition);
        ChangeState(EnemyState.SEARCHING);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    private void SetUI(bool active, string text = "", string hexColor = "#FFFFFF") //asi se pone un valor por defecto
    {
        if (canvasWorld != null)
        {
            canvasWorld.SetActive(active);
            if (stateText != null)
            {
                stateText.text = text;
                //para poder ponerle un color especifico con los colorutility
                ColorUtility.TryParseHtmlString(hexColor, out Color color);
                stateText.color = color;
            }
        }
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        InitState(EnemyState.PATROL);
        SetUI(false);

    }

    void Update()
    {
        UpdateState();
        ApplyGravity();
    }
    private void ApplyGravity()
    {
        verticalVelocity.y += gravity * Time.deltaTime;
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
                arma.transform.localPosition = new Vector3(-0.048f, -0.122f, 0.009f);
                arma.transform.localRotation = Quaternion.Euler(-73.674f, 115.541f, -128.345f);
                SetUI(false);
                agent.speed = speedPatrol;
                agent.stoppingDistance = 0.2f;
                agent.SetDestination(destination[currentIndex].position);
                break;

            case EnemyState.CHASE:
                agent.isStopped = false;
                animator.SetBool("onChase", true);
                arma.transform.localPosition = new Vector3(-0.05697f, -0.131036f, -0.00457f);
                arma.transform.localRotation = Quaternion.Euler(-100.667f, -73.7940f, 71.358f);
                SetUI(true, "!", "#F11200");
                agent.speed = speedChase;
                agent.stoppingDistance = radiusAttack;
                break;

            case EnemyState.ATTACK:
                animator.SetBool("onShoot", true);
                arma.transform.localPosition = new Vector3(-0.03414f, -0.13247f, -0.007156f);
                arma.transform.localRotation = Quaternion.Euler(-96.586f, -34.216f, 31.978f);
                break;

            case EnemyState.SEARCHING:
                animator.SetBool("onPatrol", true);
                agent.isStopped = false;
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
            case EnemyState.IDLE:
                if (PlayerInArea(radiusChase))
                {
                    ChangeState(EnemyState.CHASE);
                }
                else if (!PlayerInArea(radiusChase))
                {
                    ChangeState(EnemyState.PATROL);
                }
                else if (PlayerInArea(radiusLook))
                {
                    ChangeState(EnemyState.LOOK);
                }
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
                else if (!PlayerInArea(radiusChase) && !PlayerInArea(radiusLook))
                    ChangeState(EnemyState.SEARCHING);
                break;

            case EnemyState.ATTACK:
                LookToPlayer();
                Shoot();
              if (!PlayerInAttackArea())
                {
                    if (PlayerInArea(radiusChase))
                        ChangeState(EnemyState.CHASE);
                    else
                        ChangeState(EnemyState.SEARCHING);
                }
                break;


            case EnemyState.SEARCHING:
                if (PlayerInArea(radiusChase))
                    ChangeState(EnemyState.CHASE);
                else if (PlayerInArea(radiusLook))
                    ChangeState(EnemyState.CHASE);
                break;

            case EnemyState.LOOK:

                if (playerTransform != null) LookToPlayer();

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
                break;
            case EnemyState.CHASE:
                animator.SetBool("onChase", false);
                break;
            case EnemyState.ATTACK:
                animator.SetBool("onShoot", false);
                break;
            case EnemyState.SEARCHING:
                animator.SetBool("onSearching", false);
                if (searchCoroutine != null) StopCoroutine(searchCoroutine);
                break;
            case EnemyState.LOOK:
                animator.SetBool("onLook", false);
                if (lookCoroutine != null) StopCoroutine(lookCoroutine);
                break;
        }
    }

    private IEnumerator SearchRoutine()
    {
        // Espera hasta que llegue al destino
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            yield return null;

       // agent.isStopped = true;
        animator.SetBool("onPatrol", false);
        animator.SetBool("onSearching", true);
        arma.transform.localPosition = new Vector3(-0.022f, -0.132f, 0f);
        arma.transform.localRotation = Quaternion.Euler(-93.801f, -29.005f, 22.375f);

        // Una vez llegado, espera el tiempo de búsqueda
        yield return new WaitForSeconds(searchDuration);

        if (currentState == EnemyState.SEARCHING)
            ChangeState(EnemyState.PATROL);
    }
    private IEnumerator LookRoutine()
    {
        yield return new WaitForSeconds(searchDuration);

        if (currentState == EnemyState.LOOK)
        {
            //si sigo viéndole, perseguir; si no, patrullar
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
    }

    private bool PlayerInArea(float radius)
    {
        Collider hit = Physics.OverlapSphere(transform.position, radius, playerLayer).FirstOrDefault();
        if (hit == null) return false;

        // Buscar en el padre si el script no está en el mismo objeto que el collider
        PlayerController player = hit.GetComponentInParent<PlayerController>();
        Vector3 targetForAngle = player != null ? player.GetEyePoint().position : hit.transform.position;

        float angle = Vector3.Angle(transform.forward,
                      (targetForAngle - transform.position).normalized);

        if (angle <= 60f)
        {
            Vector3 origin = headTransform != null ? headTransform.position : transform.position + Vector3.up;
            Vector3 targetPoint = player != null ? player.GetEyePoint().position : hit.bounds.max;

            Vector3 direction = (targetPoint - origin).normalized;
            float distance = Vector3.Distance(origin, targetPoint);

            bool blocked = Physics.Raycast(origin, direction, distance, obstacleLayer);
            Debug.DrawRay(origin, direction * distance, blocked ? Color.red : Color.green);

            if (!blocked)
            {
                playerTransform = hit.transform;
                return true;
            }
        }
        return false;
    }

    private bool PlayerInAttackArea()
    {
        Collider hit = Physics.OverlapSphere(transform.position, radiusAttack, playerLayer).FirstOrDefault();
        if (hit == null) return false;

        playerTransform = hit.transform;

        Vector3 origin = headTransform != null ? headTransform.position : transform.position + Vector3.up;

        // Buscar en el padre igual que en PlayerInArea
        PlayerController player = hit.GetComponentInParent<PlayerController>();
        Vector3 targetPoint = player != null ? player.GetEyePoint().position : hit.bounds.center;

        Vector3 direction = (targetPoint - origin).normalized;
        float distance = Vector3.Distance(origin, targetPoint);
        bool blocked = Physics.Raycast(origin, direction, distance, obstacleLayer);

        Debug.DrawRay(origin, direction * distance, blocked ? Color.red : Color.green);
        return !blocked;
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
       // agent.stoppingDistance = radiusAttack;
        agent.SetDestination(playerTransform.position);
    }

    private void Shoot()
    {
        if (hasShot) return;
        hasShot = true;

        if (shootingPoint != null && bala != null)
        {
            //calcula dirección hacia el punto de mira del jugador
            Vector3 shootDirection = transform.forward; //fallback
            if (playerTransform != null)
            {
                PlayerController player = playerTransform.GetComponentInParent<PlayerController>();
                Vector3 targetPoint = player != null
                    ? player.GetEyePoint().position : playerTransform.position;
                shootDirection = (targetPoint - shootingPoint.transform.position).normalized;
            }

            switch (enemyType)
            {
                case EnemyType.FUERTE:
                    int pellets = 5;
                    float spreadAngle = 15f;
                    for (int i = 0; i < pellets; i++)
                    {
                        Quaternion spread = Quaternion.Euler(Random.Range(-spreadAngle, spreadAngle), Random.Range(-spreadAngle, spreadAngle),0);
                        Vector3 direction = spread * shootDirection; //usar shootDirection
                        GameObject go = Instantiate(bala, shootingPoint.transform.position, shootingPoint.transform.rotation);
                        BalaController balaController = go.GetComponent<BalaController>();
                        if (balaController != null) balaController.SetDamage(damageAmount);
                        Rigidbody rb = go.GetComponent<Rigidbody>();
                        if (rb != null) rb.linearVelocity = direction * bulleSpeed;
                    }
                    break;

                default:
                    GameObject goDefault = Instantiate(bala, shootingPoint.transform.position, shootingPoint.transform.rotation);
                    BalaController balaDefault = goDefault.GetComponent<BalaController>();
                    if (balaDefault != null) balaDefault.SetDamage(damageAmount);
                    Rigidbody rbDefault = goDefault.GetComponent<Rigidbody>();
                    if (rbDefault != null) rbDefault.linearVelocity = shootDirection * bulleSpeed; //usar shootDirection
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
        yield return new WaitForSeconds(shootCooldown); //ajusta el tiempo entre disparos
        hasShot = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Objetos"))
        {
            string tag = collision.gameObject.tag;
            if (tag == "Botella" || tag == "Piedra")
            {
                //dirección desde la que viene el objeto
                Vector3 hitDirection = (transform.position - collision.transform.position).normalized;
                hitDirection.y = 0;

                if (hitDirection != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(hitDirection);

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


        /*
        Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
        Gizmos.DrawWireSphere(origin, listenRadius);*/
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
        Destroy(this.gameObject);
    }
}