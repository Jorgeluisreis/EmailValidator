using System.Diagnostics;

public class Program
{
    private static readonly bool runningInIde = Debugger.IsAttached;

    private static readonly string basePath = runningInIde
        ? Path.Combine(Directory.GetCurrentDirectory())
        : AppContext.BaseDirectory;

    private static readonly string configDir = Path.Combine(basePath, "Config");

    private static readonly string idFile = Path.Combine(configDir, "id.txt");
    private static readonly string sitesFile = Path.Combine(configDir, "sites.txt");
    private static readonly string subdominiosFile = Path.Combine(configDir, "subdominio.txt");

    private static readonly string validFile = Path.Combine(basePath, "validos.txt");
    private static readonly string invalidFile = Path.Combine(basePath, "invalidos.txt");
    static async Task Main()
    {

        Console.WriteLine(@"
  $$$$$$$$\                         $$\ $$\ $$\    $$\          $$\ $$\       $$\            $$\                         
  $$  _____|                        \__|$$ |$$ |   $$ |         $$ |\__|      $$ |           $$ |                        
  $$ |      $$$$$$\$$$$\   $$$$$$\  $$\ $$ |$$ |   $$ |$$$$$$\  $$ |$$\  $$$$$$$ | $$$$$$\ $$$$$$\    $$$$$$\   $$$$$$\  
  $$$$$\    $$  _$$  _$$\  \____$$\ $$ |$$ |\$$\  $$  |\____$$\ $$ |$$ |$$  __$$ | \____$$\\_$$  _|  $$  __$$\ $$  __$$\ 
  $$  __|   $$ / $$ / $$ | $$$$$$$ |$$ |$$ | \$$\$$  / $$$$$$$ |$$ |$$ |$$ /  $$ | $$$$$$$ | $$ |    $$ /  $$ |$$ |  \__|
  $$ |      $$ | $$ | $$ |$$  __$$ |$$ |$$ |  \$$$  / $$  __$$ |$$ |$$ |$$ |  $$ |$$  __$$ | $$ |$$\ $$ |  $$ |$$ |      
  $$$$$$$$\ $$ | $$ | $$ |\$$$$$$$ |$$ |$$ |   \$  /  \$$$$$$$ |$$ |$$ |\$$$$$$$ |\$$$$$$$ | \$$$$  |\$$$$$$  |$$ |      
  \________|\__| \__| \__| \_______|\__|\__|    \_/    \_______|\__|\__| \_______| \_______|  \____/  \______/ \__|      
                                                                                                                       
                                                                                                                       
                                                           by Jorgeluisreis
                                                                v1.0.0
                                                   www.linkedin.com/in/ojorge-luis
---------------------------------------------------------------------------------------------------------------
"); ;

        if (!File.Exists(idFile) || !File.Exists(sitesFile))
        {
            Console.WriteLine("Arquivos id.txt ou sites.txt não encontrados.");
            Console.ReadLine();
            return;
        }
        else if (new FileInfo(idFile).Length == 0 || new FileInfo(sitesFile).Length == 0)
        {
            Console.WriteLine("Os arquivos id.txt e sites.txt não podem estar vazios.");
            Console.ReadLine();
            return;
        }

        var ids = File.ReadAllLines(idFile);
        var sites = File.ReadAllLines(sitesFile);

        var subdominios = new List<string>();
        if (File.Exists(subdominiosFile) && new FileInfo(subdominiosFile).Length > 0)
        {
            subdominios = File.ReadAllLines(subdominiosFile)
                              .Select(line => line.Trim())
                              .Where(line => !string.IsNullOrEmpty(line))
                              .ToList();
        }

        var validEmails = new List<string>();
        var invalidEmails = new List<string>();

        var smtpValidator = new SmtpValidator();
        var invalidDomains = new HashSet<string>();

        for (int i = 0; i < sites.Length; i++)
        {
            Console.WriteLine($"Verificando {i + 1}/{sites.Length}...");

            var site = sites[i].Trim();
            if (invalidDomains.Contains(site))
            {
                Console.WriteLine($"Dominio {site} já invalidado, pulando...");
                continue;
            }

            foreach (var id in ids)
            {
                string email = $"{id}@{site}";

                var (isValid, errorMessage, triedWithSubdomain) = await smtpValidator.Validate(email, id, ids, subdominios.ToArray());

                if (!isValid && subdominios.Any() && string.IsNullOrEmpty(triedWithSubdomain))
                {
                    foreach (var subdomain in subdominios)
                    {
                        email = $"{id}@{subdomain}.{site}";
                        Console.WriteLine($"Tentando com subdomínio: {email}");

                        var (retryValid, retryErrorMessage, _) = await smtpValidator.Validate(email, id, ids, subdominios.ToArray());
                        if (retryValid)
                        {
                            validEmails.Add(email);
                            Console.WriteLine($"Validado com subdomínio: {email}");
                            break;
                        }
                        else
                        {
                            invalidEmails.Add($"{email}: {retryErrorMessage}");
                            Console.WriteLine($"Invalidado com subdomínio: {email} - {retryErrorMessage}");
                        }
                    }
                }
                else if (isValid)
                {
                    validEmails.Add(email);
                    Console.WriteLine($"Validado: {email}");
                }
                else
                {
                    invalidEmails.Add($"{email}: {errorMessage}");
                    if (!isValid && errorMessage.Contains("Não foi possível se conectar ao servidor SMTP") || errorMessage.Contains("requer autenticação para funcionar"))
                    {
                        invalidDomains.Add(site);
                        Console.WriteLine($"Invalidado: Dominio {site} foi invalidado. Pulando...");
                        break;
                    } else if (!isValid && errorMessage.Contains("requer autenticação para funcionar"))
                    {
                        invalidDomains.Add(site);
                        Console.WriteLine($"Invaldado: Dominio {site} foi invalidado por falta de conexão com o servidor de Email. Pulando...");
                        break;
                    }
                }
            }
        }

        File.WriteAllLines(validFile, validEmails);
        File.WriteAllLines(invalidFile, invalidEmails);

        Console.WriteLine("\n------------------------------------------------------------------------");
        Console.WriteLine("-                              Resultado                               -");
        Console.WriteLine("------------------------------------------------------------------------");
        Console.WriteLine("- Processo concluído! Resultados salvos em validos.txt e invalidos.txt.-");
        Console.WriteLine("------------------------------------------------------------------------");
        Console.WriteLine($"Total verificado: {validEmails.Count + invalidEmails.Count}");
        Console.WriteLine($"Total de válidos: {validEmails.Count}");
        Console.WriteLine($"Total de inválidos: {invalidEmails.Count}");

    }
}