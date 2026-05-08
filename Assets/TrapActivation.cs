using JetBrains.Annotations;
using UnityEngine;

public class TrapActivation : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    static int warningCount = 3;
    float warningSizeMultiplier = 2;
    static float warningTime = 6;
    float TimerStart;
    float sizePosition;
    public GameObject lava;

    
    void Start()
    {
        TimerStart = Time.time;
        Color color = GetComponent<Renderer>().material.color;
        color.a = 0.6f;
        GetComponent<Renderer>().material.color = color;


    }

    // Update is called once per frame
    void Update()
    {
        sizePosition = Mathf.Sin(Time.time * (Time.time / 10)) + 2;
        
        transform.localScale = new Vector3(sizePosition, sizePosition, sizePosition);
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
