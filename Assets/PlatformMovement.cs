using Unity.VisualScripting;
using UnityEngine;

public class PlatformMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    Rigidbody rb;
    bool forward;
    public float Xspeed = -1;
    public float Zspeed = 2;
    public float XTarget = 0;
    public float ZTarget = 12;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        forward = true;
        //if(Xspeed < 0)
        //{
        //    Zspeed = Mathf.FloorToInt(Random.Range(2, 5));
        //}
        //if (Zspeed < 0)
        //{
        //    Xspeed = Mathf.FloorToInt(Random.Range(2, 5));
        //}

    }

    // Update is called once per frame
    void Update()
    {
        
        if (forward && Xspeed < 0)
        {
            
            rb.linearVelocity = new Vector3(0, 0, Zspeed);
            if(transform.position.z > ZTarget)
            {
                //Zspeed = Mathf.FloorToInt(Random.Range(2, 5));
                forward = false;
            }
        }
        else if(!forward && Xspeed < 0)
        {
            
            rb.linearVelocity = new Vector3(0, 0, -Zspeed);
            if (transform.position.z < -ZTarget)
            {
                //Zspeed = Mathf.FloorToInt(Random.Range(2, 5));
                forward = true;
            }
        }
        else if (forward && Zspeed < 0)
        {
            
            rb.linearVelocity = new Vector3(Xspeed, 0, 0);
            if (transform.position.x > XTarget)
            {
                //Xspeed = Mathf.FloorToInt(Random.Range(2, 5));
                forward = false;
            }
        }
        else if (!forward && Zspeed < 0)
        {
            rb.linearVelocity = new Vector3(-Xspeed, 0, 0);
            if (transform.position.x < -XTarget)
            {
                //Xspeed = Mathf.FloorToInt(Random.Range(2, 5));
                forward = true;
            }
        }

    }
}
