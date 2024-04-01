using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public TMP_Text coinsText;
    public Stats stats;

    private void Start()
    {
        coinsText.text = stats.coins.ToString();
    }

    public void UpdateCoins()
    {
        coinsText.text = stats.coins.ToString();
    }
}
