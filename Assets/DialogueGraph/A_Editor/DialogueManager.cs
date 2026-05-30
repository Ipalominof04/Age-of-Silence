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
    [SerializeField] private float charDelay = 0.05f; //velocitat entre aparicio de caracters
    private Dictionary<string, RuntimeDialogueNode> _nodeLookup = new Dictionary<string, RuntimeDialogueNode>();
    private RuntimeDialogueNode _currentNode;
    private Coroutine _typewriterCoroutine;
    private bool _isTyping = false; //per saber si s'esta escribint
    private bool _isInfo = false;

    [Header("Events")]
    public GameEvent OnDialogueStarted;
    public GameEvent OnDialogueEnded;
    public GameEvent OnInfoStarted;
    public GameEvent OnInfoEnded;    


    private void Awake()
    {
        //singleton
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
        //el panell de dialeg comenca ocult
        DialoguePanel.SetActive(false);
    }
    //inicia un dialeg amb aiden, bloqueja el moment del jugador
    //construeix un diccionari de nodes per accedir-hi per ID
    public void StartDialogue(RuntimeDialogueGraph graph)
    {
        _nodeLookup.Clear();
        foreach (var node in graph.AllNodes)
            _nodeLookup[node.NodeID] = node;

        //bloqueja al player per a que no es pugui moure mentres esta en un dialeg
        OnDialogueStarted.Raise();
        if (!string.IsNullOrEmpty(graph.EntryNodeID))
            ShowNode(graph.EntryNodeID);
    }
    //inicia un dialeg d'informacio, bloqueja el moviment del jugador
    //els info no tindran opcio de resposta ni modificacio de confiança, pero aixo es un tipus dialogue diferent 
    public void StartInfo(RuntimeDialogueGraph graph)
    {
        _isInfo = true;
        //bloqueja al player per a que no es pugui moure mentres esta en un info
        OnInfoStarted.Raise();
        _nodeLookup.Clear();
        foreach (var node in graph.AllNodes)
            _nodeLookup[node.NodeID] = node;

        if (!string.IsNullOrEmpty(graph.EntryNodeID))
            ShowNode(graph.EntryNodeID);
    }

    private void Update()
    {
        //per avancar o saltar dialegs
        //nomes actua si el node actual no te opcions de resposta (ChoiceNode)
        if (Mouse.current.leftButton.wasPressedThisFrame && _currentNode != null && _currentNode.Choices.Count == 0)
        {
           //si l'efecte de maquina d'escriure esta en curs, completar el text a l'instant
            if (_isTyping)
            {
                SkipTypewriter();
                return;
            }
            //avancar al seguent node o finalitzar el dialeg
            if (!string.IsNullOrEmpty(_currentNode.NextNodeID))
                ShowNode(_currentNode.NextNodeID);
            else
                EndDialogue();
        }
    }
    //mostra el node indicat per nodeID, actualitza la UI i inicia l'efecte de maquina d'escriure
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

        //netejar els botons de resposta de l'anterior node
        foreach (Transform child in ChoiceButtonContainer)
            Destroy(child.gameObject);

        //iniar l'efecte de maquina d'escriure
        if (_typewriterCoroutine != null)
            StopCoroutine(_typewriterCoroutine);
        _typewriterCoroutine = StartCoroutine(TypewriterEffect(_currentNode.DialogueText, _currentNode.Choices));
    }

    //corrutina que afegeix un caracter cada charDelay per generar l'efecte de maquina d'escriure
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

        //els botons de resposta nomes es mostren quan el text s'acabat d'escriure en cas de que hi hagui text de dialeg en un choice
        if (choices.Count > 0)
            SpawnChoiceButtons(choices);
    }
    //completa el text a l'instant aturant la corrutina de l'efecte de maquina d'escriure
    private void SkipTypewriter()
    {
        if (_typewriterCoroutine != null)
            StopCoroutine(_typewriterCoroutine);

        DialogueText.SetText(_currentNode.DialogueText);
        _isTyping = false;

        if (_currentNode.Choices.Count > 0)
            SpawnChoiceButtons(_currentNode.Choices);
    }
    //instancia els botons de resposta del ChoiceNode i assigna els listeners
    //que apliquen el canvi de confianca i avancen al node de desti
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
                    //aplicar el canvi de confianca correspont a l'opcio escollida
                    if (choice.ValueChange > 0)
                        GameManager.Instance.SumValor(choice.ValueChange);
                    else if (choice.ValueChange < 0)
                        GameManager.Instance.RestValor(Mathf.Abs(choice.ValueChange));

                    //llencar l'event opcional associat a l'opcio si es que hi ha
                    choice.OnChoiceSelected?.Raise();

                    //avancar al node de desti o finalitzar el dialeg
                    if (!string.IsNullOrEmpty(choice.DestinationNodeID))
                        ShowNode(choice.DestinationNodeID);
                    else
                        EndDialogue();
                });
            }
        }
    }
    //finalitza el dialeg, llanca l'event corresponent segons si es un dialeg normal o un info
    //neteja l'estat i oculta el panell de dialeg del canvas
    private void EndDialogue()
    {
        if (_typewriterCoroutine != null)
            StopCoroutine(_typewriterCoroutine);

        //llancar l'event corresponent segos si es un dialeg normal o un dialeg de info
        if (_isInfo)
            OnInfoEnded.Raise();
        else
            OnDialogueEnded.Raise();

        _isInfo = false;
        _nodeLookup.Clear();
        _currentNode = null;
        _isTyping = false;

        DialoguePanel.SetActive(false);

        //netejar els botons de resposta
        foreach (Transform child in ChoiceButtonContainer)
            Destroy(child.gameObject);
    }
}