using System;
using Unity.GraphToolkit.Editor;
using UnityEngine;

//node d'entrada del graf, indica per on comenca el dialeg
[Serializable]
public class StartNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        //port de sortida que es connecta al primer node de la conversa
        context.AddOutputPort("out").Build();
    }
}

//node de sortida del graf, indica on acaba el dialeg
[Serializable]
public class EndNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        //port d'entrada que rep la connexio de l'ultim node de la conversa
        context.AddInputPort("in").Build();
    }
}

//node de dialeg simple per a dialeg sense opcio de resposta
[Serializable]
public class DialogueNode : Node
{
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();
        context.AddOutputPort("out").Build();
        //port per al nom del personatge que parla
        context.AddInputPort<string>("Speaker").Build();
        //por per al text del dialeg
        context.AddInputPort<string>("Dialogue").Build();
    }

}
//node de decisio amb multiples opcions de resposta configurables
[Serializable]
public class ChoiceNode : Node
{
    //identificador de l'opcio que controla el nombre de ports d'opcio
    const string optionID = "portCount";
    protected override void OnDefinePorts(IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();

        context.AddInputPort<string>("Speaker").Build();
        context.AddInputPort<string>("Dialogue").Build();

        //generar dinamicament els ports de cada opcio de resposta
        var option = GetNodeOptionByName(optionID);
        option.TryGetValue(out int portCount);
        for (int i = 0; i < portCount; i++)
        {
            //text que es mostrara al boto de resposta
            context.AddInputPort<string>($"Choice Text {i}").Build();
            //valor de canvi de confianca al seleccionar aquesta opcio
            context.AddInputPort<int>($"Choice Value {i}").Build();
            //event opcional que es llanca al seleccionar aquesta opcio
            context.AddInputPort<GameEvent>($"Choice Event {i}").Build();
            //port de sortida que es connecta al node de desti d'aquesta opcio
            context.AddOutputPort($"Choice {i}").Build();
        }
    }

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        //opcio configurable per definir el nombre d'opcions de resposta del node
        context.AddOption<int>(optionID);
    }
}