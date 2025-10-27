using UnityEngine;

public class movimentoplayer : MonoBehaviour

{
    [Header("Movimento")]
    public float moveSpeed = 5f;
    public float acceleration = 10f;
    private float horizontalInput;
    private float currentSpeed;

    [Header("Pulo")]
    public float jumpForce = 12f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    private bool isGrounded;

    private Rigidbody2D rb;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // Input Horizontal
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Movimentação suave
        currentSpeed = Mathf.Lerp(currentSpeed, horizontalInput * moveSpeed, acceleration * Time.deltaTime);

        // Flip personagem
        if (horizontalInput != 0)
            transform.localScale = new Vector3(Mathf.Sign(horizontalInput), 1, 1);

        // Verifica se está no chão
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Pulo
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Animações
        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(currentSpeed));
            anim.SetBool("isGrounded", isGrounded);
            anim.SetFloat("VerticalVelocity", rb.linearVelocity.y);
        }
    }

    void FixedUpdate()
    {
        // Aplicar velocidade horizontal
        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);
    }

    // Debug: mostra a área de checagem do chão no editor
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
