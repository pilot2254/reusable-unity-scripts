using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Dash : MonoBehaviour
{
    private Player _playerScript;
    private PlayerInputsManager _playerInputsManager;
    private Rigidbody2D _rb;

    [Header("Dash Settings")]
    [SerializeField] private float _distance = 5f;
    [SerializeField] private float _dashSpeed = 20f;
    [SerializeField] private float _maxDashTime = 0.3f;
    [SerializeField] private float _dashCooldown = 1f;
    private float _curDashCooldown;
    [Header("Visual Settings")]
    [SerializeField] private Slider _dashCooldownSlider;
    private Transform _gfx;
    [Header("Trail Settings")]
    [SerializeField] private GameObject _playerTrailPrefab;
    [SerializeField] private Sprite _trailSprite;
    [SerializeField] private float _trailLifeTime;

    private void Awake()
    {
        _playerScript = GetComponent<Player>();
        _playerInputsManager = GetComponent<PlayerInputsManager>();
        _rb = GetComponent<Rigidbody2D>();

        _gfx = GetComponent<Player>().gfx;
    }

    private void Update()
    {
        _curDashCooldown = Mathf.Clamp(_curDashCooldown + Time.deltaTime, 0f, _dashCooldown);
        if(_dashCooldownSlider != null) _dashCooldownSlider.value = _curDashCooldown;
    }

    public void DashAction()
    {
        if(_playerScript.playerMovementType == PlayerMovementType.Platformer)
        {
            if (!GetComponent<PlatformerMovement>().canWalk) return;
            if (_curDashCooldown < _dashCooldown) return;

            GetComponent<PlatformerMovement>().canWalk = false;
            StartCoroutine(PlatformerDashCoro());
        }
        else if (_playerScript.playerMovementType == PlayerMovementType.Topdown)
        {
            if (!GetComponent<TopDownMovement>().canWalk) return;
            if (_curDashCooldown < _dashCooldown) return;

            GetComponent<TopDownMovement>().canWalk = false;
            StartCoroutine(TopDownDashCoro());
        }
    }

    private IEnumerator PlatformerDashCoro()
    {
        Vector2 moveInput = _playerInputsManager.lastMoveInputXY.normalized;
        Vector2 startPosition = transform.position;

        float dashTime = 0f;

        Vector2 lastTrailSpawnTransform = transform.position;

        _gfx.transform.up = _playerInputsManager.lastMoveInputXY.normalized;

        while (Vector2.Distance(transform.position, startPosition) < _distance)
        {
            //GetComponent<Animator>().SetBool("Dashing", true);
            dashTime += Time.deltaTime;
            _rb.linearVelocity = moveInput * _dashSpeed;

            yield return new WaitForSeconds(Time.deltaTime);

            if (_trailLifeTime != 0f) { if (Vector2.Distance(transform.position, lastTrailSpawnTransform) >= _distance / 4) { StartCoroutine("SpawnTrail"); lastTrailSpawnTransform = transform.position; } }
            if (dashTime >= _maxDashTime) break;
        }

        _rb.linearVelocityY = 0f;
        GetComponent<PlatformerMovement>().canWalk = true;
        //GetComponent<Animator>().SetBool("Dashing", false);
        _gfx.transform.up = Vector3.up;
        _curDashCooldown = 0f;
    }

    private IEnumerator TopDownDashCoro()
    {
        Vector2 moveInput = _playerInputsManager.lastMoveInputXY.normalized;
        Vector2 startPosition = transform.position;

        float dashTime = 0f;

        Vector2 lastTrailSpawnTransform = transform.position;

        while (Vector2.Distance(transform.position, startPosition) < _distance)
        {
            //GetComponent<Animator>().SetBool("Dashing", true);
            dashTime += Time.deltaTime;
            _rb.linearVelocity = moveInput * _dashSpeed;

            yield return new WaitForSeconds(Time.deltaTime);

            if (_trailLifeTime != 0f) { if (Vector2.Distance(transform.position, lastTrailSpawnTransform) >= _distance / 4) { StartCoroutine("SpawnTrail"); lastTrailSpawnTransform = transform.position; } }
            if (dashTime >= _maxDashTime) break;
        }

        _rb.linearVelocityY = 0f;
        GetComponent<TopDownMovement>().canWalk = true;
        //GetComponent<Animator>().SetBool("Dashing", false);
        _curDashCooldown = 0f;
    }

    private IEnumerator SpawnTrail()
    {
        GameObject trail = Instantiate(_playerTrailPrefab, transform.position, _gfx.transform.rotation);
        if(_trailSprite != null) trail.GetComponent<SpriteRenderer>().sprite = _trailSprite;
        else trail.GetComponent<SpriteRenderer>().sprite = _gfx.GetComponent<SpriteRenderer>().sprite;
        yield return new WaitForSeconds(_trailLifeTime);
        Destroy(trail);
    }
}
