using UnityEngine;

public class GrabberZombie : MonoBehaviour
{
    //used for movement/rotation
    public float moveSpeed = 3f;
    public float rotateSpeed = 3f;

    public float grabDistance = 1f; //distance at which the grab triggers
    public float pullSpeed = 3f; //how fast the player is pulled in
    public float releaseDistance = 1f; //distance at which the pull stops
    public bool cooldown = false; //prevents instantly regrabbing the player
    //false when not on cooldown, true when on cooldown
    public float cooldownTimer = 0f;
    public float cooldownAmount = 10f;
    //both floats above are used to determine if the grab is still on cooldown

    //used to determine moving towards player
    private Transform player;
    private bool grabbedPlayer = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        //above may need to be updated
    }

    void Update()
    {
        if (player == null)
        {
            //there is no player
            return;
        }

        //code for if the grab is on cooldown
        if (cooldown)
        {
            //increments the cooldown timer
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer == cooldownAmount)
            {
                cooldown = false; //grab no longer on cooldown
                cooldownTimer = 0; //reset timer afterwards
            }
        }

        if (!grabbedPlayer)
        {
            //move towards player and check if player can be grabbed
            Move();
            CheckGrabCondition();
        }
        else
        {
            //grab and pull the player, staying in place while doing so
            PullPlayer();
        }
    }

    //code for moving
    void Move()
    {
        Vector3 destination = player.position - transform.position;
        destination.y = 0f; //keeps movement flat for top-down

        float distance = destination.magnitude;

        //rotate toward player
        if (destination != Vector3.zero)
        {
            //sets direction to face as the rotation to move towards the player
            Quaternion facing = Quaternion.LookRotation(destination);
            //adjusts rotation with Slerp
            transform.rotation = Quaternion.Slerp(transform.rotation, facing,
            rotateSpeed * Time.deltaTime);
        }

        //move until within grab range
        if (distance > grabDistance)
        {
            Vector3 movement = destination.normalized * moveSpeed * Time.deltaTime;
            transform.position += movement;
        }
    }

    //code for checking if close enough to grab the player
    void CheckGrabCondition()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= grabDistance && !cooldown)
        {
            grabbedPlayer = true;
            //include somewhere in here the code for the grabbing animation
            //also write code to disable player movement here
        }
    }

    //code to pull the player towards the grabber zombie
    void PullPlayer()
    {
        //assigns the direction of the zombie relative to the player
        Vector3 directionToZombie = transform.position - player.position;
        float distance = directionToZombie.magnitude;

        if (distance > releaseDistance)
        {
            //as long as the player is not within the release distance of the zombie,
            //it will continue to pull the player towards it
            Vector3 pull = directionToZombie.normalized * pullSpeed * Time.deltaTime;
            //may need to edit this for forced player movement
            //player.position += pull;
        }
        else
        {
            //release player, should exit forced movement if that is implemented
            cooldown = true; //grab on cooldown
            grabbedPlayer = false;
        }
    }
}
