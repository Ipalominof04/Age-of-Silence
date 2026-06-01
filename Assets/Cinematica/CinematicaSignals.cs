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

    public void CambiarEscena() //par a la cinematica en la q el player es mor
    {
        GameManager.Instance.ChangeScene(nombreEscena);
    }

    // diccionari per cercar nodes per id de manera rapida
    private Dictionary<string, RuntimeDialogueNode> _nodeLookup = new Dictionary<string, RuntimeDialogueNode>();
    private RuntimeDialogueNode _currentNode;
    private Coroutine _typewriterCoroutine; // referencia a corrutina activa per poder-la aturar

    // dispara les particules de flash de arma i sang a les cinematiques al frame exacte
    public void DispararFlash(int index)
    {
        if (index < 0 || index >= shootFlashes.Length) return;
        // neteja les particules anteriors per evitar solapament visuals
        shootFlashes[index].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        shootFlashes[index].Play();
    }

    // primer signal de la cinematica
    // carga tots els nodes del graf en el diccionari i mostra el node de entrada
    public void IniciarDialogo()
    {
        _nodeIndex = 0;
        _nodeLookup.Clear();
        foreach (var node in cinematicaGraph.AllNodes)
            _nodeLookup[node.NodeID] = node;

        if (!string.IsNullOrEmpty(cinematicaGraph.EntryNodeID))
            ShowNode(cinematicaGraph.EntryNodeID);
    }

    // signal q es crida una vegada per cada linea de dialeg
    public void SiguienteLinea()
    {
        if (_currentNode == null) return;

        if (!string.IsNullOrEmpty(_currentNode.NextNodeID))
            ShowNode(_currentNode.NextNodeID);
    }

    // ultim signal de la cinematica. atura la corrutina i allibera la memoria
    public void TerminarDialogo()
    {
        if (_typewriterCoroutine != null)
            StopCoroutine(_typewriterCoroutine);

        _nodeLookup.Clear();
        _currentNode = null;
    }


    // cerca el node per id, actualitza el text i envia el typewriter
    private void ShowNode(string nodeID)
    {
        if (!_nodeLookup.ContainsKey(nodeID)) return;
        _currentNode = _nodeLookup[nodeID];
        speakerText.SetText(_currentNode.SpeakerName);

        // agafa la durada de l'array si existeix
        float duration = (_nodeIndex < nodeDurations.Length) ? nodeDurations[_nodeIndex] : 3f;
        _nodeIndex++;

        if (_typewriterCoroutine != null)
            StopCoroutine(_typewriterCoroutine);
        _typewriterCoroutine = StartCoroutine(TypewriterEffect(_currentNode.DialogueText, duration));
    }
    //mostra el text progressivament al llarg de duration segons
    //s'utilitza Time.deltaTime perque sigui independent del framerate, funciona igual en cada maquina

    // la formula: (timer / duration) * totalChars = caracter a mostrar ara
    private IEnumerator TypewriterEffect(string fullText, float duration)
    {
        dialogueText.SetText("");
        float timer = 0f;
        int totalChars = fullText.Length;

        while(timer < duration)
        {
            timer+= Time.deltaTime;
            // percentatge de progres * total de caracters = caracters a mostrar
            int charsToShow =Mathf.FloorToInt((timer/duration) * totalChars);
            // clamp per garantir que l'index es mante dins del rang valid
            charsToShow = Mathf.Clamp(charsToShow, 0, totalChars);
            dialogueText.SetText(fullText.Substring(0, charsToShow));

            // si ja s'ha mostrat tots els caracters no cal seguir esperant
            if (charsToShow >= totalChars) break;
            yield return null; // espera al siguiente frame
        }
        // asegura que el text complet quedi visible en acabar
        dialogueText.SetText(fullText);
    }
}
