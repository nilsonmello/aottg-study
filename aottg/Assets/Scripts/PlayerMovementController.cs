using UnityEngine;

/// <summary>
/// Movimento base do jogador: andar, correr, pular e cair, tudo via Rigidbody.
/// Quando o GrappleHookController está ativo (voando ou grudado), este script
/// para de aplicar suas próprias forças de movimento pra não brigar com o gancho —
/// o gancho assume o controle total da física nesse período.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private MovementConfig config;

    [Tooltip("Opcional. Se vazio, procura um GrappleHookController no mesmo objeto.")]
    [SerializeField] private GrappleHookController grappleHook;

    private Rigidbody rb;
    private bool isGrounded;
    private bool sprintHeld;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // evita o jogador tombar como uma caixa

        if (cameraTransform == null) cameraTransform = Camera.main.transform;
        if (grappleHook == null) grappleHook = GetComponent<GrappleHookController>();
    }

    private void Update()
    {
        sprintHeld = Input.GetKey(KeyCode.LeftShift);

        if (Input.GetButtonDown("Jump") && isGrounded && !IsGrappled)
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        CheckGrounded();

        // O gancho está no controle — não aplicamos movimento próprio.
        // (O GrappleHookController já lê o input horizontal/vertical pro swing.)
        if (IsGrappled) return;

        ApplyMovement();
        ApplyExtraGravity();
    }

    private bool IsGrappled => grappleHook != null && grappleHook.IsGrappled;

    private void CheckGrounded()
    {
        isGrounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            config.groundCheckDistance + 0.1f,
            config.groundLayer
        );
    }

    private void ApplyMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Direção relativa à câmera, projetada no plano horizontal
        Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
        Vector3 wishDir = (forward * v + right * h).normalized;

        float targetSpeed = sprintHeld ? config.sprintSpeed : config.walkSpeed;
        Vector3 targetVelocity = wishDir * targetSpeed;

        // Velocidade atual só no plano horizontal (preserva a vertical/queda)
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 currentHorizontal = new Vector3(currentVelocity.x, 0f, currentVelocity.z);

        float accel = isGrounded ? config.groundAcceleration : config.groundAcceleration * config.airControlMultiplier;
        float decel = isGrounded ? config.groundDeceleration : config.groundDeceleration * config.airControlMultiplier;

        float rate = wishDir.sqrMagnitude > 0.01f ? accel : decel;
        Vector3 newHorizontal = Vector3.MoveTowards(currentHorizontal, targetVelocity, rate * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector3(newHorizontal.x, currentVelocity.y, newHorizontal.z);
    }

    private void Jump()
    {
        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0f; // pulo consistente mesmo saindo de uma queda
        rb.linearVelocity = velocity;
        rb.AddForce(Vector3.up * config.jumpForce, ForceMode.VelocityChange);
    }

    private void ApplyExtraGravity()
    {
        if (!isGrounded)
        {
            rb.AddForce(Physics.gravity * (config.extraGravityMultiplier - 1f), ForceMode.Acceleration);
        }
    }
}