using System;
using Unity.GraphToolkit.Editor;
using UnityEditor;

//defineix el tipus d'asset .dialoguegraph i el registra al menu de creacio
//d'assets de unity per poder crear nous grafs de dialeg des de l'editor
[Serializable]
[Graph(AssetExtension)]
public class DialogueGraph : Graph
{
    public const string AssetExtension = "dialoguegraph";

    //afegeix l'opcio "Create/Dialogue Graph" al menu d'assets de unity
    [MenuItem("Assets/Create/Dialogue Graph", false)]

    private static void CreateAssetFile()
    {
        GraphDatabase.PromptInProjectBrowserToCreateNewAsset<DialogueGraph>();
    }
}
