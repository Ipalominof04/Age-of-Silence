using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI Components")]
    public GameObject DialoguePanel;
    public TextMeshProUGUI SpeakerNameText;
    public TextMeshProUGUI DialogueText;

    [Header("Choice Button UI")]
    public Button ChoiceButtonPrefab;
    public Transform ChoiceButtonContainer;

    [Header("Typewriter Settings")]
    [SerializeField] private float charDelay = 0.05f; //velocidad entre caracteres
    private Dictionary<string, RuntimeDialogueNode> _nodeLookup = new Dictionary<string, RuntimeDialogueNode>();
    private RuntimeDialogueNode _currentNode;
    private Coroutine _typewriterCoroutine;
    private bool _isTyping = false; //para saber si estA escribiendo
    //private bool _isInfo = false;

    [Header("Events")]
    public GameEvent OnDialogueStarted;
    public GameEvent OnDialogueEnded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        DialoguePanel.SetActive(false);
    }
    public void StartDialogue(RuntimeDialogueGraph graph)
    {
        _nodeLookup.Clear();
        foreach (var node in graph.AllNodes)
            _nodeLookup[node.NodeID] = node;

        OnDialogueStarted.Raise();
        if (!string.IsNullOrEmpty(graph.EntryNodeID))
            ShowNode(graph.EntryNodeID);
    }
    public void StartInfo(RuntimeDialogueGraph graph)
    {
       // _isInfo = true;
        //bloquea al jugador igual que un dialogo normal
        OnDialogueStarted.Raise();
        _nodeLookup.Clear();
        foreach (var node in graph.AllNodes)
            _nodeLookup[node.NodeID] = node;

        if (!string.IsNullOrEmpty(graph.EntryNodeID))
            ShowNode(graph.EntryNodeID);
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && _currentNode != null && _currentNode.Choices.Count == 0)
        {
            // Si est� escribiendo, completa el texto al instante
            if (_isTyping)
            {
                SkipTypewriter();
                return;
            }

            if (!string.IsNullOrEmpty(_currentNode.NextNodeID))
                ShowNode(_currentNode.NextNodeID);
            else
                EndDialogue();
        }
    }

    private void ShowNode(string nodeID)
    {
        if (!_nodeLookup.ContainsKey(nodeID))
        {
            EndDialogue();
            return;
        }

        _currentNode = _nodeLookup[nodeID];
        DialoguePanel.SetActive(true);
        SpeakerNameText.SetText(_currentNode.SpeakerName);

        // Limpiar botones anteriores
        foreach (Transform child in ChoiceButtonContainer)
            Destroy(child.gameObject);

        // Iniciar typewriter
        if (_typewriterCoroutine != null)
            StopCoroutine(_typewriterCoroutine);
        _typewriterCoroutine = StartCoroutine(TypewriterEffect(_currentNode.DialogueText, _currentNode.Choices));
    }

    private IEnumerator TypewriterEffect(string fullText, List<ChoiceData> choices)
    {
        _isTyping = true;
        DialogueText.SetText("");

        foreach (char c in fullText)
        {
            DialogueText.text += c;
            yield return new WaitForSeconds(charDelay);
        }

        _isTyping = false;

        // Mostrar botones de eleccion solo cuando termina de escribir
        if (choices.Count > 0)
            SpawnChoiceButtons(choices);
    }

    private void SkipTypewriter()
    {
        if (_typewriterCoroutine != null)
            StopCoroutine(_typewriterCoroutine);

        DialogueText.SetText(_currentNode.DialogueText);
        _isTyping = false;

        if (_currentNode.Choices.Count > 0)
            SpawnChoiceButtons(_currentNode.Choices);
    }

    private void SpawnChoiceButtons(List<ChoiceData> choices)
    {
        foreach (var choice in choices)
        {
            Button button = Instantiate(ChoiceButtonPrefab, ChoiceButtonContainer);
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = choice.ChoiceText;

            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    if (choice.ValueChange > 0)
                        GameManager.Instance.SumValor(choice.ValueChange);
                    else if (choice.ValueChange < 0)
                        GameManager.Instance.RestValor(Mathf.Abs(choice.ValueChange));

                    choice.OnChoiceSelected?.Raise();

                    if (!string.IsNullOrEmpty(choice.DestinationNodeID))
                        ShowNode(choice.DestinationNodeID);
                    else
                        EndDialogue();
                });
            }
        }
    }

    private void EndDialogue()
    {
        if (_typewriterCoroutine != null)
            StopCoroutine(_typewriterCoroutine);

        //siempre desbloquea al player sea info o dialogo normal
        OnDialogueEnded.Raise();

       // _isInfo = false;

        _nodeLookup.Clear();
        _currentNode = null;
        _isTyping = false;

        DialoguePanel.SetActive(false);

        foreach (Transform child in ChoiceButtonContainer)
            Destroy(child.gameObject);
    }
}