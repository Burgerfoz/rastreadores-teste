var app = angular.module('rastreadoresApp', []);

app.controller('RastreadoresCtrl', ['$scope', '$http', '$timeout', function($scope, $http, $timeout) {
    
    // Configuração
    var API_URL = "http://localhost:5000/api/rastreadores";
    var KEY_STORAGE = "rastreadores_v1";
    
    $scope.rastreadores = [];
    $scope.form = {};
    $scope.editandoIndex = null;
    $scope.modalConteudo = "Carregando...";
    
    // Status do Servidor (0=Checando, 1=Online, 2=Offline)
    $scope.statusServidor = 0; 

    // --- 1. Inicialização e Destravamento ---
    function init() {
        // Carrega do LocalStorage
        if(localStorage.getItem(KEY_STORAGE)) {
            try {
                $scope.rastreadores = JSON.parse(localStorage.getItem(KEY_STORAGE));
                
                // --- CORREÇÃO DO TRAVAMENTO ---
                // Percorre todos os itens e força o destravamento dos botões
                $scope.rastreadores.forEach(function(r) {
                    r.loading = false;
                });
                
            } catch(e) { console.error("Erro storage", e); }
        }
        
        // Verifica se o servidor está online
        checarStatusApi();
    }

    // --- Nova Função de Status (Mais robusta) ---
    function checarStatusApi() {
        $scope.statusServidor = 0; // Amarelo (Verificando)
        
        // Chama um endpoint leve para testar conexão
        $http.get(API_URL + '/listarArquivos?nomeRastreador=PingCheck')
            .then(function() {
                $scope.statusServidor = 1; // Verde (Online)
            })
            .catch(function() {
                $scope.statusServidor = 2; // Vermelho (Offline)
            });
    }

    // --- 2. CRUD ---
    $scope.salvarRastreador = function() {
        var emailsArr = $scope.form.emailsStr ? $scope.form.emailsStr.split(',').map(e => e.trim()) : ['giovane@sistemagti.com.br'];

        var novoItem = {
            id: new Date().getTime(),
            nome: $scope.form.nome,
            endpoint: $scope.form.endpoint,
            login: $scope.form.login || "",
            senha: $scope.form.senha || "",
            token: $scope.form.token || "",
            emails: emailsArr,
            observacoes: $scope.form.observacoes || "",
            // Garante que nasce destravado
            loading: false, 
            contadorRequisicoes: $scope.editandoIndex !== null ? $scope.rastreadores[$scope.editandoIndex].contadorRequisicoes : 0,
            ultimaRequisicao: $scope.editandoIndex !== null ? $scope.rastreadores[$scope.editandoIndex].ultimaRequisicao : null,
            ultimoArquivo: $scope.editandoIndex !== null ? $scope.rastreadores[$scope.editandoIndex].ultimoArquivo : null,
            ultimoErro: $scope.editandoIndex !== null ? $scope.rastreadores[$scope.editandoIndex].ultimoErro : null
        };

        if ($scope.editandoIndex !== null) {
            $scope.rastreadores[$scope.editandoIndex] = novoItem;
        } else {
            $scope.rastreadores.push(novoItem);
        }

        salvarNoDisco();
        $scope.cancelarEdicao();
    };

    // --- 3. Requisição ---
    $scope.requisitar = function(rastreador) {
        if(rastreador.loading) return; // Evita duplo clique
        
        rastreador.loading = true;
        $scope.statusServidor = 1; // Assume online ao tentar

        var payload = angular.copy(rastreador);
        // Limpeza de nulos
        payload.login = payload.login || "";
        payload.senha = payload.senha || "";
        payload.token = payload.token || "";
        payload.contador = payload.contadorRequisicoes || 0;

        $http.post(API_URL + '/requisitar', payload).then(function(response) {
            var data = response.data;
            
            rastreador.ultimaRequisicao = data.ultimaRequisicao;
            rastreador.contadorRequisicoes = data.contador;

            if (data.success) {
                rastreador.ultimoArquivo = data.arquivo;
                alert('Sucesso! Arquivo salvo.');
            } else {
                rastreador.ultimoErro = data.arquivo;
                alert('Atenção: Houve um erro na resposta. Verifique o log.');
            }
            
            rastreador.loading = false;
            salvarNoDisco();

        }, function(error) {
            console.error(error);
            rastreador.loading = false; // Destrava em caso de erro
            $scope.statusServidor = 2; // Marca offline
            alert('Erro ao conectar com o Backend.');
        });
    };

    // --- 4. Visualização ---
    $scope.visualizarArquivo = function(rastreador, isErro) {
        var nomeArquivo = isErro ? rastreador.ultimoErro : rastreador.ultimoArquivo;
        if (!nomeArquivo) return;

        var modalEl = document.getElementById('modalArquivo');
        var modal = new bootstrap.Modal(modalEl);
        modal.show();
        
        $scope.modalConteudo = "Buscando...";

        $http.get(API_URL + '/arquivo', { params: { path: nomeArquivo } })
            .then(function(response) {
                $scope.modalConteudo = response.data;
            })
            .catch(function(err) {
                $scope.modalConteudo = "Erro ao ler arquivo: " + (err.data || err.statusText);
            });
    };

    // --- Auxiliares ---
    $scope.editar = function(index) {
        $scope.editandoIndex = index;
        var r = $scope.rastreadores[index];
        $scope.form = angular.copy(r);
        if(r.emails) $scope.form.emailsStr = r.emails.join(', ');
    };

    $scope.excluir = function(index) {
        if(confirm('Excluir este rastreador?')) {
            $scope.rastreadores.splice(index, 1);
            salvarNoDisco();
        }
    };

    $scope.cancelarEdicao = function() {
        $scope.form = {};
        $scope.editandoIndex = null;
    };

    $scope.carregarExemplos = function() {
        var exemplos = [
            {
                id: 1, nome: "Skycop", endpoint: "https://api.skycop.com.py/external/devices?key=gDpmzg2h130bA9YTIb4kWO5Ydc1PaWE",
                login: "", senha: "", token: "", emails: ["teste@exemplo.com"]
            },
            {
                id: 2, nome: "SmartGPS", endpoint: "https://sp-beta.trackernet.app/api/get_devices?lang=en&user_api_hash=%242y%2410%24cBI2xpZ5F%2FrzNHf7qJz%2FHegbeLfeWZvxUOnGNy2vsTqhTZevBZI7G",
                login: "", senha: "", token: "", emails: ["teste@exemplo.com"]
            },
            {
                id: 3, nome: "Ubisat", endpoint: "https://oietema.com/ubisat/r/api/service001/?token=lAqwcVdERtyh&action=historical&desde=2025-06-07_03%3A00&hasta=2025-06-09_03%3A00&imei=863844056480178",
                login: "", senha: "", token: "", emails: ["teste@exemplo.com"]
            }
        ];
        
        exemplos.forEach(function(ex) {
            if(!$scope.rastreadores.some(function(r){ return r.nome === ex.nome })) {
                $scope.rastreadores.push(ex);
            }
        });
        salvarNoDisco();
        alert("Exemplos carregados.");
    };

    function salvarNoDisco() {
        // NÃO SALVAR O LOADING NO DISCO (Prevenção futura)
        // Criamos uma cópia limpa para salvar
        var paraSalvar = angular.copy($scope.rastreadores);
        paraSalvar.forEach(function(r) { delete r.loading; });
        
        localStorage.setItem(KEY_STORAGE, JSON.stringify(paraSalvar));
    }

    // Chama o init ao carregar o arquivo
    init();
}]);