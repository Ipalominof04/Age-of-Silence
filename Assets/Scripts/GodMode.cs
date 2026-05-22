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
    private Rect ventana = new Rect(10, 10, 420, 320);
    private const int MAX_LINEAS = 30;

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
        godMap.FindAction("SubirConfianza").performed += _ => SubirConfianza(50);
        godMap.FindAction("BajarConfianza").performed += _ => BajarConfianza(50);
        godMap.FindAction("CurarJugador").performed += _ => CurarJugador();
        godMap.FindAction("MatarJugador").performed += _ => MatarJugador();
    }

    private void OnDestroy()
    {
        if (godMap == null) return;
        godMap.FindAction("Console").performed -= _ => ToggleConsole();
        godMap.FindAction("SubirConfianza").performed -= _ => SubirConfianza(50);
        godMap.FindAction("BajarConfianza").performed -= _ => BajarConfianza(50);
        godMap.FindAction("CurarJugador").performed -= _ => CurarJugador();
        godMap.FindAction("MatarJugador").performed -= _ => MatarJugador();
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
            // pequeño delay para que el log se vea antes de cerrar
            consolaActiva = false;
            godMap.Disable();
            // reactivamos solo Console para poder volver a abrir
            godMap.FindAction("Console").Enable();
        }
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
        ventana = GUI.Window(0, ventana, DibujarVentana, "God Mode Console");
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
        if (GUILayout.Button("Confianza +50")) SubirConfianza(50);
        if (GUILayout.Button("Confianza -50")) BajarConfianza(50);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Curar")) CurarJugador();
        if (GUILayout.Button("Matar")) MatarJugador();
        GUILayout.EndHorizontal();

        GUILayout.Space(4);
        if (GUILayout.Button("Cerrar [F12]")) ToggleConsole();

        GUI.DragWindow();
    }
}