using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputsManager : MonoBehaviour
{
    private PlayerInputs _inputs;
    private InputAction _moveAction;

    public Vector2 moveInputValue;
    public Vector2 lastMoveInputXY;
    public float lastMoveInputX;

    private void OnEnable()
    {
        _inputs.Enable();
    }

    private void OnDisable()
    {
        _inputs.Disable();
    }

    private void Awake()
    {
        _inputs = new PlayerInputs();

        _moveAction = _inputs.Movement.Walking;
    }

    private void Start()
    {
        _inputs.Movement.Jump.started += Jump;

        _inputs.Movement.Dash.started += Dash;

        _inputs.Movement.Run.started += Run;

        //_inputs.Movement.Crouch.started += Crouch;
    }

    private void Update()
    {
        moveInputValue = _moveAction.ReadValue<Vector2>();

        if(moveInputValue.magnitude > 0f) lastMoveInputXY = moveInputValue;
        if(Mathf.Abs(moveInputValue.x) > 0f) lastMoveInputX = moveInputValue.x;
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (!GetComponent<Jump>()) return;

        GetComponent<Jump>().JumpAction();
    }

    private void Dash(InputAction.CallbackContext context)
    {
        if (!GetComponent<Dash>()) return;

        GetComponent<Dash>().DashAction();
    }

    private void Run(InputAction.CallbackContext context)
    {
        if (!GetComponent<Run>()) return;

        GetComponent<Run>().RunAction();
    }

    /*private void Crouch(InputAction.CallbackContext context)
    {

    }*/
}
