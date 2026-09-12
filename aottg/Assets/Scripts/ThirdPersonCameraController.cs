using UnityEngine;

/// <summary>
/// Câmera de terceira pessoa em órbita ao redor de um alvo (o jogador),
/// controlada pelo mouse. Empurra pra perto quando colide com o cenário.
/// Coloque este script na própria Camera (não precisa de um "rig" separado).
/// </summary>
public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Ponto ao redor do qual a câmera orbita — normalmente um Transform vazio na altura do peito/cabeça do jogador.")]
    [SerializeField] private Transform target;

    [SerializeField] private CameraConfig config;

    private float yaw;
    private float pitch;
    private Vector3 currentVelocity; // usado pelo SmoothDamp

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Vector3 startAngles = transform.eulerAngles;
        yaw = startAngles.y;
        pitch = startAngles.x;
    }

    private void Update()
    {
        // Facilita testar no Editor: Esc solta o mouse, clique na tela trava de novo
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        ReadMouseInput();
    }

    private void ReadMouseInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * config.sensitivityX;
        float mouseY = Input.GetAxis("Mouse Y") * config.sensitivityY * (config.invertY ? 1f : -1f);

        yaw += mouseX;
        pitch = Mathf.Clamp(pitch + mouseY, config.minPitch, config.maxPitch);
    }

    // LateUpdate garante que a câmera se move DEPOIS do jogador/física no FixedUpdate,
    // evitando tremedeira (jitter) ao seguir o alvo.
    private void LateUpdate()
    {
        if (target == null) return;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 focusPoint = target.position + target.TransformDirection(config.shoulderOffset);
        Vector3 desiredPosition = focusPoint - rotation * Vector3.forward * config.distance;

        desiredPosition = HandleCollision(focusPoint, desiredPosition);

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref currentVelocity,
            1f / config.positionSmoothSpeed
        );
        transform.rotation = rotation;
    }

    /// <summary>
    /// Faz um SphereCast do ponto de foco até a posição desejada da câmera.
    /// Se bater em algo, aproxima a câmera até logo antes da colisão.
    /// </summary>
    private Vector3 HandleCollision(Vector3 focusPoint, Vector3 desiredPosition)
    {
        Vector3 direction = desiredPosition - focusPoint;
        float distance = direction.magnitude;

        if (Physics.SphereCast(
                focusPoint,
                config.collisionRadius,
                direction.normalized,
                out RaycastHit hit,
                distance,
                config.collisionLayer))
        {
            float safeDistance = Mathf.Max(hit.distance - config.collisionBuffer, 0.1f);
            return focusPoint + direction.normalized * safeDistance;
        }

        return desiredPosition;
    }

    /// <summary>
    /// Direção "para frente" no plano horizontal, baseada só no yaw da câmera
    /// (ignora o pitch). É essa que o PlayerMovementController deveria usar
    /// pra mover o jogador de forma consistente mesmo olhando pra cima/baixo.
    /// </summary>
    public Vector3 FlatForward => Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
    public Vector3 FlatRight => Quaternion.Euler(0f, yaw, 0f) * Vector3.right;
}