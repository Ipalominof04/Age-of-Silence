using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class EnemyController : MonoBehaviour, ISoundListener, IDamageable
{
    private NavMeshAgent agent;
    private Animator animator;
    private SoundEmitter soundEmitter;
    private Transform playerTransform;
    public enum EnemyState { NONE, IDLE, PATROL, CHASE, ATTACK, SEARCHING, LOOK };
    public enum EnemyType { NORMAL, FUERTE, VELOZ, VISIONS }

    [Header("Maquina de estados")]
    [SerializeField] private EnemyState currentState;

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
    private bool hasShot = false;
    private bool playerDead = false;

    [Header("Eventos")]
    [SerializeField] private GameEventScareType OnAidenScared;
    [SerializeField] private GameEventString onMusicChange;

    [Header("Config Areas y Layers")]
    [SerializeField] private Transform headTransform;
    [SerializeField] private float radiusChase;
    [SerializeField] private float radiusLook;
    [SerializeField] private float radiusAttack;
    [SerializeField] private float radiusLosePlayer;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;
    private bool justLostPlayer = false;
    private Coroutine lookCoroutine;
    private Coroutine searchCoroutine;
    private float searchDuration = 5f;

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
    private bool isDead = false;

    //implementacio de la interficie ISoundListener
    public float GetListenRadius() => listenRadius;
    public float GetMinDetectionLevel() => minDetectionLevel;

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
    //implementacio de la interficie ISoundListener s'executa quan l'enemic sent un so proper
    public void OnHearSound(Vector3 soundPosition, float volume)
    {
        if (volume < minDetectionLevel) return;
        if (currentState == EnemyState.CHASE || currentState == EnemyState.ATTACK) return;

        //comprova que el punt de so sigui accessible al NavMesh abans de moure-s'hi
        NavMeshHit navHit;
        if (!NavMesh.SamplePosition(soundPosition, out navHit, 3f, NavMesh.AllAreas)) return;
        Vector3 validDestination = navHit.position;

        agent.SetDestination(validDestination);

        //si ja estava buscant, redirigeix sense reiniciar la corrutina des de zero
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
    //retorna la posició de l'enemic (implementat per ISoundListener)
    public Vector3 GetPosition()
    {
        return transform.position;
    }
    //mostra o amaga la UI d'estat de l'enemic amb text i color personalitzables
    private void SetUI(bool active, string text = "", string hexColor = "#FFFFFF")
    {
        if (canvasWorld != null)
        {
            canvasWorld.SetActive(active);
            if (stateText != null)
            {
                //llanca l'animació del text nomes si el contingut canvia
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

    //asignar els components del enemic
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        soundEmitter = GetComponent<SoundEmitter>();
    }

    void Start()
    {
        //el enemic comenca en patrol
        InitState(EnemyState.PATROL);
        ActivateKinematic(true);
        SetUI(false);
    }

    void Update()
    {
        UpdateState();
        ApplyGravity();
    }

    //aplica gravetat manual al agent
    private void ApplyGravity()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;

        //aumula velocitat cap avall, es reinicia quan l'agent es sobre el navmesh
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

                //posicio de l'arma diferent segons el tipus d'enemic
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
                //atura la musica de persecucio
                onMusicChange.Raise("musicStop");
                break;

            case EnemyState.CHASE:
                agent.isStopped = false;
                animator.SetBool("onChase", true);
                //posicio de l'arma diferent segons el tipus d'enemic
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
                //activa la musica de persecucio
                onMusicChange.Raise("musicChase");
                SetUI(true, "!", "#F11200");
                agent.speed = speedChase;
                agent.stoppingDistance = radiusAttack;
                break;

            case EnemyState.ATTACK:
                animator.SetBool("onShoot", true);
                //posicio de l'arma diferent segons el tipus d'enemic
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
            case EnemyState.IDLE:
                if (PlayerInArea(radiusChase))
                    ChangeState(EnemyState.CHASE);
                else if (PlayerInArea(radiusLook))
                    ChangeState(EnemyState.LOOK);
                else
                    ChangeState(EnemyState.PATROL);
                break;

            case EnemyState.PATROL:
                //avanca al seguent waypoint
                Patrol();
                if (PlayerInArea(radiusChase))
                    ChangeState(EnemyState.CHASE);
                else if (PlayerInArea(radiusLook))
                    ChangeState(EnemyState.LOOK);
                break;

            case EnemyState.CHASE:
                //segueix el jugador
                ChasePlayer();
                if (PlayerInAttackArea())
                    ChangeState(EnemyState.ATTACK);
                //si el jugador surt del raw radius despres de entrar a chase, l'enemic ho perd i va a searching
                else if (!PlayerInRawRadius(radiusLosePlayer))
                {
                    //bloquejem la redeteccio inmediata al entrar a SEARCHING per evitar el bug de bucle entre SEARCHING i CHASE
                    justLostPlayer = true;
                    ChangeState(EnemyState.SEARCHING);
                }
                break;

            case EnemyState.ATTACK:
                //mira sempre al jugador mentres esta disparant per no perdre'l al segon
                LookToPlayer();
                //dispara
                Shoot();
                //el jugador ha sortit del radi d'atac: torna a perseguir o a buscar
                if (!PlayerInAttackArea())
                {
                    if (PlayerInRawRadius(radiusLosePlayer))
                        ChangeState(EnemyState.CHASE);
                    else
                        ChangeState(EnemyState.SEARCHING);
                }
                break;

            case EnemyState.SEARCHING:
                //desactiva el bool del animator de onPatrol i activa el de onSearching
                animator.SetBool("onPatrol", !agent.isStopped);
                animator.SetBool("onSearching", agent.isStopped);

                //nomes torna a detectar quan l'enemic hagui arribat al ultim punt conegut del player
                if (!justLostPlayer)
                {
                    if (PlayerInArea(radiusChase))
                        ChangeState(EnemyState.CHASE);
                    else if (PlayerInArea(radiusLook))
                        ChangeState(EnemyState.CHASE);
                }
                break;

            case EnemyState.LOOK:
                //mira cap on va veure el jugador
                if (playerTransform != null)
                    LookToPlayer();
                //si el player entra directament en el rang d'attack pasa a ATTACK
                if (PlayerInAttackArea())
                    ChangeState(EnemyState.ATTACK);
                //si el player entra directament en el rang de chase pasa a CHASE
                else if (PlayerInArea(radiusChase))
                    ChangeState(EnemyState.CHASE);
                break;
        }
    }
    //Neteja animacions i corrutines en sortir d'un estat
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
    //ragdoll de l'enemic
    private void ActivateKinematic(bool activate)
    {
        foreach (var rb in rigidbodies)
        {
            rb.isKinematic = activate;
        }
        //quan el ragdoll deixa caure a l'enemic, l'animator es desactiva per evitar bugs
        animator.enabled = activate; 
    }
    //corrutina de search: l'enemic va a l'ultima posicio coneguda del jugador, espera i si no veu al jugador torna a patrullar
    private IEnumerator SearchRoutine()
    {
        while (agent != null && agent.enabled && agent.isOnNavMesh && (agent.pathPending || agent.remainingDistance > agent.stoppingDistance))
            yield return null;

        //l'enemic ha arribat a l'ultim punt conegut del player i ja pot tornar a detectar al jugador amb els rangs de visio
        //i tornar a entrar a raw quan l'enemic pasi a chase
        justLostPlayer = false;
        agent.isStopped = true;
        animator.SetBool("onPatrol", false);
        animator.SetBool("onSearching", true);

        //asignar la posicio de l'arma tan per l'enemic fort com per a la resta
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

        //si no ha detectat res torna a patrol
        if (currentState == EnemyState.SEARCHING)
            ChangeState(EnemyState.PATROL);
    }
    //corrutina de look: l'enemic s'atura uns segons despres de que el jugador entri en el rang de look, si el player continua
    //a l'area quan s'acabi la corrutina, l'enemic el detecta
    private IEnumerator LookRoutine()
    {
        yield return new WaitForSeconds(searchDuration);

        if (currentState == EnemyState.LOOK)
        {
            //si el detecta va a CHASE, si no el detecta torna a PATROL
            if (PlayerInArea(radiusLook))
                ChangeState(EnemyState.CHASE);
            else
                ChangeState(EnemyState.PATROL);
        }
    }

    //avanca de waypoint a waypoint, aquests waypoints estan asignats a l'inspector
    private void Patrol()
    {
        if (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            return;

        currentIndex = (currentIndex + 1) % destination.Length;
        agent.SetDestination(destination[currentIndex].position);
        soundEmitter.EmitSound(ESound.ENEMY_PATROL);
    }

    //comprova si el jugador es dins del con de visió (angle + raycast que comproba si hi ha obstacles entre l'enemic i el player)
    //guarda el transform arrel del jugador (on hi ha PlayerController), no el del collider fill
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

        //con de visio de 120 (60 a cada costat a partir del forward de l'enemic)
        if (angle <= 60f)
        {
            //llença un raycast per comprobar si hi ha algun obstacle entre l'enemic i el jugador, si hi ha algun obstacle l'enemic no veu al jugador
            Vector3 origin = headTransform != null ? headTransform.position : transform.position + Vector3.up;
            Vector3 direction = (targetForAngle - origin).normalized;
            float distance = Vector3.Distance(origin, targetForAngle);
            bool blocked = Physics.Raycast(origin, direction, distance, obstacleLayer);
            Debug.DrawRay(origin, direction * distance, blocked ? Color.red : Color.green);

            if (!blocked)
            {
                //guarda el transform per usar-lo a ChasePlayer
                playerTransform = player.transform;
                return true;
            }
        }
        return false;
    }
    //comprova si el jugador es dins del radi d'atac, sense con de visio (aquest radi es un cercle)
    private bool PlayerInAttackArea()
    {
        if (playerDead) return false;
        Collider hit = Physics.OverlapSphere(transform.position, radiusAttack, playerLayer).FirstOrDefault();
        if (hit == null) return false;

        PlayerController player = hit.GetComponentInParent<PlayerController>();
        if (player == null) return false;

        //actualitza sempre el transform en atac
        playerTransform = player.transform;

        //apunta sempre al cap del player, per a poder disparar correctament independentment de si el player esta de peu o ajupit
        Vector3 origin = headTransform != null ? headTransform.position : transform.position + Vector3.up;
        Vector3 targetPoint = player.GetEyePoint().position;
        Vector3 direction = (targetPoint - origin).normalized;
        float distance = Vector3.Distance(origin, targetPoint);
        bool blocked = Physics.Raycast(origin, direction, distance, obstacleLayer);

        Debug.DrawRay(origin, direction * distance, blocked ? Color.red : Color.green);
        return !blocked;
    }

    //nomes comproba a distancia, sense angle i sense raycast
    //s'utiliza nomes quan l'enemic ja esta perseguint al player en CHASE, aixo es fa per a que l'enemic no perdi
    //tan facilment al player, sense aixo et perd nomes sortir del con de visio
    private bool PlayerInRawRadius(float radius)
    {
        if (playerDead) return false;
        Collider hit = Physics.OverlapSphere(transform.position, radius, playerLayer).FirstOrDefault();
        return hit != null;
    }
    //rota a l'enemic cap al jugador per a que pugui disparar constantment al jugador i no el perdi estan en rang d'attack
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
    //envia a l'agent cap a la posicio del jugador
    private void ChasePlayer()
    {
        if (playerTransform == null) return;
        agent.SetDestination(playerTransform.position);
        soundEmitter.EmitSound(ESound.ENEMY_CHASE);
    }
    //gestiona el dispar distingint entre l'enemic fort (que te un comportament diferent perque te una escopeta en comptes d'un rigle) i la resta
    private void Shoot()
    {
        //bloqueja el shoot durant el cooldown que te l'enemic
        if (hasShot) return;

        //calcula la direccio cap al punt dels ulls del jugador (cap al cap del jugador)
        //per poder dispara-li independement de estar ajupit o de peu
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
                    //escopeta: dispara 3 perdigons amb dispersio
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
                    //rifle: dispara una sola bala en linea recta
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

        //es llanca l'event de shoot per a que aiden s'espanti cada vegada que es dispara (excepte en escena2 perque no s'introdueix fins l'escena3)
        if (SceneManager.GetActiveScene().name != "Escena2")
            OnAidenScared?.Raise(ScareType.Shoot);

        StartCoroutine(ShootCooldown());
    }

    //corrutina de cooldown per al dispar: bloqueja el dispar durant el temps indicat (canvia depenent del tipus d'enemic)
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
    //quan l'enemic collisiona amb una ampolla o una pedra (el player se la llanca), es gira cap al impacte i entra en LOOK
    //(aixo es nomes en cas de que l'objecte llencable impacte contra l'enemic, si impacta contra el terra i l'enemic l'escolta, es dirigira cap al so)
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Objetos"))
        {
            string tag = collision.gameObject.tag;
            if (tag == "Botella" || tag == "Piedra")
            {
                Vector3 hitDirection = (transform.position - collision.transform.position).normalized;
                hitDirection.y = 0;

                //rotar a l'enemic en la direccio de la colisio
                if (hitDirection != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(hitDirection);

                //en entrar a LOOK per collisio no hi ha playerTransform, pero LookRoutine
                //esperara el tems de LOOK i despres tornara a patrullar sense problemes
                ChangeState(EnemyState.LOOK);
            }
        }
    }

    //gizmo amb els cons de visio de l'enemic (nomes visible al Scene View)
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
    //implementacio de IDamageable: resta vida i mata a l'enemic si arriba a 0
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        this.vida -= damage;
        if (this.vida <= 0)
        {
            Die();
        }
    }
    //gestiona la mort de l'enemic, atura tot
    public void Die()
    {
        if (isDead) return;
        isDead = true; 
        onMusicChange?.Raise("musicStop");
        soundEmitter?.StopSound();

        if (searchCoroutine != null)
            StopCoroutine(searchCoroutine);

        if (lookCoroutine != null)
            StopCoroutine(lookCoroutine);

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;
        }

        if (animator != null) animator.enabled = false;
        //activa el ragdoll (desactiva kinematic)
        ActivateKinematic(false);
        CapsuleCollider col = GetComponent<CapsuleCollider>();
        if (col != null) col.enabled = false;

        this.enabled = false;
    }
}