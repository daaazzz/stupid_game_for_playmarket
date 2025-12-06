using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Animator))]
public class CharacterController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _attackDelay = 1f;

    [Header("Jump Force")]
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private float _coyoteTime = 0.15f;
    [SerializeField] private float _jumpBufferTime = 0.15f;

    [Header("Ground Check")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask _groundLayer;

    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rb;
    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _attackAction;
    private PlayerCombatSystem _playerCombatSystem;

    private Vector2 _moveInput = Vector2.zero;
    private bool _isGrounded = false;
    private bool _hasAttacked = false;
    private float _lastGroundedTime = -10f;
    private float _lastJumpPressedTime = -10f;
    private bool _canMove = true;
    private float _lastAttackTime = -10f;

    private bool _isFacingRight = true;

    private void Reset()
    {
        _rb = GetComponent<Rigidbody2D>();
        _playerInput = GetComponent<PlayerInput>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _playerCombatSystem = GetComponent<PlayerCombatSystem>();
    }

    private void Awake()
    {
        _rb = _rb ? _rb : GetComponent<Rigidbody2D>();
        _playerInput = _playerInput ? _playerInput : GetComponent<PlayerInput>();
        _animator = _animator ? _animator : GetComponent<Animator>();
        _spriteRenderer = _spriteRenderer ? _spriteRenderer : GetComponent<SpriteRenderer>();
        _playerCombatSystem = _playerCombatSystem ? _playerCombatSystem : GetComponent<PlayerCombatSystem>();

        if (_playerInput == null)
        {
            Debug.LogError("you stupid whigga");
        }
    }

    private void OnEnable()
    {
        if (_playerInput != null && _playerInput.actions != null)
        {
            _moveAction = _playerInput.actions["Move"];
            _jumpAction = _playerInput.actions["Jump"];
            _attackAction = _playerInput.actions["Attack"];

            if (_moveAction != null)
            {
                _moveAction.Enable();
            }

            if (_jumpAction != null)
            {
                _jumpAction.Enable();
                _jumpAction.performed += OnJumpPerformed;
            }

            if (_attackAction != null)
            {
                _attackAction.Enable();
                _attackAction.performed += OnAttackPerformed;
            }
        }
    }

    private void OnDisable()
    {
        if(_moveAction != null)
        {
            _moveAction.Disable();
        }
        if(_jumpAction != null)
        {
            _jumpAction.Disable();
            _jumpAction.performed -= OnJumpPerformed;
        }

        if(_attackAction != null)
        {
            _attackAction.Disable();
            _attackAction.performed -= OnAttackPerformed;
        }
    }

    private void Update()
    {
        if (_moveAction != null)
        {
            _moveInput = _moveAction.ReadValue<Vector2>();
        }
        else 
        {
            _moveInput = Vector2.zero;
        }

        _animator.SetBool("IsRunning", _moveInput.x != 0);
        _animator.SetFloat("YVelocity", _rb.linearVelocityY);
        
        if (_moveInput.x > 0)
        {
            _isFacingRight = true;
        }
        else if (_moveInput.x < 0)
        {
            _isFacingRight = false;
        }

        _spriteRenderer.flipX = !_isFacingRight;


        if (_groundCheck != null)
        {
            bool wasGrounded = _isGrounded;
            _isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);
            if (_isGrounded)
            {
                _lastGroundedTime = Time.time;
            }

            if (_hasAttacked && Time.time - _lastAttackTime > _attackDelay)
            {
                _playerCombatSystem.DeactivateSword();
                _hasAttacked = false;
                _canMove = true;
            }
        }
    }

    private void FixedUpdate()
    {
        if (_canMove)
        {


            Vector2 linearVelocity = _rb.linearVelocity;
            linearVelocity.x = _moveInput.x * _moveSpeed;
            _rb.linearVelocity = linearVelocity;

            bool canUseCoyote = (Time.time - _lastGroundedTime) <= _coyoteTime;
            bool hasBufferedjump = (Time.time - _lastJumpPressedTime) <= _jumpBufferTime;

            if (canUseCoyote && hasBufferedjump)
            {
                DoJump();
                _lastJumpPressedTime = -10f;
            }
        }
    }


    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Jump");
        _lastJumpPressedTime = Time.time;
    }

    private void DoJump()
    {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0);
        _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        _playerCombatSystem.GetDamage(_playerCombatSystem.Damage);
    }
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Attack");
        if (_isGrounded && !_hasAttacked)
        {
            _playerCombatSystem.ActivateSword();
            _hasAttacked = true;
            _canMove = false;
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
            _lastAttackTime = Time.time;
            _animator.SetTrigger("AttackTrigger");
        }
    }
}