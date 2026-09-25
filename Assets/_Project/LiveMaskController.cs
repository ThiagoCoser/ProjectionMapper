using UnityEngine;

public class LiveMaskController : MonoBehaviour
{
    public Material maskMaterial;
    private WebCamTexture webcamTexture;

    void Start()
    {
        // Busca a primeira webcam disponível
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length > 0)
        {
            // Inicializa a câmera (resolução e framerate ideais para fluidez)
            webcamTexture = new WebCamTexture(devices[0].name, 1280, 720, 30);
            webcamTexture.Play();

            // Injeta o feed ao vivo diretamente na propriedade _Webcam do Shader
            maskMaterial.SetTexture("_Webcam", webcamTexture);
        }
        else
        {
            Debug.LogError("Nenhuma webcam detectada.");
        }
    }

    void OnDestroy()
    {
        // Previne vazamento de memória ao fechar o Play Mode
        if (webcamTexture != null) webcamTexture.Stop();
    }
}
