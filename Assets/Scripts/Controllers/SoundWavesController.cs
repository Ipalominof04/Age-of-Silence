using System.Collections;
using UnityEngine;

public class SoundWavesController : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;

    //arrays de textures per a cada estat
    [SerializeField] private Texture2D[] walkTextures;
    [SerializeField] private Texture2D[] runTextures;
    [SerializeField] private Texture2D[] shootTextures;

    //velocitat animaci per a cada estat
    [SerializeField] private float walkFrameRate;
    [SerializeField] private float runFrameRate;
    [SerializeField] private float shootFrameRate;

    private Coroutine animCoroutine;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    //es crida desde PlayerController amb: 0 = andar, 1 = correr, 2 = disparar, -1 nada
    public void SetWaveState(int index)
    {
        //parar la animacio anterior abans d'iniciar la nova
        if (animCoroutine != null)
            StopCoroutine(animCoroutine);

        switch (index)
        {
            //walk: animacio en bucle
            case 0: animCoroutine = StartCoroutine(Animate(walkTextures, walkFrameRate, true)); break;
            //run: animacio en bucle
            case 1: animCoroutine = StartCoroutine(Animate(runTextures, runFrameRate, true)); break;
            //shoot: animacio una sola vegada i es desactiva
            case 2: animCoroutine = StartCoroutine(Animate(shootTextures, shootFrameRate, false)); break;
        }
    }

    // recorre els frames cambiant la textura del material q te el sprite renderer del quad perque canvii la textura de l'ona
    // si loop es true es repeteix infinitament, si es false es reprodueix una sola vegada i desactiva el quad
    private IEnumerator Animate(Texture2D[] frames, float frameRate, bool loop)
    {
        while (true)
        {
            //recorre tots els frames de l'animacio
            foreach (Texture2D frame in frames)
            {
                //aplicar la textura al material del quad
                meshRenderer.material.SetTexture("_UnlitColorMap", frame);
                meshRenderer.material.SetTexture("_EmissiveColorMap", frame);
                //esperar el temps entre frames depenent de l'estat
                yield return new WaitForSeconds(frameRate);
            }

            //si no es bucle sortim del while
            if (!loop) break;
        }

        //desactivar el quad quan acaba el shoot (pq en walk i run mai arriba aqui i es desactiva directament des del player quan envia el -1)
        gameObject.SetActive(false);
    }

    //forzar la rotación para que el quad no rote con el player aunque sea hijo de este
    void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(90f, 0f, 180f);
    }
}