using UnityEngine;

public class DoofusController : MonoBehaviour
{
    private Rigidbody rb;
    private float speed = 3f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (ConfigLoader.Instance != null && ConfigLoader.Instance.Config != null)
        {
            speed = ConfigLoader.Instance.Config.player_data.speed;
        }
        else
        {
            Debug.LogWarning("ConfigLoader not ready, using default speed.");
        }
    }

    private void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");     

        Vector3 movement = new Vector3(horizontal, 0f, vertical) * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);
    }
}