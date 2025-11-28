using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Linq;

namespace RastreadoresAPI.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public void EnviarEmail(string rastreador, List<string> emails, string arquivoRelativo)
        {
            try
            {
                // Busca configuração do appsettings.json
                var smtpSecao = _config.GetSection("SmtpConfig");
                
                // Se não tiver config ou e-mail, nem tenta enviar
                if (!smtpSecao.Exists() || emails == null || !emails.Any()) return;

                var client = new SmtpClient(smtpSecao["Host"], int.Parse(smtpSecao["Port"]))
                {
                    EnableSsl = bool.Parse(smtpSecao["EnableSsl"]),
                    Credentials = new NetworkCredential(smtpSecao["UserName"], smtpSecao["Password"])
                };

                var msg = new MailMessage
                {
                    From = new MailAddress(smtpSecao["FromEmail"]),
                    Subject = $"Retorno Rastreador: {rastreador}",
                    Body = $"Requisição realizada em {DateTime.Now}."
                };

                foreach (var e in emails) 
                {
                    if(!string.IsNullOrWhiteSpace(e)) msg.To.Add(e);
                }

                // Tenta anexar o arquivo. 
                // O caminhoRelativo vem como "Skycop/response.txt", precisamos do caminho físico completo.
                var raiz = Directory.GetCurrentDirectory();
                var pathReal = Path.Combine(raiz, "Rastreadores", arquivoRelativo.Replace("/", "\\"));
                
                if (File.Exists(pathReal))
                {
                    msg.Attachments.Add(new Attachment(pathReal));
                }

                client.Send(msg);
            }
            catch (Exception ex)
            {
                // Falha silenciosa para não travar o usuário, apenas loga no console do servidor
                Console.WriteLine($"[EmailService] Falha ao enviar: {ex.Message}");
            }
        }
    }
}