using UnityEngine;

public class DoofusController : MonoBehaviour
{
    private Rigidbody rb;
    private float speed = 3f;
    private float fallYThreshold = -10f;

    [Header("Dash Settings")]
    [SerializeField] private float dashMultiplier = 2.3f;
    [SerializeField] private float dashDuration = 1f;
    [SerializeField] private float dashCooldown = 3f;

    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (ConfigLoader.Instance != null && ConfigLoader.Instance.Config != null)
        {
            speed = ConfigLoader.Instance.Config.player_data.speed;
        }
    }

    private void Update()
    {
        // Input polling in Update for responsiveness; movement itself stays in FixedUpdate
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && dashCooldownTimer <= 0f)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
            Debug.Log("[Dash] Activated");
        }

        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            return;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        float currentSpeed = speed;

        if (isDashing)
        {
            currentSpeed = speed * dashMultiplier;
            dashTimer -= Time.fixedDeltaTime;

            if (dashTimer <= 0f)
            {
                isDashing = false;
            }
        }

        Vector3 movement = new Vector3(horizontal, 0f, vertical) * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

        if (transform.position.y < fallYThreshold)
        {
            GameManager.Instance?.GameOver();
        }
    }
}