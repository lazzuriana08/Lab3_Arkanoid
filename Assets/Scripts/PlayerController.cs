using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 8f;
    public float minX = -7.5f;
    public float maxX = 7.5f;

    void Update()
    {
        float move = Input.GetAxis("Horizontal");
        Debug.Log("Input: " + move);

        Vector3 newPosition = transform.position;
        newPosition.x += move * speed * Time.deltaTime;
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        transform.position = newPosition;
    }
}   