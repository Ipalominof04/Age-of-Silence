using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GodMode : MonoBehaviour
{
    public static GodMode Instance;

    private InputActionMap godMap;
    private bool consolaActiva = false;
    private List<string> logs = new List<string>();
    private Vector2 scrollPos;
    private Rect ventana = new Rect(10, 10, 420, 0);
    private const int MAX_LINEAS = 30;
    private bool mostrarObjetos = false;
    private bool mostrarEscenas = false;
    public bool Invencible = false;

    private readonly string[] nombre_objetos = {
    "Rama", "Palanca", "Cizallas", "FusibleVerde", "FusibleAzul","FusibleAmarillo", "Galon", "Cerillas","Palet","MaderasPuente","MaderaPequeña"
    };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        godMap = InputManager.Instance.GodModeMap;
        godMap.FindAction("Console").performed += _ => ToggleConsole();
    }


    private void OnDestroy()
    {
        if (godMap == null) return;
        godMap.FindAction("Console").performed -= _ => ToggleConsole();
    }

    // --- Toggle ---
    private void ToggleConsole()
    {
        consolaActiva = !consolaActiva;

        if (consolaActiva)
        {
            godMap.Enable();
            Log("God Mode activado");
        }
        else
        {
            Log("God Mode desactivado");
            consolaActiva = false;
            godMap.Disable();
            godMap.FindAction("Console").Enable();
        }
    }

    private void EquiparObjeto(string nombreObjeto)
    {
        var player = PlayerController.Instance;
        if (player == null) { Log("[ERROR] No hay player"); return; }

        var datos = GameManager.Instance.datos;
        if (datos.objetoMano == null) { Log("[ERROR] objetoMano es null"); return; }

        datos.objetoMano.nombreObjeto = nombreObjeto;
        Log($"Objeto '{nombreObjeto}' añadido a la mano");
    }

    // --- Comandos ---
    private void SubirConfianza(int cantidad)
    {
        GameManager.Instance.SumValor(cantidad);
        Log($"Confianza +{cantidad} → {GameManager.Instance.confianza}");
    }

    private void BajarConfianza(int cantidad)
    {
        GameManager.Instance.RestValor(cantidad);
        Log($"Confianza -{cantidad} → {GameManager.Instance.confianza}");
    }

    private void CurarJugador()
    {
        var player = PlayerController.Instance;
        if (player == null) { Log("[ERROR] No hay player en escena"); return; }
        player.Curar(6);
        Log($"Jugador curado → {player.vidaActual} HP");
    }

    private void MatarJugador()
    {
        var player = PlayerController.Instance;
        if (player == null) { Log("[ERROR] No hay player en escena"); return; }
        player.TakeDamage(9999f);
        Log("Jugador eliminado");
    }

    private void EscenaFinal()
    {
        GameManager.Instance.ChangeScene("EscenaFinal");
        Log("Teletransporte a la escena final...");
    }
    private void Escena4()
    {
        GameManager.Instance.lastDoorID = "escena4";
        GameManager.Instance.ChangeScene("Escena4");
        Log("Teletransporte a la escena 4...");
    }
    private void EntradaFabrica()
    {
        GameManager.Instance.lastDoorID = "entrada";
        GameManager.Instance.ChangeScene("Entrada");
        Log("Teletransporte a la entrada de la fabrica...");
    }
    private void ExteriorGasolinera()
    {
        GameManager.Instance.lastDoorID = "gasolinera";
        GameManager.Instance.ChangeScene("ExteriorGasolinera");
        Log("Teletransporte al exterior de la gasolinera...");
    }



    // --- Log ---
    public void Log(string mensaje)
    {
        string linea = $"[{DateTime.Now:HH:mm:ss}] {mensaje}";
        logs.Add(linea);
        if (logs.Count > MAX_LINEAS)
            logs.RemoveAt(0);
        Debug.Log($"[GodMode] {mensaje}");
    }

    // --- UI ---
    private void OnGUI()
    {
        if (!consolaActiva) return;
        ventana = GUILayout.Window(0, ventana, DibujarVentana, "God Mode Console");
    }

    private void DibujarVentana(int id)
    {
        var player = PlayerController.Instance;

        GUILayout.BeginHorizontal();
        GUILayout.Label($"Confianza: {GameManager.Instance.confianza}");
        GUILayout.Label($"Vida: {(player != null ? player.vidaActual.ToString("F1") : "—")}");
        GUILayout.EndHorizontal();

        GUILayout.Space(4);

        scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(160));
        foreach (var linea in logs)
            GUILayout.Label(linea);
        GUILayout.EndScrollView();

        GUILayout.Space(4);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Confianza +15")) SubirConfianza(15);
        if (GUILayout.Button("Confianza -15")) BajarConfianza(15);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Curar")) CurarJugador();
        if (GUILayout.Button(Invencible ? "Invencible: <color='green'>ON</color>" : "Invencible: <color='red'>OFF</color>"))
        {
            Invencible = !Invencible;
            Log($"Invencibilidad {(Invencible ? "<color='green'>activada</color>" : "<color='red'>desactivada</color>")}");
        }
        if (GUILayout.Button("Matar")) MatarJugador();
        GUILayout.EndHorizontal();

        if (GUILayout.Button(mostrarEscenas ? "▲ Escenas" : "▼ Escenas"))
        {
            mostrarEscenas = !mostrarEscenas;
            ventana.height = 0;
        }

        if (mostrarEscenas)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            if (GUILayout.Button("Escena 4")) Escena4();
            if (GUILayout.Button("Entrada fabrica")) EntradaFabrica();
            if (GUILayout.Button("Exterior gasolinera")) ExteriorGasolinera();
            if (GUILayout.Button("Escena Final")) EscenaFinal();
            GUILayout.EndVertical();
        }

        GUILayout.Space(4);

        if (GUILayout.Button(mostrarObjetos ? "▲ Objetos" : "▼ Objetos"))
        {
            mostrarObjetos = !mostrarObjetos;
            ventana.height = 0;
        }

        if (mostrarObjetos)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            foreach (var obj in nombre_objetos)
            {
                if (GUILayout.Button(obj))
                    EquiparObjeto(obj);
            }
            if (GUILayout.Button("[ Vaciar mano ]"))
            {
                GameManager.Instance.datos.objetoMano.ClearObject();
                Log("Mano vaciada");
            }
            GUILayout.EndVertical();
        }
    }
}