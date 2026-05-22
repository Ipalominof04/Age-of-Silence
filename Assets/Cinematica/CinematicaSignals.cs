using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CinematicaSignals : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogueText;

    [Header("Dialogue Graph")]
    public RuntimeDialogueGraph cinematicaGraph;

    [Header("Efectos")]
    public ParticleSystem[] shootFlashes;

    [Header("Duracion por nodo")]
    public float[] nodeDurations;
    private int _nodeIndex = 0;

    [Header("Cambio de Escena")]
    public string nombreEscena;

    public void CambiarEscena() //para la cinematica en la q el player se muere
    {
        GameManager.Instance.ChangeScene(nombreEscena);
    }

    // diccionario para buscar nodos por ID rapidamente
    private Dictionary<string, RuntimeDialogueNode> _nodeLookup = new Dictionary<string, RuntimeDialogueNode>();
    private RuntimeDialogueNode _currentNode;
    private Coroutine _typewriterCoroutine;

    // dispara el flash del arma, se coloca en el frame exacto del disparo
    public void DispararFlash(int index)
    {
        if (index < 0 || index >= shootFlashes.Length) return;
        shootFlashes[index].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        shootFlashes[index].Play();
    }

    // primer signal de la cinematica
    // carga todos los nodos del graph y muestra el primero
    public void IniciarDialogo()
    {
        _nodeIndex = 0;
        _nodeLookup.Clear();
        foreach (var node in cinematicaGraph.AllNodes)
            _nodeLookup[node.NodeID] = node;

        if (!string.IsNullOrEmpty(cinematicaGraph.EntryNodeID))
            ShowNode(cinematicaGraph.EntryNodeID);
    }

    // signal que se repite por cada linea de dialogo
    public void SiguienteLinea()
    {
        if (_currentNode == null) return;

        if (!string.IsNullOrEmpty(_currentNode.NextNodeID))
            ShowNode(_currentNode.NextNodeID);
    }

    // ultimo signal de la cinematica, limpia todo
    public void TerminarDialogo()
    {
        if (_typewriterCoroutine != null)
            StopCoroutine(_typewriterCoroutine);

        _nodeLookup.Clear();
        _currentNode = null;
    }


    // busca el nodo por ID y arranca el typewriter con su duracion
    private void ShowNode(string nodeID)
    {
        if (!_nodeLookup.ContainsKey(nodeID)) return;
        _currentNode = _nodeLookup[nodeID];
        speakerText.SetText(_currentNode.SpeakerName);

        // coge la duracion del array si existe, si no usa 3s por defecto
        float duration = (_nodeIndex < nodeDurations.Length) ? nodeDurations[_nodeIndex] : 3f;
        _nodeIndex++;

        if (_typewriterCoroutine != null)
            StopCoroutine(_typewriterCoroutine);
        _typewriterCoroutine = StartCoroutine(TypewriterEffect(_currentNode.DialogueText, duration));
    }

    // muestra el texto progresivamente usando Time.deltaTime
    // asi es independiente del framerate, funciona igual en cualquier ordenador
    // la formula: (timer / duration) * totalChars = cuantos caracteres mostrar en este momento
    private IEnumerator TypewriterEffect(string fullText, float duration)
    {
        dialogueText.SetText("");
        float timer = 0f;
        int totalChars = fullText.Length;

        while(timer < duration)
        {
            timer+= Time.deltaTime;
            // porcentaje de progreso * total de caracteres = caracteres a mostrar
            int charsToShow =Mathf.FloorToInt((timer/duration) * totalChars);
            // clamp para asegurarse de no salirse del rango
            charsToShow = Mathf.Clamp(charsToShow, 0, totalChars);
            dialogueText.SetText(fullText.Substring(0, charsToShow));

            // si ya mostro todos los caracteres no hace falta seguir esperando
            if (charsToShow >= totalChars) break;
            yield return null; // espera al siguiente frame
        }
        // asegura que el texto completo queda visible al final
        dialogueText.SetText(fullText);
    }
}
