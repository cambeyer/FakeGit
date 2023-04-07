using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace GitWrapper
{
    public class Program
    {
        public static readonly HashSet<string> RelevantCommands = new()
        {
            "checkout",
            "reset"
        };

        public static int Main()
        {
            var commandLine = Environment.CommandLine;
            var applicationPath = Environment.GetCommandLineArgs()[0];
            var gitArguments = commandLine[(commandLine.IndexOf(applicationPath) + applicationPath.Length)..].Trim();

            var gitProcess = new Process
            {
                StartInfo = new ProcessStartInfo("git")
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

            ReplaceTextInRepository(gitArguments);

            return gitProcess.ExitCode;
        }

        public static void ReplaceTextInRepository(string gitArguments)
        {
            if (!RelevantCommands.Any(command =>
                gitArguments.StartsWith($"{command} ", StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            foreach (var file in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.yaml", SearchOption.AllDirectories))
            {
                ReplaceTextInFile(file, @"\$\{artifact\.metadata\.image}", "<+artifact.image>");
                ReplaceTextInFile(file, @"\$\{env\.name}", "<+env.name>");
            }
        }

        private static void ReplaceTextInFile(string filePath, string pattern, string replacement)
        {
            string fileContent = File.ReadAllText(filePath);
            string newFileContent = Regex.Replace(fileContent, pattern, replacement);

            if (!string.Equals(fileContent, newFileContent, StringComparison.Ordinal))
            {
                File.WriteAllText(filePath, newFileContent);
            }
        }
    }
}