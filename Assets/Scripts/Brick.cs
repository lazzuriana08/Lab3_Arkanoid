using UnityEngine;

public class Brick : MonoBehaviour
{
    public int hitPoints = 1;
    public int scoreValue = 100;

    private int currentHitPoints;
    private SpriteRenderer spriteRenderer;
    private LevelSceneController levelController;

    public void Initialize(Color baseColor, LevelSceneController controller)
    {
        levelController = controller;
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHitPoints = Mathf.Max(1, hitPoints);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = baseColor;
            UpdateColorByHitPoints();
        }
    }

    public void TakeHit()
    {
        currentHitPoints--;

        if (currentHitPoints <= 0)
        {
            levelController?.OnBrickDestroyed(this);
            Destroy(gameObject);
            return;
        }

        UpdateColorByHitPoints();
    }

    private void UpdateColorByHitPoints()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        float intensity = Mathf.Clamp01(0.45f + currentHitPoints * 0.35f);
        Color color = spriteRenderer.color;
        spriteRenderer.color = new Color(color.r * intensity, color.g * intensity, color.b * intensity, 1f);
    }
}
