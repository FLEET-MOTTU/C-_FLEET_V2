# C--POC V1


## Descrição do Projeto

Esta API RESTful, trata-se de um Proof of Concept desenvolvida em C# com ASP.NET Core 8 e Entity Framework Core, é um componente do sistema de gerenciamento de pátios da empresa Mottu. Seu objetivo principal é gerenciar o cadastro e o estado das motocicletas, além de processar eventos de IoT simulados para rastreamento dentro dos pátios.

A API implementa um CRUD completo para a entidade `Moto` (incluindo sua `TagBle`(Tag Bluetooth Low Energy) associada) e está preparada para receber interações de tags BLE via um endpoint dedicado, que será alimentado por um simulador Python de eventos IoT. A persistência dos dados é feita em um banco de dados Oracle.

**Principais Funcionalidades**
* CRUD completo para Motos e suas Tags BLE associadas.
* Validação de dados de entrada e regras de negócio.
* Geração e aplicação de migrations do banco de dados via EF Core.
* Documentação da API com Swagger.
* Endpoint para recebimento de eventos simulados de IoT (interação de tag).

**Tecnologias Utilizadas:**
* C# e ASP.NET Core 8
* Entity Framework Core 8
* Oracle Database (conectado via EF Core)
* Docker e Docker Compose para ambiente de desenvolvimento e deploy
* Swagger (Swashbuckle) para documentação da API
* Princípios SOLID e Clean Architecture


## Estrutura do Projeto (Visão Geral)

* **`Controllers/`**: Contém os API Controllers que lidam com as requisições HTTP e respostas.
* **`Services/`**: Contém a lógica de negócio e orquestração das operações.
* **`DTOs/`**: Define os objetos usados para transferir dados entre o cliente e a API, e para validação.
    * **`ValidationAttributes/`**: Contém atributos de validação customizados.
* **`Entities/`**: Contém as classes que representam as tabelas do banco de dados (modelos de domínio para EF Core).
    * **`Enums/`**: Contém as enumerações usadas pelas entidades e DTOs.
* **`Data/`**: Contém a classe `AppDbContext` do Entity Framework Core.
* **`Exceptions/`**: Contém as classes de exceção personalizadas.
* **`Middleware/`**: Contém middlewares customizados, como o `GlobalExceptionHandlerMiddleware`.
* **`Migrations/`**: Contém os arquivos de migration gerados pelo EF Core.


## Pré-requisitos

Para rodar esta aplicação localmente usando Docker, você precisará de:

1.  **Docker Desktop**
2.  **Docker Compose**
3.  Acesso a uma instância do **Oracle Database** e as respectivas credenciais (usuário, senha, data source string).


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


## Testando a API

**CRUD de motos**

Os endpoits relacionados ao CRUD de motos podem ser testados através do Swagger ou via ferramentas de teste de API, como o Postman (seguem instruções para teste via Postman):
* Copie todo o conteúdo co arquivo JSON 'Mottu_CSharp_API.postman_collection' localizado na raiz do projeto
* Abra o Postman.
* Cole o contúdo do JSON 'Mottu_CSharp_API.postman_collection' na aba "Raw text", clique em "Continue" e depois em "Import".
* Uma nova coleção chamada "Mottu C# API - Pátio" aparecerá no Postman.
* IMPORTANTE: Você precisará configurar a variável de coleção baseUrl.
    * Clique na coleção "Mottu C# API - Pátio".
    * Vá na aba "Variables".
    * Edite a variável baseUrl e no campo "CURRENT VALUE" coloque: http://localhost:8080 (ou a porta que a API C# estiver usando localmente).


**Simulação IoT**

Para testar a funcionalidade de rastreamento e atualização de status baseada em eventos de IoT (como detecção de tags por beacons), esta API C# espera receber eventos em seu endpoint `/api/iot-events/tag-interaction`.
Um **simulador Python/FastAPI dedicado** foi desenvolvido para gerar e enviar esses eventos. Para instruções detalhadas sobre como configurar, rodar e usar o simulador Python, por favor, consulte o README no seguinte repositório: https://github.com/FLEET-MOTTU/PY-SIM

Basicamente, você precisará:
1.  Rodar o simulador Python em um container Docker.
2.  Configurar o simulador para apontar para esta API C# (que estará rodando em `http://localhost:8080`).
3.  Usar o endpoint do simulador Python para enviar eventos de "interação de tag", que serão processados por esta API C#.