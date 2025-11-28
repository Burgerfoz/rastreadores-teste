using Microsoft.AspNetCore.Mvc;
using RastreadoresAPI.Services;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RastreadoresAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RastreadoresController : ControllerBase
    {
        private readonly FileService _fileService;
        private readonly EmailService _emailService;

        public RastreadoresController(FileService fileService, EmailService emailService)
        {
            _fileService = fileService;
            _emailService = emailService;
        }

        [HttpPost("requisitar")]
        public async Task<IActionResult> Requisitar([FromBody] RastreadorRequest req)
        {
            // Requisito: Incrementar contador e gerar timestamp no Backend
            var result = new RastreadorResult 
            { 
                UltimaRequisicao = DateTime.Now,
                Contador = req.Contador + 1 // Incrementa o valor recebido
            };

            string conteudo = "";
            bool erro = false;

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(10); // Requisito: Timeout
                try
                {
                    // 1. Autenticação Basic (Login/Senha)
                    if (!string.IsNullOrEmpty(req.Login) && !string.IsNullOrEmpty(req.Senha))
                    {
                        var bytes = Encoding.ASCII.GetBytes($"{req.Login}:{req.Senha}");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(bytes));
                    }
                    // 2. Autenticação Token (Requisito: Token no header)
                    else if (!string.IsNullOrEmpty(req.Token))
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", req.Token);
                    }

                    var resp = await client.GetAsync(req.Endpoint);
                    var rawContent = await resp.Content.ReadAsStringAsync();

                    // Formatação Pretty JSON (Melhoria Visual)
                    try 
                    {
                        if (!string.IsNullOrWhiteSpace(rawContent) && (rawContent.Trim().StartsWith("{") || rawContent.Trim().StartsWith("[")))
                        {
                            conteudo = JToken.Parse(rawContent).ToString(Formatting.Indented);
                        }
                        else
                        {
                            conteudo = rawContent;
                        }
                    }
                    catch { conteudo = rawContent; }

                    if (!resp.IsSuccessStatusCode)
                    {
                        erro = true;
                        conteudo = $"Status: {resp.StatusCode}\nErro: {conteudo}";
                    }
                    else
                    {
                        result.Success = true;
                    }
                }
                catch (Exception ex)
                {
                    erro = true;
                    conteudo = $"Exception: {ex.Message}";
                }
            }

            // Requisito: Salvar arquivo (Sucesso ou Erro)
            result.Arquivo = _fileService.SalvarArquivo(req.Nome, conteudo, erro);
            result.IsErro = erro;

            // Requisito: Enviar e-mail apenas se sucesso
            if (!erro) 
            {
                _emailService.EnviarEmail(req.Nome, req.Emails, result.Arquivo);
            }

            // Retorna contador atualizado e timestamp para o frontend salvar
            return Ok(result);
        }

        [HttpGet("arquivo")]
        public IActionResult GetArquivo([FromQuery] string path)
        {
            try
            {
                var texto = _fileService.LerArquivo(path);
                return Content(texto, "text/plain");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    // DTOs atualizados conforme requisitos
    public class RastreadorRequest
    {
        public string Nome { get; set; }
        public string Endpoint { get; set; }
        public string? Login { get; set; }
        public string? Senha { get; set; }
        public string? Token { get; set; }
        public List<string>? Emails { get; set; }
        public int Contador { get; set; } // Necessário para o backend saber o valor atual
    }

    public class RastreadorResult
    {
        public bool Success { get; set; }
        public bool IsErro { get; set; }
        public string Arquivo { get; set; }
        public DateTime UltimaRequisicao { get; set; }
        public int Contador { get; set; } // Retorna o valor incrementado
    }
}