using UnityEngine;

[CreateAssetMenu(menuName = "ObjetoAgarrado", fileName = "ObjetoAgarrado")]
public class ObjetoAgarrado : ScriptableObject
{

    public GameObject objeto;


    public void GuardarObjeto(GameObject obj)
    {
        objeto = obj;
    }

    public void UsarObjeto()
    {
        objeto = null;

    }

}
