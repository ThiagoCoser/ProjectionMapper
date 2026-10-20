using UnityEngine;
using UnityEngine.Video;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class Controles : MonoBehaviour
{
    [Header("Referências")]
    public Material materialMascara;
    public VideoPlayer videoPlayer;
    public Transform quadTela;

    [Header("Configurações do Filtro")]
    public float velocidadeThreshold = 0.5f; // Valor alterado para funcionar como velocidade contínua

    [Header("Configurações de Calibração Espacial")]
    public float velocidadeMovimento = 0.5f;
    public float velocidadeEscala = 0.5f;

    private List<string> listaVideos = new List<string>();
    private int videoAtualIndex = 0;
    private string caminhoPastaVideos;

    void Start()
    {
        string pastaRaiz = Path.GetDirectoryName(Application.dataPath);
        caminhoPastaVideos = Path.Combine(pastaRaiz, "videos");

        if (!Directory.Exists(caminhoPastaVideos))
        {
            Directory.CreateDirectory(caminhoPastaVideos);
        }

        CarregarListaDeVideos();

        if (listaVideos.Count > 0)
        {
            videoPlayer.source = VideoSource.Url;
            TocarVideoIndex(0);
        }
    }

    void Update()
    {
        // --- 1. CONTROLES DO THRESHOLD (Setas Cima / Baixo contínuas) ---
        // Agora usamos GetKey para detectar se a tecla está sendo segurada
        if (Input.GetKey(KeyCode.UpArrow))
        {
            AjustarThreshold(velocidadeThreshold * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            AjustarThreshold(-velocidadeThreshold * Time.deltaTime);
        }

        // --- 2. CONTROLES DE VÍDEO (Setas Direita / Esquerda) ---
        if (listaVideos.Count > 0)
        {
            // Estes continuam com GetKeyDown para não pular 60 vídeos por segundo se segurar a tecla
            if (Input.GetKeyDown(KeyCode.RightArrow)) ProximoVideo();
            if (Input.GetKeyDown(KeyCode.LeftArrow)) VideoAnterior();
        }

        // --- 3. CONTROLES DE CALIBRAÇÃO ESPACIAL (W, A, S, D e + / -) ---
        if (quadTela != null)
        {
            // Mover para os lados e cima/baixo (Pan)
            if (Input.GetKey(KeyCode.W)) quadTela.Translate(Vector3.up * velocidadeMovimento * Time.deltaTime);
            if (Input.GetKey(KeyCode.S)) quadTela.Translate(Vector3.down * velocidadeMovimento * Time.deltaTime);
            if (Input.GetKey(KeyCode.A)) quadTela.Translate(Vector3.left * velocidadeMovimento * Time.deltaTime);
            if (Input.GetKey(KeyCode.D)) quadTela.Translate(Vector3.right * velocidadeMovimento * Time.deltaTime);

            // Escala / Zoom In (+) - Multiplica mantendo a proporção exata e ignora o Z
            if (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.KeypadPlus))
            {
                float fatorZoom = 1f + (velocidadeEscala * Time.deltaTime);
                quadTela.localScale = new Vector3(
                    quadTela.localScale.x * fatorZoom,
                    quadTela.localScale.y * fatorZoom,
                    quadTela.localScale.z // O Z continua sempre intacto
                );
            }

            // Escala / Zoom Out (-) - Multiplica mantendo a proporção exata e ignora o Z
            if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus))
            {
                float fatorZoom = 1f - (velocidadeEscala * Time.deltaTime);
                quadTela.localScale = new Vector3(
                    quadTela.localScale.x * fatorZoom,
                    quadTela.localScale.y * fatorZoom,
                    quadTela.localScale.z // O Z continua sempre intacto
                );
            }
        }
    }

    private void AjustarThreshold(float valor)
    {
        if (materialMascara != null && materialMascara.HasProperty("_Threshold"))
        {
            float atual = materialMascara.GetFloat("_Threshold");
            float novoValor = Mathf.Clamp(atual + valor, 0f, 1f);
            materialMascara.SetFloat("_Threshold", novoValor);
        }
    }

    private void CarregarListaDeVideos()
    {
        listaVideos.Clear();
        string[] formatosSuportados = new string[] { ".mp4", ".mov", ".avi", ".mkv" };
        DirectoryInfo dir = new DirectoryInfo(caminhoPastaVideos);
        if (dir.Exists)
        {
            FileInfo[] arquivos = dir.GetFiles("*.*");
            foreach (FileInfo arquivo in arquivos)
            {
                if (formatosSuportados.Contains(arquivo.Extension.ToLower()))
                    listaVideos.Add(arquivo.FullName);
            }
        }
    }

    private void ProximoVideo()
    {
        videoAtualIndex = (videoAtualIndex + 1) % listaVideos.Count;
        TocarVideoIndex(videoAtualIndex);
    }

    private void VideoAnterior()
    {
        videoAtualIndex = (videoAtualIndex - 1 + listaVideos.Count) % listaVideos.Count;
        TocarVideoIndex(videoAtualIndex);
    }

    private void TocarVideoIndex(int index)
    {
        videoPlayer.url = listaVideos[index];
        videoPlayer.Play();
    }
}
