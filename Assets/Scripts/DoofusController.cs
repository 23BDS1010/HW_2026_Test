using UnityEngine;

public class DoofusController : MonoBehaviour
{
    private Rigidbody rb;
    private float speed = 3f;
    private float fallYThreshold = -10f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (ConfigLoader.Instance != null && ConfigLoader.Instance.Config != null)
        {
            speed = ConfigLoader.Instance.Config.player_data.speed;
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            return;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0f, vertical) * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

        if (transform.position.y < fallYThreshold)
        {
            GameManager.Instance?.GameOver();
        }
    }
}