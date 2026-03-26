using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private string doorID;
    [SerializeField] private float aidenOffset = 1f;

    public string GetDoorID()
    {
        return doorID;
    }

    public float AidenOffset
    {
        get { return aidenOffset; }
    }

    //private void Start()
    //{
    //    if (GameManager.Instance.lastDoorID == doorID)
    //    {
    //        GameObject player = GameObject.FindGameObjectWithTag("Player");
    //        GameObject aiden = GameObject.FindGameObjectWithTag("Aiden");
    //        if (player != null && aiden != null)
    //        {
    //            player.transform.position = transform.position;
    //            player.transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
    //            aiden.transform.position = transform.position + (-transform.right * aidenOffset);
    //            aiden.transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
    //        }  
    //    }
    //}
}
