using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using UnityEngine.Windows;

public class PlatformerMovement : MonoBehaviour
{
    private Rigidbody2D _rb;
    private PlayerInputsManager _inputs;

    private Transform _gfx;

    [Header("Movement Settings/Ground Checking")]
    [SerializeField] private LayerMask _ground;
    [SerializeField] private float _halfPlayersHeight = 1f;
    [Header("Movement Settings/Movement")]
    public float walkSpeed = 1f;
    public float runSpeed = 2f;
    [SerializeField] private float _airMultiplier = 0.4f;
    [SerializeField] private float _groundDrag = 5f;
    [Header("Movement/Slopes")]
    [SerializeField] private float _maxSlopeAngle = 40f;
    private RaycastHit2D _slopeHit;
    [Header("Movement Settings/Other Variables")]
    public float speed;
    public bool canWalk = true;
    public bool slopesSpeedControl = true;
    public bool run;
    public bool isGrounded;

    private void Awake()
    {
        //if (GetComponent<TopDownMovement>()) {print("There is another movement script!!!"); this.enabled = false; }
        _rb = GetComponent<Rigidbody2D>();
        _inputs = GetComponent<PlayerInputsManager>();

        _gfx = GetComponent<Player>().gfx;

        speed = walkSpeed;
    }

    private void Update()
    {
        SpeedControl();
        Gravitation();
    }

    private void FixedUpdate()
    {
        Movement();
    }

    private void Movement()
    {
        if (!canWalk) return;

        float direction = _inputs.moveInputValue.x;

        /*if (!run) _animator.SetFloat("MoveInputValue", direction.magnitude * 0.5f, _animSmoothTime, Time.deltaTime);
        else _animator.SetFloat("MoveInputValue", direction.magnitude, _animSmoothTime, Time.deltaTime);*/

        if(_inputs.lastMoveInputX > 0f) _gfx.GetComponent<SpriteRenderer>().flipX = false;
        else if(_inputs.lastMoveInputX < 0f) _gfx.GetComponent<SpriteRenderer>().flipX = true;

        if (Mathf.Abs(direction) < 0.1f) return;

        Vector2 moveDir = new Vector2(1f, 0f) * direction;

        if (OnSlope())
        {
            moveDir = GetSlopeMoveDirection(moveDir);
            moveDir *= 8f;
        }

        Debug.DrawRay(transform.position, moveDir.normalized * 10f, Color.green);
        if (isGrounded) _rb.AddForce(moveDir.normalized * speed * 100f, ForceMode2D.Force);
        else _rb.AddForce(moveDir.normalized * speed * 100f * _airMultiplier, ForceMode2D.Force);
    }

    private void Gravitation()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector3.down, _halfPlayersHeight + 0.1f, _ground);

        //_animator.SetBool("Falling", !_isGrounded);

        if (!canWalk) return;

        if (isGrounded) _rb.linearDamping = _groundDrag;
        else _rb.linearDamping = 0f;
    }

    private bool OnSlope()
    {
        _slopeHit = Physics2D.Raycast(transform.position, Vector2.down, _halfPlayersHeight + 0.1f, _ground);

        if (_slopeHit != null)
        {
            float angle = Vector2.Angle(Vector2.up, _slopeHit.normal);
            return angle < _maxSlopeAngle && angle != 0f;
        }

        return false;
    }

    private Vector2 GetSlopeMoveDirection(Vector2 normal)
    {
        Vector3 dir = Vector3.ProjectOnPlane(normal, _slopeHit.normal).normalized;

        return new Vector2(dir.x, dir.y);
    }

    private void SpeedControl()
    {
        if (!canWalk) return;

        if (OnSlope())
        {
            if(!slopesSpeedControl) return;

            if (_rb.linearVelocity.magnitude > speed)
            {
                _rb.linearVelocity = _rb.linearVelocity.normalized * speed;
            }
        }
        else
        {
            float flatVel = _rb.linearVelocity.x;

            if (Mathf.Abs(flatVel) > speed)
            {
                float limitedVelocity = _rb.linearVelocity.normalized.x * speed;
                _rb.linearVelocity = new Vector2(limitedVelocity, _rb.linearVelocity.y);
            }
        }
    }

    private void useGravity(bool use)
    {
        float gravityScale = _rb.gravityScale;

        if (use) _rb.gravityScale = 1f;
        else _rb.gravityScale = 0f;

        if(_rb.gravityScale != gravityScale) _rb.linearVelocity = Vector2.zero;
    }
}
