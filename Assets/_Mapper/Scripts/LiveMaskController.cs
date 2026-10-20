using UnityEngine;

public class LiveMaskController : MonoBehaviour
{
    [Header("Configurações")]
    public Material maskMaterial;
    public int indiceCameraPadrao = 0; // Você pode mudar isso no Inspector

    private WebCamTexture webcamTexture;
    private WebCamDevice[] devices;
    private int cameraAtualIndex;

    void Start()
    {
        // Busca todas as câmeras conectadas ao computador
        devices = WebCamTexture.devices;

        if (devices.Length > 0)
        {
            Debug.Log("--- CÂMERAS DETECTADAS ---");
            for (int i = 0; i < devices.Length; i++)
            {
                Debug.Log($"Câmera [{i}]: {devices[i].name}");
            }
            Debug.Log("--------------------------");
            Debug.Log("Aperte a tecla 'C' para alternar entre as câmeras.");

            // Tenta iniciar com o índice escolhido no Inspector, ou volta pro 0 se não existir
            cameraAtualIndex = (indiceCameraPadrao >= 0 && indiceCameraPadrao < devices.Length) ? indiceCameraPadrao : 0;
            IniciarCamera(cameraAtualIndex);
        }
        else
        {
            Debug.LogError("Nenhuma webcam detectada.");
        }
    }

    void Update()
    {
        // Troca de câmera ao apertar 'C'
        if (Input.GetKeyDown(KeyCode.C) && devices.Length > 1)
        {
            cameraAtualIndex = (cameraAtualIndex + 1) % devices.Length;
            IniciarCamera(cameraAtualIndex);
        }
    }

    void IniciarCamera(int index)
    {
        // Para a câmera anterior antes de ligar a nova para não travar a porta USB
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            webcamTexture.Stop();
        }

        webcamTexture = new WebCamTexture(devices[index].name, 1280, 720, 30);
        webcamTexture.Play();

        // Injeta o feed ao vivo diretamente na propriedade _Webcam do Shader
        maskMaterial.SetTexture("_Webcam", webcamTexture);

        Debug.Log($"[Câmera Ativa] -> {devices[index].name}");
    }

    void OnDestroy()
    {
        if (webcamTexture != null) webcamTexture.Stop();
    }
}
