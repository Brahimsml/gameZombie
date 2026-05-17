using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    private PlayerController playerController;
    private PlayerHealth playerHealth;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float chaseRange = 15f;
    public float stopCloseRange = 2f;

    [Header("Random Movement Settings")]
    public float minMoveSpeed = 2.4f;
    public float maxMoveSpeed = 3.8f;
    public float minChaseRange = 25f;
    public float maxChaseRange = 45f;

    [Header("Animation")]
    public Animator anim;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex;
    public Transform pointsHolder;
    public float pointWaitTime = 3f;
    private float waitCounter;

    [Header("Health / Death")]
    private bool isDead;
    public float maxHealth = 100f;
    private float currentHealth;
    public float waitToDisappear = 4f;

    [Header("Attack")]
    public float attackDamage = 10f;
    public float attackRate = 1.5f;
    public float attackHitDelay = 0.35f;
    private float attackCounter;
    private bool isAttacking;

    [Header("Sounds")]
    public AudioSource audioSource;
    public AudioClip zombieAttackSound;
    public AudioClip zombieDeathSound;
    public AudioClip zombiePainSound;
    public AudioClip zombieChaseSound;
    public float minChaseSoundTime = 5f;
    public float maxChaseSoundTime = 8f;
    private float chaseSoundCounter;

    [Header("Anti Stuck")]
    private Vector3 lastPosition;
    private float stuckTimer;
    public float stuckCheckTime = 2f;
    public float stuckDistance = 0.15f;
    public float patrolReachDistance = 2.5f;

    private NavMeshAgent agent;

    void Start()
    {
        currentHealth = maxHealth;

        playerController = FindObjectOfType<PlayerController>();

        if (playerController != null)
        {
            playerHealth = playerController.GetComponent<PlayerHealth>();
        }

        agent = GetComponent<NavMeshAgent>();

        moveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);
        chaseRange = Random.Range(minChaseRange, maxChaseRange);

        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = stopCloseRange;
            agent.isStopped = false;
        }

        if (pointsHolder != null)
        {
            pointsHolder.SetParent(null);
        }

        waitCounter = Random.Range(.75f, 1.25f) * pointWaitTime;
        lastPosition = transform.position;

        chaseSoundCounter = Random.Range(minChaseSoundTime, maxChaseSoundTime);
    }

    void Update()
    {
        if (isDead)
        {
            HandleDeathDisappear();
            return;
        }

        if (playerController == null || agent == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, playerController.transform.position);

        if (distance < chaseRange)
        {
            ChaseAndAttack(distance);
        }
        else
        {
            Patrol();
        }

        CheckIfStuck();
    }

    void ChaseAndAttack(float distance)
    {
        HandleChaseSound();

        if (distance > stopCloseRange)
        {
            agent.isStopped = false;

            if (agent.isOnNavMesh)
            {
                agent.SetDestination(playerController.transform.position);
            }

            anim.SetBool("moving", true);
            anim.SetBool("attacking", false);
        }
        else
        {
            agent.isStopped = true;
            agent.ResetPath();

            LookAtPlayer();

            anim.SetBool("moving", false);
            anim.SetBool("attacking", true);

            attackCounter -= Time.deltaTime;

            if (attackCounter <= 0 && !isAttacking)
            {
                StartCoroutine(AttackAfterDelay());
                attackCounter = attackRate;
            }
        }
    }

    void HandleChaseSound()
    {
        if (isDead)
        {
            return;
        }

        chaseSoundCounter -= Time.deltaTime;

        if (chaseSoundCounter <= 0f)
        {
            if (audioSource != null && zombieChaseSound != null)
            {
                audioSource.PlayOneShot(zombieChaseSound);
            }

            chaseSoundCounter = Random.Range(minChaseSoundTime, maxChaseSoundTime);
        }
    }

    IEnumerator AttackAfterDelay()
    {
        isAttacking = true;

        yield return new WaitForSeconds(attackHitDelay);

        if (!isDead && playerHealth != null && playerController != null)
        {
            float distance = Vector3.Distance(transform.position, playerController.transform.position);

            Vector3 directionToPlayer = (playerController.transform.position - transform.position).normalized;
            float facingAmount = Vector3.Dot(transform.forward, directionToPlayer);

            if (distance <= stopCloseRange + 0.5f && facingAmount > 0.4f)
            {
                if (audioSource != null && zombieAttackSound != null)
                {
                    audioSource.PlayOneShot(zombieAttackSound);
                }

                playerHealth.TakeDamage(attackDamage);
            }
        }

        isAttacking = false;
    }

    void Patrol()
    {
        anim.SetBool("attacking", false);

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            agent.isStopped = true;
            anim.SetBool("moving", false);
            return;
        }

        Transform targetPoint = patrolPoints[currentPatrolIndex];

        if (targetPoint == null)
        {
            GoToNextPatrolPoint();
            return;
        }

        float distanceToPoint = Vector3.Distance(transform.position, targetPoint.position);

        if (distanceToPoint < patrolReachDistance)
        {
            agent.isStopped = true;
            anim.SetBool("moving", false);

            waitCounter -= Time.deltaTime;

            if (waitCounter <= 0)
            {
                GoToNextPatrolPoint();
                waitCounter = Random.Range(.75f, 1.25f) * pointWaitTime;
            }
        }
        else
        {
            agent.isStopped = false;

            Vector3 validDestination;

            if (GetValidNavMeshPosition(targetPoint.position, out validDestination))
            {
                agent.SetDestination(validDestination);
                anim.SetBool("moving", true);
            }
            else
            {
                GoToNextPatrolPoint();
            }
        }
    }

    bool GetValidNavMeshPosition(Vector3 targetPosition, out Vector3 validPosition)
    {
        NavMeshHit hit;

        if (NavMesh.SamplePosition(targetPosition, out hit, 3f, NavMesh.AllAreas))
        {
            validPosition = hit.position;
            return true;
        }

        validPosition = targetPosition;
        return false;
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            return;
        }

        currentPatrolIndex++;

        if (currentPatrolIndex >= patrolPoints.Length)
        {
            currentPatrolIndex = 0;
        }

        if (agent != null && agent.isOnNavMesh)
        {
            agent.ResetPath();

            if (patrolPoints[currentPatrolIndex] != null)
            {
                Vector3 validDestination;

                if (GetValidNavMeshPosition(patrolPoints[currentPatrolIndex].position, out validDestination))
                {
                    agent.SetDestination(validDestination);
                }
            }
        }
    }

    void CheckIfStuck()
    {
        if (agent == null || isDead || agent.isStopped)
        {
            return;
        }

        float movedDistance = Vector3.Distance(transform.position, lastPosition);

        if (movedDistance < stuckDistance)
        {
            stuckTimer += Time.deltaTime;

            if (stuckTimer >= stuckCheckTime)
            {
                GoToNextPatrolPoint();

                stuckTimer = 0f;
                lastPosition = transform.position;
            }
        }
        else
        {
            stuckTimer = 0f;
            lastPosition = transform.position;
        }
    }

    void LookAtPlayer()
    {
        Vector3 direction = playerController.transform.position - transform.position;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * 8f
            );
        }
    }

    void HandleDeathDisappear()
    {
        waitToDisappear -= Time.deltaTime;

        if (waitToDisappear <= 0)
        {
            transform.localScale = Vector3.MoveTowards(
                transform.localScale,
                Vector3.zero,
                Time.deltaTime
            );

            if (transform.localScale.x <= 0.01f)
            {
                if (pointsHolder != null)
                {
                    Destroy(pointsHolder.gameObject);
                }

                Destroy(gameObject);
            }
        }
    }

    public void SetStartingPatrolIndex(int index)
    {
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            currentPatrolIndex = index % patrolPoints.Length;
        }
    }

    public void TakeDamage(float damageToTake)
    {
        if (isDead)
        {
            return;
        }

        currentHealth -= damageToTake;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (audioSource != null && zombiePainSound != null)
            {
                audioSource.PlayOneShot(zombiePainSound);
            }
        }
    }

    public void Die()
    {
        isDead = true;

        if (audioSource != null && zombieDeathSound != null)
        {
            audioSource.PlayOneShot(zombieDeathSound);
        }

        anim.SetBool("moving", false);
        anim.SetBool("attacking", false);
        anim.SetTrigger("die");

        WaveSpawner waveSpawner = Object.FindFirstObjectByType<WaveSpawner>();

        if (waveSpawner != null)
        {
            waveSpawner.EnemyKilled();
        }

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        Collider col = GetComponent<Collider>();

        if (col != null)
        {
            col.enabled = false;
        }
    }
}