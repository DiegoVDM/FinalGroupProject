using UnityEngine;

public class TwinStickPlayer : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;

    [Header("Shooting")]
    public float bulletSpeed = 16f;
    public float fireRate = 0.12f;

    private float nextFireTime;
    private Vector3 lastAimDirection = Vector3.forward;

    void Start()
    {
        SetupPlayerVisual();
    }

    void Update()
    {
        MovePlayer();
        AimAndShoot();
    }

    void MovePlayer()
    {
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
            moveDirection += Vector3.forward;

        if (Input.GetKey(KeyCode.S))
            moveDirection += Vector3.back;

        if (Input.GetKey(KeyCode.A))
            moveDirection += Vector3.left;

        if (Input.GetKey(KeyCode.D))
            moveDirection += Vector3.right;

        moveDirection.Normalize();

        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        // If not shooting, face movement direction
        if (moveDirection.sqrMagnitude > 0.01f && !IsShootingInputHeld())
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }

    void AimAndShoot()
    {
        Vector3 aimDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.UpArrow))
            aimDirection += Vector3.forward;

        if (Input.GetKey(KeyCode.DownArrow))
            aimDirection += Vector3.back;

        if (Input.GetKey(KeyCode.LeftArrow))
            aimDirection += Vector3.left;

        if (Input.GetKey(KeyCode.RightArrow))
            aimDirection += Vector3.right;

        if (aimDirection.sqrMagnitude > 0.01f)
        {
            aimDirection.Normalize();
            lastAimDirection = aimDirection;

            transform.rotation = Quaternion.LookRotation(aimDirection);

            if (Time.time >= nextFireTime)
            {
                FireBullet(aimDirection);
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    bool IsShootingInputHeld()
    {
        return Input.GetKey(KeyCode.UpArrow)
            || Input.GetKey(KeyCode.DownArrow)
            || Input.GetKey(KeyCode.LeftArrow)
            || Input.GetKey(KeyCode.RightArrow);
    }

    void FireBullet(Vector3 direction)
    {
        GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bullet.name = "Prototype Bullet";

        bullet.transform.position = transform.position + direction * 0.9f + Vector3.up * 0.2f;
        bullet.transform.localScale = Vector3.one * 0.3f;

        Renderer bulletRenderer = bullet.GetComponent<Renderer>();
        if (bulletRenderer != null)
        {
            bulletRenderer.material.color = Color.yellow;
        }

        SphereCollider collider = bullet.GetComponent<SphereCollider>();
        collider.isTrigger = true;

        Rigidbody rb = bullet.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.linearVelocity = direction * bulletSpeed;

        BulletProjectile projectile = bullet.AddComponent<BulletProjectile>();
        projectile.lifeTime = 2f;
    }

    void SetupPlayerVisual()
    {
        Renderer playerRenderer = GetComponent<Renderer>();
        if (playerRenderer != null)
        {
            playerRenderer.material.color = Color.cyan;
        }

        // Simple gun/barrel so we can see which direction the player is facing
        GameObject gun = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gun.name = "Prototype Gun";
        gun.transform.SetParent(transform);

        gun.transform.localPosition = new Vector3(0f, 0.15f, 0.75f);
        gun.transform.localRotation = Quaternion.identity;
        gun.transform.localScale = new Vector3(0.35f, 0.2f, 0.8f);

        Renderer gunRenderer = gun.GetComponent<Renderer>();
        if (gunRenderer != null)
        {
            gunRenderer.material.color = Color.black;
        }

        Collider gunCollider = gun.GetComponent<Collider>();
        if (gunCollider != null)
        {
            Destroy(gunCollider);
        }
    }
}

public class BulletProjectile : MonoBehaviour
{
    public float lifeTime = 2f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        DemoZombie zombie = other.GetComponent<DemoZombie>();

        if (zombie != null)
        {
            zombie.Die();
            Destroy(gameObject);
        }
    }
}