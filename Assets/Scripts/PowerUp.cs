using UnityEngine;

public enum PowerUpType
{
    ExpandPaddle,
    SpeedBall,
    ExtraLife
}

public class PowerUp : MonoBehaviour
{
    public PowerUpType powerUpType;
    public float fallSpeed = 2.4f;

    void Update()
    {
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        if (transform.position.y < -6f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        LevelSceneController.Instance?.ApplyPowerUp(powerUpType);
        Destroy(gameObject);
    }

    public Color GetPowerUpColor()
    {
        if (powerUpType == PowerUpType.ExpandPaddle)
        {
            return new Color(0.35f, 0.9f, 0.45f);
        }

        if (powerUpType == PowerUpType.SpeedBall)
        {
            return new Color(0.95f, 0.65f, 0.15f);
        }

        return new Color(0.35f, 0.8f, 0.95f);
    }
}
