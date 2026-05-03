using UnityEngine;
using UnityEngine.UI;
using StarterAssets; // IMPORTANT for ThirdPersonController
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public float fallDuration = 0.5f;
    private int currentHealth;

    public GameObject youDiedUI;
    public GameObject restartButton;
    public Slider healthBar;

    private bool isDead = false;
    public bool IsDead => isDead;

    private ThirdPersonController playerController;
    private StarterAssetsInputs starterAssetsInputs;
    private Coroutine fallCoroutine;

    void Start()
    {
        currentHealth = maxHealth;

        // Cache controller
        playerController = GetComponent<ThirdPersonController>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();

        // Setup health bar safely
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
        else
        {
            Debug.LogWarning("HealthBar is not assigned!");
        }

        // Hide death UI safely
        if (youDiedUI != null)
        {
            youDiedUI.SetActive(false);
        }
        else
        {
            Debug.LogWarning("YouDiedUI is not assigned!");
        }

        if (restartButton != null)
        {
            restartButton.SetActive(false);
        }
        else
        {
            Debug.LogWarning("RestartButton is not assigned!");
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Update UI
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;

        Debug.Log("Player Died");

        if (fallCoroutine == null)
        {
            fallCoroutine = StartCoroutine(FallDown());
        }

        // Show UI
        if (youDiedUI != null)
        {
            youDiedUI.SetActive(true);
        }

        if (restartButton != null)
        {
            restartButton.SetActive(true);
        }

        // Let the player use the mouse to click the restart button.
        if (starterAssetsInputs != null)
        {
            starterAssetsInputs.cursorLocked = false;
            starterAssetsInputs.cursorInputForLook = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Disable movement (CORRECT WAY)
        if (playerController != null)
        {
            playerController.enabled = false;
        }
    }

    IEnumerator FallDown()
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(90f, 0f, 0f);
        float elapsedTime = 0f;

        while (elapsedTime < fallDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / fallDuration);
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, progress);
            yield return null;
        }

        transform.rotation = endRotation;
    }
}
