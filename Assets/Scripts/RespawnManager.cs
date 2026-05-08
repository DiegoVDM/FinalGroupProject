using Unity.VisualScripting;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    PlayerStats stats;
    Vector3 startingPos = new Vector3(-13, 3.25f, -13);
    void Start()
    {
        stats = GetComponent<PlayerStats>();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < 1)
        {
            stats.health -= 25;
            transform.position = startingPos;

        }
    }
}
