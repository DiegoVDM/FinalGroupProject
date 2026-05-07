using UnityEngine;
using UnityEngine.AI;

public class PowerUpZombie : MonoBehaviour
{
    //used to determine if the zombie should focus the player or other zombies
    public float attackRange = 1f;
    public float playerFocusRange = 10f;

    //used to increase the health and damage of the zombie
    //after killing another zombie
    public float damageIncreasePerKill = 5f;
    public float healthIncreasePerKill = 20f;
    //include here private variables for health and damage code

    //used for enemy NavMesh movement
    private NavMeshAgent nav;
    private Transform player;

    //used to determine the current target to move towards
    private bool focusingPlayer = false;
    private Transform currentTarget;

    void Start()
    {
        nav = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        //include in here code to associate with health and damage

        nav.updateRotation = true; //allows the enemy to rotate to face the player
        nav.updateUpAxis = false; //just to be safe since the game is top down
    }

    void Update()
    {
        if (player == null)
        {
            //there is no player
            return;
        }

        SelectTarget();
        MoveAndAttack();
    }

    //code for selecting the target to move to and attack
    void SelectTarget()
    {
        float playerDistance = Vector3.Distance(transform.position, player.position);

        //if already focusing the player, stay focused unless they leave the range
        if (focusingPlayer)
        {
            if (playerDistance > playerFocusRange)
            {
                focusingPlayer = false; //player escaped, focus zombies nearby
            }
            //otherwise keep focusing player
            currentTarget = player;
            return;
        }

        //find nearest zombie that isn't this zombie
        Transform nearestZombie = FindNearestZombie();

        //assign distance to nearest zombie
        float distanceToZombie = Vector3.Distance(transform.position, 
            nearestZombie.position);

        //if player is closer than any zombie,
        //switch to player until they leave the range
        if (playerDistance < distanceToZombie)
        {
            focusingPlayer = true;
            currentTarget = player;
        }
        else
        {
            currentTarget = nearestZombie;
        }
    }

    //code to find the nearest zombie to attack
    Transform FindNearestZombie()
    {
        //an array of every zombie in the scene
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("Zombie");

        Transform nearest = null;
        float minDist = Mathf.Infinity;

        //goes through all possible zombies
        foreach (GameObject z in zombies)
        {
            if (z.transform == transform)
                continue; //skip self

            float dist = Vector3.Distance(transform.position, z.transform.position);

            //assigns closest zombie based on comparison to current closest
            if (dist < minDist)
            {
                minDist = dist;
                nearest = z.transform;
            }
        }
        //closest zombie selected
        return nearest;
    }

    //code for moving towards current target and attacking
    void MoveAndAttack()
    {
        if (currentTarget == null)
        {
            //there is no current target
            return;
        }

        //travel to nearest target via NavMesh
        nav.SetDestination(currentTarget.position);

        float distance = Vector3.Distance(transform.position, 
            currentTarget.position);

        if (distance <= attackRange)
        {
            //close enough to attack
            nav.isStopped = true;
            AttackTarget();
        }
        else
        {
            nav.isStopped = false;
        }
    }

    void AttackTarget()
    {
        if (currentTarget == null)
        {
            //there is no current target
            return;
        }

        //include code for dealing damage/attacking

        //if the zombie died, power up
        /*if (logic for zombie dying)
        {
            OnZombieKilled();
        }*/
    }

    void OnZombieKilled()
    {
        //include code for increasing health and damage

        //prepare to hunt next target
        currentTarget = null;
    }
}
