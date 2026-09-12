using UnityEngine;

/// <summary>
/// Configuração mínima do gancho — só o essencial pra pendurar e balançar.
/// Vamos adicionar mais coisas (reel, boost, gancho duplo) em cima dessa base
/// depois que essa parte estiver com a sensação certa.
/// </summary>
[CreateAssetMenu(fileName = "HookConfig", menuName = "Grapple/Hook Config")]
public class HookConfig : ScriptableObject
{
    [Header("Mira / Alcance")]
    [Tooltip("Distância máxima que o gancho pode grudar.")]
    public float maxDistance = 40f;

    [Tooltip("Camadas que o gancho pode grudar (prédios, terreno, árvores).")]
    public LayerMask grappleLayer;
}