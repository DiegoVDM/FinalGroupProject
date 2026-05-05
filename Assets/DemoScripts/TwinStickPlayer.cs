using System.Collections;
using UnityEngine;

public class TwinStickPlayer : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;

    [Header("Animation")]
    public Animator playerAnimator;
    public string speedParameterName = "Speed";

    [Header("Shooting")]
    public float bulletSpeed = 16f;
    public float fireRate = 0.12f;
    public Transform bulletSpawnPoint;

    [Header("Weapon Audio")]
    public AudioClip gunfireSound;
    public AudioSource weaponAudioSource;
    [Range(0f, 1f)]
    public float gunfireVolume = 0.7f;

    [Header("Weapon VFX")]
    public GameObject muzzleFlashObject;
    public float muzzleFlashDuration = 0.05f;

    private float nextFireTime;
    private Vector3 lastAimDirection = Vector3.forward;
    private Coroutine muzzleFlashCoroutine;

    void Start()
    {
        SetupPlayerVisual();
        SetupAnimatorReference();
        SetupWeaponAudio();
        SetupMuzzleFlash();
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

        UpdateMovementAnimation(moveDirection);

        // If not shooting, face movement direction.
        if (moveDirection.sqrMagnitude > 0.01f && !IsShootingInputHeld())
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }

    void UpdateMovementAnimation(Vector3 moveDirection)
    {
        if (playerAnimator == null)
            return;

        float movementAmount = moveDirection.sqrMagnitude > 0.01f ? 1f : 0f;
        playerAnimator.SetFloat(speedParameterName, movementAmount);
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

        if (bulletSpawnPoint != null)
        {
            bullet.transform.position = bulletSpawnPoint.position;
        }
        else
        {
            // Fallback in case BulletSpawnPoint is not assigned.
            bullet.transform.position = transform.position + direction * 0.9f + Vector3.up * 0.2f;
        }

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

        PlayGunfireSound();
        TriggerMuzzleFlash();
    }

    void PlayGunfireSound()
    {
        if (weaponAudioSource == null || gunfireSound == null)
            return;

        weaponAudioSource.PlayOneShot(gunfireSound, gunfireVolume);
    }

    void TriggerMuzzleFlash()
    {
        if (muzzleFlashObject == null)
            return;

        muzzleFlashObject.SetActive(true);

        if (muzzleFlashCoroutine != null)
        {
            StopCoroutine(muzzleFlashCoroutine);
        }

        muzzleFlashCoroutine = StartCoroutine(HideMuzzleFlashAfterDelay());
    }

    IEnumerator HideMuzzleFlashAfterDelay()
    {
        yield return new WaitForSeconds(muzzleFlashDuration);

        if (muzzleFlashObject != null)
        {
            muzzleFlashObject.SetActive(false);
        }

        muzzleFlashCoroutine = null;
    }

    void SetupAnimatorReference()
    {
        if (playerAnimator == null)
        {
            playerAnimator = GetComponentInChildren<Animator>();
        }
    }

    void SetupWeaponAudio()
    {
        if (weaponAudioSource == null)
        {
            weaponAudioSource = GetComponent<AudioSource>();
        }

        if (weaponAudioSource == null)
        {
            weaponAudioSource = gameObject.AddComponent<AudioSource>();
        }

        weaponAudioSource.playOnAwake = false;
        weaponAudioSource.spatialBlend = 0f;
    }

    void SetupMuzzleFlash()
    {
        if (muzzleFlashObject != null)
        {
            muzzleFlashObject.SetActive(false);
        }
    }

    void SetupPlayerVisual()
    {
        Renderer playerRenderer = GetComponent<Renderer>();
        if (playerRenderer != null)
        {
            playerRenderer.material.color = Color.cyan;
        }

        // The old prototype cube gun was removed.
        // The real gun model now lives under AimPivot > WeaponSocket.
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