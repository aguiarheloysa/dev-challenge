
# Desafio Umbler

Esta é uma aplicação web que recebe um domínio e mostra suas informações de DNS.

Este é um exemplo real de sistema que utilizamos na Umbler.

Ex: Consultar os dados de registro do dominio `umbler.com`

**Retorno:**
- Name servers (ns254.umbler.com)
- IP do registro A (177.55.66.99)
- Empresa que está hospedado (Umbler)

Essas informações são descobertas através de consultas nos servidores DNS e de WHOIS.

*Obs: WHOIS (pronuncia-se "ruís") é um protocolo específico para consultar informações de contato e DNS de domínios na internet.*

Nesta aplicação, os dados obtidos são salvos em um banco de dados, evitando uma segunda consulta desnecessaria, caso seu TTL ainda não tenha expirado.

*Obs: O TTL é um valor em um registro DNS que determina o número de segundos antes que alterações subsequentes no registro sejam efetuadas. Ou seja, usamos este valor para determinar quando uma informação está velha e deve ser renovada.*

Tecnologias Backend utilizadas:

- C#
- Asp.Net Core
- MySQL
- Entity Framework

Tecnologias Frontend utilizadas:

- Webpack
- Babel
- ES7

Para rodar o projeto você vai precisar instalar:

- dotnet Core SDK (https://www.microsoft.com/net/download/windows dotnet Core 6.0.201 SDK)
- Um editor de código, acoselhamos o Visual Studio ou VisualStudio Code. (https://code.visualstudio.com/)
- NodeJs v17.6.0 para "buildar" o FrontEnd (https://nodejs.org/en/)
- Um banco de dados MySQL (vc pode rodar localmente ou criar um site PHP gratuitamente no app da Umbler https://app.umbler.com/ que lhe oferece o banco Mysql adicionamente)

Com as ferramentas devidamente instaladas, basta executar os seguintes comandos:

Para "buildar" o javascript basta executar:

`npm install`
`npm run build`

Para Rodar o projeto:

Execute a migration no banco mysql:

`dotnet tool update --global dotnet-ef`
`dotnet tool ef database update`

E após: 

`dotnet run` (ou clique em "play" no editor do vscode)

# Objetivos:

Se você rodar o projeto e testar um domínio, verá que ele já está funcionando. Porém, queremos melhorar varios pontos deste projeto:

# FrontEnd

 - Os dados retornados não estão formatados, e devem ser apresentados de uma forma legível.
 - Não há validação no frontend permitindo que seja submetido uma requsição inválida para o servidor (por exemplo, um domínio sem extensão).
 - Está sendo utilizado "vanilla-js" para fazer a requisição para o backend, apesar de já estar configurado o webpack. O ideal seria utilizar algum framework mais moderno como ReactJs ou Blazor.  

# BackEnd

 - Não há validação no backend permitindo que uma requisição inválida prossiga, o que ocasiona exceptions (erro 500).
 - A complexidade ciclomática do controller está muito alta, o ideal seria utilizar uma arquitetura em camadas.
 - O DomainController está retornando a própria entidade de domínio por JSON, o que faz com que propriedades como Id, Ttl e UpdatedAt sejam mandadas para o cliente web desnecessariamente. Retornar uma ViewModel (DTO) neste caso seria mais aconselhado.

# Testes

 - A cobertura de testes unitários está muito baixa, e o DomainController está impossível de ser testado pois não há como "mockar" a infraestrutura.
 - O Banco de dados já está sendo "mockado" graças ao InMemoryDataBase do EntityFramework, mas as consultas ao Whois e Dns não. 

# Dica

- Este teste não tem "pegadinha", é algo pensado para ser simples. Aconselhamos a ler o código, e inclusive algumas dicas textuais deixadas nos testes unitários. 
- Há um teste unitário que está comentado, que obrigatoriamente tem que passar.
- Diferencial: criar mais testes.

# Entrega

- Enviei o link do seu repositório com o código atualizado.
- O repositório deve estar público para que possamos acessar..
- Modifique Este readme adicionando informações sobre os motivos das mudanças realizadas.

# Modificações:

Este documento descreve as modificações realizadas no projeto. As alterações foram organizadas seguindo os princípios de arquitetura limpa, com separação clara de responsabilidade entre camadas.

# Controller
DomainController.cs
É responsável por expor endpoints HTTP para consulta de domínios.
Responsabilidades:
Receber requisições HTTP, validar entrada básica, delegar processamento ao Service e por fim retornar a resposta da requisição formatada.

# Service
DomainService.cs
Implementa o caso de uso principal de consulta de domínio.
Responsabilidades:
Orquestrar o fluxo de consulta, verificar cache no repositório, consultar DNS/WHOIS externo quando necessário, atualizar cache persistido, mapear dados para DTO de saída, injeção de dependências via interfaces, uso de wrapper para cliente WHOIS, ajustes para permitir mockar testes.

DomainAPIService.cs
Encapsula integrações externas (DNS/WHOIS)
Responsabilidades:
Isolar chamadas externas.

WhoisClientWrapper.cs
Responsabilidades:
Evitar dependência direta de bibliotecas externas no service principal.

# Repository
DomainRepository.cs
Implementação de acesso a dados via Entity Framework Core.
Responsabilidades:
Buscar domínio no banco de dados, inserir registros, atualizar cachê, persistir resultados.

# Models
DatabaseContext - atualizado
Responsabilidades:
Mapear entidades, configurar o banco e gerenciar migrations.
Desacoplamento: o DatabaseContext não possui mais a entidade Domain.

Domain.cs
Responsabilidades:
Representar a entidade “Domain” do sistema, servir como contrato de dados interno da aplicação, mapear uma tabela do banco de dados

DomainQueryResult.cs
Responsabilidades:
Modelagem de Resposta de Interface, simplificação de dados

# Interface

IDomainValidator.cs
Responsabilidades:
Separar validação da lógica de negócio.

IWhoisClientWrapper.cs
Responsabilidades:
Isolar dependência externa, facilitar mocks/testes.

# DTO
	Objeto de transferência de dados da API.
DomainDTO.cs
Responsabilidades:
Definir formato de saída, evitar exposição direta da entidade de domínio.

# Camada de Interface de Usuário
Index.cshtml
Layout.cshtml
Responsabilidades:
Capturar o domínio informado, chamar o backend, exibir resultados formatados.

# Docker-Compose
Configuração de serviço de banco de dados
app.db
app.db-shm
app.db-wal
Justificativa: execução imediata, testes locais rápidos.


## TESTES

DomainServiceTests.cs
Consulta em cache, consulta externa, atualização de cache expirado.

ControllersTests.cs
Mocks adequados, injeção de novas dependências, asserts atualizados.
Justificativa: as refatorações de arquitetura exigiram atualizações dos testes.

