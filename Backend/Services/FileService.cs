using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace RastreadoresAPI.Services
{
    public class FileService
    {
        private readonly IConfiguration _config;

        public FileService(IConfiguration config)
        {
            _config = config;
        }

        public string SalvarArquivo(string nomeRastreador, string conteudo, bool isErro)
        {
            // Limpa nome pra não dar erro no Windows
            var nomeLimpo = Regex.Replace(nomeRastreador, @"[^a-zA-Z0-9_-]", "_");
            
            var pastaTipo = isErro ? "Erros" : "";
            
            // Pega a raiz de onde o projeto tá rodando
            var raiz = Directory.GetCurrentDirectory();
            var caminhoBase = Path.Combine(raiz, "Rastreadores", nomeLimpo, pastaTipo);

            if (!Directory.Exists(caminhoBase))
                Directory.CreateDirectory(caminhoBase);

            var nomeArquivo = $"{DateTime.Now:yyyyMMdd_HHmmss}_{(isErro ? "error" : "response")}.txt";
            var pathCompleto = Path.Combine(caminhoBase, nomeArquivo);

            File.WriteAllText(pathCompleto, conteudo);

            // Retorna o caminho "web-friendly" (com barras normais)
            return Path.Combine(nomeLimpo, pastaTipo, nomeArquivo).Replace("\\", "/");
        }

        public string LerArquivo(string pathRelativo)
        {
            // Segurança básica
            if (pathRelativo.Contains("..")) throw new Exception("Caminho inválido");

            var raiz = Directory.GetCurrentDirectory();
            var pathCompleto = Path.Combine(raiz, "Rastreadores", pathRelativo);

            if (!File.Exists(pathCompleto)) return "ERRO: Arquivo não encontrado no disco.";

            return File.ReadAllText(pathCompleto);
        }
    }
}