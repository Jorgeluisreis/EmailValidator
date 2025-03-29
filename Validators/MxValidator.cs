using System.Diagnostics;

public class MxValidator
{
    public async Task<bool> Validate(string domain)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "nslookup",
                    Arguments = $"-type=MX {domain}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string output = await process.StandardOutput.ReadToEndAsync();
            process.WaitForExit();

            return output.Contains("mail exchanger");
        }
        catch
        {
            return false;
        }
    }
}