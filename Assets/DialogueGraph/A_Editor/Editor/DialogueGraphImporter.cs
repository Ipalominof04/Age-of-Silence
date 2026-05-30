using System;
using System.Collections.Generic;
using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEditor.AssetImporters;
using UnityEngine;

//importador que converteix automaticament el graf de l'editor en un
//RuntimeDialogueGraph utilitzable en temps d'execució quan s'importa el fitxer
[ScriptedImporter(1, DialogueGraph.AssetExtension)]
public class DialogueGraphImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        DialogueGraph editorGraph = GraphDatabase.LoadGraphForImporter<DialogueGraph>(ctx.assetPath);
        RuntimeDialogueGraph runtimeGraph = ScriptableObject.CreateInstance<RuntimeDialogueGraph>();
        //assignar un ID unic a cada node del graf
        var nodeIDMap = new Dictionary<INode, string>();

        foreach (var node in editorGraph.GetNodes())
        {
            nodeIDMap[node] = Guid.NewGuid().ToString();
        }

        //trobar el StartNode i assignar l'ID del primer node de la conversa
        var startNode = editorGraph.GetNodes().OfType<StartNode>().FirstOrDefault();
        if (startNode != null)
        {
            var entryPort = startNode.GetOutputPorts().FirstOrDefault()?.firstConnectedPort;
            if (entryPort != null)
            {
                runtimeGraph.EntryNodeID = nodeIDMap[entryPort.GetNode()];
            }
        }
        //processar tots els nodes del graf excepte StarNode i EndNode
        foreach (var iNode in editorGraph.GetNodes())
        {
            if (iNode is StartNode || iNode is EndNode) continue;

            var runtimeNode = new RuntimeDialogueNode { NodeID = nodeIDMap[iNode] };
            if (iNode is DialogueNode dialogueNode)
            {
                ProcessDialogueNode(dialogueNode, runtimeNode, nodeIDMap);
            }
            else if (iNode is ChoiceNode choiseNode)
            {
                ProcessChoiceNode(choiseNode, runtimeNode, nodeIDMap);
            }

            runtimeGraph.AllNodes.Add(runtimeNode);

        }

        ctx.AddObjectToAsset("RuntimeData", runtimeGraph);
        ctx.SetMainObject(runtimeGraph);
    }
    //processa un DialogueNode extraient el text, el nomb del personatge i l'ID del node seguent
    private void ProcessDialogueNode(DialogueNode node, RuntimeDialogueNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.SpeakerName = GetPortValue<string>(node.GetInputPortByName("Speaker"));
        runtimeNode.DialogueText = GetPortValue<string>(node.GetInputPortByName("Dialogue"));

        var nextNodePort = node.GetOutputPortByName("out")?.firstConnectedPort;
        if (nextNodePort != null)
            runtimeNode.NextNodeID = nodeIDMap[nextNodePort.GetNode()];
    }
    //processa un ChoiceNode extraient el text, el nom del personatge i totes les opcions de resposta amb els seus valors
    //de confianca i events
    private void ProcessChoiceNode(ChoiceNode node, RuntimeDialogueNode runtimeNode, Dictionary<INode, string> nodeIDMap)
    {
        runtimeNode.SpeakerName = GetPortValue<string>(node.GetInputPortByName("Speaker"));
        runtimeNode.DialogueText = GetPortValue<string>(node.GetInputPortByName("Dialogue"));

        //processar cada port de sortida que correspon a una opcio de resposta
        var choiceOutputPorts = node.GetOutputPorts().Where(p => p.name.StartsWith("Choice "));
        foreach (var outputPort in choiceOutputPorts)
        {
            var index = outputPort.name.Substring("Choice ".Length);
            var textPort = node.GetInputPortByName($"Choice Text {index}");

            var choiceData = new ChoiceData
            {
                ChoiceText = GetPortValue<string>(textPort),
                //si el port de sorta no esta connectat, el node de desti es null i el dialeg finalitza
                DestinationNodeID = outputPort.firstConnectedPort != null ? nodeIDMap[outputPort.firstConnectedPort.GetNode()] : null,
                ValueChange = GetPortValue<int>(node.GetInputPortByName($"Choice Value {index}")),
                OnChoiceSelected = GetPortValue<GameEvent>(node.GetInputPortByName($"Choice Event {index}")) 
            };

            runtimeNode.Choices.Add(choiceData);
        }
    }

    //obte el valor d'un port
    private T GetPortValue<T>(IPort port)
    {
        if (port == null) return default;

        if (port.isConnected)
        {
            if (port.firstConnectedPort.GetNode() is IVariableNode variableNode)
            {
                variableNode.variable.TryGetDefaultValue(out T value);
                return value;
            }
        }

        port.TryGetValue(out T fallbackValue);
        return fallbackValue;
    }
}
