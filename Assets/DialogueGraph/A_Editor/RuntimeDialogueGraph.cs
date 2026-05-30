using System;
using UnityEngine;
using System.Collections.Generic;

//scriptableObject generat pel DialogueGraphImporter que conté el graf de dialeg
//per al seu us en temps d'execucio
public class RuntimeDialogueGraph : ScriptableObject
{
    //ID del primer node de la conversa, obtingut del StartNode del graf
    public string EntryNodeID;
    //llista de tots els nodes del graf en temps d'execucio
    public List<RuntimeDialogueNode> AllNodes = new List<RuntimeDialogueNode>();
}

//representa un node individual del graf en temps d'execucio
[Serializable]
public class RuntimeDialogueNode
{
    public string NodeID;
    public string SpeakerName;
    public string DialogueText;
    //llista d'opcions de resposta, buida si es un DialogueNode normal
    public List<ChoiceData> Choices = new List<ChoiceData>();
    public string NextNodeID;
}

//representa cada opcio de resposta d'un ChoiceNode
[Serializable]
public class ChoiceData
{
    public string ChoiceText;
    public string DestinationNodeID;
    public int ValueChange;
    //gameEvent opcional que es llanca al seleccionar aquesta opcio
    public GameEvent OnChoiceSelected;
}