
using UnityEngine;
using UnityEngine.Rendering;

public class TeleportationManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    Vector3 pos0 = new Vector3(8.5f, 3, 8.5f);
    Vector3 pos1 = new Vector3(8.5f, 3, -8.5f);
    Vector3 pos2 = new Vector3(-8.5f, 3, 8.5f);
    Vector3 pos3 = new Vector3(-8.5f, 3, -8.5f);
    Vector3[] pos;
    int currentPos = 0;
    public int health = 300;
    public GameObject portalPrefab;
    int tpHealth;
    void Start()
    {
        pos = new []{pos0, pos1, pos2, pos3};
        tpHealth = health - 25;
    }

    // Update is called once per frame
    void Update()
    {
        health = 300 - Mathf.FloorToInt(Time.time) * 5;
        if (health <= 0)
        {
            Instantiate(portalPrefab, new Vector3(8.5f, 2, 17), Quaternion.Euler(0, 0, 0));
            portalPrefab.GetComponent<SceneChanger>().scene = 1;

            Destroy(gameObject);
        }
        
        if (health + 1 <= tpHealth)
        {
            tpHealth -= 25;
            int futurePos = Mathf.FloorToInt(Random.value / 0.25f);
            while (futurePos == currentPos)
            {
                futurePos = Mathf.FloorToInt(Random.value / 0.25f);
            }
            float timeStart = Time.time;
            float timeDiff = 0;
            while(timeDiff < 2)
            {
                transform.localScale = new Vector3((float) (Mathf.Sin(timeDiff) * 0.5), (float) (Mathf.Sin(timeDiff) * 0.5), (float) (Mathf.Sin(timeDiff) * 0.5));
                timeDiff = Time.time - timeStart;
            }
            currentPos = futurePos;
            transform.position = pos[futurePos];
        }
        if (health + 10 <= tpHealth)
        {
            Instantiate(portalPrefab, new Vector3(8.5f, 2, 17), Quaternion.Euler(0, 0, 0));
            portalPrefab.GetComponent<SceneChanger>().scene = 1;

            Destroy(gameObject);
        }

    }
}
