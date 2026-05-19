using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageVignette : MonoBehaviour
{
    public static DamageVignette Instance;

    [SerializeField] private Image vignetteImage;

    [Header("Alpha persistent per vida")]
    [SerializeField] private float alphaMaxVida = 0f;
    [SerializeField] private float alphaMinVida = 0.15f;
    [SerializeField] private float smoothSpeed = 2f;

    [Header("Flash de daño")]
    [SerializeField] private float flashAlphaExtra = 0.3f;
    [SerializeField] private float flashInDuration = 0.1f;
    [SerializeField] private float flashHold = 0.05f;
    [SerializeField] private float flashOutDuration = 0.4f;

    [Header("Color")]
    [SerializeField]
    private Color vignetteColor =
        new Color(0.6f, 0f, 0f, 0f);

    private float targetBaseAlpha = 0f;
    private float currentBaseAlpha = 0f;
    private float flashAlpha = 0f;

    private bool vignetteActive = true;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SetAlpha(0f);
    }

    private void Update()
    {
        if (!vignetteActive)
            return;

        currentBaseAlpha = Mathf.Lerp(
            currentBaseAlpha,
            targetBaseAlpha,
            Time.deltaTime * smoothSpeed
        );

        SetAlpha(
            Mathf.Clamp01(currentBaseAlpha + flashAlpha)
        );
    }

    public void SetVignetteActive(bool active)
    {
        vignetteActive = active;

        vignetteImage.gameObject.SetActive(active);

        if (!active)
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                flashCoroutine = null;
            }

            targetBaseAlpha = 0f;
            currentBaseAlpha = 0f;
            flashAlpha = 0f;

            SetAlpha(0f);
        }
    }

    public void UpdateHealthRatio(float healthRatio)
    {
        targetBaseAlpha =
            Mathf.Lerp(alphaMinVida, alphaMaxVida, healthRatio);
    }
    public void ResetHealthRatio(float healthRatio)
    {
        targetBaseAlpha = Mathf.Lerp(alphaMinVida, alphaMaxVida, healthRatio);
        currentBaseAlpha = targetBaseAlpha;
        flashAlpha = 0f;
        SetAlpha(Mathf.Clamp01(currentBaseAlpha));
    }

    public void PlayDamageFlash()
    {
        if (!vignetteActive)
            return;

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float t = 0f;

        while (t < flashInDuration)
        {
            t += Time.deltaTime;

            flashAlpha = Mathf.Lerp(
                0f,
                flashAlphaExtra,
                t / flashInDuration
            );

            yield return null;
        }

        yield return new WaitForSeconds(flashHold);

        t = 0f;

        while (t < flashOutDuration)
        {
            t += Time.deltaTime;

            flashAlpha = Mathf.Lerp(
                flashAlphaExtra,
                0f,
                t / flashOutDuration
            );

            yield return null;
        }

        flashAlpha = 0f;
        flashCoroutine = null;
    }

    private void SetAlpha(float alpha)
    {
        Color c = vignetteColor;
        c.a = alpha;
        vignetteImage.color = c;
    }
}