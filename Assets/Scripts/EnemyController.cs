using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class EnemyController : MonoBehaviour, ISoundListener
{
    public enum EnemyState { NONE, IDLE, PATROL, CHASE, ATTACK, SEARCHING, LOOK };

    [Header("Maquina de estados")]
    [SerializeField] private EnemyState currentState;
    private NavMeshAgent agent;

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

    private float chaseTimer = 0f;
    private bool chaseFromLook = false;

    [SerializeField] private float searchDuration = 5f;

    private bool hasShot = false;

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
        agent.SetDestination(soundPosition);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        InitState(EnemyState.PATROL);
    }

    void Update()
    {
        UpdateState();
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
                break;

            case EnemyState.PATROL:
                agent.isStopped = false;
                agent.speed = 1f;
                agent.stoppingDistance = 0.2f;
                agent.SetDestination(destination[currentIndex].position);
                break;

            case EnemyState.CHASE:
                agent.isStopped = false;
                agent.speed = 2f;
                agent.stoppingDistance = radiusAttack;
                chaseTimer = chaseFromLook ? 2f : 0f;
                break;

            case EnemyState.ATTACK:
                agent.isStopped = true;
                break;

            case EnemyState.SEARCHING:
                agent.isStopped = true;
                break;

            case EnemyState.LOOK:
                agent.isStopped = true;
                break;
        }
    }

    private void UpdateState()
    {
        switch (currentState)
        {
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
                if (chaseTimer > 0f)
                    chaseTimer -= Time.deltaTime;

                MoveToPlayer();

                if (PlayerInAttackArea())
                {
                    chaseFromLook = false;
                    ChangeState(EnemyState.ATTACK);
                }
                else if (!PlayerInArea(radiusChase) && chaseTimer <= 0f)
                {
                    chaseFromLook = false;
                    ChangeState(EnemyState.SEARCHING);
                }
                break;

            case EnemyState.ATTACK:
                LookToPlayer();
                Shoot();
                if (!PlayerInAttackArea())
                    ChangeState(EnemyState.CHASE);
                break;

            case EnemyState.SEARCHING:
                StartCoroutine(SearchRoutine());
                if (PlayerInArea(radiusChase))
                {
                    ChangeState(EnemyState.CHASE);
                }
                else if (PlayerInArea(radiusLook))
                {
                    chaseFromLook = true;
                    ChangeState(EnemyState.CHASE);
                }

                break;

            case EnemyState.LOOK:
                if (PlayerInAttackArea())
                    ChangeState(EnemyState.ATTACK);
                else if (PlayerInArea(radiusChase))
                    ChangeState(EnemyState.CHASE);
                else if (!PlayerInArea(radiusLook))
                    ChangeState(EnemyState.PATROL);
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
    private IEnumerator SearchRoutine()
    {
        yield return new WaitForSeconds(searchDuration);

        if (currentState == EnemyState.SEARCHING)
            ChangeState(EnemyState.PATROL);

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

        float angle = Vector3.Angle(transform.forward,
                      (hit.transform.position - transform.position).normalized);

        if (angle <= 45f)
        {
            Vector3 origin = headTransform != null ? headTransform.position : transform.position + Vector3.up;
            Vector3 targetPoint = hit.bounds.max;
            Vector3 direction = (targetPoint - origin).normalized;
            float distance = Vector3.Distance(origin, targetPoint);

            bool blocked = Physics.Raycast(origin, direction, distance, obstacleLayer);

            // Rojo = bloqueado, Verde = ve al jugador
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
        if (hit != null)
        {
            playerTransform = hit.transform;
            return true;
        }
        return false;
    }

    private void LookToPlayer()
    {
        if (playerTransform == null) return;

        Vector3 direction = (playerTransform.position - transform.position);
        direction.y = 0;
        if (direction == Vector3.zero) return;

        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void MoveToPlayer()
    {
        if (playerTransform == null) return;
        agent.SetDestination(playerTransform.position);
    }

    private void Shoot()
    {
        if (hasShot) return;
        StartCoroutine(ShootCooldown());

        print("pium pium");
        if (SceneManager.GetActiveScene().name != "Escena2")
            OnAidenScared?.Raise(ScareType.Shoot);
    }

    private IEnumerator ShootCooldown()
    {
        hasShot = true;
        yield return new WaitForSeconds(1f); // ajusta el tiempo entre disparos
        hasShot = false;
    }

    private void OnDrawGizmos()
    {
        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;
        float halfAngle = 45f;
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
}