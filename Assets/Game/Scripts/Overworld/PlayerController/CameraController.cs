using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Référence du player.
    public Transform player;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Met à jour les positions x, y et z de la caméra regardant le player
        transform.position = new Vector3(player.position.x, player.position.y, -10);
    }
}
