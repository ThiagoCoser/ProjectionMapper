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

    [Header("Configurações")]
    public float stepThreshold = 0.05f; // Quantidade de ajuste por clique na seta

    private List<string> listaVideos = new List<string>();
    private int videoAtualIndex = 0;
    private string caminhoPastaVideos;

    void Start()
    {
        // Na Build, Application.dataPath é a pasta "NomeDoJogo_Data".
        // Path.GetDirectoryName nos joga um nível acima, caindo exatamente na pasta onde está o .exe.
        string pastaRaiz = Path.GetDirectoryName(Application.dataPath);
        caminhoPastaVideos = Path.Combine(pastaRaiz, "videos");

        // Se a pasta não existir, a Unity cria automaticamente para você saber onde colocar os vídeos
        if (!Directory.Exists(caminhoPastaVideos))
        {
            Directory.CreateDirectory(caminhoPastaVideos);
            Debug.LogWarning("Pasta 'videos' criada em: " + caminhoPastaVideos + ". Coloque seus arquivos .mp4 lá!");
        }

        CarregarListaDeVideos();

        // Se encontrou vídeos, configura o player para ler do disco (URL) e roda o primeiro
        if (listaVideos.Count > 0)
        {
            videoPlayer.source = VideoSource.Url;
            TocarVideoIndex(0);
        }
        else
        {
            Debug.LogError("Nenhum vídeo encontrado na pasta: " + caminhoPastaVideos);
        }
    }

    void Update()
    {
        // --- CONTROLES DO THRESHOLD (Setas Cima / Baixo) ---
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            AjustarThreshold(stepThreshold);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            AjustarThreshold(-stepThreshold);
        }

        // --- CONTROLES DE VÍDEO (Setas Direita / Esquerda) ---
        if (listaVideos.Count > 0)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ProximoVideo();
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                VideoAnterior();
            }
        }
    }

    private void AjustarThreshold(float valor)
    {
        if (materialMascara != null && materialMascara.HasProperty("_Threshold"))
        {
            float atual = materialMascara.GetFloat("_Threshold");
            // Mathf.Clamp garante que o threshold nunca passe de 1 (branco puro) nem fique menor que 0 (preto)
            float novoValor = Mathf.Clamp(atual + valor, 0f, 1f);
            materialMascara.SetFloat("_Threshold", novoValor);
            Debug.Log("Threshold atualizado para: " + novoValor);
        }
    }

    private void CarregarListaDeVideos()
    {
        listaVideos.Clear();

        // Quais formatos o script vai procurar na pasta
        string[] formatosSuportados = new string[] { ".mp4", ".mov", ".avi", ".mkv" };

        DirectoryInfo dir = new DirectoryInfo(caminhoPastaVideos);
        if (dir.Exists)
        {
            FileInfo[] arquivos = dir.GetFiles("*.*");
            foreach (FileInfo arquivo in arquivos)
            {
                if (formatosSuportados.Contains(arquivo.Extension.ToLower()))
                {
                    listaVideos.Add(arquivo.FullName);
                }
            }
        }
        Debug.Log("Foram carregados " + listaVideos.Count + " vídeos.");
    }

    private void ProximoVideo()
    {
        // O módulo (%) faz o carrossel voltar para o vídeo zero quando chega no final da lista
        videoAtualIndex = (videoAtualIndex + 1) % listaVideos.Count;
        TocarVideoIndex(videoAtualIndex);
    }

    private void VideoAnterior()
    {
        // Lógica matemática para carrossel inverso (ir para o último vídeo se apertar esquerda no primeiro)
        videoAtualIndex = (videoAtualIndex - 1 + listaVideos.Count) % listaVideos.Count;
        TocarVideoIndex(videoAtualIndex);
    }

    private void TocarVideoIndex(int index)
    {
        videoPlayer.url = listaVideos[index];
        videoPlayer.Play();
        Debug.Log("Tocando vídeo: " + Path.GetFileName(listaVideos[index]));
    }
}
