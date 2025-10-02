# C--POC V1


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
    * **`Enums/`**: Contém as enumerações usadas pelas entidades e DTOs.
* **`Profiles/`**: **[NOVO]** Contém os perfis de mapeamento do AutoMapper.
* **`SwaggerExamples/`**: **[NOVO]** Contém as classes que fornecem exemplos de payloads para o Swagger.
* **`Data/`**: Contém a classe `AppDbContext` do Entity Framework Core.
* **`Exceptions/`**: Contém as classes de exceção personalizadas.
* **`Middleware/`**: Contém middlewares customizados, como o `GlobalExceptionHandlerMiddleware`.
* **`Migrations/`**: Contém os arquivos de migration gerados pelo EF Core.


## Justificativa da Arquitetura

A arquitetura do projeto foi pensada para seguir o padrão de **Clean Architecture**, promovendo uma clara separação de responsabilidades.

* **`Controllers`**: Atuam como a camada mais externa, lidando apenas com a entrada e saída de dados (recebendo requisições e formatando respostas). Eles não contêm lógica de negócio.
* **`Services`**: Representam a camada de domínio e lógica de negócio. Todas as regras da aplicação são implementadas aqui, garantindo que o comportamento da API seja consistente e centralizado. Essa separação facilita a escrita de testes unitários sem a necessidade de instanciar a camada web.
* **`Data`**: É a camada de persistência, responsável pela comunicação com o banco de dados via Entity Framework Core. O isolamento desta camada permite a fácil substituição do banco de dados (por exemplo, de Oracle para SQL Server) sem afetar a lógica de negócio nos `Services`.

Essa estrutura, em conjunto com o uso de DTOs e injeção de dependência, garante que a API siga os princípios **SOLID**.


## Pré-requisitos

Para rodar esta aplicação localmente usando Docker, você precisará de:

1. **Docker Desktop**
2. **Docker Compose**
3. Acesso a uma instância do **Oracle Database** e as respectivas credenciais.

Para rodar sem usar o Docker, execute na pasta raiz do projeto:
1. `cd Csharp.Api`
2. `dotnet user-secrets init`
3. `dotnet user-secrets set "ConnectionStrings:OracleConnection" "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=oracle.fiap.com.br)(PORT=1521)))(CONNECT_DATA=(SID=ORCL)));User ID=SEU_ID_ORACLE;Password=SUA_SENHA_ORACLE"`
4. `dotnet run`

## Configuração do Ambiente Local e Inicialização

**1. Clonar o Repositório:**

```sh
git clone https://github.com/FLEET-MOTTU/C--POC
cd Csharp.Api
```

**2. Configure a Conexão com o Banco de Dados Oracle:**
* Na pasta raiz do projeto (onde está o `docker-compose.yml`), siga as instruções do arquivo `docker-compose.override.yml.example`.
 
 **3. Construa as Imagens e Inicie os Containers:**
* No seu terminal, na pasta raiz do projeto, execute o comando:

```sh
docker compose up --build
```

* **Aplicação de Migrations:** Ao iniciar pela primeira vez (ou se houver novas migrations commitadas no repositório que ainda não foram aplicadas ao seu banco), a API tentará aplicar automaticamente as migrations pendentes no banco de dados Oracle. Acompanhe os logs do container da API para verificar.
* Após a inicialização, a API estará acessível em `http://localhost:8080`.

**4. Acesse a Documentação Swagger:**
* `http://localhost:8080/swagger`


## Gerenciamento de Migrations do Banco de Dados (Entity Framework Core)

Caso hajam alterações no modelo de dados C# (Entidades ou `AppDbContext`) que precisam ser refletidas no esquema do banco de dados:

**1. Crie uma Nova Migration:**
* No seu terminal, na pasta raiz do projeto (onde está o `docker-compose.yml`), rode:

```sh       
docker-compose run --rm ef-tools sh -c "dotnet restore && dotnet ef migrations add NomeDescritivoParaSuaMudanca --verbose"
```

* Substitua `NomeDescritivoParaSuaMudanca` por um nome que descreva a alteração.
* Novos arquivos de migration serão gerados na pasta `Csharp.Api/Migrations/`.

**2. Aplicação da Nova Migration:**
* Da próxima vez que você ou outro desenvolvedor rodar `docker compose up --build`, a API aplicará essa nova migration automaticamente ao banco de dados (devido ao `dbContext.Database.Migrate();` no `Program.cs`). Acompanhe os logs do container para verificar se as migrations foram aplicadas com sucesso.
* Após a inicialização, a API estará acessível em `http://localhost:8080`.

**3. Acesse a Documentação Swagger:**
* `http://localhost:8080/swagger`


## Endpoints da API (Rotas Principais)

A documentação completa e interativa de todos os endpoints, incluindo schemas de request/response e exemplos, está disponível via Swagger UI em /swagger quando a API está rodando (ex: `http://localhost:8080/swagger`).

Abaixo estão os detalhes de todos os endpoints disponíveis na API.

### Endpoints de Motos (`/api/motos`)

| Método | Rota | Descrição | Status de Sucesso | Erros Comuns |
| :--- | :--- | :--- | :--- | :--- |
| **`POST`** | `/api/motos` | Cria uma nova moto no sistema. | `201 Created` | `400 Bad Request`, `409 Conflict` |
| **`GET`** | `/api/motos` | Lista todas as motos com suporte a paginação e filtros. | `200 OK` | `400 Bad Request` |
| **`GET`** | `/api/motos/{id}` | Obtém os detalhes de uma moto pelo seu ID (GUID). | `200 OK` | `404 Not Found` |
| **`GET`** | `/api/motos/por-placa/{placa}`| Obtém os detalhes de uma moto pela placa. | `200 OK` | `400 Bad Request`, `404 Not Found` |
| **`PUT`** | `/api/motos/{id}` | Atualiza uma moto existente. | `200 OK` | `400 Bad Request`, `404 Not Found`, `409 Conflict` |
| **`DELETE`**| `/api/motos/{id}` | Remove uma moto e sua tag associada. | `204 No Content` | `404 Not Found` |

### Endpoints de Beacons (`/api/beacons`)

| Método | Rota | Descrição | Status de Sucesso | Erros Comuns |
| :--- | :--- | :--- | :--- | :--- |
| **`POST`** | `/api/beacons` | Cria um novo beacon no sistema. | `201 Created` | `400 Bad Request`, `409 Conflict` |
| **`GET`** | `/api/beacons` | Lista todos os beacons com suporte a paginação. | `200 OK` | `400 Bad Request` |
| **`GET`** | `/api/beacons/{id}` | Obtém os detalhes de um beacon pelo seu ID (GUID). | `200 OK` | `404 Not Found` |
| **`GET`** | `/api/beacons/by-beaconid/{beaconId}`| Obtém os detalhes de um beacon pelo seu ID único. | `200 OK` | `404 Not Found` |
| **`PUT`** | `/api/beacons/{id}` | Atualiza um beacon existente. | `200 OK` | `400 Bad Request`, `404 Not Found` |
| **`DELETE`**| `/api/beacons/{id}` | Remove um beacon do sistema. | `204 No Content` | `404 Not Found` |

### Endpoints de IoT Events (`/api/iot-events`)

| Método | Rota | Descrição | Status de Sucesso | Erros Comuns |
| :--- | :--- | :--- | :--- | :--- |
| **`POST`** | `/api/iot-events/tag-interaction`| Recebe e processa um evento de interação de tag BLE. | `202 Accepted` | `400 Bad Request` |


## Implementação da Paginação

Para garantir a escalabilidade e o desempenho da API, especialmente ao lidar com grandes volumes de dados, as listagens de motos e beacons foram implementadas com paginação.

* **Parâmetros de Query:** Os endpoints `GET /api/motos` e `GET /api/beacons` aceitam os parâmetros de query opcionais `page` (número da página, padrão `1`) e `pageSize` (tamanho da página, padrão `10`).
* **Estrutura de Resposta:** O retorno da API não é mais uma lista direta de objetos, mas sim um objeto `PaginatedResponseDto` que encapsula os resultados. Isso fornece informações essenciais para o cliente, como o número total de itens, o número da página atual, o tamanho da página e se há páginas anteriores ou seguintes.
* **Exemplo:** Um request como `GET /api/motos?page=2&pageSize=5` retornaria os 5 primeiros itens da segunda página, junto com metadados de paginação.


## Testando a API

**CRUD de Motos, Beacons e simulação IoT**

A forma mais fácil de testar a API é importando a coleção de testes do Postman que já está preparada na riaz do projeto. Essa coleção inclui cenários de sucesso e falha para todos os endpoints de Motos e Beacons, além de um teste de simulação de evento IoT para que o teste não seja mais dependente da API de python.

* Copie todo o conteúdo co arquivo JSON 'Mottu_CSharp_API.postman_collection' localizado na raiz do projeto
* Abra o Postman.
* Cole o contúdo do JSON 'Mottu_CSharp_API.postman_collection' na aba "Raw text", clique em "Continue" e depois em "Import".
* Uma nova coleção chamada "Mottu C# API - Pátio" aparecerá no Postman.
* IMPORTANTE: Você precisará configurar a variável de coleção baseUrl.
    * Clique na coleção "Mottu C# API - Pátio".
    * Vá na aba "Variables".
    * Edite a variável baseUrl e no campo "CURRENT VALUE" coloque: http://localhost:8080 (ou a porta que a API C# estiver usando localmente).
* As requisições de criação (1. Create New Moto e 1. Create New Beacon) usam dados dinâmicos para evitar conflitos e preenchem as variáveis da coleção (motoId, beaconId, etc.).
* É crucial executar os requests em ordem para que os testes de GET, PUT e DELETE funcionem, pois eles dependem dos IDs criados nos primeiros requests.


**Simulação IoT (Obsoleto mas legal de saber, o script de testes na raiz já simula automaticamente no postman)**

Para testar a funcionalidade de rastreamento e atualização de status baseada em eventos de IoT (como detecção de tags por beacons), esta API C# espera receber eventos em seu endpoint `/api/iot-events/tag-interaction`.
Um **simulador Python/FastAPI dedicado** foi desenvolvido para gerar e enviar esses eventos. Para instruções detalhadas sobre como configurar, rodar e usar o simulador Python, por favor, consulte o README no seguinte repositório: https://github.com/FLEET-MOTTU/PY-SIM

Basicamente, você precisará:
1.  Rodar o simulador Python em um container Docker.
2.  Configurar o simulador para apontar para esta API C# (que estará rodando em `http://localhost:8080`).
3.  Usar o endpoint do simulador Python para enviar eventos de "interação de tag", que serão processados por esta API C#.

## Integrantes
AMANDA MESQUITA CIRINO DA SILVA - RM 559177
BEATRIZ FERREIRA CRUZ - RM555698
JOURNEY TIAGO LOPES – RM 556071
