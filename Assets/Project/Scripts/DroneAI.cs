using UnityEngine;
using UnityEngine.AI;

public class DroneAI : MonoBehaviour
{
    private enum State { Patrolling, Chasing, Hovering, Retreating }

    private State currentState;
    private NavMeshAgent agent;
    private AudioSource audioSource;
    private Transform player;
    private PlayerController playerController;

    [Header("Attack / Timers")]
    [Tooltip("Time between autonomous starts of chase when not triggered by proximity")]
    public float timedAttackInterval = 10f;
    public float proximityAttackDistance = 25f;

    [Header("Movement Parameters")]
    [Tooltip("Distance to enter hover state")]
    public float hoverEnterDistance = 3f;
    [Tooltip("Distance to exit hover (hysteresis)")]
    public float hoverExitDistance = 4f;
    public float hoverAfterCrouchDuration = 3f;
    public float retreatDistance = 40f;
    public float walkPointRange = 50f;

    [Header("Audio & Penalty")]
    public float maxHearingDistance = 50f;
    public float penaltyDistance = 15f;
    public int scorePenaltyPerSecond = 5;

    private float timedAttackTimer = 0f;
    private float hoverTimer = 0f;
    private float penaltyTimer = 0f;
    

    private AudioLowPassFilter lowPassFilter;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        lowPassFilter = GetComponent<AudioLowPassFilter>();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerController = playerObject.GetComponent<PlayerController>();
        }
    }

    private void Start()
    {
        currentState = State.Patrolling;
        if (agent != null) agent.updateRotation = true;
    }

    private void Update()
    {
        if (player == null || playerController == null) return;

        switch (currentState)
        {
            case State.Patrolling: Patrol(); break;
            case State.Chasing: Chase(); break;
            case State.Hovering: Hover(); break;
            case State.Retreating: Retreat(); break;
        }

        HandleAudioAndPenalty();
    }

    private void Patrol()
    {
        if (agent == null) return;
        agent.isStopped = false;
        agent.updateRotation = true;
        timedAttackTimer += Time.deltaTime;

        if (!agent.hasPath || agent.remainingDistance < 1.5f)
        {
            SearchWalkPoint(transform.position, walkPointRange);
        }

        if (timedAttackTimer >= timedAttackInterval ||
            Vector3.Distance(transform.position, player.position) < proximityAttackDistance)
        {
            currentState = State.Chasing;
        }
    }

    private void Chase()
    {
        if (agent == null) return;

        agent.isStopped = false;
        agent.updateRotation = true;
        agent.stoppingDistance = hoverEnterDistance * 0.9f;
        agent.SetDestination(player.position);

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist < hoverEnterDistance)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            agent.updateRotation = false;
            currentState = State.Hovering;
            hoverTimer = 0f;
        }
    }

    private void Hover()
    {
        if (player != null)
        {
            Vector3 lookTarget = player.position;
            lookTarget.y = transform.position.y;
            Vector3 toTarget = (lookTarget - transform.position);
            if (toTarget.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 8f * Time.deltaTime);
            }
        }

        if (playerController.isCrouching)
        {
            hoverTimer += Time.deltaTime;
            if (hoverTimer >= hoverAfterCrouchDuration)
            {
                agent.isStopped = false;
                agent.updateRotation = true;
                agent.stoppingDistance = 0f;
                currentState = State.Retreating;
                hoverTimer = 0f;
            }
        }
        else
        {
            hoverTimer = 0f;
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist > hoverExitDistance)
            {
                agent.isStopped = false;
                agent.updateRotation = true;
                currentState = State.Chasing;
            }
            else
            {
                agent.isStopped = false;
                agent.updateRotation = true;
                currentState = State.Chasing;
            }
        }
    }

    private void Retreat()
    {
        if (agent == null) return;

        agent.isStopped = false;
        agent.updateRotation = true;

        float currentDistance = Vector3.Distance(transform.position, player.position);
        if (currentDistance > retreatDistance)
        {
            currentState = State.Patrolling;
            timedAttackTimer = 0f;
            return;
        }

        if (!agent.hasPath || agent.remainingDistance < 1.5f)
        {
            Vector3 directionFromPlayer = (transform.position - player.position).normalized;
            if (directionFromPlayer.sqrMagnitude < 0.001f)
            {
                directionFromPlayer = Random.onUnitSphere; directionFromPlayer.y = 0f; directionFromPlayer.Normalize();
            }
            Vector3 idealRetreatPoint = transform.position + directionFromPlayer * 10f;
            SearchWalkPoint(idealRetreatPoint, 8f);
        }
    }

    private void HandleAudioAndPenalty()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        // --- MODIFIED SECTION ---

        float multiplier = 1f;
        float cutoff = 22000f; // Default full-spectrum sound

        if (GameSettings.Instance != null)
        {
            multiplier = GameSettings.Instance.NoiseMultiplier;
            cutoff = GameSettings.Instance.NoiseCutoffFrequency; // Get cutoff from settings
        }

        if (audioSource != null)
        {
            // Apply the volume multiplier
            audioSource.volume = (1.0f - Mathf.Clamp01(dist / maxHearingDistance)) * multiplier;
        }

        if (lowPassFilter != null)
        {
            // Apply the filter cutoff
            lowPassFilter.cutoffFrequency = cutoff;
        }

        // --- NEW/MODIFIED PENALTY CHECK ---
        bool isUnderPenalty = (dist < penaltyDistance);

        if (PenaltyUI.Instance != null)
        {
            PenaltyUI.Instance.SetPenaltyState(isUnderPenalty);
        }

        if (isUnderPenalty)
        {
            penaltyTimer += Time.deltaTime;
            if (penaltyTimer >= 1f)
            {
                if (ResourceManager.Instance != null) ResourceManager.Instance.RemoveScore(scorePenaltyPerSecond);
                penaltyTimer -= 1f;
            }
        }
        else
        {
            penaltyTimer = 0f;
        }
        // ----------------------------------



        if (dist < penaltyDistance)
        {
            penaltyTimer += Time.deltaTime;
            if (penaltyTimer >= 1f)
            {
                if (ResourceManager.Instance != null) ResourceManager.Instance.RemoveScore(scorePenaltyPerSecond);
                penaltyTimer -= 1f;
            }
        }
        else
        {
            penaltyTimer = 0f;
        }
    }

    private void SearchWalkPoint(Vector3 aroundPoint, float range)
    {
        Vector3 randomPoint = aroundPoint + Random.insideUnitSphere * range;
        randomPoint.y = aroundPoint.y;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, range + 1f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            Vector3 backup = transform.position + Random.insideUnitSphere * range;
            if (NavMesh.SamplePosition(backup, out hit, range, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }
}
