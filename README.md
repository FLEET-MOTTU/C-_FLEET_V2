# C--POC V2


## Descrição do Projeto

Esta API RESTful é um componente do sistema de gerenciamento de pátios da empresa Mottu. Desenvolvida em C# com ASP.NET Core 8 e Entity Framework Core, seu objetivo principal é gerenciar o cadastro, o estado e o rastreamento de motocicletas dentro dos pátios.


A API agora implementa um **CRUD completo** para as entidades `Moto` (junto de sua `TagBle` associada) e `Beacon`. Ela está preparada para processar eventos de IoT simulados para rastreamento de motos via um endpoint dedicado, que será alimentado por um simulador Python. A persistência dos dados é feita em um banco de dados Oracle.

**Principais Funcionalidades**
* **CRUD completo para Motos e suas Tags BLE associadas.**
* **CRUD completo para Beacons**, que são os pontos de rastreamento do pátio.
* **Paginação e HATEOAS** (Hypermedia as an Engine of Application State) nos endpoints de listagem, seguindo as melhores práticas REST.
* Validação de dados de entrada e regras de negócio robustas.
* Geração e aplicação de migrations do banco de dados via EF Core.
* Documentação da API com Swagger, incluindo exemplos de payloads.
* Endpoint para recebimento de eventos simulados de IoT.

**Tecnologias Utilizadas:**
* C# e ASP.NET Core 8
* Entity Framework Core 8 e Oracle Database
* **AutoMapper** para mapeamento de DTOs e entidades
* **Swashbuckle.AspNetCore.Filters** para exemplos no Swagger
* Docker e Docker Compose para ambiente de desenvolvimento e deploy
* Swagger (Swashbuckle) para documentação da API
* Princípios SOLID e Clean Architecture


## Estrutura do Projeto (Visão Geral)

* **`Controllers/`**: Contém os API Controllers para `Motos`, `Beacons` e `IoTEvents`.
* **`Services/`**: Contém a lógica de negócio e orquestração das operações.
* **`DTOs/`**: Define os objetos de transferência de dados.
    * **`ValidationAttributes/`**: Contém atributos de validação customizados.
* **`Entities/`**: Contém as classes que representam as tabelas do banco de dados.
    # C--POC V1 — API de Gerenciamento de Pátios (Mottu)

    Este repositório contém a API RESTful responsável pelo gerenciamento de motocicletas (Motos), Beacons e eventos de IoT em pátios. Desenvolvida em C# com ASP.NET Core 8 e EF Core, a API oferece CRUD, paginação, tratamento de regras de negócio, integração com serviços externos e documentação automática via Swagger.

    Principais responsabilidades da API
    - CRUD completo para Moto (inclui TagBle associada)
    - CRUD completo para Beacon (gateways do pátio)
    - Endpoints de ingestão de eventos IoT (ex.: `/api/iot-events/tag-interaction`)
    - Paginação e respostas padronizadas (`PaginatedResponseDto<T>`)
    - Validação de entrada, tratamento centralizado de exceções e códigos HTTP apropriados
    - Integração com um modelo ML (ML.NET) para previsões de vistoria/reparo
    - Documentação OpenAPI/Swagger com comentários XML gerados a partir do código

    Resumo da arquitetura
    - Controllers: expõem os endpoints e fazem orquestração mínima
    - Services: contêm a lógica de negócio e validações
    - Data (AppDbContext): configuração e mapeamento do EF Core para Oracle
    - DTOs e Profiles (AutoMapper) para mapeamento entre entidades e respostas

    Observação importante sobre execução: a API precisa ser executada via Docker Compose (docker compose up --build) no ambiente atual do repositório — sem o Compose a classe Program.cs não conseguirá ler as váriaveis de ambiente (.env) pois o código já está adaptado para produção com deploy no Azure Container Instace (ACI).

    Conteúdo desta README
    - Como executar a API (via Docker Compose)
    - Como rodar os testes (unitários e de integração)
    - Como acessar o Swagger e onde encontrar a documentação gerada
    - Como trabalhar com migrations do EF Core
    - Notas sobre integração com Java e "listeners"/consumidores de eventos

    Pré-requisitos
    - Docker Desktop (Windows) com Compose v2
    - Credenciais para conexão com Oracle (usadas por containers ou variáveis de ambiente)

    Executando a API (recomendado: Docker Compose)    
    1) Ajuste segredos/variáveis:
         - Crie um arquivo .env na raiz da pasta Csharp.Api, conforme o .env.example
    3) Construa e inicie os containers:

    ```powershell
    docker compose up --build
    ```

    4) A API estará acessível em `http://localhost:8080` (porta padrão do compose). Acesse o Swagger em:

    ```
    http://localhost:8080/swagger/index.html
    ```

    Se você precisa executar apenas localmente sem Docker (não recomendado): abra `Csharp.Api` e siga os passos de configuração de `user-secrets` para a string de conexão. Algumas dependências de runtime podem não estar disponíveis e a aplicação pode falhar ao aplicar migrations automaticamente.

    Rodando os testes
    - Tests unitários e de integração são projetos separados na solução.
    - Para rodar todos os testes localmente (dotnet SDK instalado):
    - OU navegue até a pasta raiz do projeto (onde está este README e as 3 pastas Csharp.Api, Csharp.Api.Tests...) e rode:

    ```powershell
    dotnet test
    ```

    - Observações:
        - Os testes de integração podem mockar autenticação via `FakeAuthHandler` presente em `Csharp.Api.Tests.Integration`.
        - Se quiser rodar apenas os testes de unidade:

    ```powershell
    dotnet test Csharp.Api.Tests.Unit\Csharp.Api.Tests.Unit.csproj -c Debug
    ```

    Swagger / OpenAPI
    - A documentação OpenAPI é gerada automaticamente via Swashbuckle.
    - Os comentários XML (que você verá no Swagger) são gerados no build em `Csharp.Api/bin/Debug/net8.0/Csharp.Api.xml` e incluídos na geração do Swagger.
    - Endpoints por versão aparecem no Swagger UI (o projeto usa API Versioning; haverá um documento por versão descoberta. VERSÃO ATUAL: 2.0).

    Trabalhando com EF Core e Migrations
    - Para criar uma nova migration (via container de ferramentas fornecido no Compose):

    ```powershell
    docker compose run --rm ef-tools sh -c "dotnet restore && dotnet ef migrations add NomeDescritivoParaSuaMudanca --project Csharp.Api --verbose"
    ```

    - Os arquivos de migration são gerados em `Csharp.Api/Migrations`. Ao iniciar os containers a API tenta aplicar `dbContext.Database.Migrate()` automaticamente.

    Integração com Java e "Listeners"
    - Esta API consome eventos publicados pela api de java em uma fila hospedada no Azure Service Bus
        1) funcionario-criado-queue (responsável por eventos de sincronização entre os bancos de dados dos dois microserviços com relação as entidades FUNCIONARIO, PATEO e ZONA, cuja criação é responsabilidade de um administrador de pátio de Mottu, API de Java)

    - Esta API consome eventos publicados pelos gateways (beacons) de IoT em uma fila hospedada no Azure Service Bus
        2) tag-position-updates (eventos de tracking das motos dentro de um pátio)

    Notas operacionais e dicas
    - XML docs: durante o build a documentação XML é gerada e incluída no Swagger, então sempre rode `dotnet build` após alterar comentários para ver o resultado no Swagger.
    - ML: a API contém integração com um modelo ML (ML.NET). Verifique o caminho do `model.zip` no projeto `Csharp.Api/ML` e ajuste o serviço de previsão se renomear ou re-treinar o modelo.

    Erros e mensagens comuns
    - Se a API falhar ao iniciar dentro do Compose, verifique os logs do container `api`:

    ```powershell
    docker compose logs -f api
    ```

    - Verifique variáveis de conexão com o Oracle e se o container do banco está acessível.

    Integrantes:
    - AMANDA MESQUITA CIRINO DA SILVA - RM 559177
    - BEATRIZ FERREIRA CRUZ - RM555698
    - JOURNEY TIAGO LOPES – RM 556071

    --
