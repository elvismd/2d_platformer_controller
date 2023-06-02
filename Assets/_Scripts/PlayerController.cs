using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public enum PlayerFace
    {
        RIGHT = 1,
        LEFT = -1
    }

    public static PlayerController instance;

    [Header("Movement Properties")]
    public float walkSpeed = 14f;
    public float runSpeed = 14f;
    public float dashSpeed = 5f;
    public float dashTime = 0.3f;
    public float dashGraphicsTime = 0.15f;
    public float dashCooldown = 0.5f;
    public float accel = 6f;
    public float airAccel = 3f;
    public float jump = 14f;  //I could use the "speed" variable, but this is only coincidental in my case.  Replace line 89 if you think otherwise.
    public float cachedDashTime = 1.5f;
    private GroundState groundState;
    [SerializeField]
    private PlayerFace Facing = PlayerFace.RIGHT;
    [SerializeField]
    private LayerMask groundLayer;
    [SerializeField]
    private float _coyoteTime = 0.1f;
    [SerializeField]
    private float _jumpBufferTime = 0.1f;

    [Header("Data Refs")]
    [SerializeField]
    private PlayerInputManager PlayerInputManager;
    [SerializeField]
    private Transform ModelRoot;
    [SerializeField]
    private GameObject DashingCircle;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private Rigidbody2D rb2D;
    [SerializeField]
    private Collider2D CapsuleCollider;
    [SerializeField]
    private TrailRenderer ElectricTrail;

    [Header("Status")]
    [SerializeField]
    private bool dashing = false;
    [SerializeField]
    private Health health;

    Vector2 _startingPos;
    float _dashGraphicsElapsed;
    float _dashingElapsed = -1;
    float _dashCooldownElapsed;
    float _cachedDashingElapsed = 0;
    Vector2 _dashDir = Vector2.zero;
    bool _prevGroundState;

    float _currentGravityScale;

    bool dashDown;
    bool doubleJump;

    float _timeSinceLeftGround;
    float _timeSinceJumpPressed;

    bool _canDash = false;
    Vector2 _initialScale;

    private void Awake()
    {
        _currentGravityScale = rb2D.gravityScale;
        _initialScale = transform.localScale;
    }

    void Start()
    {
        //Create an object to check if player is grounded or touching wall
        groundState = new GroundState(transform.gameObject, CapsuleCollider, groundLayer, 0.25f);
        instance = this;

        _startingPos = transform.position;
    }

    void Update()
    {
        animator.SetBool("dashing", dashing);

        ElectricTrail.emitting = dashing;
        ElectricTrail.gameObject.SetActive(dashing);

        if (PlayerInputManager.Jump)
        {
            _timeSinceJumpPressed = Time.time;
        }
    }

    void FixedUpdate()
    {
        bool grounded = groundState.isGround();
        bool onWall = groundState.isWall();
        bool isTouching = grounded || onWall;

        Vector2 force = new Vector2(0.0f, 0.0f);
        Vector2 velocity = new Vector2(0.0f, 0.0f);
        float targetSpeed = runSpeed;

        float animatorVelX = Mathf.Abs(PlayerInputManager.MovementInput);
        if (PlayerInputManager.Walk)
        {
            targetSpeed = walkSpeed;
            animatorVelX *= .5f;
        }

        if (_prevGroundState && !grounded)
        {
            _timeSinceLeftGround = Time.time;
        }

        bool canJump = (_timeSinceLeftGround + 0.1f > Time.time || (_timeSinceJumpPressed + 0.1f > Time.time && grounded));

        velocity.x = (PlayerInputManager.MovementInput == 0 && grounded) ? 0 : rb2D.velocity.x;
        velocity.y = rb2D.velocity.y;
        if( (PlayerInputManager.Jump && (canJump || onWall)) || (PlayerInputManager.Jump && doubleJump))
        {
            velocity.y = jump * (onWall ? 1f : 1);
            if(grounded)
            {
                _timeSinceLeftGround = 0f;
            }

            if(!grounded && !onWall && !canJump)
            {
                if(doubleJump) animator.SetBool("double_jump", true);
                doubleJump = false;
            }
        }

        if(!_prevGroundState && grounded)
        {
            animator.SetBool("double_jump", false);
            doubleJump = true;        
        }

        if(isTouching)
            animator.SetBool("double_jump", false);

        animator.SetFloat("vel_x", animatorVelX);
        animator.SetBool("ground", grounded);
        animator.SetBool("wall", onWall);

        UpdateFacingDirection();

        float acceleration = (grounded ? accel : airAccel);
        force.x = ((PlayerInputManager.MovementInput * targetSpeed) - rb2D.velocity.x) * acceleration;

        if (onWall && !groundState.isGround() && PlayerInputManager.Jump)
        {
            velocity.x = -groundState.wallDirection() * runSpeed * 0.75f;          
        }

        if (!_canDash && groundState.isTouching())
        {
            _canDash = true;
        }

        if (_dashingElapsed > Time.time)
        {
            velocity = _dashDir * dashSpeed;
            force = Vector2.zero;
            rb2D.gravityScale = 0;
        }
        else
        {
            if (onWall)
            {
                rb2D.gravityScale = _currentGravityScale * 0.5f;
                velocity.y *= 0.98f;
            }
            else
            {
                rb2D.gravityScale = _currentGravityScale;
            }

            if (dashing)
            {
                _dashCooldownElapsed = Time.time + dashCooldown;
                velocity *= 0.18f;
            }

            dashing = false;
        }

        if (PlayerInputManager.Dash && _canDash && _dashingElapsed < Time.time && (_dashCooldownElapsed < Time.time))
        {
            _dashDir = new Vector2((int)Facing, 0f);

            _dashingElapsed = Time.time + dashTime;
            _dashGraphicsElapsed = Time.time + dashGraphicsTime;

            DashingCircle.SetActive(true);
            ModelRoot.gameObject.SetActive(false);

            dashing = true;
            _canDash = false;

            animator.SetBool("double_jump", false);

            animator.SetTrigger("attack");
            //AudioMaster.instance.PlaySfx(AudioMaster.instance.dash);
        }

        if (_dashGraphicsElapsed < Time.time)
        {
            DashingCircle.SetActive(false);
            ModelRoot.gameObject.SetActive(true);
        }

        rb2D.AddForce(force); //Move player.
        rb2D.velocity = velocity;

        PlayerInputManager.Jump =false;
        PlayerInputManager.Dash = false;

        if (!_prevGroundState && grounded)
        {
            animator.SetBool("double_jump", false);
        }

        _prevGroundState = grounded;
    }

    void UpdateFacingDirection()
    {
        if (PlayerInputManager.MovementInput < 0)       Facing = PlayerFace.LEFT;
        else if (PlayerInputManager.MovementInput > 0)  Facing = PlayerFace.RIGHT;

        switch (Facing)
        {
            case PlayerFace.LEFT:
                ModelRoot.parent.localScale = new Vector3(-_initialScale.x, _initialScale.y, 1);
                break;
            case PlayerFace.RIGHT:
                ModelRoot.parent.localScale = new Vector3(_initialScale.x, _initialScale.y, 1);
                break;
        }    
    }

    public void AddImpulseFrom(Transform transformRef)
    {
        rb2D.velocity = Vector2.zero;
        Vector2 dir = (rb2D.position - (Vector2)transformRef.position).normalized;
        //Debug.Log(dir);

        if(groundState.isGround())
        {
            dir.y = 0.5f;   
        }

        Vector2 impulseDirection = (dir * 25 + Vector2.up * 1.2f);
        rb2D.AddForce(impulseDirection, ForceMode2D.Impulse);
    }
}