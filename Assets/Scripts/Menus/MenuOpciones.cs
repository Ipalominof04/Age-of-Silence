using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MenuOpciones : MonoBehaviour
{
    [SerializeField] GameObject canvasOpciones;
    [SerializeField] VideoPlayer player;
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider volumeSlider;
    [SerializeField] Slider musicSlider;

    private GameObject canvasAnterior;

    private void Awake()
    {
        player.targetCamera = Camera.main;
    }

    private void Start()
    {
        masterSlider.value = SoundManager.Instance.masterVolumeMultiplier;
        masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);


        volumeSlider.value = SoundManager.Instance.playerVolumeMultiplier;
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        musicSlider.value = SoundManager.Instance.musicVolumeMultiplier;
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
    }

    private void OnDestroy()
    {
        volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
    }

    private void OnMasterVolumeChanged(float value)
    {
        SoundManager.Instance.SetMasterVolume(value);
    }

    private void OnVolumeChanged(float value)
    {
        SoundManager.Instance.SetPlayerVolume(value);
    }
    private void OnMusicVolumeChanged(float value)
    {
        SoundManager.Instance.SetMusicVolume(value);
    }

    public void AbrirOpcionesDesde(GameObject origen)
    {
        canvasAnterior = origen;
        canvasAnterior.SetActive(false);
        canvasOpciones.SetActive(true);
    }

    public void Volver()
    {
        canvasOpciones.SetActive(false);
        if (canvasAnterior != null)
            canvasAnterior.SetActive(true);
    }
}