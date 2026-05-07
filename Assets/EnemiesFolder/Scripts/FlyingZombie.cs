using UnityEngine;
using UnityEngine.AI;

public class FlyingZombie : MonoBehaviour
{
    //used for movement/rotation
    public float moveSpeed = 3f;
    public float rotateSpeed = 3f;
    //used to determine when the zombie is close enough to attack
    public float stopDistance = 1.0f;

    //used for enemy 3D movement since flying to home in on player
    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        //above may need to be updated
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            //there is no player
            return;
        }

        //sets movement destination to the player
        Vector3 destination = player.position - transform.position;
        float distance = destination.magnitude;

        //rotates towards the player
        if (destination != Vector3.zero)
        {
            //sets direction to face as the rotation to move towards the destination
            Quaternion facing = Quaternion.LookRotation(destination) * 
                Quaternion.Euler(0, 180f, 0);
            //adjusts rotation with Slerp
            transform.rotation = Quaternion.Slerp(transform.rotation, facing, 
                rotateSpeed * Time.deltaTime);
        }

        //stops when within attacking range
        if (distance > stopDistance)
        {
            Vector3 movement = destination.normalized * moveSpeed * Time.deltaTime;
            transform.position += movement;
        }
        else
        {
            //attack logic here
        }
    }
}
