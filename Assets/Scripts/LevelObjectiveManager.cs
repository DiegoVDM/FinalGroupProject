using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelObjectiveManager : MonoBehaviour
{
    [SerializeField] private int requiredKills = 3;
    [SerializeField] private string nextSceneName;
    [SerializeField] private string winSceneName = "WinScene";
    [SerializeField] private string deathSceneName = "DeathScene";
    [SerializeField] private bool isFinalLevel;
    [SerializeField] private Text objectiveText;
    [SerializeField] private PlayerHealth playerHealth;

    private int startingKills;
    private int lastDisplayedZombiesLeft = -1;
    private bool hasCompletedObjective;
    private bool hasLoadedDeathScene;

    void Start()
    {
        startingKills = DeadOpsDemoManager.kills;

        if (playerHealth == null)
            playerHealth = PlayerHealth.Instance;

        UpdateObjectiveText();
    }

    void Update()
    {
        if (!hasLoadedDeathScene && playerHealth != null && playerHealth.IsDead)
        {
            hasLoadedDeathScene = true;
            SceneManager.LoadScene(deathSceneName);
            return;
        }

        UpdateObjectiveText();

        if (!hasCompletedObjective && GetZombiesLeft() <= 0)
        {
            hasCompletedObjective = true;
            SceneManager.LoadScene(isFinalLevel ? winSceneName : nextSceneName);
        }
    }

    int GetLevelKills()
    {
        return Mathf.Max(0, DeadOpsDemoManager.kills - startingKills);
    }

    int GetZombiesLeft()
    {
        return Mathf.Max(0, requiredKills - GetLevelKills());
    }

    void UpdateObjectiveText()
    {
        int zombiesLeft = GetZombiesLeft();
        if (lastDisplayedZombiesLeft == zombiesLeft)
            return;

        lastDisplayedZombiesLeft = zombiesLeft;

        if (objectiveText != null)
            objectiveText.text = string.Format("Zombies left: {0}", zombiesLeft);
    }
}
