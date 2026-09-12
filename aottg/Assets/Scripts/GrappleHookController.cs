using UnityEngine;

/// <summary>
/// BASE mínima do sistema de gancho: mirar, disparar, pendurar, balançar,
/// soltar. Nenhuma física é calculada na mão — um ConfigurableJoint
/// configurado como restrição esférica de distância faz o trabalho todo.
/// Isso dá gravidade real + corda rígida de verdade, resolvidas pelo PhysX.
///
/// De propósito, NÃO tem: reel-in/out, boost, controle de swing, gancho
/// duplo. É a base pura pra sentirmos se o pêndulo em si está bom antes de
/// adicionar qualquer coisa em cima.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class GrappleHookController : MonoBehaviour
{
    public enum HookState { Idle, Attached }

    [Header("Referências")]
    [Tooltip("Câmera usada pra mirar o gancho.")]
    [SerializeField] private Camera aimCamera;

    [Tooltip("Ponto de onde o cabo 'sai' visualmente. Se vazio, usa a própria posição do jogador.")]
    [SerializeField] private Transform hookOrigin;

    [SerializeField] private HookConfig config;

    [Tooltip("Opcional: LineRenderer pra desenhar o cabo.")]
    [SerializeField] private LineRenderer rope;

    private Rigidbody rb;
    private ConfigurableJoint joint;
    private GameObject anchorObject;   // pequeno objeto cinemático no ponto do gancho
    private HookState state = HookState.Idle;
    private Vector3 anchorPoint;

    public HookState CurrentState => state;
    public Vector3 AnchorPoint => anchorPoint;
    public bool IsGrappled => state == HookState.Attached;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (hookOrigin == null) hookOrigin = transform;
        if (aimCamera == null) aimCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && state == HookState.Idle)
        {
            TryAttach();
        }
        else if (Input.GetMouseButtonUp(0) && state == HookState.Attached)
        {
            Detach();
        }

        UpdateRopeVisual();
    }

    /// <summary>
    /// Raycast a partir da câmera. Se acertar algo grampeável dentro do
    /// alcance, gruda IMEDIATAMENTE no ponto e cria o ConfigurableJoint.
    ///
    /// O joint se conecta a um pequeno objeto CINEMÁTICO criado no ponto do
    /// gancho, em vez de usar connectedBody = null. ConfigurableJoint tem um
    /// comportamento conhecido de não respeitar direito o connectedAnchor em
    /// espaço de mundo quando não há um connectedBody de verdade — conectar
    /// a um Rigidbody kinematic real é a forma confiável de evitar isso.
    /// </summary>
    private void TryAttach()
    {
        Ray ray = new Ray(aimCamera.transform.position, aimCamera.transform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, config.maxDistance, config.grappleLayer))
        {
            return; // não acertou nada grampeável — não faz nada
        }

        anchorPoint = hit.point;
        state = HookState.Attached;

        anchorObject = new GameObject("GrappleAnchor (temp)");
        anchorObject.transform.position = anchorPoint;
        Rigidbody anchorRb = anchorObject.AddComponent<Rigidbody>();
        anchorRb.isKinematic = true;
        anchorRb.useGravity = false;

        joint = gameObject.AddComponent<ConfigurableJoint>();
        joint.connectedBody = anchorRb;
        joint.autoConfigureConnectedAnchor = true; // com connectedBody real, o auto-cálculo funciona direito

        // Restrição esférica: livre pra se mover em qualquer direção até a
        // distância do disparo, travado (não elástico) a partir daí.
        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;

        // Sem restrição de rotação nenhuma — o jogador já trava a própria
        // rotação via Rigidbody.freezeRotation, não precisamos do joint pra isso.
        joint.angularXMotion = ConfigurableJointMotion.Free;
        joint.angularYMotion = ConfigurableJointMotion.Free;
        joint.angularZMotion = ConfigurableJointMotion.Free;

        var limit = new SoftJointLimit
        {
            limit = Vector3.Distance(rb.position, anchorPoint)
            // spring/damper do limite ficam em 0 (padrão) = parada rígida, sem elasticidade
        };
        joint.linearLimit = limit;

        // Projection: corrige (encaixa de volta) a posição sempre que o erro
        // de distância passar de projectionDistance. Sem isso, sob força
        // constante (gravidade), o solver pode deixar a corda "esticar"
        // aos poucos além do limite configurado, mesmo com spring em 0.
        joint.projectionMode = JointProjectionMode.PositionAndRotation;
        joint.projectionDistance = 0.01f;
        joint.projectionAngle = 0f;
    }

    public void Detach()
    {
        if (state == HookState.Idle) return;

        state = HookState.Idle;
        if (joint != null) Destroy(joint);
        if (anchorObject != null) Destroy(anchorObject);
    }

    private void UpdateRopeVisual()
    {
        if (rope == null) return;

        rope.enabled = state == HookState.Attached;
        if (state != HookState.Attached) return;

        rope.positionCount = 2;
        rope.SetPosition(0, hookOrigin.position);
        rope.SetPosition(1, anchorPoint);
    }

    private void OnDrawGizmosSelected()
    {
        if (state == HookState.Attached)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, anchorPoint);
            Gizmos.DrawWireSphere(anchorPoint, 0.3f);
        }
    }
}