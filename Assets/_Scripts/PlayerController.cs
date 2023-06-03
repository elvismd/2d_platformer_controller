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
    [SerializeField] private float walkSpeed = 14f;
    [SerializeField] private float runSpeed = 14f;
    [SerializeField] private float dashSpeed = 5f;
    [SerializeField] private float dashTime = 0.3f;
    [SerializeField] private float dashGraphicsTime = 0.15f;
    [SerializeField] private float dashCooldown = 0.5f;
    [SerializeField] private float accel = 6f;
    [SerializeField] private float airAccel = 3f;
    [SerializeField] private float jump = 14f;  //I could use the "speed" variable, but this is only coincidental in my case.  Replace line 89 if you think otherwise.
    [SerializeField] private float cachedDashTime = 1.5f;
    [SerializeField] private PlayerFace Facing = PlayerFace.RIGHT;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float _coyoteTime = 0.1f;
    [SerializeField] private float _jumpBufferTime = 0.1f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;

    [Header("Data Refs")]
    [SerializeField] private PlayerInputManager PlayerInputManager;
    [SerializeField] private Transform ModelRoot;
    [SerializeField] private Health health;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb2D;
    [SerializeField] private Collider2D CapsuleCollider;
    [SerializeField] private GameObject DashingCircle;
    [SerializeField] private TrailRenderer ElectricTrail;

    [Header("Status")]
    [SerializeField, ReadOnly] private bool dashing = false;

    private GroundState _groundState;
    private Vector2 _startingPos;
    private float _dashGraphicsElapsed;
    private float _dashingElapsed = -1;
    private float _dashCooldownElapsed;
    private float _cachedDashingElapsed = 0;
    private Vector2 _dashDir = Vector2.zero;
    private bool _prevGroundState;

    private float _currentGravityScale;

    private bool _doubleJump;

    private bool _canDash = false;
    private float _timeSinceLeftGround;
    private float _timeSinceJumpPressed;

    private Vector2 _initialScale;

    private bool _shouldJump;

    private void Awake()
    {
        _currentGravityScale = rb2D.gravityScale;
        _initialScale = transform.localScale;
    }

    void Start()
    {
        //Create an object to check if player is grounded or touching wall
        _groundState = new GroundState(transform.gameObject, CapsuleCollider, groundLayer, 0.25f);
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

        if (!_shouldJump)
            _shouldJump = PlayerInputManager.Jump;
    }

    void FixedUpdate()
    {
        bool grounded = _groundState.isGround();
        bool onWall = _groundState.isWall();
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
        if ((_shouldJump && (canJump || onWall)) || (_shouldJump && _doubleJump))
        {
            velocity.y = jump * (onWall ? 1f : 1);
            if (grounded)
            {
                _timeSinceLeftGround = 0f;
            }

            if (!grounded && !onWall && !canJump)
            {
                if (_doubleJump) animator.SetBool("double_jump", true);
                _doubleJump = false;
            }
        }

        if (!_prevGroundState && grounded)
        {
            animator.SetBool("double_jump", false);
            _doubleJump = true;
        }

        if (isTouching)
            animator.SetBool("double_jump", false);

        animator.SetFloat("vel_x", animatorVelX);
        animator.SetBool("ground", grounded);
        animator.SetBool("wall", onWall);

        UpdateFacingDirection();

        float acceleration = (grounded ? accel : airAccel);
        force.x = ((PlayerInputManager.MovementInput * targetSpeed) - rb2D.velocity.x) * acceleration;

        if (onWall && !_groundState.isGround() && _shouldJump)
        {
            velocity.x = -_groundState.wallDirection() * runSpeed * 0.75f;
        }

        if (!_canDash && _groundState.isTouching())
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

        if (rb2D.velocity.y < 0f)
        {
            velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb2D.velocity.y > 0f && !PlayerInputManager.JumpDown)
        {
            velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }

        rb2D.AddForce(force); //Move player.
        rb2D.velocity = velocity;

        _shouldJump = false;

        if (!_prevGroundState && grounded)
        {
            animator.SetBool("double_jump", false);
        }

        _prevGroundState = grounded;
    }

    void UpdateFacingDirection()
    {
        if (PlayerInputManager.MovementInput < 0) Facing = PlayerFace.LEFT;
        else if (PlayerInputManager.MovementInput > 0) Facing = PlayerFace.RIGHT;

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

        if (_groundState.isGround())
        {
            dir.y = 0.5f;
        }

        Vector2 impulseDirection = (dir * 25 + Vector2.up * 1.2f);
        rb2D.AddForce(impulseDirection, ForceMode2D.Impulse);
    }
}