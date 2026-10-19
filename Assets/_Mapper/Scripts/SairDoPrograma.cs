using UnityEngine;
using UnityEngine.InputSystem;

public class SairDoPrograma : MonoBehaviour
{
    [Tooltip("Arraste a sua Action do Input Action Asset que contém o comando Esc aqui.")]
    public InputActionReference acaoSair;

    private void OnEnable()
    {
        if (acaoSair != null)
        {
            // O SEGREDO ESTÁ AQUI: Habilita o Asset/Mapa inteiro na memória da Build
            acaoSair.asset.Enable();

            // Depois inscreve o evento na ação específica
            acaoSair.action.performed += FecharAplicacao;
        }
    }

    private void OnDisable()
    {
        if (acaoSair != null)
        {
            acaoSair.action.performed -= FecharAplicacao;

            // Desabilita o Asset ao invés da ação isolada
            acaoSair.asset.Disable();
        }
    }

    private void FecharAplicacao(InputAction.CallbackContext context)
    {
        Debug.Log("Encerrando o programa...");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
