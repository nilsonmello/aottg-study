using UnityEngine;

[RequireComponent(typeof(PlayerMovementAdvanced))]
public class GrappleHook : MonoBehaviour
{
    [Header("References")]
    private Camera playerCam;
    private PlayerMovementAdvanced pm;
    private LineRenderer lr;

    [Header("Detecção do Ponto")]
    public LayerMask grappleMask;
    public float maxGrappleDistance = 25f;
    public float grappleSphereRadius = 1.5f;

    [Header("Comportamento")]
    public float minDistanceToRelease = 2f;
    public float maxGrappleTime = 2f;
    private float grappleTimer;
    private float ropeLength;

    [Header("Steering")]
    public float steerAcceleration = 15f;

    [Header("Puxão em Direção ao Ponto")]
    public float pullForce = 4f;

    [Header("Corda Visual")]
    public float ropeWidth = 0.05f;
    public Color ropeColor = Color.white;
    public Material ropeMaterial;

    [Header("Input")]
    public KeyCode grappleKey = KeyCode.E;

    public bool grappling;
    private Vector3 grapplePoint;

    private void Start()
    {
        pm = GetComponent<PlayerMovementAdvanced>();

        playerCam = GetComponentInChildren<Camera>();
        if (playerCam == null)
            playerCam = Camera.main;

        SetupLineRenderer();
    }

    private void SetupLineRenderer()
    {
        lr = GetComponent<LineRenderer>();
        if (lr == null)
            lr = gameObject.AddComponent<LineRenderer>();

        lr.positionCount = 2;
        lr.startWidth = ropeWidth;
        lr.endWidth = ropeWidth;
        lr.material = ropeMaterial != null ? ropeMaterial : new Material(Shader.Find("Sprites/Default"));
        lr.startColor = ropeColor;
        lr.endColor = ropeColor;
        lr.useWorldSpace = true;
        lr.enabled = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(grappleKey) && !grappling)
            TryStartGrapple();

        if (grappling)
        {
            UpdateGrapple();
            DrawRope();

            bool closeEnough = Vector3.Distance(transform.position, grapplePoint) < minDistanceToRelease;
            grappleTimer -= Time.deltaTime;

            if (closeEnough || grappleTimer <= 0f || Input.GetKeyUp(grappleKey))
                StopGrapple();
        }
    }

    private void TryStartGrapple()
    {
        Vector3 origin = playerCam.transform.position;
        Vector3 dir = playerCam.transform.forward;

        if (Physics.SphereCast(origin, grappleSphereRadius, dir, out RaycastHit hit, maxGrappleDistance, grappleMask))
        {
            grapplePoint = hit.point;
            ropeLength = Vector3.Distance(transform.position, grapplePoint);

            grappling = true;
            pm.grappling = true;
            grappleTimer = maxGrappleTime;

            lr.enabled = true;
        }
    }

    private void UpdateGrapple()
    {
        Vector3 velocity = pm.horizontalVelocity + Vector3.up * pm.verticalVelocity;

        velocity += Vector3.up * pm.gravity * Time.deltaTime;

        Vector3 toAnchor = grapplePoint - transform.position;
        float distance = toAnchor.magnitude;
        Vector3 radialDir = toAnchor.normalized; // aponta do player PARA o ponto

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = pm.orientation.right * h + pm.orientation.forward * v;

        bool taut = distance >= ropeLength;

        if (taut)
        {
            // Corda esticada: input só pode influenciar tangencialmente
            // (não pode esticar ainda mais a corda).
            Vector3 steerDir = Vector3.ProjectOnPlane(inputDir, radialDir);
            if (steerDir.sqrMagnitude > 0.0001f)
                velocity += steerDir.normalized * steerAcceleration * Time.deltaTime;

            // Cancela SÓ a componente que afasta do ponto (esticaria a corda).
            // A componente que aproxima (velocidade "pra dentro") não é tocada —
            // é isso que evita o "empurrão" pra baixo/lado ao prender de frente.
            float outward = Vector3.Dot(velocity, -radialDir);
            if (outward > 0f)
                velocity += radialDir * outward;

            velocity += radialDir * pullForce * Time.deltaTime;
        }
        else
        {
            // Corda frouxa: sem nenhuma restrição, só um pouco de controle aéreo.
            if (inputDir.sqrMagnitude > 0.0001f)
                velocity += inputDir.normalized * steerAcceleration * Time.deltaTime;
        }

        pm.horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
        pm.verticalVelocity = velocity.y;
    }

    private void DrawRope()
    {
        lr.SetPosition(0, transform.position);
        lr.SetPosition(1, grapplePoint);
    }

    private void StopGrapple()
    {
        grappling = false;
        pm.grappling = false;
        lr.enabled = false;
    }
}