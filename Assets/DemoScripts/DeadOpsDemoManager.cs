using UnityEngine;
using UnityEngine.AI;

// DeadOpsDemoManager Script
public class DeadOpsDemoManager : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Zombie Spawning")]
    public float spawnInterval = 1.2f;
    public int maxZombies = 12;
    public float spawnRadius = 14f;
    [SerializeField] private GameObject zombieVisualPrefab;

    [Header("Money")]
    [SerializeField] private int moneyPerZombieKill = 25;

    public static int kills;
    public static int playerHits;
    public static int money;

    private const string MoneySaveKey = "DeadOpsMoney";

    private static DeadOpsDemoManager instance;

    private float nextSpawnTime;

    public static int Money => money;

    /// <summary> Reward per kill used by the demo; safe after demo manager is destroyed (defaults to 25). </summary>
    public static int GetMoneyPerZombieKill()
    {
        return instance != null ? instance.moneyPerZombieKill : 25;
    }

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

        // Use the assigned Player root height instead of forcing every map to Y = 1.
        // This lets zombies spawn correctly on maps where the floor is not at world Y = 0.
        spawnPosition.y = player.position.y;

        GameObject zombie = new GameObject("Zombie");
        zombie.transform.position = spawnPosition;

        CapsuleCollider zombieCollider = zombie.AddComponent<CapsuleCollider>();
        zombieCollider.height = 2f;
        zombieCollider.radius = 0.5f;
        zombieCollider.center = Vector3.up;
        zombieCollider.isTrigger = false;

        Rigidbody zombieRigidbody = zombie.AddComponent<Rigidbody>();
        zombieRigidbody.useGravity = false;
        zombieRigidbody.isKinematic = true;
        zombieRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        if (zombieVisualPrefab != null)
            CreateZombieVisual(zombie.transform);
        else
            CreateFallbackZombieVisual(zombie.transform);

        DemoZombie zombieAI = zombie.AddComponent<DemoZombie>();
        zombieAI.target = player;
        zombieAI.moveSpeed = Random.Range(2.5f, 4.2f);
    }

    void CreateZombieVisual(Transform root)
    {
        GameObject visual = Instantiate(zombieVisualPrefab, root);
        visual.name = zombieVisualPrefab.name;
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;

        DisableVisualBehaviorComponents(visual);
    }

    void CreateFallbackZombieVisual(Transform root)
    {
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.name = "Prototype Zombie Visual";
        visual.transform.SetParent(root, false);
        visual.transform.localPosition = Vector3.up;
        visual.transform.localRotation = Quaternion.identity;

        Collider visualCollider = visual.GetComponent<Collider>();
        if (visualCollider != null)
            Destroy(visualCollider);

        Renderer zombieRenderer = visual.GetComponent<Renderer>();
        if (zombieRenderer != null)
            zombieRenderer.material.color = Color.red;
    }

    void DisableVisualBehaviorComponents(GameObject visual)
    {
        foreach (BasicZombie zombieBehavior in visual.GetComponentsInChildren<BasicZombie>(true))
            zombieBehavior.enabled = false;

        foreach (ChargerZombie zombieBehavior in visual.GetComponentsInChildren<ChargerZombie>(true))
            zombieBehavior.enabled = false;

        foreach (FlyingZombie zombieBehavior in visual.GetComponentsInChildren<FlyingZombie>(true))
            zombieBehavior.enabled = false;

        foreach (GrabberZombie zombieBehavior in visual.GetComponentsInChildren<GrabberZombie>(true))
            zombieBehavior.enabled = false;

        foreach (PowerUpZombie zombieBehavior in visual.GetComponentsInChildren<PowerUpZombie>(true))
            zombieBehavior.enabled = false;

        foreach (NavMeshAgent agent in visual.GetComponentsInChildren<NavMeshAgent>(true))
            agent.enabled = false;
    }

    int CountZombies()
    {
        return FindObjectsOfType<DemoZombie>().Length;
    }

    public static void RegisterZombieKill()
    {
        kills++;

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.RegisterKill(1);
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
        style.fontSize = 26;
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
