using UnityEngine;
using UnityEngine.AI;

public class ChargerZombie : MonoBehaviour
{
    //states for the charger zombie
    public enum ChargerState
    {
        Approaching,
        Windup,
        Charging,
        Recovering,
        Stunned
    }

    //speed for the charger zombie's charge
    public float chargeSpeed = 12f;

    //variables for the timing of the states
    public float windupTime = 1f;
    public float chargeDuration = 1f;
    public float recoveryTime = 2f;
    public float stunDuration = 2f;

    //how far the charger moves before charging
    public float chargeStopDistance = 1f;

    //used for enemy NaMesh movement
    private NavMeshAgent nav;
    private Transform player;

    //sets the default state
    private ChargerState state = ChargerState.Approaching;
    //timer for state transitions
    private float stateTimer = 0f;
    //charger charges in a straight line
    private Vector3 chargeDirection;

    void Start()
    {
        //assigns for NavMesh movement
        nav = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

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

        //state machine
        switch (state)
        {
            case ChargerState.Approaching:
                Approach();
                break;

            case ChargerState.Windup:
                Windup();
                break;

            case ChargerState.Charging:
                Charge();
                break;

            case ChargerState.Recovering:
                Recovery();
                break;

            case ChargerState.Stunned:
                Stun();
                break;
        }
    }

    //code for when the charger zombie approaches the player
    void Approach()
    {
        //moves towards the player with NavMesh
        nav.isStopped = false;
        nav.SetDestination(player.position);

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= chargeStopDistance)
        {
            EnterWindup();
        }
    }

    //code to enter windup
    void EnterWindup()
    {
        state = ChargerState.Windup;
        stateTimer = windupTime;

        //stops moving to indicate charging beginning
        nav.isStopped = true;

        //face the player
        Vector3 dir = (player.position - transform.position);
        dir.y = 0f;
        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }

        //lock in the charge direction
        chargeDirection = transform.forward;
    }

    //code to exit windup
    void Windup()
    {
        //stalls for the amount of time in windup, can't move while in windup
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            //enough time has passed, charger charges forward
            EnterCharge();
        }
    }

    //code to prepare to charge
    void EnterCharge()
    {
        state = ChargerState.Charging;
        stateTimer = chargeDuration;

        nav.isStopped = true; //manual movement is used for the charge
    }

    //code for the charge
    void Charge()
    {
        //charges continiously for a set time
        stateTimer -= Time.deltaTime;

        //move manually in a straight line
        transform.position += chargeDirection * chargeSpeed * Time.deltaTime;

        if (stateTimer <= 0f)
        {
            //enter recovery state
            EnterRecovery();
        }
    }

    //code to prepare recovery state before moving again
    void EnterRecovery()
    {
        state = ChargerState.Recovering;
        stateTimer = recoveryTime;

        //stopped while recovering
        nav.isStopped = true;
    }

    void Recovery()
    {
        //stalls during recovery time, can't move while recovering
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            //begin moving again
            state = ChargerState.Approaching;
        }
    }

    //code for when the charger becomes stunned
    void EnterStun()
    {
        state = ChargerState.Stunned;
        stateTimer = stunDuration;

        nav.isStopped = true; //can't move while stunned
    }

    //code for when the charger is stunned
    void Stun()
    {
        //stalls for duration of stun, can't move while stunned
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            //begin moving again
            state = ChargerState.Approaching;
        }
    }

    //used for collision handling when hitting a wall and becoming stunned
    void OnCollisionEnter(Collision collision)
    {
        //can only get stunned while charging
        if (state == ChargerState.Charging)
        {
            if (collision.collider.CompareTag("Wall")) //this may get changed
            {
                EnterStun();
            }
        }
    }
}
