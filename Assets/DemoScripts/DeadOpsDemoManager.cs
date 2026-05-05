using UnityEngine;
//DeadOpsDemoManager Script
public class DeadOpsDemoManager : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Zombie Spawning")]
    public float spawnInterval = 1.2f;
    public int maxZombies = 12;
    public float spawnRadius = 14f;

    public static int kills;
    public static int playerHits;

    private float nextSpawnTime;

    void Update()
    {
        if (player == null)
            return;

        if (Time.time >= nextSpawnTime && CountZombies() < maxZombies)
        {
            SpawnZombie();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    void SpawnZombie()
    {
        Vector2 randomCircle = Random.insideUnitCircle.normalized;

        if (randomCircle == Vector2.zero)
            randomCircle = Vector2.up;

        Vector3 spawnPosition = player.position + new Vector3(randomCircle.x, 0f, randomCircle.y) * spawnRadius;
        spawnPosition.y = 1f;

        GameObject zombie = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        zombie.name = "Prototype Zombie";
        zombie.transform.position = spawnPosition;

        Renderer zombieRenderer = zombie.GetComponent<Renderer>();
        if (zombieRenderer != null)
        {
            zombieRenderer.material.color = Color.red;
        }

        DemoZombie zombieAI = zombie.AddComponent<DemoZombie>();
        zombieAI.target = player;
        zombieAI.moveSpeed = Random.Range(2.5f, 4.2f);
    }

    int CountZombies()
    {
        return FindObjectsOfType<DemoZombie>().Length;
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 18;
        style.alignment = TextAnchor.MiddleLeft;

        string text =
            "Dead Ops Arcade Prototype\n" +
            "WASD: Move\n" +
            "Arrow Keys: Aim + Auto Shoot\n" +
            "Kills: " + kills + "\n" +
            "Player Hits: " + playerHits;

        GUI.Box(new Rect(10, 10, 360, 140), text, style);
    }
}

public class DemoZombie : MonoBehaviour
{
    public Transform target;
    public float moveSpeed = 3.2f;
    public float hitDistance = 1.2f;

    void Update()
    {
        if (target == null)
            return;

        Vector3 targetPosition = target.position;
        targetPosition.y = transform.position.y;

        Vector3 direction = targetPosition - transform.position;

        if (direction.magnitude <= hitDistance)
        {
            DeadOpsDemoManager.playerHits++;
            Destroy(gameObject);
            return;
        }

        Vector3 moveDirection = direction.normalized;

        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }

    public void Die()
    {
        DeadOpsDemoManager.kills++;
        Destroy(gameObject);
    }
}