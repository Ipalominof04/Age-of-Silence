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

    [Header("Typewriter")]
    [SerializeField] private int charsPerFrame = 2;

    [Header("Efectos")]
    public ParticleSystem shootFlash;

    private Dictionary<string, RuntimeDialogueNode> _nodeLookup = new Dictionary<string, RuntimeDialogueNode>();
    private RuntimeDialogueNode _currentNode;
    private Coroutine _typewriterCoroutine;
    private bool _isTyping = false;

    public void DispararFlash()
    {
        shootFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        shootFlash.Play();
    }
    public void IniciarDialogo()
    {
        _nodeLookup.Clear();
        foreach (var node in cinematicaGraph.AllNodes)
            _nodeLookup[node.NodeID] = node;

        if (!string.IsNullOrEmpty(cinematicaGraph.EntryNodeID))
            ShowNode(cinematicaGraph.EntryNodeID);
    }

    public void SiguienteLinea()
    {
        if (_currentNode == null) return;

        if (_isTyping)
        {
            SkipTypewriter();
            return;
        }

        if (!string.IsNullOrEmpty(_currentNode.NextNodeID))
            ShowNode(_currentNode.NextNodeID);
    }

    public void TerminarDialogo()
    {
        if (_typewriterCoroutine != null)
            StopCoroutine(_typewriterCoroutine);

        _nodeLookup.Clear();
        _currentNode = null;
        _isTyping = false;
    }

    private void ShowNode(string nodeID)
    {
        if (!_nodeLookup.ContainsKey(nodeID)) return;

        _currentNode = _nodeLookup[nodeID];
        speakerText.SetText(_currentNode.SpeakerName);

        if (_typewriterCoroutine != null)
            StopCoroutine(_typewriterCoroutine);
        _typewriterCoroutine = StartCoroutine(TypewriterEffect(_currentNode.DialogueText));
    }

    private IEnumerator TypewriterEffect(string fullText)
    {
        _isTyping = true;
        dialogueText.SetText("");
        int count = 0;

        foreach (char c in fullText)
        {
            dialogueText.text += c;
            count++;
            if (count >= charsPerFrame)
            {
                count = 0;
                yield return new WaitForEndOfFrame();
            }
        }

        _isTyping = false;
    }

    private void SkipTypewriter()
    {
        if (_typewriterCoroutine != null)
            StopCoroutine(_typewriterCoroutine);

        dialogueText.SetText(_currentNode.DialogueText);
        _isTyping = false;
    }
}
