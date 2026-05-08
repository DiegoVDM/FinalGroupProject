using UnityEngine;

public class TrapManager: MonoBehaviour
{
    float goalTimer;
    bool startTraps = false;
    float startTimer = 0;
    public GameObject trapPrefab;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        goalTimer = 5;
        
        
    }

    // Update is called once per frame
    void Update()
    {
        startTraps = Time.time > goalTimer + startTimer;
        //print(goalTimer);
        //print(Time.time - startTimer);
        if(startTraps)
        {
            goalTimer =  4 + (Random.value * 10/(0.5f * Time.time));
            startTimer = Time.time;
            print("Reached Goal!");
            Instantiate(trapPrefab, new Vector3(Random.value * 10 - 5, 1, Random.value * 10 - 5), transform.rotation);
            //Instantiate(trapPrefab, new Vector3(1.5f, 1, 1.5f), transform.rotation);

        }

        
    }
}
