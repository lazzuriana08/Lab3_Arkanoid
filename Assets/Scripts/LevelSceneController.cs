using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSceneController : MonoBehaviour
{
    public static LevelSceneController Instance { get; private set; }

    private readonly List<Brick> bricks = new List<Brick>();
    private BallController ballController;
    private PlayerController playerController;
    private int levelIndex = 1;
    private bool transitioning;
    private float powerUpChance;
    private readonly List<Sprite> brickSprites = new List<Sprite>();
    private PhysicsMaterial2D bounceMaterial;

    private static Sprite cachedSprite;

    void Awake()
    {
        Instance = this;
        levelIndex = ParseLevelIndex(SceneManager.GetActiveScene().name);
    }

    void Start()
    {
        LoadBrickSpritesFromAssets();
        EnsureGameplayObjects();
        CreateBoundaries();
        SpawnBlocksForLevel();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void OnGUI()
    {
        GUIStyle hudStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold
        };

        GUI.Label(new Rect(12, 10, 260, 30), $"Pontos: {GameSession.Score}", hudStyle);
        GUI.Label(new Rect(12, 35, 260, 30), $"Vidas: {GameSession.Lives}", hudStyle);
        GUI.Label(new Rect(12, 60, 260, 30), $"Nível: {levelIndex}", hudStyle);
        GUI.Label(new Rect(Screen.width - 230, 10, 220, 30), "R: Reiniciar nível", hudStyle);
    }

    public void RegisterBrick(Brick brick)
    {
        if (!bricks.Contains(brick))
        {
            bricks.Add(brick);
        }
    }

    public void OnBrickDestroyed(Brick brick)
    {
        if (brick == null)
        {
            return;
        }

        bricks.Remove(brick);
        GameSession.AddScore(brick.scoreValue);

        TrySpawnPowerUp(brick.transform.position);

        if (bricks.Count == 0 && !transitioning)
        {
            StartCoroutine(LoadNextStageRoutine());
        }
    }

    public void HandleBallLost()
    {
        if (transitioning)
        {
            return;
        }

        StartCoroutine(BallLostRoutine());
    }

    public void ApplyPowerUp(PowerUpType powerUpType)
    {
        if (powerUpType == PowerUpType.ExpandPaddle)
        {
            playerController?.SetTemporarySizeMultiplier(1.6f, 8f);
            return;
        }

        if (powerUpType == PowerUpType.SpeedBall)
        {
            ballController?.MultiplySpeed(1.25f);
            return;
        }

        if (powerUpType == PowerUpType.ExtraLife)
        {
            GameSession.AddLife(1);
        }
    }

    private IEnumerator BallLostRoutine()
    {
        transitioning = true;
        GameSession.LoseLife(1);

        if (GameSession.Lives <= 0)
        {
            GameSession.SetResult(false);
            yield return new WaitForSeconds(0.6f);
            SceneManager.LoadScene("EndScene");
            yield break;
        }

        if (ballController != null)
        {
            ballController.ResetToPaddle();
        }

        yield return new WaitForSeconds(0.4f);
        transitioning = false;
    }

    private IEnumerator LoadNextStageRoutine()
    {
        transitioning = true;
        yield return new WaitForSeconds(0.8f);

        string nextLevel = $"Level{levelIndex + 1}";
        if (Application.CanStreamedLevelBeLoaded(nextLevel))
        {
            SceneManager.LoadScene(nextLevel);
        }
        else
        {
            GameSession.SetResult(true);
            SceneManager.LoadScene("EndScene");
        }
    }

    private void EnsureGameplayObjects()
    {
        GameObject paddleObject = GameObject.FindWithTag("Player");
        if (paddleObject == null)
        {
            paddleObject = CreatePaddle();
        }

        GameObject ballObject = GameObject.FindWithTag("Ball");
        if (ballObject == null)
        {
            ballObject = CreateBall(paddleObject.transform.position + Vector3.up * 0.6f);
        }

        playerController = paddleObject.GetComponent<PlayerController>();
        if (playerController == null)
        {
            playerController = paddleObject.AddComponent<PlayerController>();
        }

        Collider2D paddleCollider = paddleObject.GetComponent<Collider2D>();
        if (paddleCollider != null)
        {
            paddleCollider.sharedMaterial = GetBounceMaterial();
        }

        ballController = ballObject.GetComponent<BallController>();
        if (ballController == null)
        {
            ballController = ballObject.AddComponent<BallController>();
        }

        Collider2D ballCollider = ballObject.GetComponent<Collider2D>();
        if (ballCollider != null)
        {
            ballCollider.sharedMaterial = GetBounceMaterial();
        }

        if (levelIndex <= 1)
        {
            ballController.speed = 5.7f;
        }
        else
        {
            ballController.speed = 6.8f;
            ballController.minVerticalDirection = 0.33f;
        }

        ballController.ResetToPaddle();
    }

    private void CreateBoundaries()
    {
        CreateWall("TopWall", new Vector2(0f, 5.15f), new Vector2(17.5f, 0.4f));
        CreateWall("LeftWall", new Vector2(-8.55f, 0f), new Vector2(0.4f, 10.5f));
        CreateWall("RightWall", new Vector2(8.55f, 0f), new Vector2(0.4f, 10.5f));
    }

    private void SpawnBlocksForLevel()
    {
        int rows = levelIndex == 1 ? 4 : 4;
        int cols = levelIndex == 1 ? 8 : 8;
        float startX = -5.6f;
        float startY = 3.8f;
        float stepX = 1.6f;
        float stepY = 0.65f;

        powerUpChance = levelIndex == 1 ? 0.30f : 0.18f;

        Color[] paletteLevel1 =
        {
            new Color(0.94f, 0.25f, 0.25f),
            new Color(0.98f, 0.55f, 0.19f),
            new Color(0.95f, 0.85f, 0.2f),
            new Color(0.25f, 0.78f, 0.35f)
        };

        Color[] paletteLevel2 =
        {
            new Color(0.73f, 0.31f, 0.93f),
            new Color(0.28f, 0.53f, 0.93f),
            new Color(0.15f, 0.82f, 0.88f),
            new Color(0.24f, 0.84f, 0.39f),
            new Color(0.97f, 0.7f, 0.17f),
            new Color(0.95f, 0.32f, 0.32f)
        };

        Color[] palette = levelIndex == 1 ? paletteLevel1 : paletteLevel2;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector3 position = new Vector3(startX + c * stepX, startY - r * stepY, 0f);
                GameObject brickObject = new GameObject($"Brick_{r}_{c}");
                brickObject.transform.position = position;

                SpriteRenderer renderer = brickObject.AddComponent<SpriteRenderer>();
                if (brickSprites.Count > 0)
                {
                    renderer.sprite = brickSprites[(r + c) % brickSprites.Count];
                    renderer.color = Color.white;
                }
                else
                {
                    renderer.sprite = GetWhiteSprite();
                    renderer.color = palette[r % palette.Length];
                }

                ResizeSpriteToWorldSize(brickObject.transform, renderer, new Vector2(1.35f, 0.45f));

                BoxCollider2D collider = brickObject.AddComponent<BoxCollider2D>();
                collider.size = Vector2.one;
                collider.sharedMaterial = GetBounceMaterial();

                Brick brick = brickObject.AddComponent<Brick>();
                if (levelIndex == 1)
                {
                    brick.scoreValue = 100;
                    brick.hitPoints = 1;
                }
                else
                {
                    brick.scoreValue = 150;
                    brick.hitPoints = Random.value < 0.65f ? 2 : 1;
                }

                brick.Initialize(renderer.color, this);

                RegisterBrick(brick);
            }
        }
    }

    private void LoadBrickSpritesFromAssets()
    {
        if (brickSprites.Count > 0)
        {
            return;
        }

        string[] brickFiles =
        {
            "element_blue_rectangle.png",
            "element_green_rectangle.png",
            "element_grey_rectangle.png",
            "element_purple_rectangle.png",
            "element_red_rectangle.png",
            "element_yellow_rectangle.png"
        };

        foreach (string fileName in brickFiles)
        {
            string assetPath = Path.Combine(Application.dataPath, "Components", fileName);
            if (!File.Exists(assetPath))
            {
                continue;
            }

            byte[] imageBytes = File.ReadAllBytes(assetPath);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;

            if (!texture.LoadImage(imageBytes))
            {
                Destroy(texture);
                continue;
            }

            Sprite loadedSprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f
            );

            brickSprites.Add(loadedSprite);
        }
    }

    private void TrySpawnPowerUp(Vector3 position)
    {
        if (Random.value > powerUpChance)
        {
            return;
        }

        GameObject powerUpObject = new GameObject("PowerUp");
        powerUpObject.transform.position = position;

        SpriteRenderer renderer = powerUpObject.AddComponent<SpriteRenderer>();
        renderer.sprite = GetWhiteSprite();

        PowerUp powerUp = powerUpObject.AddComponent<PowerUp>();
        powerUp.powerUpType = (PowerUpType)Random.Range(0, 3);
        renderer.color = powerUp.GetPowerUpColor();

        CircleCollider2D collider = powerUpObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.28f;

        powerUpObject.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
    }

    private GameObject CreatePaddle()
    {
        GameObject paddle = new GameObject("Player_Spaceship");
        paddle.transform.position = new Vector3(0f, -4f, 0f);

        SpriteRenderer renderer = paddle.AddComponent<SpriteRenderer>();
        renderer.sprite = GetWhiteSprite();
        renderer.color = new Color(0.7f, 0.9f, 1f);

        BoxCollider2D collider = paddle.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(1.2f, 0.3f);
        collider.sharedMaterial = GetBounceMaterial();

        Rigidbody2D rigidbody2D = paddle.AddComponent<Rigidbody2D>();
        rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        rigidbody2D.gravityScale = 0f;

        paddle.transform.localScale = new Vector3(1.2f, 0.3f, 1f);
        AssignTagSafely(paddle, "Player");

        return paddle;
    }

    private GameObject CreateBall(Vector3 position)
    {
        GameObject ball = new GameObject("Ball");
        ball.transform.position = position;

        SpriteRenderer renderer = ball.AddComponent<SpriteRenderer>();
        renderer.sprite = GetWhiteSprite();
        renderer.color = Color.white;

        CircleCollider2D collider = ball.AddComponent<CircleCollider2D>();
        collider.radius = 0.2f;
        collider.sharedMaterial = GetBounceMaterial();

        Rigidbody2D rigidbody2D = ball.AddComponent<Rigidbody2D>();
        rigidbody2D.gravityScale = 0f;
        rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;

        ball.transform.localScale = new Vector3(0.28f, 0.28f, 1f);
        AssignTagSafely(ball, "Ball");

        return ball;
    }

    private void CreateWall(string name, Vector2 position, Vector2 size)
    {
        GameObject wall = GameObject.Find(name);
        if (wall == null)
        {
            wall = new GameObject(name);
        }

        wall.transform.position = position;
        wall.transform.localScale = new Vector3(size.x, size.y, 1f);

        BoxCollider2D collider = wall.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = wall.AddComponent<BoxCollider2D>();
        }

        collider.size = Vector2.one;
        collider.offset = Vector2.zero;
        collider.isTrigger = false;
        collider.sharedMaterial = GetBounceMaterial();

        Rigidbody2D wallBody = wall.GetComponent<Rigidbody2D>();
        if (wallBody == null)
        {
            wallBody = wall.AddComponent<Rigidbody2D>();
        }

        wallBody.bodyType = RigidbodyType2D.Static;
        wallBody.simulated = true;
        wallBody.gravityScale = 0f;

        SpriteRenderer renderer = wall.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = wall.AddComponent<SpriteRenderer>();
        }

        renderer.sprite = GetWhiteSprite();
        renderer.color = new Color(0.2f, 0.2f, 0.35f, 0.25f);

        WallBounce wallBounce = wall.GetComponent<WallBounce>();
        if (wallBounce == null)
        {
            wallBounce = wall.AddComponent<WallBounce>();
        }

        wallBounce.minBounceSpeed = 5f;
        wallBounce.speedMultiplier = 1f;
    }

    private PhysicsMaterial2D GetBounceMaterial()
    {
        if (bounceMaterial != null)
        {
            return bounceMaterial;
        }

        bounceMaterial = new PhysicsMaterial2D("ArkanoidBounce")
        {
            friction = 0f,
            bounciness = 1f
        };

        bounceMaterial.frictionCombine = PhysicsMaterialCombine2D.Minimum;
        bounceMaterial.bounceCombine = PhysicsMaterialCombine2D.Maximum;
        return bounceMaterial;
    }

    private static Sprite GetWhiteSprite()
    {
        if (cachedSprite != null)
        {
            return cachedSprite;
        }

        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        cachedSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return cachedSprite;
    }

    private static void ResizeSpriteToWorldSize(Transform targetTransform, SpriteRenderer spriteRenderer, Vector2 targetWorldSize)
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
        {
            return;
        }

        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
        {
            return;
        }

        targetTransform.localScale = new Vector3(
            targetWorldSize.x / spriteSize.x,
            targetWorldSize.y / spriteSize.y,
            1f
        );
    }

    private static int ParseLevelIndex(string sceneName)
    {
        string digits = string.Empty;
        foreach (char character in sceneName)
        {
            if (char.IsDigit(character))
            {
                digits += character;
            }
        }

        if (int.TryParse(digits, out int parsed))
        {
            return parsed;
        }

        return 1;
    }

    private void AssignTagSafely(GameObject target, string tag)
    {
        try
        {
            target.tag = tag;
        }
        catch
        {
            target.tag = "Untagged";
        }
    }
}
