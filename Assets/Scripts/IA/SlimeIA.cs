using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class SlimeIA : MonoBehaviour, IDamageable<float>, IKillable<float>, IAttacker
{

    private Animator anim;
    private GameController _GameController;
    //private FieldOfView fieldOfView;

    [Header("Config Stats")]
    public float maxHP = 3f;
    public float maxMP = 6f;
    public float force = 1f;
    public float intelligence = 1f;
    public float fisicResistence = .1f;
    public float magicResistence = .1f;
    public float bSpeed = 0f;
    public float bForce = 0f;
    public float bIntelligence = 0f;
    public Transform HPBar;
    public Image HPBarFilled;

    [Header("Config State")]
    public EnemyState initialState;

    [Header("Config Respawn")]
    public float respawnTime = 10f;

    [Header("Config IA")]
    [Range(1f, 20f)]
    public float movimentRange = 12f;

    private float currentHP;
    private float currentMP;
    private EnemyState currentState;

    private NavMeshAgent agent;
    private NavMeshPath navMeshPath;
    private SkinnedMeshRenderer shader;
    private Vector3 destination;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool isWalk;
    private bool isAlert;
    private bool isAttack;
    private bool isDie;
    private Transform mainTarget;
    private float remainingDist;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        _GameController = FindObjectOfType(typeof(GameController)) as GameController;
        //fieldOfView = GetComponent<FieldOfView>();
        agent = GetComponent<NavMeshAgent>();
        navMeshPath = new NavMeshPath();
        shader = GetComponentInChildren<SkinnedMeshRenderer>();
        currentState = initialState;
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        WakeUp();
    }

    // Update is called once per frame
    void Update()
    {
        CheckIsWalk();
        UpdateHPBar();
        UpdateAnimations();
    }

    private void LateUpdate()
    {
        StateManager();
    }

    #region Meus Métodos

    public Vector3 GetInitialPosition()
    {
        return initialPosition;
    }

    public float GetMovimentRange()
    {
        return movimentRange;
    }

    bool IsValidDestination()
    {
        agent.CalculatePath(destination, navMeshPath);
        return navMeshPath.status == NavMeshPathStatus.PathComplete;
    }

    bool IsMinDistance(Vector2 ini, Vector2 end, float minDistance)
    {
        return Vector2.Distance(ini, end) >= minDistance;
    }

    bool IsOutRange()
    {
        return Vector3.Distance(initialPosition, transform.position) > (movimentRange + 1f);
    }

    bool IsTargetVisible()
    {
        return mainTarget != null;
    }

    void UpdateAnimations()
    {
        anim.SetBool("isWalk", isWalk);
        anim.SetBool("isAlert", isAlert);
    }

    void CheckIsWalk()
    {
        if (!isAttack && agent.desiredVelocity.magnitude >= 0.1f)
        {
            isWalk = true;
        }
        else
        {
            isWalk = false;
        }
    }

    void StateManager()
    {
        if (_GameController.GetCurrentGameState() == GameState.DIE && 
            (currentState == EnemyState.FOLLOW || currentState == EnemyState.FURY || currentState == EnemyState.ALERT))
        {
            ChangeState(EnemyState.RESETPOSITION);
            return;
        }

        //print(currentState);
        switch (currentState)
        {
            case EnemyState.IDLE:
                break;
            case EnemyState.ALERT:
                LookAt();
                break;
            case EnemyState.PATROL:
                break;
            case EnemyState.FOLLOW:
            case EnemyState.FURY:
                LookAt();
                destination = _GameController.player.position;
                agent.destination = destination;

                remainingDist = Vector3.Distance(transform.position, destination);

                if (IsOutRange())
                {
                    ChangeState(EnemyState.RESETPOSITION);
                }
                else if(remainingDist <= agent.stoppingDistance)
                {
                    Attack();
                }
                break;
            case EnemyState.EXPLORE:
                break;
            case EnemyState.RESETPOSITION:
                break;
            case EnemyState.DIE:
                break;
        }
    }

    void ChangeState(EnemyState newState)
    {
        StopAllCoroutines();
        isAlert = false;
        isAttack = false;

        switch (newState)
        {
            case EnemyState.IDLE:
                destination = transform.position;
                agent.stoppingDistance = 0f;
                agent.destination = destination;
                StartCoroutine("IDLE");
                break;
            case EnemyState.ALERT:
                destination = transform.position;
                agent.stoppingDistance = 0f;
                agent.destination = destination;
                isAlert = true;
                StartCoroutine("ALERT");
                break;
            case EnemyState.PATROL:
                SetRandomDestination();
                agent.stoppingDistance = 0f;
                agent.destination = destination;
                StartCoroutine("PATROL");
                break;
            case EnemyState.FURY:
                isAlert = true;
                destination = transform.position;
                agent.stoppingDistance = _GameController.slimeDistanceToAttack;
                agent.destination = destination;
                break;
            case EnemyState.FOLLOW:
                isAlert = true;
                destination = transform.position;
                agent.stoppingDistance = _GameController.slimeDistanceToAttack;
                agent.destination = destination;
                StartCoroutine("FOLLOW");
                break;
            case EnemyState.EXPLORE:
                break;
            case EnemyState.RESETPOSITION:
                destination = initialPosition;
                agent.stoppingDistance = 0f;
                agent.destination = destination;
                StartCoroutine("RESETPOSITION");
                break;
            case EnemyState.DIE:
                destination = transform.position;
                agent.stoppingDistance = 0f;
                agent.destination = destination;
                StartCoroutine("Die");
                break;
        }

        currentState = newState;
    }

    void Attack()
    {
        if (!isAttack && IsTargetVisible())
        {
            isAttack = true;
            anim.SetTrigger("Attack");
        }
    }

    void LookAt()
    {
        if (isAttack)
        {
            return;
        }
        Vector3 lookDirection = (_GameController.player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, _GameController.slimeLookAtSpeed * Time.deltaTime);
    }

    void SetRandomDestination()
    {
        Vector2 initialPoint;
        Vector2 randomDestination;
        
        do
        {
            initialPoint = new Vector2(initialPosition.x, initialPosition.z);
            randomDestination = initialPoint - (Random.insideUnitCircle.normalized * Random.Range(0f, movimentRange));
            destination = new Vector3(randomDestination.x, transform.position.y, randomDestination.y);
        } while (!IsValidDestination() || !IsMinDistance(new Vector2(transform.position.x, transform.position.z), randomDestination, (movimentRange / 2)));
    }

    void ResetStats()
    {
        currentHP = maxHP;
        currentMP = maxMP;
        isDie = false;
    }

    void WakeUp()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        ResetStats();
        SetIsTrigger(false);
        SetCutOffShader(0f);
        SetActiveHPBar(true);
        anim.Play("IdleNormal");
        ChangeState(initialState);
    }

    void SetCutOffShader(float value)
    {
        shader.material.SetFloat("_Cutoff", value);
    }

    void SetIsTrigger(bool value)
    {
        gameObject.GetComponent<CapsuleCollider>().isTrigger = value;
    }

    void SetActiveHPBar(bool active)
    {
        HPBar.gameObject.SetActive(active);
    }

    int Rand()
    {
        return Random.Range(0, 100);
    }

    void StayStill(int percentSuccessIdle)
    {
        if(Rand() < percentSuccessIdle)
        {
            ChangeState(EnemyState.IDLE);
        }
        else
        {
            ChangeState(EnemyState.PATROL);
        }
    }

    void AttackIsDone()
    {
        StartCoroutine("AttackDelay");
    }

    #endregion

    #region Interfaces Implemetations

    public void GetHit(float amount)
    {
        if (isDie)
        {
            return;
        }

        currentHP -= amount;
        if (currentHP <= 0f)
        {
            isDie = true;
            agent.destination = transform.position;
            anim.SetTrigger("Die");
            StopAllCoroutines();
            ChangeState(EnemyState.DIE);
        }
        else
        {
            ChangeState(EnemyState.FURY);
            anim.SetTrigger("GetHit");
        }
    }

    public void UpdateHPBar()
    {
        float percentHP = currentHP / maxHP;
        float decresPercent = HPBarFilled.fillAmount - percentHP;
        HPBarFilled.fillAmount -= decresPercent * Time.deltaTime;
    }

    public void TargetViwed(Transform target)
    {
        if(_GameController.GetCurrentGameState() != GameState.GAMEPLAY)
        {
            return;
        }

        switch (currentState)
        {
            case EnemyState.IDLE:
            case EnemyState.PATROL:
                mainTarget = target;
                ChangeState(EnemyState.ALERT);
                break;
            case EnemyState.FOLLOW:
            case EnemyState.FURY:
                mainTarget = target;
                if (currentState == EnemyState.FOLLOW)
                {
                    StopCoroutine("FOLLOW");
                    ChangeState(EnemyState.FOLLOW);
                }
                break;
        }
    }

    public void NoTargetViwed()
    {
        mainTarget = null;
    }

    #endregion

    #region Coroutines

    IEnumerator Die()
    {
        SetActiveHPBar(false);
        currentHP = maxHP;

        float cutoff = 0f;
        do
        {
            yield return new WaitForSeconds(.05f);
            cutoff += .01f;
            SetCutOffShader(cutoff);
        } while (cutoff < .7f);

        if (_GameController.Perc(_GameController.gemPercDrop))
        {
            Instantiate(_GameController.gemPrefab, transform.position, _GameController.gemPrefab.transform.rotation);
        }

        SetIsTrigger(true);
        StartCoroutine("Respawn");
    }

    IEnumerator AttackDelay()
    {
        yield return new WaitForSeconds(_GameController.slimeAttackDelay);
        isAttack = false;
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTime);
        WakeUp();
    }

    IEnumerator IDLE()
    {
        yield return new WaitForSeconds(_GameController.slimeIdleWaitTime);
        StayStill(5);
    }

    IEnumerator ALERT()
    {
        yield return new WaitForSeconds(_GameController.slimeAlertWaitTime);
        if(IsTargetVisible())
        {
            ChangeState(EnemyState.FOLLOW);
        }
        else
        {
            StayStill(5);
        }
    }

    IEnumerator PATROL()
    {
        yield return new WaitUntil(() => (agent.remainingDistance <= 0 || IsOutRange()));
        if (IsOutRange())
        {
            ChangeState(EnemyState.RESETPOSITION);
        }
        else
        {
            StayStill(5);
        }
    }

    IEnumerator FOLLOW()
    {
        yield return new WaitUntil(() => !IsTargetVisible());
        yield return new WaitForSeconds(_GameController.slimeAlertWaitTime);
        StayStill(5);
    }

    IEnumerator RESETPOSITION()
    {
        yield return new WaitUntil(() => (agent.remainingDistance <= 0));
        transform.rotation = initialRotation;
        ResetStats();
        ChangeState(EnemyState.IDLE);
    }

    #endregion
}
