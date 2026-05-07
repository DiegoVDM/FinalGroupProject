using System;
using System.Reflection;
using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    public float lifeTime = 2f;
    public float objectiveDamage = 1f;

    private Vector3 moveDirection = Vector3.forward;
    private float moveSpeed;
    private bool initialized;
    private bool hasHit;
    private Collider projectileCollider;
    private Rigidbody projectileRigidbody;
    private Vector3 previousPosition;

    void Awake()
    {
        projectileCollider = GetComponent<Collider>();
        projectileRigidbody = GetComponent<Rigidbody>();
        previousPosition = transform.position;
    }

    public void Initialize(Vector3 direction, float speed)
    {
        if (direction.sqrMagnitude <= 0.01f)
        {
            direction = transform.forward;
        }

        moveDirection = direction.normalized;
        moveSpeed = speed;
        initialized = true;
        previousPosition = transform.position;

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }

        Destroy(gameObject, lifeTime);
    }

    void Start()
    {
        if (!initialized)
        {
            Initialize(transform.forward, moveSpeed);
        }
    }

    void FixedUpdate()
    {
        if (hasHit || moveSpeed <= 0f)
            return;

        Vector3 nextPosition = transform.position + moveDirection * moveSpeed * Time.fixedDeltaTime;

        if (TryProcessSweptHit(previousPosition, nextPosition))
            return;

        if (projectileRigidbody != null && projectileRigidbody.isKinematic)
        {
            projectileRigidbody.MovePosition(nextPosition);
            previousPosition = nextPosition;
            return;
        }

        transform.position = nextPosition;
        previousPosition = nextPosition;
    }

    void OnTriggerEnter(Collider other)
    {
        TryProcessHit(other);
    }

    bool TryProcessSweptHit(Vector3 startPosition, Vector3 endPosition)
    {
        Vector3 travel = endPosition - startPosition;
        float travelDistance = travel.magnitude;

        if (travelDistance <= 0.001f)
            return false;

        RaycastHit[] hits = Physics.SphereCastAll(
            startPosition,
            GetSweepRadius(),
            travel.normalized,
            travelDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Collide);

        if (hits == null || hits.Length == 0)
            return false;

        Array.Sort(hits, (first, second) => first.distance.CompareTo(second.distance));

        for (int i = 0; i < hits.Length; i++)
        {
            Collider hitCollider = hits[i].collider;
            if (hitCollider == null || ShouldIgnoreCollider(hitCollider))
                continue;

            if (TryProcessHit(hitCollider))
                return true;
        }

        return false;
    }

    bool TryProcessHit(Collider other)
    {
        if (hasHit || other == null || ShouldIgnoreCollider(other))
            return false;

        DemoZombie zombie = other.GetComponentInParent<DemoZombie>();

        if (zombie != null)
        {
            hasHit = true;
            zombie.Die();
            Destroy(gameObject);
            return true;
        }

        if (TryDamageDestructibleObjective(other))
        {
            hasHit = true;
            Destroy(gameObject);
            return true;
        }

        return false;
    }

    bool ShouldIgnoreCollider(Collider other)
    {
        if (other == projectileCollider)
            return true;

        if (other.GetComponentInParent<TwinStickPlayer>() != null)
            return true;

        if (other.GetComponentInParent<PlayerHealth>() != null)
            return true;

        return false;
    }

    float GetSweepRadius()
    {
        SphereCollider sphereCollider = projectileCollider as SphereCollider;
        if (sphereCollider != null)
        {
            Vector3 scale = transform.lossyScale;
            float largestAxis = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            return Mathf.Max(0.05f, sphereCollider.radius * largestAxis);
        }

        if (projectileCollider != null)
            return Mathf.Max(0.05f, projectileCollider.bounds.extents.magnitude * 0.5f);

        return 0.05f;
    }

    bool TryDamageDestructibleObjective(Collider other)
    {
        Component objective = GetComponentInParentByName(other.transform, "DestructibleObjective");
        if (objective == null)
            return false;

        Type objectiveType = objective.GetType();
        MethodInfo takeDamage = objectiveType.GetMethod(
            "TakeDamage",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new Type[] { typeof(float) },
            null);

        if (takeDamage != null)
        {
            takeDamage.Invoke(objective, new object[] { objectiveDamage });
            return true;
        }

        takeDamage = objectiveType.GetMethod(
            "TakeDamage",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            Type.EmptyTypes,
            null);

        if (takeDamage != null)
        {
            takeDamage.Invoke(objective, null);
            return true;
        }

        Debug.LogWarning("DestructibleObjective was hit, but it has no supported TakeDamage method.");
        return false;
    }

    Component GetComponentInParentByName(Transform startingTransform, string componentName)
    {
        Transform current = startingTransform;

        while (current != null)
        {
            Component component = current.GetComponent(componentName);
            if (component != null)
                return component;

            current = current.parent;
        }

        return null;
    }
}
