using UnityEngine;

// DeadOpsDemoManager Script
public class DeadOpsDemoManager : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Zombie Spawning")]
    public float spawnInterval = 1.2f;
    public int maxZombies = 12;
    public float spawnRadius = 14f;

    [Header("Money")]
    [SerializeField] private int moneyPerZombieKill = 25;

    public static int kills;
    public static int playerHits;
    public static int money;

    private const string MoneySaveKey = "DeadOpsMoney";

    private static DeadOpsDemoManager instance;

    private float nextSpawnTime;

    public static int Money => money;

    void Awake()
    {
        instance = this;
        LoadMoney();
    }

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

    public static void RegisterZombieKill()
    {
        kills++;

        int rewardAmount = 25;

        if (instance != null)
        {
            rewardAmount = instance.moneyPerZombieKill;
        }

        AddMoney(rewardAmount);
    }

    public static void AddMoney(int amount)
    {
        if (amount <= 0)
            return;

        money += amount;
        SaveMoney();
    }

    public static bool TrySpendMoney(int amount)
    {
        if (amount <= 0)
            return false;

        if (money < amount)
            return false;

        money -= amount;
        SaveMoney();
        return true;
    }

    public static void ResetMoney()
    {
        money = 0;
        SaveMoney();
    }

    static void LoadMoney()
    {
        money = PlayerPrefs.GetInt(MoneySaveKey, 0);
    }

    static void SaveMoney()
    {
        PlayerPrefs.SetInt(MoneySaveKey, money);
        PlayerPrefs.Save();
    }

    void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
        {
            SaveMoney();
        }
    }

    void OnApplicationQuit()
    {
        SaveMoney();
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 18;
        style.alignment = TextAnchor.UpperLeft;
        style.normal.textColor = Color.white;

        string text =
            "Kills: " + kills + "\n" +
            "Money: $" + money;

        GUI.Label(new Rect(10, 10, 220, 60), text, style);
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
            PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(playerHealth.DamagePerHit);
            }

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
        DeadOpsDemoManager.RegisterZombieKill();
        Destroy(gameObject);
    }
}