using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private string doorID;
    private void Start()
    {
        if (GameManager.Instance.lastDoorID == doorID)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = transform.position;
                player.transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            }  
        }
    }
}
