// LuzVidaPlayer.cs
using UnityEngine;

public class LuzVidaPlayer : MonoBehaviour
{
    [SerializeField] private Light luzPlayer;
    [SerializeField] private GameEventFloat onVidaCambiada;

    private void OnEnable()
    {
        onVidaCambiada.Subscribe(ActualizarLuz);
    }
    private void OnDisable()
    {
        onVidaCambiada.Unsubscribe(ActualizarLuz);
    }

    private void ActualizarLuz(float porcentaje)
    {
        if (porcentaje <= 0.34f)
            luzPlayer.color = Color.red;
        else if (porcentaje <= 0.67f)
            luzPlayer.color = Color.orange; // naranja
        else
            luzPlayer.color = Color.green; // color original
    }
}