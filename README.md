# CodeIn8-Mai-Image

Este projeto demonstra como usar o modelo MAI-Image-2.6 para gerar imagens a partir de um prompt textual, consumindo a API de geração de imagens da Azure AI Foundry.

A implementação está localizada na pasta `src/MaiImageConsoleDotnet` e usa um aplicativo de console em .NET para enviar uma requisição HTTP ao endpoint de geração de imagens, salvar a resposta bruta em JSON e converter a imagem base64 em um arquivo PNG.

## Objetivo

O objetivo desta solução é mostrar um fluxo simples e funcional para:

- enviar um prompt para o modelo de geração de imagem;
- receber a resposta da API em JSON;
- extrair o campo `b64_json`;
- salvar a imagem gerada em um arquivo `.png` local;
- manter um registro da resposta da API em um arquivo `.txt` para facilitar depuração.

## Estrutura do projeto

```text
CodeIn8-Mai-Image/
├── README.md
└── src/
    └── MaiImageConsoleDotnet/
        ├── Program.cs
        ├── MaiImageConsole.csproj
        ├── response_YYYYMMDD_HHMMSS.txt
        └── generated_image_YYYYMMDD_HHMMSS.png
```

## Implementação realizada

A lógica principal está em `src/MaiImageConsoleDotnet/Program.cs` e realiza os seguintes passos:

1. Carrega as variáveis de ambiente usando a biblioteca `DotNetEnv`.
2. Lê a variável `AZURE_API_KEY`.
3. Cria um cliente HTTP configurado para enviar requisições para a API de imagens.
4. Monta um payload com:
   - prompt
   - largura
   - altura
   - modelo `MAI-Image-2.6`
5. Envia a requisição para o endpoint:
   `https://nizzola-mkt-resource.services.ai.azure.com/mai/v1/images/generations`
6. Salva a resposta da API em um arquivo `response_*.txt`.
7. Verifica se a requisição foi bem-sucedida.
8. Extrai o campo `b64_json` da resposta JSON.
9. Converte a imagem em base64 para bytes e salva em `generated_image_*.png`.

## Pré-requisitos

- .NET SDK 10 ou superior
- Uma chave de API válida para a Azure AI Foundry / Azure AI services
- A variável de ambiente `AZURE_API_KEY` configurada no ambiente local

## Configuração da chave da API

Crie um arquivo `.env` na raiz do projeto ou no diretório do projeto que será executado, com o conteúdo:

```env
AZURE_API_KEY=sua_chave_aqui
```

> O código usa `Env.Load();`, então o arquivo `.env` é lido automaticamente antes da chamada à API.

## Como executar

Abra o terminal na pasta do projeto e execute:

```bash
dotnet run --project src/MaiImageConsoleDotnet/MaiImageConsole.csproj
```

O programa gera uma imagem com o prompt padrão definido no código:

```text
A photograph of earth from space with an big starship as the same model of a Star Wars Cruiser
```

Se a execução for bem-sucedida, serão criados arquivos como:

```text
response_20261003_084523.txt
generated_image_20261003_084523.png
```

## Arquivos gerados

### `response_*.txt`
Contém a resposta JSON completa da API, incluindo metadados e os dados da imagem gerada.

### `generated_image_*.png`
Arquivo final da imagem convertida a partir de `b64_json`.

## Observações importantes

- O código usa um endpoint específico da Azure AI e o modelo `MAI-Image-2.6`.
- A largura e a altura da imagem estão configuradas em `1024x1024`.
- O arquivo de imagem e a resposta JSON são salvos na pasta de execução do projeto.
- Se a variável `AZURE_API_KEY` não for configurada, a aplicação encerra com erro.

## Personalização

Para alterar a imagem gerada, edite o campo `prompt` dentro de `Program.cs`:

```csharp
var payload = new
{
    prompt = "Sua descrição da imagem aqui",
    width = 1024,
    height = 1024,
    model = "MAI-Image-2.6"
};
```

Você também pode trocar as dimensões da imagem, o modelo e outros parâmetros conforme a API suportar.

## Dica de uso

Se quiser testar diferentes prompts, altere a string do `prompt` no código e execute novamente o comando acima. A cada execução um novo arquivo de resposta e imagem será criado com timestamp, facilitando a comparação entre variações.

## Resumo

A solução implementada funciona como um pequeno cliente de geração de imagens usando Azure AI Foundry com MAI-Image-2.6. Em poucas linhas, ela envia um prompt, recebe a imagem em base64, salva a resposta da API e grava a imagem final em PNG no diretório do projeto.
