using UnityEngine;
using UnityEngine.AI;

public class BasicZombie : MonoBehaviour
{
    //used to determine when the zombie is close enough to attack
    public float stopDistance = 1.0f;

    //used for enemy NavMesh movement
    private NavMeshAgent nav;
    private Transform player;

    void Start()
    {
        //assigns the enemy's NavMeshAgent for NavMesh movement
        nav = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        //above may need to be updated

        nav.updateRotation = true; //allows the enemy to rotate to face the player
        nav.updateUpAxis = false; //just to be safe since the game is top down
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
        nav.SetDestination(player.position);

        //stops when within attacking range
        if (nav.remainingDistance <= stopDistance)
        {
            nav.isStopped = true;
        }
        else
        {
            nav.isStopped = false;
            //attack logic here
        }
    }
}
