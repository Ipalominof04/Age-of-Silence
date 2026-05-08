using System.Collections;
using TMPro;
using UnityEngine;

public class CinematicaSignals : MonoBehaviour
{
    [Header("Dialogos UI")]
    public GameObject dialogueContainer;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogueText;

    [Header("Typewriter")]
    [SerializeField] private float charDelay = 0.05f;
    private Coroutine _typewriterCoroutine;

    [Header("Efectos")]
    public ParticleSystem shootFlash;

    private IEnumerator TypewriterEffect(string speaker, string dialogue)
    {
        speakerText.text = speaker;
        dialogueText.SetText("");
        foreach (char c in dialogue)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(charDelay);
        }
    }

    void ShowDialogue(string speaker, string dialogue)
    {
        if (_typewriterCoroutine != null)
            StopCoroutine(_typewriterCoroutine);
        _typewriterCoroutine = StartCoroutine(TypewriterEffect(speaker, dialogue));
    }

    public void Androide1() => ShowDialogue("Androide 1", "Necesitan más patrullas en otra zona. Encárgate tú del niño y de los cuerpos.");
    public void Nino1() => ShowDialogue("Niño", "N-no por favor, no me mates.");
    public void Pensamiento() => ShowDialogue("Androide", "Mmm... Esto no está bien.");
    public void Nino2() => ShowDialogue("Niño", "¡No te acerques!");
    public void Jugador1() => ShowDialogue("Androide 2", "No te asustes, no quiero hacerte daño, confía en mí. Voy a llevarte a un sitio seguro.");
    public void Nino3() => ShowDialogue("Niño", "Pero... Habéis matado a mis compañeros.");
    public void Jugador2() => ShowDialogue("Androide 2", "No, ha sido el otro androide. No tienes motivos para desconfiar de mí.");
    public void Nino4() => ShowDialogue("Niño", "Solo matáis. Matasteis a mis padres, a los que trabajan para vosotros, a los de ahora...");
    public void Jugador3() => ShowDialogue("Androide 2", "Si quieres sobrevivir, no tienes otra opción que venir conmigo. Te protegeré.");

    public void DispararFlash()
    {
        shootFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        shootFlash.Play();
    }
}
