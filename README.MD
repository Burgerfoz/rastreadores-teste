# Rastreadores Web App

Aplicação Full-Stack para gerenciamento de rastreadores.
Projeto de nivelamento técnico: AngularJS + .NET 6.

## 🚀 Tecnologias

- **Backend:** .NET 6 (Web API)
- **Frontend:** AngularJS 1.8 + Bootstrap 5 (Arquivos locais em `/lib`)
- **Persistência:** LocalStorage (Configs) e Sistema de Arquivos (Logs)

## ⚠️ Nota Importante sobre a API

O Backend é uma **API REST pura**. 
Ao acessar `http://localhost:5000` no navegador, é esperado receber um erro **404 Not Found**, pois não há página configurada na raiz da API. Isso indica que o servidor está **rodando corretamente**.

A interface do usuário fica separada na pasta `Frontend`.

## ⚙️ Como Rodar o Projeto

### Passo 1: Iniciar o Backend
1. Certifique-se de ter o **.NET 6 SDK** instalado.
2. Navegue até a pasta `Backend`:
   ```bash
   cd Backend
(Opcional) Configure seu e-mail no arquivo appsettings.json para testar envios reais.

Execute o servidor:

Bash

dotnet run
Aguarde a mensagem "Now listening on: http://localhost:5000"

Passo 2: Iniciar o Frontend
Não é necessário servidor web (Apache/Nginx).

Vá até a pasta Frontend.

Dê um duplo clique no arquivo index.html para abrir no seu navegador padrão.

Verifique o badge "Servidor: ONLINE" no topo da página.

📋 Funcionalidades Implementadas
CRUD Completo: Adicionando e editando rastreadores persistidos no LocalStorage.

Requisição Real: Conexão com APIs externas (Skycop, SmartGPS, Ubisat).

Logs em Arquivo: Respostas salvas em .txt formatado (JSON Pretty) na pasta Backend/Rastreadores.

Envio de E-mail: Integração via SMTP (Gmail) com anexo do log gerado.

Tratamento de Erros: Exibição de falhas e bloqueio de botões quando não há dados.

Autor: Emerson Rodrigo Alves

Status do Projeto: Aguardando avaliação


