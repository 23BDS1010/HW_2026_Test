using UnityEngine;

public class DoofusController : MonoBehaviour
{
    private Rigidbody rb;
    private float speed = 3f;
    private float fallYThreshold = -10f;

    private PulpitSpawner pulpitSpawner;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pulpitSpawner = FindFirstObjectByType<PulpitSpawner>();

        if (ConfigLoader.Instance != null && ConfigLoader.Instance.Config != null)
        {
            speed = ConfigLoader.Instance.Config.player_data.speed;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
            HandleStateChanged(GameManager.Instance.CurrentState); // apply current state immediately
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
        }
    }

    private void HandleStateChanged(GameState state)
    {
        // Freeze Doofus (no gravity/physics) until actual gameplay starts
        rb.isKinematic = (state != GameState.Playing);

        if (state == GameState.Playing)
        {
            // Reset position/velocity right as gameplay begins
            transform.position = new Vector3(0f, 0.75f, 0f);
            rb.linearVelocity = Vector3.zero;
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            return;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0f, vertical) * speed * Time.fixedDeltaTime;

        // Cancel any residual horizontal momentum before applying controlled movement
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

        rb.MovePosition(rb.position + movement);

        if (movement != Vector3.zero)
        {
            pulpitSpawner?.MovePulpitsTowardPlayer(transform, movement);
        }

        if (transform.position.y < fallYThreshold)
        {
            GameManager.Instance?.GameOver();
        }
    }
}