using System.Net.Sockets;

public class SmtpValidator
{
    private static readonly List<int> smtpPorts = new List<int> {25, 587, 2525, 465};
    private const int ConnectionTimeout = 10000;

    public async Task<(bool isValid, string errorMessage, string usedDomain)> Validate(string email, string id, string[] ids, string[] subdomains)
    {
        try
        {
            var domain = email.Split('@')[1];
            var host = domain;

            var mxValidator = new MxValidator();
            bool isValidMx = await mxValidator.Validate(domain);
            if (!isValidMx)
            {
                return (false, $"{domain}: Não foi encontrado servidor de email.", host);
            }

            foreach (var port in smtpPorts)
            {
                var (isValid, errorMessage, usedDomain) = await ValidatePortAsync(host, port, email, subdomains);

                if (isValid)
                {
                    return (true, $"{email} validado com sucesso! O servidor SMTP aceitou o e-mail ({usedDomain}).", usedDomain);
                }
                else if (!isValid && errorMessage.Contains("não existe no servidor SMTP"))
                {
                    return (false, $"O {email} não foi encontrado para o site {host}", usedDomain);
                }
                else if (!isValid && errorMessage.Contains("requer autenticação para funcionar"))
                {
                    return (false, $"O servidor de Email do site {host} requer autenticação para funcionar.", usedDomain);
                }
                else
                {
                    continue;
                }
            }

            return (false, $" | Não foi possível se conectar ao servidor SMTP em nenhuma porta.", host);
        }
        catch (Exception ex)
        {
            return (false, $"Erro ao validar o e-mail {email}: {ex.Message}", email);
        }
    }

    private async Task<(bool isValid, string errorMessage, string usedDomain)> ValidatePortAsync(string host, int port, string email, string[]? subdomains = null)
    {
        var cancellationTokenSource = new CancellationTokenSource(ConnectionTimeout);
        var cancellationToken = cancellationTokenSource.Token;
        string usedDomain = host;

        try
        {
            using (var tcpClient = new TcpClient())
            {
                var connectTask = tcpClient.ConnectAsync(host, port);

                if (await Task.WhenAny(connectTask, Task.Delay(ConnectionTimeout, cancellationToken)) == connectTask)
                {
                    using (var networkStream = tcpClient.GetStream())
                    using (var reader = new StreamReader(networkStream))
                    using (var writer = new StreamWriter(networkStream))
                    {
                        writer.NewLine = "\r\n";
                        writer.AutoFlush = true;

                        var greeting = await reader.ReadLineAsync();
                        if (string.IsNullOrEmpty(greeting))
                        {
                            return (false, "Saudação do servidor SMTP não recebida.", usedDomain);
                        }

                        await writer.WriteLineAsync("HELO " + host);
                        var heloResponse = await reader.ReadLineAsync();

                        await writer.WriteLineAsync("MAIL FROM:<test@example.com>");
                        var mailFromResponse = await reader.ReadLineAsync();

                        await writer.WriteLineAsync($"RCPT TO:<{email}>");
                        var response = await reader.ReadLineAsync();

                        if (response != null && response.StartsWith("250"))
                        {
                            return (true, string.Empty, usedDomain);
                        }
                        else if (response != null && response.StartsWith("550"))
                        {
                            return (false, $"{email} não existe no servidor SMTP. Resposta do RCPT TO: {response}", usedDomain);
                        }
                        else if (response != null && response.StartsWith("503"))
                        {
                            return (false, $"O servidor de Email do site {host} requer autenticação para funcionar.", usedDomain);
                        }
                        else
                        {
                            return (false, $"{email} não foi aceito pelo servidor SMTP. Resposta do RCPT TO: {response}", usedDomain);
                        }
                    }
                }
                else
                {
                    if (subdomains != null)
                    {
                        foreach (var subdomain in subdomains)
                        {
                            string newHost = $"{subdomain}.{host}";
                            try
                            {
                                using (var subTcpClient = new TcpClient())
                                {
                                    var subConnectTask = subTcpClient.ConnectAsync(newHost, port);

                                    if (await Task.WhenAny(subConnectTask, Task.Delay(ConnectionTimeout, cancellationToken)) == subConnectTask)
                                    {
                                        usedDomain = newHost;
                                        Console.WriteLine($"Conexão bem-sucedida com o subdomínio: {newHost}");

                                        using (var networkStream = subTcpClient.GetStream())
                                        using (var reader = new StreamReader(networkStream))
                                        using (var writer = new StreamWriter(networkStream))
                                        {
                                            writer.NewLine = "\r\n";
                                            writer.AutoFlush = true;

                                            var greeting = await reader.ReadLineAsync();
                                            if (string.IsNullOrEmpty(greeting))
                                            {
                                                Console.WriteLine("Saudação do servidor SMTP não recebida.");
                                                return (false, "Saudação do servidor SMTP não recebida.", usedDomain);
                                            }
                                            else
                                            {
                                                Console.WriteLine($"Saudação recebida: {greeting}");
                                            }

                                            await writer.WriteLineAsync("HELO " + newHost);
                                            var heloResponse = await reader.ReadLineAsync();
                                            Console.WriteLine($"Resposta HELO: {heloResponse}");

                                            await writer.WriteLineAsync("MAIL FROM:<test@example.com>");
                                            var mailFromResponse = await reader.ReadLineAsync();
                                            Console.WriteLine($"Resposta MAIL FROM: {mailFromResponse}");

                                            await writer.WriteLineAsync($"RCPT TO:<{email}>");
                                            var response = await reader.ReadLineAsync();
                                            Console.WriteLine($"Resposta RCPT TO: {response}");

                                            if (response != null && response.StartsWith("250"))
                                            {
                                                Console.WriteLine($"O e-mail {email} é válido no servidor SMTP.");
                                                return (true, string.Empty, usedDomain);
                                            }
                                            else if (response != null && response.StartsWith("550"))
                                            {
                                                return (false, $"{email} não existe no servidor SMTP. Resposta do RCPT TO: {response}", usedDomain);
                                            }
                                            else
                                            {
                                                return (false, $"{email} não foi aceito pelo servidor SMTP. Resposta do RCPT TO: {response}", usedDomain);
                                            }
                                        }
                                    }
                                }
                            }
                            catch (SocketException sockEx)
                            {
                                Console.WriteLine($"Erro de rede ao tentar conectar com o subdomínio {newHost}: {sockEx.Message}, Código de erro: {sockEx.SocketErrorCode}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Erro inesperado ao tentar conectar com o subdomínio {newHost}: {ex.Message}");
                            }
                        }
                    }

                    return (false, $"Não foi possível se conectar à porta {port} com o domínio ou subdomínio.", usedDomain);
                }
            }
        }
        catch (SocketException ex)
        {
            return (false, $"Erro ao tentar se conectar com a porta {port}: {ex.Message}", usedDomain);
        }
        catch (InvalidOperationException ex)
        {
            return (false, $"Erro ao tentar se conectar com a porta {port}: {ex.Message}", usedDomain);
        }
        catch (Exception ex)
        {
            return (false, $"Erro inesperado ao tentar se conectar com a porta {port}: {ex.Message}", usedDomain);
        }
    }
}
