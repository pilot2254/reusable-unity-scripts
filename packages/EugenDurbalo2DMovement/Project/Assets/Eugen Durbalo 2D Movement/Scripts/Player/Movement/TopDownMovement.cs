using UnityEngine;

public class TopDownMovement : MonoBehaviour
{
    private Rigidbody2D _rb;
    private PlayerInputsManager _inputs;

    private Transform _gfx;

    [Header("Movement Settings/Movement")]
    public float walkSpeed = 1f;
    public float runSpeed = 2f;
    [SerializeField] private float _groundDrag = 5f;
    [Header("Movement Settings/Other Variables")]
    public float speed;
    public bool canWalk = true;
    public bool run;

    private void Awake()
    {
        //if (GetComponent<TopDownMovement>()) {print("There is another movement script!!!"); this.enabled = false; }
        _rb = GetComponent<Rigidbody2D>();
        _inputs = GetComponent<PlayerInputsManager>();

        _rb.linearDamping = _groundDrag;
        _gfx = GetComponent<Player>().gfx;

        speed = walkSpeed;
    }

    private void Update()
    {
        SpeedControl();
    }

    private void FixedUpdate()
    {
        Movement();
    }

    private void Movement()
    {
        if (!canWalk) return;

        Vector2 direction = _inputs.moveInputValue;

        /*if (!run) _animator.SetFloat("MoveInputValue", direction.magnitude * 0.5f, _animSmoothTime, Time.deltaTime);
        else _animator.SetFloat("MoveInputValue", direction.magnitude, _animSmoothTime, Time.deltaTime);*/

        if (_inputs.lastMoveInputX > 0f) _gfx.GetComponent<SpriteRenderer>().flipX = false;
        else if (_inputs.lastMoveInputX < 0f) _gfx.GetComponent<SpriteRenderer>().flipX = true;

        if (direction.magnitude < 0.1f) return;

        Vector2 moveDir = direction;

        Debug.DrawRay(transform.position, moveDir.normalized * 10f, Color.green);
        _rb.AddForce(moveDir.normalized * speed * 100f, ForceMode2D.Force);
    }

    private void SpeedControl()
    {
        if (!canWalk) return;

        if (_rb.linearVelocity.magnitude > speed)
        {
            _rb.linearVelocity = _rb.linearVelocity.normalized * speed;
        }
    }
}
