using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using System.Linq;

namespace PinPon
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(Animator))]
    public class PinPonPlayerController : MonoBehaviour
    {
        [SerializeField] private ScriptableStats _stats;
        private Rigidbody2D _rb;
        private CapsuleCollider2D _col;
        private Animator _anim;
        private Vector2 _frameVelocity;
        private bool _cachedQueryStartInColliders;
        private bool _isFacingLeft;
        private int _playerLayer;
        private int _platformLayer;
        
        [Header("Hitting")]
        [SerializeField] private GameObject _racquetObject;
        [SerializeField] private float _hitCooldown;
        [SerializeField] private float _hitZoneTime;
        [SerializeField] private GameObject _hitZoneObject;
        private float _timeLastHit = float.MinValue;
        private Vector3 _racquetOriginalLocalPos;
        private Vector3 _racquetOriginalLocalScale;
        private Vector3 _hitZoneOriginalLocalPos;
        private Vector3 _hitZoneOriginalLocalScale;

        [Header("Bomb")]
        public GameObject aimLine;
        [Tooltip("Quantos pontos a linha de trajetória terá.")]
        [SerializeField] private int _trajectoryPointCount = 20;
        [Tooltip("O intervalo de tempo entre os pontos da trajetória. Afeta o comprimento da previsão.")]
        [SerializeField] private float _trajectoryTimeStep = 0.1f;
        [Tooltip("A densidade dos pontos na linha. Aumente para mais pontos no mesmo espaço.")]
        [SerializeField] private float _dotDensity = 1f;
        private bool _isBombHeld = false;
        private GameObject bombToLaunch;
        private List<BombController> _collectibleBombs = new List<BombController>();
        #region Interface

        private Vector2 _moveInput;
        private bool _jumpHeldInput;
        private bool _fallDownInput;
        public int PlayerIndex { get; private set; }
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;
        public iceberg assignedIceberg;
        public bool _movementBlocked = false;
        #endregion

        private float _time;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<CapsuleCollider2D>();
            _anim = GetComponent<Animator>();
            PlayerIndex = GetComponent<PlayerInput>().playerIndex;
            // trailRenderer = GetComponent<TrailRenderer>();
            trailRenderer.emitting = false;
            _playerLayer = gameObject.layer;
            _platformLayer = LayerMask.NameToLayer("tile");

            _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;

            if (_racquetObject != null)
            {
                _racquetObject.SetActive(false);
                _racquetOriginalLocalPos = _racquetObject.transform.localPosition;
                _racquetOriginalLocalScale = _racquetObject.transform.localScale;
            }

            if (_hitZoneObject != null)
            {
                _hitZoneObject.SetActive(false);
                _hitZoneOriginalLocalPos = _hitZoneObject.transform.localPosition;
                _hitZoneOriginalLocalScale = _hitZoneObject.transform.localScale;
            }
        }

        private void Update()
        {
            _time += Time.deltaTime;

            // if (_fallDownInput)
            // {
            //     if (_grounded) HandleFallThrough();
            //     _fallDownInput = false;
            // }

            UpdateAnimator();
        }

        #region Métodos de Callback de Input
        public void OnMove(InputAction.CallbackContext context)
        {
            if (_movementBlocked) return;
            _moveInput = context.ReadValue<Vector2>();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                if (_moveInput.y <= -0.8f)
                {
                    _fallDownInput = true;
                    return;
                }
                
                _jumpToConsume = true;
                _timeJumpWasPressed = _time;
            }

            if (context.canceled)
            {
                if (_rb.velocity.y > 0) _endedJumpEarly = true;
            }

            _jumpHeldInput = context.ReadValueAsButton();
        }

        public void OnHit(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                if (_isBombHeld)
                {
                    LaunchBomb();
                }

                if (_collectibleBombs.Any())
                    {
                        BombController bombToPickUp = _collectibleBombs.FirstOrDefault();
                        if (bombToPickUp != null && !bombToPickUp.isThrown)
                        {
                            PickUpBomb(bombToPickUp);
                            return;
                        }
                    }
        
                else
                {
                    TryHit();
                }
            }
            // if (context.started)
            // {
            //     if (_isBombPickable && !_isBombHeld)
            //     {
            //     Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1f, LayerMask.GetMask("bomb"));
            //     foreach (var hit in hits)
            //     {
            //         if (hit.CompareTag("Bomb"))
            //         {
            //             PickUpBomb(hit.gameObject);
            //             break;
            //         }
            //     }
            //     }
            //     else
            //     {
            //         TryHit();
            //     }
                
            // }
        }

        public void OnFallDown(InputAction.CallbackContext context)
        {
            if (context.started)
            {

                _fallDownInput = true;
                _fallToConsume = true;
                _timeFallWasPressed = _time;

            }
        }

        public void OnDash(InputAction.CallbackContext context)
        {
            if (_movementBlocked) return;
            
            if (context.started)
            {
                Debug.Log("Botão de Dash pressionado!");
                HandleDash();
            }
        }
        #endregion
        public void BlockMovement()
        {
            if (_movementBlocked) 
            return;

            _frameVelocity = Vector2.zero;
            _moveInput = Vector2.zero;
            _jumpHeldInput = false;
            _fallDownInput = false;
            _rb.gravityScale = 0;
            _rb.velocity = Vector2.zero;
            _rb.isKinematic = true;

            _movementBlocked = true;
        }
        public void UnblockMovement()
        {
            if (_movementBlocked)
                {
                    _movementBlocked = false;
                    
                    _rb.isKinematic = false;
                    _rb.gravityScale = 1;
                }
        }

        private void FixedUpdate()
        {
            CheckCollisions();

            if (_movementBlocked)
                return;

            HandleJump();
            HandleFallThrough();
            HandleDirection();
            HandleGravity();
            
            ApplyMovement();
        }
        
        private void UpdateAnimator()
        {
            _anim.SetBool("isJumping", !_grounded);
            _anim.SetFloat("HorizontalSpeed", _moveInput.x);
            _anim.SetFloat("VerticalSpeed", _rb.velocity.y);
            _anim.SetBool("isMoving", _moveInput.x != 0);
            _anim.SetBool("isFacingLeft", _isFacingLeft);
        }

        #region Collisions
        
        private float _frameLeftGrounded = float.MinValue;
        private bool _grounded;

        private void CheckCollisions()
        {
            Physics2D.queriesStartInColliders = false;


            // Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1f, LayerMask.GetMask("bomb"));
            
            // if (hits.Length > 0)
            // {
            //     foreach (var hit in hits)
            //     {
            //         if (hit.CompareTag("Bomb"))
            //         {
            //             hit.gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 0.5f, 0.5f);
            //         }
            //     }
            // }
            // else
            // {
                
            // }

            bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, _stats.GrounderDistance, ~_stats.PlayerLayer);
            // bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, _stats.GrounderDistance, ceilingCheckMask);

            // if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

            if (!_grounded && groundHit)
            {
                _grounded = true;
                _coyoteUsable = true;
                _bufferedJumpUsable = true;
                _bufferedFallUsable = true;
                _endedJumpEarly = false;
                GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));

                Physics2D.IgnoreLayerCollision(_playerLayer, _platformLayer, false);
            }
            else if (_grounded && !groundHit)
            {
                _grounded = false;
                _frameLeftGrounded = _time;
                GroundedChanged?.Invoke(false, 0);

                Physics2D.IgnoreLayerCollision(_playerLayer, _platformLayer, true);
            }

            Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
        }
        
        #endregion
        
        #region Dash
        private bool _isDashing;
        private float _timeLastDash;
        private float _dashCooldown = 1f;
        private float _dashDistance = 2.5f;
        private float _dashSpeed = 15f;
        public TrailRenderer trailRenderer;

        private void HandleDash()
        {
            if (_isDashing || (_dashCooldown > 0 && Time.time < _timeLastDash + _dashCooldown)) return;

            StartCoroutine(DashCoroutine());
        }

        private IEnumerator DashCoroutine()
        {
            SoundManager.Instance.PlaySFX("Jump");

            trailRenderer.emitting = true;

            Debug.Log("Iniciando Dash!");

            _timeLastDash = Time.time;
            _isDashing = true;

            Vector2 dashDirection = _isFacingLeft ? Vector2.left : Vector2.right;
            _frameVelocity += dashDirection * _dashSpeed;

            float dashTime = _dashDistance / _dashSpeed;
            yield return new WaitForSeconds(dashTime);

            yield return new WaitUntil(() => _grounded); 

            _isDashing = false;
            trailRenderer.emitting = false;

            Debug.Log("Dash concluído!");
        }

        #endregion
        #region Fall Through
        private bool _fallToConsume;
        private bool _bufferedFallUsable;
        private float _timeFallWasPressed;
        private bool HasBufferedFall => _bufferedFallUsable && _time < _timeFallWasPressed + _stats.FallBuffer;
        private void HandleFallThrough()
        {
            if (!_fallToConsume && !HasBufferedFall) return;

            if (_grounded || CanUseCoyote) ExecuteFall();

            _fallToConsume = false;

        }

        private void ExecuteFall()
        {
            _timeFallWasPressed = 0;
            _bufferedFallUsable = false;
            _coyoteUsable = false;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1f, ~_stats.PlayerLayer);

            if (hit.collider != null)
            {
                var platform = hit.collider.GetComponent<PlatformController>();
                if (platform != null)
                {
                    platform.Drop();
                }
            }
        }

        #endregion


        #region Jumping

        private bool _jumpToConsume;
        private bool _bufferedJumpUsable;
        private bool _endedJumpEarly;
        private bool _coyoteUsable;
        private float _timeJumpWasPressed;

        private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
        private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;

        private void HandleJump()
        {
            if (!_endedJumpEarly && !_grounded && !_jumpHeldInput && _rb.velocity.y > 0) _endedJumpEarly = true;

            if (!_jumpToConsume && !HasBufferedJump) return;

            if (_grounded || CanUseCoyote) ExecuteJump();

            _jumpToConsume = false;
        }

        private void ExecuteJump()
        {
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;
            _frameVelocity.y = _stats.JumpPower;
            
            SoundManager.Instance.PlaySFX("Jump");
            Jumped?.Invoke();
        }

        #endregion

        #region Horizontal

        private void HandleDirection()
        {
            if (_moveInput.x < 0)
            {
                _isFacingLeft = true;
            }
            else if (_moveInput.x > 0)
            {
                _isFacingLeft = false;
            }
            
            if (_moveInput.x == 0f)
            {
                var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
            }
            else
            {
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _moveInput.x * _stats.MaxSpeed, _stats.Acceleration * Time.fixedDeltaTime);
            }
        }

        #endregion

        #region Gravity

        private void HandleGravity()
        {
            if (_grounded && _frameVelocity.y <= 0f)
            {
                _frameVelocity.y = _stats.GroundingForce;
            }
            else
            {
                var inAirGravity = _stats.FallAcceleration;
                if (_endedJumpEarly && _frameVelocity.y > 0) inAirGravity *= _stats.JumpEndEarlyGravityModifier;
                _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
            }
        }

        #endregion
        
        #region Hitting

        private bool _isHitting;

        public void TryHit()
        {
            // if (_isBombPickable) return;

            // if (_isBombHeld)
            // {
            //     LaunchBomb(bombToLaunch);
            // }
            // Don't hit if we're already hitting, or if the hit is on cooldown
            if (_isHitting || Time.time < _timeLastHit + _hitCooldown) return;

            // It's possible the objects aren't assigned in the inspector
            if (_racquetObject == null || _hitZoneObject == null) return;

            StartCoroutine(HitCoroutine());
        }

        private IEnumerator HitCoroutine()
        {
            _isHitting = true;
            _timeLastHit = Time.time;

            float facingMultiplier = _isFacingLeft ? -1f : 1f;

            // Position and scale the visual racquet
            _racquetObject.transform.localPosition = new Vector3(Mathf.Abs(_racquetOriginalLocalPos.x) * facingMultiplier, _racquetOriginalLocalPos.y, _racquetOriginalLocalPos.z);
            _racquetObject.transform.localScale = new Vector3(Mathf.Abs(_racquetOriginalLocalScale.x) * facingMultiplier, _racquetOriginalLocalScale.y, _racquetOriginalLocalScale.z);

            // Position and scale the hitbox
            _hitZoneObject.transform.localPosition = new Vector3(Mathf.Abs(_hitZoneOriginalLocalPos.x) * facingMultiplier, _hitZoneOriginalLocalPos.y, _hitZoneOriginalLocalPos.z);
            _hitZoneObject.transform.localScale = new Vector3(Mathf.Abs(_hitZoneOriginalLocalScale.x) * facingMultiplier, _hitZoneOriginalLocalScale.y, _hitZoneOriginalLocalScale.z);
            
            // Activate both objects
            _racquetObject.SetActive(true);
            _hitZoneObject.SetActive(true);

            // Wait for the duration of the hit
            yield return new WaitForSeconds(_hitZoneTime);

            // Deactivate both objects
            _racquetObject.SetActive(false);
            _hitZoneObject.SetActive(false);
            _isHitting = false;
        }
        
        public void ApplyKnockback(Vector2 direction, float force)
        {
            Debug.Log($"Applying knockback with direction {direction} and force {force}");
            _frameVelocity = Vector2.zero;
            _frameVelocity += direction * force;
        }


        #endregion

        #region Bomb
        public void AddCollectible(BombController bomb)
        {
            if (!_collectibleBombs.Contains(bomb) && !bomb.isThrown && bomb.bombLanded)
            {
                Debug.Log("Adicionando bomba coletável à lista do jogador.");
                _collectibleBombs.Add(bomb);
                bomb.ShowHighlight();
            }
        }
        public void RemoveCollectible(BombController bomb)
        {
            if (_collectibleBombs.Contains(bomb))
            {
                Debug.Log("Removendo bomba coletável da lista do jogador.");
                _collectibleBombs.Remove(bomb);
                bomb.HideHighlight();
            }
        
        }
        // public void PickUpBomb(GameObject bomb)
        // {
        //     _isBombPickable = false;
        //     _isBombHeld = true;

        //     _rb.isKinematic = true;
        //     _frameVelocity = Vector2.zero;
        //     _moveInput = Vector2.zero;
        //     _jumpHeldInput = false;
            
        //     bomb.transform.SetParent(transform);
        //     bomb.transform.localPosition = new Vector3(0, 3f, 0);

        //     bombToLaunch = bomb;
        //     StartCoroutine(AimBombToLaunch());
        // }

        public void PickUpBomb (BombController bomb)
        {
            RemoveCollectible(bomb);

            _isBombHeld = true;
            bombToLaunch = bomb.gameObject;

            BlockMovement();

            bomb.transform.SetParent(transform);
            bomb.transform.localPosition = new Vector3(0, 1.5f, 0);
            bomb.GetComponent<Rigidbody2D>().isKinematic = true;

            StartCoroutine(AimBombToLaunch());
        }

        private IEnumerator AimBombToLaunch()
        {
            BlockMovement();
            var lineRenderer = aimLine.GetComponent<LineRenderer>();
            var bombController = bombToLaunch.GetComponent<BombController>();

            // LEMBRETE: Configure o "Texture Mode" do LineRenderer para "Tile" no Inspector!

            aimLine.SetActive(true);
            while (_isBombHeld)
            {
                var playerCamera = GetComponent<PlayerInput>().camera;
                if (playerCamera == null) yield return null; 

                var initialPosition = bombToLaunch.transform.position;
                var mouseScreenPos = Mouse.current.position.ReadValue();
                var mouseWorldPos = (Vector2)playerCamera.ScreenToWorldPoint(mouseScreenPos);
                
                var launchDirection = (mouseWorldPos - (Vector2)transform.position).normalized;
                var initialVelocity = launchDirection * bombController.speed;

                var linePoints = new Vector3[_trajectoryPointCount];
                float lineLength = 0f;
                linePoints[0] = initialPosition;

                for (int i = 1; i < linePoints.Length; i++)
                {
                    float t = i * _trajectoryTimeStep;
                    linePoints[i] = (Vector2)initialPosition + (initialVelocity * t) + (Physics2D.gravity * t * t / 2f);
                    lineLength += Vector3.Distance(linePoints[i], linePoints[i-1]);
                }
                lineRenderer.positionCount = linePoints.Length;
                lineRenderer.SetPositions(linePoints);

                // Usa a densidade do ponto definida no Inspector
                lineRenderer.material.mainTextureScale = new Vector2(lineLength * _dotDensity, 1f);

                yield return null;
            }

            aimLine.SetActive(false);
        }

        // public void LaunchBomb(GameObject bomb)
        // {
        //     aimLine.SetActive(false);
        //     bombToLaunch = null;
        //     _isBombHeld = false;

        //     UnblockMovement();

        //     bomb.transform.SetParent(null);
        //     var mousePosition = (Vector2)Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        //     var launchDirection = (mousePosition - (Vector2)transform.position).normalized;

        //     bomb.GetComponent<BombController>().Throw(launchDirection);

        // }
        public void LaunchBomb()
        {
            if (bombToLaunch == null) return;

            _isBombHeld = false;

            UnblockMovement();

            var bombObject = bombToLaunch;
            var bombController = bombObject.GetComponent<BombController>();
            var bombRb = bombObject.GetComponent<Rigidbody2D>();

            bombObject.transform.SetParent(null);
            bombRb.isKinematic = false;

            var playerCamera = GetComponent<PlayerInput>().camera;
            if (playerCamera == null) return; // Adiciona uma verificação de segurança

            var mousePosition = (Vector2)playerCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            var launchDirection = (mousePosition - (Vector2)transform.position).normalized;

            bombController.Throw(launchDirection);
            
            bombToLaunch = null;
            
        }
        public void Stun(float duration)
        {
            StartCoroutine(StunCoroutine(duration));
        }

        private IEnumerator StunCoroutine(float duration)
        {
            while (duration > 0)
            {
                duration -= Time.deltaTime;

                BlockMovement();

                yield return null;
            }

            UnblockMovement();
        }



        #endregion
        private void ApplyMovement() => _rb.velocity = _frameVelocity;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
        }
#endif
    }

    public interface IPlayerController
    {
        public event Action<bool, float> GroundedChanged;

        public event Action Jumped;
    }
}