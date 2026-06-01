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
    // L'Awake assigna la camera principal al VideoPlayer perque el video es reprodueixi correctament
    private void Awake()
    {
        player.targetCamera = Camera.main;
    }
    // L'Start inicialitza els sliders amb els volums actuals i subscriu els listeners per detectar canvis
    private void Start()
    {
        masterSlider.value = SoundManager.Instance.masterVolumeMultiplier;
        masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        volumeSlider.value = SoundManager.Instance.playerVolumeMultiplier;
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        musicSlider.value = SoundManager.Instance.musicVolumeMultiplier;
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
    }
    // L'OnDestroy dessubscriu els listeners dels sliders per evitar memory leaks
    private void OnDestroy()
    {
        volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
    }
    // L'OnMasterVolumeChanged s'executa quan es mou el slider de master i aplica el nou volum
    private void OnMasterVolumeChanged(float value)
    {
        SoundManager.Instance.SetMasterVolume(value);
    }
    // L'OnVolumeChanged s'executa quan es mou el slider de so i aplica el nou volum
    private void OnVolumeChanged(float value)
    {
        SoundManager.Instance.SetPlayerVolume(value);
    }
    // L'OnMusicVolumeChanged s'executa quan es mou el slider de musica i aplica el nou volum
    private void OnMusicVolumeChanged(float value)
    {
        SoundManager.Instance.SetMusicVolume(value);
    }
    // L'AbrirOpcionesDesde guarda el canvas d'origen, el desactiva i obre el menu d'opcions
    public void AbrirOpcionesDesde(GameObject origen)
    {
        canvasAnterior = origen;
        canvasAnterior.SetActive(false);
        canvasOpciones.SetActive(true);
    }
    // El Volver tanca el menu d'opcions i reactiva el canvas des d'on s'havia obert
    public void Volver()
    {
        canvasOpciones.SetActive(false);
        if (canvasAnterior != null)
            canvasAnterior.SetActive(true);
    }
}