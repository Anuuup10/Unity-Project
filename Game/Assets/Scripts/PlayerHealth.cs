using UnityEngine;
using UnityEngine.UI;
using StarterAssets;
using System.Collections;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public float fallDuration = 0.5f;
    public float throwDistance = 4f;
    public float throwHeight = 1.4f;
    public float throwSpinAngle = 130f;
    private int currentHealth;

    public GameObject youDiedUI;
    public GameObject restartButton;
    public Slider healthBar;

    private bool isDead = false;
    public bool IsDead => isDead;

    private ThirdPersonController playerController;
    private StarterAssetsInputs starterAssetsInputs;
    private Coroutine fallCoroutine;
    private Vector3 lastImpactDirection = Vector3.forward;
    private float lastImpactForce = 1f;

    void Start()
    {
        currentHealth = maxHealth;

        playerController = GetComponent<ThirdPersonController>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        ConfigureDeathUi();

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
        else
        {
            Debug.LogWarning("HealthBar is not assigned!");
        }

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

    private void ConfigureDeathUi()
    {
        if (youDiedUI != null)
        {
            RectTransform deathTextRect = youDiedUI.GetComponent<RectTransform>();
            if (deathTextRect != null)
            {
                deathTextRect.sizeDelta = new Vector2(820f, 230f);
                deathTextRect.anchoredPosition = Vector2.zero;
            }

            TMP_Text deathText = youDiedUI.GetComponent<TMP_Text>();
            if (deathText != null)
            {
                deathText.fontSize = 140f;
                deathText.enableWordWrapping = false;
                deathText.alignment = TextAlignmentOptions.Center;
            }
        }

        if (restartButton != null)
        {
            RectTransform restartRect = restartButton.GetComponent<RectTransform>();
            if (restartRect != null)
            {
                restartRect.sizeDelta = new Vector2(430f, 120f);
                restartRect.anchoredPosition = new Vector2(0f, -230f);
            }

            TMP_Text restartText = restartButton.GetComponentInChildren<TMP_Text>(true);
            if (restartText != null)
            {
                restartText.fontSize = 64f;
                restartText.alignment = TextAlignmentOptions.Center;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void TakeHit(int damage, Vector3 impactDirection, float impactForce)
    {
        impactDirection.y = 0f;

        if (impactDirection.sqrMagnitude > 0.01f)
        {
            lastImpactDirection = impactDirection.normalized;
        }

        lastImpactForce = Mathf.Clamp(impactForce, 1f, 2.5f);
        TakeDamage(damage);
    }

    void Die()
    {
        isDead = true;

        if (fallCoroutine == null)
        {
            fallCoroutine = StartCoroutine(FallDown());
        }

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

        if (playerController != null)
        {
            playerController.enabled = false;
        }
    }

    IEnumerator FallDown()
    {
        Vector3 impactDirection = lastImpactDirection.sqrMagnitude > 0.01f ? lastImpactDirection.normalized : transform.forward;
        float impactDistance = throwDistance * lastImpactForce;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(90f, throwSpinAngle, 0f);
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + impactDirection * impactDistance;
        float elapsedTime = 0f;

        while (elapsedTime < fallDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / fallDuration);
            float arcHeight = Mathf.Sin(progress * Mathf.PI) * throwHeight * lastImpactForce;

            transform.position = Vector3.Lerp(startPosition, endPosition, progress) + Vector3.up * arcHeight;
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, progress);
            yield return null;
        }

        transform.position = endPosition;
        transform.rotation = endRotation;
    }
}
