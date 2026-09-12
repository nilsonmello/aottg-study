using UnityEngine;

/// <summary>
/// Configuração do movimento base do jogador (fora do gancho).
/// Crie via Create > Grapple > Movement Config.
/// </summary>
[CreateAssetMenu(fileName = "MovementConfig", menuName = "Grapple/Movement Config")]
public class MovementConfig : ScriptableObject
{
    [Header("Movimento no chão")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 10f;

    [Tooltip("Quão rápido o jogador acelera até a velocidade alvo (maior = resposta mais imediata).")]
    public float groundAcceleration = 40f;

    [Tooltip("Quão rápido o jogador desacelera quando solta o input (freio).")]
    public float groundDeceleration = 50f;

    [Header("Movimento no ar")]
    [Tooltip("Multiplicador de controle enquanto no ar e SEM gancho ativo (1 = igual ao chão).")]
    [Range(0f, 1f)]
    public float airControlMultiplier = 0.35f;

    [Header("Pulo")]
    public float jumpForce = 8f;

    [Tooltip("Multiplicador de gravidade extra pra queda mais 'pesada' e responsiva.")]
    public float extraGravityMultiplier = 1.5f;

    [Header("Detecção de chão")]
    public float groundCheckDistance = 0.3f;
    public LayerMask groundLayer;
}