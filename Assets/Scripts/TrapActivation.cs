using JetBrains.Annotations;
using UnityEngine;

public class TrapActivation : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    float TimerStart;
    float sizePosition;
    public GameObject lava;
    public float opacity;
    float timeMultiplier;
    
    void Start()
    {
        TimerStart = Time.time;
        Color color = GetComponent<Renderer>().material.color;
        color.a = opacity;
        GetComponent<Renderer>().material.color = color;


    }

    // Update is called once per frame
    void Update()
    {
        timeMultiplier = Time.time * (Time.time / 20);
        sizePosition = Mathf.Sin(timeMultiplier) + 2;
    
        transform.localScale = new Vector3(sizePosition, 0.1f, sizePosition);
        if (Time.time - TimerStart > 2 + 2 * Mathf.Exp(-0.01f * Time.time))
        {
            print(2 + 10 * Mathf.Exp(-0.1f * Time.time));
            print(Time.time - TimerStart);
            Instantiate(lava, transform.position, transform.rotation);
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
            Debug.Log("Player In Warning Zone");
            //other.GetComponent<PlayerStats>().health -= damage;
        }
    }
}
