using System.Diagnostics;

var commandLine = Environment.CommandLine;
var applicationPath = Environment.GetCommandLineArgs()[0];
var gitArguments = commandLine[(commandLine.IndexOf(applicationPath) + applicationPath.Length)..].Trim();

var gitProcess = new Process
{
    StartInfo = new ProcessStartInfo("realgit")
    {
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true,
        Arguments = gitArguments
    }
};

try
{
    gitProcess.Start();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error starting git process: {ex.Message}");
    return 1;
}

if (Console.IsInputRedirected)
{
    var input = Console.In.ReadToEnd();
    StreamWriter gitInputWriter = gitProcess.StandardInput;
    gitInputWriter.Write(input);
}

gitProcess.OutputDataReceived += (sender, e) =>
{
    if (e.Data != null)
    {
        Console.Out.WriteLine(e.Data);
    }
};
gitProcess.BeginOutputReadLine();

gitProcess.ErrorDataReceived += (sender, e) =>
{
    if (e.Data != null)
    {
        Console.Error.WriteLine(e.Data);
    }
};
gitProcess.BeginErrorReadLine();

gitProcess.WaitForExit();

return gitProcess.ExitCode;