using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class PlayerController: MonoBehaviour
{
    public float gravity = 20.0f;
    public float jumpHeight = 2.5f;
    public float[] xPositions;
    
    public float crouchTime;
    public float swipeDetectionOffset;
    
    public AudioSource backgroundMusic;

    public AudioClip hitSound;
    public AudioClip jumpSound;
    public AudioClip slideSound;
    public AudioClip slideDownSound;
    
    public AudioClip breakSound;
    public AudioClip magnetSound;

    private int xPosIndex = 1;
    
    private bool _grounded;
    private bool _crouching;
    
    private bool _magnetActive;
    private bool _boosterActive;
    private bool _shieldActive;

    private Rigidbody _rb;
    private AudioSource _audioSource;
    private Animator _animator;
    private Vector3 _defaultScale;
    
    private Vector2 _touchPosStart;
    private Vector2 _touchPosEnd;

    // Start is called before the first frame update
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        _defaultScale = transform.localScale;
        
        _animator.SetBool("running", MapGenerator.gameStarted);
    }

    private void Update()
    {
        if (_magnetActive)
            DoMagnet();
        
        DoKeyBoardControls();
        DoSwipeControls();
        
        if (_crouching)
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(_defaultScale.x, _defaultScale.y * 0.4f, _defaultScale.z), Time.deltaTime * 7);
        else
            transform.localScale = Vector3.Lerp(transform.localScale, _defaultScale, Time.deltaTime * 7);
    }

    private void DoKeyBoardControls()
    {
        // Jump
        if (Input.GetKeyDown(KeyCode.W))
        {
            Jump();
        }

        //Crouch
        if (Input.GetKey(KeyCode.S))
        {
            StartCoroutine(CrouchCoroutine());
        }
        
        // Left
        if (Input.GetKeyDown(KeyCode.A))
        {
            Left();
        }
        
        // Right
        if (Input.GetKeyDown(KeyCode.D))
        {
            Right();
        }
    }
    
    private void DoSwipeControls()
    {
        // one finger touches the screen
        if (Input.touchCount > 0)
        {
            // first finger that touches
            var touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began)
                _touchPosStart = touch.position;
            else if (touch.phase == TouchPhase.Ended)
            {
                _touchPosEnd = touch.position;

                var swipeDir = _touchPosEnd - _touchPosStart;
                
                // Jump
                if (swipeDir.y > swipeDetectionOffset)
                {
                    Jump();
                }
                
                // Crouch
                if (swipeDir.y < (swipeDetectionOffset * - 1))
                {
                    StartCoroutine(CrouchCoroutine());
                }
                
                // Left
                if (swipeDir.x < (swipeDetectionOffset * - 1))
                {
                    Left();
                }
                
                // Right
                if (swipeDir.x > swipeDetectionOffset)
                {
                    Right();
                }
            }
        }
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        // We apply gravity manually for more tuning control
        _rb.AddForce(new Vector3(0, -gravity * _rb.mass, 0));
        transform.position = Vector3.Lerp(transform.position, new Vector3(xPositions[xPosIndex], transform.position.y, transform.position.z), Time.fixedDeltaTime * 5);

        _grounded = false;
    }

    private void OnCollisionStay()
    {
        _grounded = true;
    }

    private float CalculateJumpVerticalSpeed()
    {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        return Mathf.Sqrt(2 * jumpHeight * gravity);
    }

    private IEnumerator CrouchCoroutine()
    {
        if (!_crouching)
        {
            _crouching = true;
            _audioSource.PlayOneShot(slideDownSound);
            yield return new WaitForSeconds(crouchTime);
            _crouching = false;
        }
    }

    private void Jump()
    {
        if (_grounded)
        {
            _rb.velocity = new Vector3(_rb.velocity.x, CalculateJumpVerticalSpeed(), _rb.velocity.z);
            _audioSource.PlayOneShot(jumpSound);
        }
    }

    private void Left()
    {
        if (xPosIndex > 0)
        {
            xPosIndex--;
            _audioSource.PlayOneShot(slideSound);
        }
    }
    
    private void Right()
    {
        if (xPosIndex < 2)
        {
            xPosIndex++;
            _audioSource.PlayOneShot(slideSound);
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if(collider.CompareTag("Obstacle"))
        {
            if (_boosterActive) return; // Ignore Obstacles when boosting

            if (_shieldActive)
            {
                _shieldActive = false; // shield ignores only one obstacle
                _animator.SetBool("shielded", false);
                _audioSource.PlayOneShot(breakSound);
                return;
            }
            
            //print("GameOver!");
            _audioSource.PlayOneShot(hitSound);
            MapGenerator.instance.gameOver = true;
            _animator.SetBool("running", false);
        }
        else if (collider.CompareTag("PowerUp"))
        {
            MapGenerator.instance.powerUpActive = false;

            var powerUp = collider.GetComponent<PowerUp>();
            powerUp.PlaySound();
            
            switch (powerUp.type)
            {
                case PowerUpType.Magnet: StartCoroutine(MagnetCoroutine(powerUp.duration));
                    break;
                case PowerUpType.Booster: StartCoroutine(BoosterCoroutine(powerUp.duration));
                    break;
                case PowerUpType.Shield: StartCoroutine(ShieldCoroutine(powerUp.duration));
                    break;
                case PowerUpType.SlowMotion: StartCoroutine(SlowMotionCoroutine(powerUp.duration));
                    break;
            }
            
            collider.gameObject.SetActive(false);
        }
    }
    
    private void DoMagnet()
    {
        var colliders = Physics.OverlapSphere(transform.position, 10f);

        for (var i = 0; i < colliders.Length; i++)
        {
            var collider = colliders[i];
            if (collider.CompareTag("Coin"))
                collider.GetComponent<Coin>().MoveToPlayer();
        }
    }

    private IEnumerator MagnetCoroutine(float duration)
    {
        _magnetActive = true;
        _animator.SetBool("magnetic", true);
        _audioSource.PlayOneShot(magnetSound);
        yield return new WaitForSeconds(duration);
        
        _animator.SetBool("magnetic", false);
        _magnetActive = false;
    }
    
    private IEnumerator BoosterCoroutine(float duration)
    {
        MapGenerator.instance.boostedSpeedMultiplier = 2;
        backgroundMusic.pitch = 1.5f;
        _boosterActive = true;
        _animator.SetBool("boosted", true);
        yield return new WaitForSeconds(duration);
        
        MapGenerator.instance.boostedSpeedMultiplier = 1;
        _animator.SetBool("boosted", false);
        backgroundMusic.pitch = 1;
        _boosterActive = false;
    }
    
    private IEnumerator ShieldCoroutine(float duration)
    {
        _shieldActive = true;
        _animator.SetBool("shielded", true);
        yield return new WaitForSeconds(duration);

        _animator.SetBool("shielded", false);
        _shieldActive = false;
    }
    
    private IEnumerator SlowMotionCoroutine(float duration)
    {
        MapGenerator.instance.boostedSpeedMultiplier = 0.5f;
        backgroundMusic.pitch = 0.5f;
        _animator.SetBool("slowmotion", true);
        yield return new WaitForSeconds(duration);
        
        _animator.SetBool("slowmotion", false);
        backgroundMusic.pitch = 1;
        MapGenerator.instance.boostedSpeedMultiplier = 1;
    }
}