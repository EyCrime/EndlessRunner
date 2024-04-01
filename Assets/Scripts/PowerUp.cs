using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PowerUp : MonoBehaviour
{
    public PowerUpType type;
    public float duration;

    private AudioSource _audioSource;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void FixedUpdate()
    {
        transform.Rotate(Vector3.up, 90 * Time.fixedDeltaTime);
    }

    public void SetRandomType()
    {
        var types = Enum.GetValues(typeof(PowerUpType));
        type = (PowerUpType)types.GetValue(Random.Range(0, 4));
    }

    public void PlaySound()
    {
        _audioSource.Play();
    }
}

public enum PowerUpType
{
    Magnet, Booster, Shield, SlowMotion
}
