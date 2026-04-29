using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageVignette : MonoBehaviour
{
    public static DamageVignette Instance;

    [SerializeField] private Image vignetteImage;

    [Header("Alpha persistent per vida")]
    [SerializeField] private float alphaMaxVida = 0f;     // alpha quan vida al màxim
    [SerializeField] private float alphaMinVida = 0.15f;  // alpha quan vida quasi 0
    [SerializeField] private float smoothSpeed = 2f;     // velocitat de transició suau

    [Header("Flash de dany")]
    [SerializeField] private float flashAlphaExtra = 0.3f;   // alpha extra del flash
    [SerializeField] private float flashInDuration = 0.1f;
    [SerializeField] private float flashHold = 0.05f;
    [SerializeField] private float flashOutDuration = 0.4f;

    [Header("Color")]
    [SerializeField] private Color vignetteColor = new Color(0.6f, 0f, 0f, 0f);

    private float targetBaseAlpha = 0f;  // alpha objectiu persistent
    private float currentBaseAlpha = 0f;  // alpha persistent actual (suavitzat)
    private float flashAlpha = 0f;  // alpha extra del flash
    private bool vignetteActive = false;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        SetAlpha(0f);
    }

    private void Update()
    {
        if (!vignetteActive) return;

        // Suavitza la transició de l'alpha base
        currentBaseAlpha = Mathf.Lerp(currentBaseAlpha, targetBaseAlpha, Time.deltaTime * smoothSpeed);

        // Alpha final = base persistent + flash temporal
        SetAlpha(Mathf.Clamp01(currentBaseAlpha + flashAlpha));
    }

    /// <summary>
    /// Activa la viñeta per a escenes d'acció.
    /// Crida-la en entrar a una escena d'acció.
    /// </summary>
    public void SetVignetteActive(bool active)
    {
        vignetteActive = active;
        if (!active)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            currentBaseAlpha = 0f;
            flashAlpha = 0f;
            SetAlpha(0f);
        }
    }

    /// <summary>
    /// Actualitza l'alpha base segons la vida (0=mort, 1=vida màxima).
    /// Crida-la cada vegada que canviï la vida del jugador.
    /// </summary>
    public void UpdateHealthRatio(float healthRatio)
    {
        // Com menys vida, més alpha
        targetBaseAlpha = Mathf.Lerp(alphaMinVida, alphaMaxVida, healthRatio);
    }

    /// <summary>
    /// Dispara el flash de dany.
    /// </summary>
    public void PlayDamageFlash()
    {
        if (!vignetteActive) return;
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        // Flash in
        float t = 0f;
        while (t < flashInDuration)
        {
            t += Time.deltaTime;
            flashAlpha = Mathf.Lerp(0f, flashAlphaExtra, t / flashInDuration);
            yield return null;
        }

        yield return new WaitForSeconds(flashHold);

        // Fade out
        t = 0f;
        while (t < flashOutDuration)
        {
            t += Time.deltaTime;
            flashAlpha = Mathf.Lerp(flashAlphaExtra, 0f, t / flashOutDuration);
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