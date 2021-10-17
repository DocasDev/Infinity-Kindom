using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamageable<float>, IKillable<float>
{
    private CharacterController controller;
    private Animator anim;
    private GameController _GameController;

    [Header("Config Player")]
    public float movimentSpeedWalk = 3f;
    public float movimentSpeedRun = 10f;
    public float animationSpeedWalk = 1f;
    public float animationSpeedRun = 2f;
    public float jumpForce = 8f;
    public float distanceGround = 1f;
    public Transform groundCheck;

    [Header("Config Attack")]
    public ParticleSystem fxAttack;
    public Transform hitBox;
    [Range(.2f, 2f)]
    public float hitRange;
    public LayerMask hitMask;

    [Header("Config Stats")]
    public float maxHP = 10f;
    public float maxMP = 6f;
    public float force = 1f;
    public float intelligence = 1f;
    public float fisicResistence = .1f;
    public float magicResistence = .1f;
    public float hpRestoreDelay = 5f;
    public float hpRestoreAmount = .5f;
    public float bSpeed = 0f;
    public float bForce = 0f;
    public float bIntelligence = 0f;

    //CurrentStats
    [SerializeField]
    private float currentHP;
    private float currentMP;

    private Vector3 moveDirection, jumpDirection;
    private bool inGround, inMoviment, isAttack;
    private LayerMask affectedLayers = -1;
    private float currentMovimentSpeed;
    private float currentAnimationSpeed;
    [SerializeField]
    private Collider[] hitInfo;

    // Start is called before the first frame update
    void Start()
    {
        _GameController = FindObjectOfType(typeof(GameController)) as GameController;
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        currentMovimentSpeed = movimentSpeedWalk;
        currentAnimationSpeed = animationSpeedWalk;
        affectedLayers = LayerMask.GetMask("Ground");
        ResetStats();

        StartCoroutine("RestoreHP");
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHPBar();
        if (_GameController.GetCurrentGameState() != GameState.GAMEPLAY)
        {
            return;
        }
        Inputs();
        Move();
        SetAnimations();
    }

    private void FixedUpdate()
    {
        CheckGrounded();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "TakeDamage")
        {
            GetHit(1f);
        }
    }

    #region Meus Métodos

    void ResetStats()
    {
        currentHP = maxHP;
        currentMP = maxMP;
    }

    float GetDamage()
    {
        return 1f;
    }

    void CheckGrounded()
    {
        inGround = Physics.CheckSphere(groundCheck.position, 0.3f, affectedLayers);
    }

    void Inputs()
    {
        if (!isAttack && Input.GetButtonDown("Fire1"))
        {
            Attack();
        }

        if (Input.GetButtonDown("Run") && inGround && !isAttack)
        {
            ToggleMovimentRun();
        }

        if (Input.GetButtonUp("Run") || isAttack || !inGround)
        {
            ToggleMovimentWalk();
        }

        if (inGround && Input.GetButtonDown("Jump"))
        {
            //Jump();
        }
    }

    void Attack()
    {
        isAttack = true;
        anim.SetTrigger("Atack");
        fxAttack.Emit(1);

        hitInfo = Physics.OverlapSphere(hitBox.position, hitRange, hitMask);

        foreach(Collider c in hitInfo)
        {
            c.gameObject.SendMessage("GetHit", GetDamage(), SendMessageOptions.DontRequireReceiver);
        }
    }

    void Jump()
    {
        jumpDirection.y = jumpForce;
    }

    void ToggleMovimentRun()
    {
        currentMovimentSpeed = movimentSpeedRun;
        currentAnimationSpeed = animationSpeedRun;
    }

    void ToggleMovimentWalk()
    {
        currentMovimentSpeed = movimentSpeedWalk;
        currentAnimationSpeed = animationSpeedWalk;
    }

    void Move()
    {
        //Prepare moviment state
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        moveDirection = new Vector3(horizontal, 0, vertical).normalized;

        if (/*inGround && */moveDirection.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, targetAngle, 0);
            inMoviment = true;
        }
        else
        {
            inMoviment = false;
        }

        //Apply moviment
        controller.Move(moveDirection * currentMovimentSpeed * Time.deltaTime);

        //Apply gravity
        /*if (inGround && jumpDirection.y < 0f)
        {
            jumpDirection.y = -2f;
        }

        jumpDirection.y += Physics.gravity.y * Time.deltaTime;
        controller.Move(jumpDirection * Time.deltaTime);*/
    }

    void SetAnimations()
    {
        anim.SetBool("isWalk", inMoviment);
        anim.speed = currentAnimationSpeed;
    }

    void AttackIsDone()
    {
        isAttack = false;
    }

    public void GetHit(float amount)
    {
        currentHP -= amount;
        if(currentHP <= 0)
        {
            _GameController.ChangeGameState(GameState.DIE);
            anim.SetTrigger("Die"); 
        }
        else
        {
            anim.SetTrigger("Hit");
        }
    }

    public void UpdateHPBar()
    {
        float percentHP = currentHP / maxHP;
        float decresPercent = _GameController.HPBarFilled.fillAmount - percentHP;
        _GameController.HPBarFilled.fillAmount -= decresPercent * Time.deltaTime;
        if(percentHP <= .5f)
        {
            _GameController.HPBarFilled.color = Color.red;
        }
        else
        {
            _GameController.HPBarFilled.color = Color.green;
        }
    }

    #endregion

    #region Corroutines

    IEnumerator RestoreHP()
    {
        yield return new WaitForSeconds(hpRestoreDelay);
        if(currentHP + hpRestoreAmount > maxHP)
        {
            currentHP = maxHP;
        }
        else if(currentHP < maxHP)
        {
            currentHP += hpRestoreAmount;
        }

        StartCoroutine("RestoreHP");
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        if(hitBox != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(hitBox.position, hitRange);
        }
    }
}
