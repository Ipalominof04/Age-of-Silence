using UnityEngine;
using UnityEngine.Playables;

public class DespuesCinematica : MonoBehaviour
{
    public PlayableDirector cine;
    public float duracionCinematica;
    float t;

    public GameObject cineObj; //obj q tiene tod lo q acabamos de animar
    public GameObject gameplayObj; // contenido del gamelay
    void Start()
    {
        duracionCinematica = (float)cine.duration;
       // gameplayObj.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime;
        if(t>= duracionCinematica)
        {
            cineObj.SetActive(false);
            gameplayObj.SetActive(true);
            Destroy(this.gameObject);
        }
    }
}
