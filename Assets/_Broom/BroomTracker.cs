using UnityEngine;

public class BroomTracker : MonoBehaviour
{
    [Header("Configurações Principais")]
    public Transform apagador;
    public Renderer telaDebug;

    [Header("Correção de Eixos")]
    public bool inverterEixoX = false;
    public bool inverterEixoY = false;
    public float distanciaDaParede = 10f; // Distância Z da câmera principal até o plano da parede

    [Header("Filtro de Cor (Verde Chroma Key)")]
    [Range(0f, 1f)] public float hueMin = 0.25f;
    [Range(0f, 1f)] public float hueMax = 0.40f;
    [Range(0f, 1f)] public float saturacaoMin = 0.4f;
    [Range(0f, 1f)] public float brilhoMin = 0.3f;

    [Header("Análise e Performance")]
    public int pularPixels = 4;
    public float velocidadeSuavizacao = 15f;

    [Header("Modo Debug (Desative ao projetar)")]
    public bool mostrarMascaraPretoEBranco = true;

    private WebCamTexture webcamTexture;
    private Texture2D texturaMascara;
    private Color32[] pixelsMascara;

    void Start()
    {
        webcamTexture = new WebCamTexture();
        webcamTexture.Play();
    }

    void Update()
    {
        if (webcamTexture.didUpdateThisFrame && webcamTexture.width > 100)
        {
            // Inicializa a textura de debug apenas quando a webcam já tem resolução carregada
            if (texturaMascara == null || texturaMascara.width != webcamTexture.width)
            {
                texturaMascara = new Texture2D(webcamTexture.width, webcamTexture.height);
                pixelsMascara = new Color32[webcamTexture.width * webcamTexture.height];
            }

            RastrearCentroDeMassa();
        }
    }

    void RastrearCentroDeMassa()
    {
        Color32[] pixels = webcamTexture.GetPixels32();
        int width = webcamTexture.width;
        int height = webcamTexture.height;

        float somaX = 0;
        float somaY = 0;
        int pontosEncontrados = 0;

        // Limpa a máscara para preto se o debug estiver ativo
        if (mostrarMascaraPretoEBranco)
        {
            for (int i = 0; i < pixelsMascara.Length; i++)
                pixelsMascara[i] = new Color32(0, 0, 0, 255);
        }

        // Analisa a imagem e tira a média (Centro de Massa)
        for (int y = 0; y < height; y += pularPixels)
        {
            for (int x = 0; x < width; x += pularPixels)
            {
                int index = y * width + x;
                Color32 pixel = pixels[index];

                Color.RGBToHSV(pixel, out float h, out float s, out float v);

                if (h >= hueMin && h <= hueMax && s >= saturacaoMin && v >= brilhoMin)
                {
                    somaX += x;
                    somaY += y;
                    pontosEncontrados++;

                    if (mostrarMascaraPretoEBranco)
                    {
                        // Pinta de branco na máscara para podermos ver o que foi detectado
                        pixelsMascara[index] = new Color32(255, 255, 255, 255);
                    }
                }
            }
        }

        // Atualiza a tela de debug
        if (telaDebug != null)
        {
            if (mostrarMascaraPretoEBranco)
            {
                texturaMascara.SetPixels32(pixelsMascara);
                texturaMascara.Apply();
                telaDebug.material.mainTexture = texturaMascara;
            }
            else
            {
                telaDebug.material.mainTexture = webcamTexture;
            }
        }

        // Se encontrou a vassoura, move a bolinha
        if (pontosEncontrados > 20)
        {
            float centroX = somaX / pontosEncontrados;
            float centroY = somaY / pontosEncontrados;

            float viewportX = centroX / width;
            float viewportY = centroY / height;

            // Corrige a inversão dos eixos conforme as chaves do Inspector
            if (inverterEixoX) viewportX = 1.0f - viewportX;
            if (inverterEixoY) viewportY = 1.0f - viewportY;

            // Mapeia para a tela do Unity
            Vector3 posicaoTela = new Vector3(viewportX * Screen.width, viewportY * Screen.height, distanciaDaParede);
            Vector3 posicaoMundo = Camera.main.ScreenToWorldPoint(posicaoTela);

            // Zera a profundidade para manter a bolinha num plano 2D se preferir
            posicaoMundo.z = 0;

            // Interpola a posição da esfera para ela não tremer
            apagador.position = Vector3.Lerp(apagador.position, posicaoMundo, Time.deltaTime * velocidadeSuavizacao);
        }
    }
}
