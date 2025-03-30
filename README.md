<p align="center">
  <img src="https://i.imgur.com/9Kaat1q.jpeg" alt="EmailValidator Logo">
</p>
<p align="center"> <a href="https://github.com/Jorgeluisreis/EmailValidator"> <img alt="GitHub last commit" src="https://img.shields.io/github/last-commit/Jorgeluisreis/EmailValidator"> </a> <a href="https://github.com/Jorgeluisreis/EmailValidator"> <img alt="GitHub repo size" src="https://img.shields.io/github/repo-size/Jorgeluisreis/EmailValidator"> </a> <a href="https://github.com/Jorgeluisreis/EmailValidator"> <img alt="License" src="https://img.shields.io/github/license/Jorgeluisreis/EmailValidator"> </a> </p>

# EmailValidator

🚀 EmailValidator é uma aplicação .NET para estudos, desenvolvida para validar endereços de e-mail por meio de múltiplos métodos, incluindo verificação de sintaxe, consulta **MX** e validação via **SMTP**.

## 📚 Conceito

O EmailValidator foi criado para testar uma forma de validação de e-mails em sistemas que necessitam garantir a autenticidade dos endereços cadastrados.

### OBS:

Esta aplicação usa uma forma *não convencional* para validar estes emails, podem haver **falsos positivos**.

---

## 🛠️ Tecnologias Utilizadas

- ![.NET](https://img.shields.io/badge/.NET-8.0-blue) - Plataforma de desenvolvimento
- ![C#](https://img.shields.io/badge/C%23-11.0-blue) - Linguagem de programação utilizada
- ![SMTP Validation](https://img.shields.io/badge/SMTP_Validation-Yes-green) - Validação via SMTP
- ![TXT](https://img.shields.io/badge/TXT-Supported-green) - Suporte a configurações via TXT.

## 🌐 Funcionalidades

* **Validação de E-mails** - Verifica sintaxe, domínio e acessibilidade via **SMTP**.
<p align="center">
  <img src="https://i.imgur.com/WwHoI4H.jpeg" alt="Validação de E-mails">
</p>

* **Configuração Personalizável** - Permite definir regras de validação via **TXT**.
<p align="center">
  <img src="https://i.imgur.com/OV3OA6c.jpeg" alt="Configuração Personalizável">
</p>

* **Relatórios de Validação** - Gera logs detalhados sobre os e-mails processados.
<p align="center">
  <img src="https://i.imgur.com/GWKpy2T.jpeg" alt="Relatórios de Validação">
</p>

<p align="center">
  <img src="https://i.imgur.com/AmL5U7X.jpeg" alt="Relatórios dos Invalidados">
</p>

## 📥 Requisitos Mínimos

* **.NET 8.0** - Plataforma necessária para execução.
* **Conexão com a Internet** - Necessária para validações via **SMTP**.

## ⚙️ Configuração

A aplicação EmailValidator utiliza três arquivos principais para configuração, todos localizados na pasta **/Config**.

### id.txt

**Lista de usuários** do e-mail que serão validados.

Exemplo de conteúdo:
```bash
rh
carreiras
vagas
```

### sites.txt

**Lista de domínios** que deseja para validação. Qualquer e-mail pertencente a um domínio fora dessa lista será considerado inválido.

Exemplo de conteúdo:
```bash
gmail.com
yahoo.com
outlook.com
```

### subdominio.txt (Opcional)

Contém **subdomínios** permitidos para validação. Se você o deixar vazio,não será usado o fluxo de validação com subdominios.

Exemplo de conteúdo:
```bash
mail
webmail
email
```

## 🛠️ Instalação

### Windows

1. Faça o download da aplicação clicando [aqui](https://github.com/Jorgeluisreis/EmailValidator/releases).
2. Descompacte o arquivo baixado.
3. Execute o programa com:

```sh
EmailValidator.exe
```

### Linux

1. Faça o download da aplicação clicando [aqui](https://github.com/Jorgeluisreis/EmailValidator/releases).
2. Descompacte o arquivo baixado.
3. Torne o arquivo executável e execute a aplicação:

```sh
chmod +x EmailValidator
./EmailValidator
```
