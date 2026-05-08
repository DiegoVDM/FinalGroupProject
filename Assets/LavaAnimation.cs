using UnityEngine;

public class LavaAnimation : MonoBehaviour
{
    public Material mat;
    public float speed = 1f;

    float spread = 0;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
    }
    void Update()
    {
        spread += Time.deltaTime * speed;
        mat.SetFloat("_Spread", spread);
    }
}
