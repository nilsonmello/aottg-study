using UnityEngine;

/// <summary>
/// Trava o cursor no centro da tela e o esconde, pro mouse controlar a câmera
/// livremente sem sair da janela do jogo. Esc solta o cursor (útil em menus),
/// clique na tela trava de novo.
/// Coloque este script em qualquer objeto que exista desde o início da cena
/// (ex: o próprio Player, ou um GameManager vazio).
/// </summary>
public class CursorLocker : MonoBehaviour
{
    [Tooltip("Se marcado, já começa travado e escondido assim que a cena carrega.")]
    [SerializeField] private bool lockOnStart = true;

    private void Start()
    {
        if (lockOnStart) LockCursor();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnlockCursor();
        }
        else if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
        {
            LockCursor();
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
