using UnityEngine;

public class BallController : MonoBehaviour
{
    public float speed = 6f;

    private Rigidbody2D rb;
    private bool isLaunched = false;
    private Transform player;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player").transform;

        Debug.Log("BallController iniciou");
    }

    void Update()
    {
        if (!isLaunched)
        {
            transform.position = player.position + Vector3.up * 0.6f;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Espaço pressionado");
                Launch();
            }
        }
    }

    void Launch()
    {
        isLaunched = true;
        rb.linearVelocity = Vector2.up * speed;
        Debug.Log("Bola lan�ada");
    }
}