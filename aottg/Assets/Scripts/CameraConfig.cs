using UnityEngine;

/// <summary>
/// Configuração da câmera de terceira pessoa orbital.
/// Crie via Create > Grapple > Camera Config.
/// </summary>
[CreateAssetMenu(fileName = "CameraConfig", menuName = "Grapple/Camera Config")]
public class CameraConfig : ScriptableObject
{
    [Header("Sensibilidade do mouse")]
    public float sensitivityX = 3f;
    public float sensitivityY = 2f;

    [Tooltip("Inverte o eixo Y (olhar pra cima/baixo).")]
    public bool invertY = false;

    [Header("Ângulos")]
    [Tooltip("Quanto a câmera pode olhar pra baixo (graus).")]
    public float minPitch = -40f;

    [Tooltip("Quanto a câmera pode olhar pra cima (graus).")]
    public float maxPitch = 75f;

    [Header("Posicionamento")]
    [Tooltip("Distância padrão da câmera até o alvo.")]
    public float distance = 5f;

    [Tooltip("Deslocamento lateral/vertical em relação ao ponto de foco (ombro).")]
    public Vector3 shoulderOffset = new Vector3(0.6f, 1.6f, 0f);

    [Header("Suavização")]
    [Tooltip("Velocidade de suavização do movimento da câmera (maior = mais responsiva).")]
    public float positionSmoothSpeed = 20f;

    [Header("Colisão")]
    [Tooltip("Camadas que empurram a câmera pra mais perto do jogador (paredes, chão).")]
    public LayerMask collisionLayer;

    [Tooltip("Raio da esfera usada pra checar colisão da câmera com o cenário.")]
    public float collisionRadius = 0.2f;

    [Tooltip("Margem extra ao colidir, pra câmera não ficar colada na parede.")]
    public float collisionBuffer = 0.15f;
}