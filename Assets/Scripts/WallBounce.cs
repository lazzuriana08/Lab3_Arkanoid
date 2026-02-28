using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WallBounce : MonoBehaviour
{
    public float minBounceSpeed = 5f;
    public float speedMultiplier = 1f;

    private void Reset()
    {
        Collider2D collider2D = GetComponent<Collider2D>();
        if (collider2D != null)
        {
            collider2D.isTrigger = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryBounce(collision);
    }


    private void TryBounce(Collision2D collision)
    {
        Rigidbody2D ballBody = collision.collider.attachedRigidbody;
        if (ballBody == null || collision.contactCount == 0)
        {
            return;
        }

        if (ballBody.bodyType != RigidbodyType2D.Dynamic)
        {
            return;
        }

        Vector2 currentVelocity = ballBody.linearVelocity;
        float targetSpeed = Mathf.Max(minBounceSpeed, currentVelocity.magnitude * speedMultiplier);

        Vector2 normal = collision.GetContact(0).normal;
        if (normal.sqrMagnitude < 0.0001f)
        {
            return;
        }

        if (currentVelocity.sqrMagnitude < 0.0001f)
        {
            currentVelocity = -normal * targetSpeed;
        }

        Vector2 reflectedDirection = Vector2.Reflect(currentVelocity.normalized, normal).normalized;
        ballBody.linearVelocity = reflectedDirection * targetSpeed;

        Vector2 separation = normal * 0.02f;
        ballBody.position += separation;
    }
}