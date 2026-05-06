using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MenuOpciones : MonoBehaviour
{
    [SerializeField] VideoPlayer player;
    [SerializeField] Slider volumeSlider;

    private void Awake()
    {
        player.targetCamera = Camera.main;
    }

    private void Start()
    {
        // Inicializar el slider con el valor actual
        volumeSlider.value = SoundManager.Instance.playerVolumeMultiplier;

        // Suscribirse al evento del slider
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    private void OnDestroy()
    {
        volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
    }

    private void OnVolumeChanged(float value)
    {
        SoundManager.Instance.SetPlayerVolume(value);
    }
}