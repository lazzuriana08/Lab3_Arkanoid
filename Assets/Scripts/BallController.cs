using UnityEngine;

public class BallController : MonoBehaviour
{
    public float speed = 6f;
    public float minVerticalDirection = 0.25f;

    private Rigidbody2D rb;
    private bool isLaunched = false;
    private Transform player;
    private float currentSpeed;
    private Vector2 lastMoveDirection = Vector2.up;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        Collider2D collider2D = GetComponent<Collider2D>();
        if (collider2D != null)
        {
            collider2D.isTrigger = false;
        }

        player = GameObject.FindWithTag("Player")?.transform;
        currentSpeed = speed;
        ResetToPaddle();
    }

    void Update()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player")?.transform;
            if (player == null)
            {
                return;
            }
        }

        if (!isLaunched)
        {
            transform.position = player.position + Vector3.up * 0.6f;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Launch();
            }
        }

        if (isLaunched && transform.position.y < -5.7f)
        {
            LevelSceneController.Instance?.HandleBallLost();
        }
    }

    void FixedUpdate()
    {
        if (!isLaunched) return;

        Vector2 dir = rb.linearVelocity.normalized;

        if (Mathf.Abs(dir.y) < minVerticalDirection)
        {
            dir.y = Mathf.Sign(dir.y == 0 ? 1 : dir.y) * minVerticalDirection;
            dir.Normalize();
        }

        rb.linearVelocity = dir * currentSpeed;
    }

    void Launch()
    {
        isLaunched = true;
        Vector2 startDirection = new Vector2(Random.Range(-0.4f, 0.4f), 1f).normalized;
        lastMoveDirection = startDirection;
        rb.linearVelocity = startDirection * currentSpeed;
    }

    public void ResetToPaddle()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        isLaunched = false;
        currentSpeed = speed;
        lastMoveDirection = Vector2.up;
        rb.linearVelocity = Vector2.zero;
    }

    public void MultiplySpeed(float factor)
    {
        currentSpeed *= factor;
        currentSpeed = Mathf.Clamp(currentSpeed, speed, speed * 2.2f);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Brick brick = collision.collider.GetComponent<Brick>();
        if (brick != null)
        {
            brick.TakeHit();
        }

        if (collision.collider.CompareTag("Player"))
        {
            float paddleX = collision.transform.position.x;
            float offset = (transform.position.x - paddleX) / 0.8f;
            Vector2 newDirection = new Vector2(offset, Mathf.Abs(rb.linearVelocity.y));
            if (newDirection.sqrMagnitude < 0.01f)
            {
                newDirection = Vector2.up;
            }

            Vector2 paddleDirection = newDirection.normalized;
            lastMoveDirection = paddleDirection;
            rb.linearVelocity = paddleDirection * currentSpeed;
        }
    }
}