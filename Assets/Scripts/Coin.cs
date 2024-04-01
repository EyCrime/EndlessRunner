using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public SignalObject coinsSignal;
    public Stats stats;
    public float magnetSpeed;

    private Rigidbody _rb;
    private GameObject _target;
    private AudioSource _audioSource;
    private bool collected;
    private bool moveToPlayer;
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _audioSource = GetComponent<AudioSource>();
        _target = GameObject.Find("Player");
    }

    private void FixedUpdate()
    {
        if (moveToPlayer)
            _rb.MovePosition(Vector3.MoveTowards(_rb.position, _target.transform.position, magnetSpeed * Time.fixedDeltaTime));
        else
        {
            if (!collected)
                transform.Rotate(Vector3.forward, 90 * Time.fixedDeltaTime);
            else
            {
                transform.Translate(Vector3.back * (Time.fixedDeltaTime * 12));
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !collected)
        {
            moveToPlayer = false;
            collected = true;
                
            _audioSource.Play();
            stats.coins++;
            coinsSignal.Raise();
        }
    }

    public void MoveToPlayer()
    {
        if (!collected)
            moveToPlayer = true;
    }
}
