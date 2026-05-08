using UnityEngine;
using UnityEngine.UIElements;

public class LavaScript : MonoBehaviour
{
    public float damage = 20f;
    public float TimerStart;
    float currTime = 0;
    float timeFunction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TimerStart = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        currTime = Time.time - TimerStart;
        timeFunction = 0.8f + 16 / Time.time;
        transform.localScale = new Vector3(1 + (2 * currTime / timeFunction), 0.1f, 1 + (2 * currTime / timeFunction));
        
        if (currTime > timeFunction)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerStay(Collider other)
    {
        // 'other' is the Collider that just entered this trigger
        print("Object entered: " + other.name);

        // Common practice: Check for a specific Tag to filter results
        if (other.CompareTag("Player"))
        {
            Debug.Log("The Player has entered the zone!");
            other.GetComponent<PlayerStats>().health -= damage;
        }
    }
}
