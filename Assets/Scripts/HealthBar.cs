using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider healthSlider;
    public TMP_Text healthBarText;

    private Damageable playerDamageable;

    private void Awake()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("No player found in the scene. Make sure it has the tag 'Player'.");
            enabled = false; // Disable this script if there's no player
            return;
        }

        playerDamageable = player.GetComponent<Damageable>();
        if (playerDamageable == null)
        {
            Debug.LogError("Player GameObject is missing the Damageable component.");
            enabled = false; // Disable this script if Damageable is missing
            return;
        }
    }

    private void Start()
    {
        if (healthSlider == null || healthBarText == null)
        {
            Debug.LogError("HealthBar: Slider or Text reference is missing. Assign them in the Inspector.");
            enabled = false;
            return;
        }

        // Update UI with initial values
        UpdateHealthDisplay(playerDamageable.Health, playerDamageable.MaxHealth);
    }

    private void OnEnable()
    {
        if (playerDamageable != null)
            playerDamageable.healthChanged.AddListener(OnPlayerHealthChanged);
    }

    private void OnDisable()
    {
        if (playerDamageable != null)
            playerDamageable.healthChanged.RemoveListener(OnPlayerHealthChanged);
    }

    private float CalculateSliderPercentage(int currentHealth, int maxHealth)
    {
        if (maxHealth <= 0) return 0f;
        return (float)currentHealth / maxHealth;
    }

    private void UpdateHealthDisplay(int currentHealth, int maxHealth)
    {
        healthSlider.value = CalculateSliderPercentage(currentHealth, maxHealth);
        healthBarText.text = "HP " + currentHealth + " / " + maxHealth;
    }

    private void OnPlayerHealthChanged(int newHealth, int maxHealth)
    {
        if (healthSlider == null || healthBarText == null) return;
        UpdateHealthDisplay(newHealth, maxHealth);
    }
}
