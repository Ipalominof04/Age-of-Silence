using System.Collections;
using UnityEngine;

public class SoundWavesController : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;

    //arrays de texturas para cada estado
    [SerializeField] private Texture2D[] walkTextures;
    [SerializeField] private Texture2D[] runTextures;
    [SerializeField] private Texture2D[] shootTextures;

    //velocidad de animación para cada estado
    [SerializeField] private float walkFrameRate;
    [SerializeField] private float runFrameRate;
    [SerializeField] private float shootFrameRate;

    private Coroutine animCoroutine;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    //se llama desde PlayerController con: 0 = andar, 1 = correr, 2 = disparar, -1 nada
    public void SetWaveState(int index)
    {
        //parar la animacion anterior antes de iniciar la nueva
        if (animCoroutine != null)
            StopCoroutine(animCoroutine);

        switch (index)
        {
            //walk: animacion en bucle
            case 0: animCoroutine = StartCoroutine(Animate(walkTextures, walkFrameRate, true)); break;
            //run: animacion en bucle
            case 1: animCoroutine = StartCoroutine(Animate(runTextures, runFrameRate, true)); break;
            //shoot: animacion una sola vez y se desactiva
            case 2: animCoroutine = StartCoroutine(Animate(shootTextures, shootFrameRate, false)); break;
        }
    }

    //recorre los frames cambiando la textura del material que tiene el sprite renderer del quad para que cambie la textura de la onda
    //si loop es true se repite infinitamente, si es false se reproduce una sola vez y desactiva el quad
    private IEnumerator Animate(Texture2D[] frames, float frameRate, bool loop)
    {
        while (true)
        {
            //recorre todos los frames de la animacion
            foreach (Texture2D frame in frames)
            {
                //aplicar la textura al material del quad
                meshRenderer.material.SetTexture("_UnlitColorMap", frame);
                meshRenderer.material.SetTexture("_EmissiveColorMap", frame);
                //esperar el tiempo entre frames dependiendo del estado
                yield return new WaitForSeconds(frameRate);
            }

            //si no es bucle salimos del while
            if (!loop) break;
        }

        //desactivar el quad cuando acaba el shoot (pq en walk y run nunca llega aqui y se desactiva directamente desde el player cuando le envia el -1)
        gameObject.SetActive(false);
    }

    //forzar la rotación para que el quad no rote con el player aunque sea hijo de este
    void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(90f, 0f, 180f);
    }
}