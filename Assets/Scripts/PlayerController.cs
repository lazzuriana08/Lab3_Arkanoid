using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 8f;
    public float minX = -7.5f;
    public float maxX = 7.5f;

    private float defaultScaleX;
    private Coroutine sizeRoutine;

    void Awake()
    {
        defaultScaleX = transform.localScale.x;
    }

    void Update()
    {
        float move = Input.GetAxis("Horizontal");

        Vector3 newPosition = transform.position;
        newPosition.x += move * speed * Time.deltaTime;
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        transform.position = newPosition;
    }

    public void SetTemporarySizeMultiplier(float multiplier, float duration)
    {
        if (sizeRoutine != null)
        {
            StopCoroutine(sizeRoutine);
        }

        sizeRoutine = StartCoroutine(TemporarySizeRoutine(multiplier, duration));
    }

    private System.Collections.IEnumerator TemporarySizeRoutine(float multiplier, float duration)
    {
        Vector3 scale = transform.localScale;
        scale.x = defaultScaleX * multiplier;
        transform.localScale = scale;

        yield return new WaitForSeconds(duration);

        scale.x = defaultScaleX;
        transform.localScale = scale;
        sizeRoutine = null;
    }
}   