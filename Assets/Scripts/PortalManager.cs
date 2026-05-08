using UnityEngine;

public class PortalManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject portalPrefab;
    bool portalSpawned = false;
    public Vector3 portalPlacement;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time > 15 && !portalSpawned)
        {
            Instantiate(portalPrefab, portalPlacement, Quaternion.Euler(0,0,0));
            portalSpawned = true;
        }
    }
}
