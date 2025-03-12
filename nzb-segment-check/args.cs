
namespace check_headers;

public class AppArgs
{
    public string NzbFile { get; set; } = string.Empty;
    public string ConfigFile { get; set; } = "config.yaml";
}

public class ArgsExtract
{
    public static AppArgs ParseArgs(string[] args)
    {
        AppArgs appArgs = new AppArgs();

        if (args.Length == 0)
        {
            Console.WriteLine("No arguments provided.");
            return appArgs;
        }

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith("-nzb") && i + 1 < args.Length)
            {
                appArgs.NzbFile = args[i + 1];
                i++;
            }
            else if (args[i].StartsWith("-config") && i + 1 < args.Length)
            {
                appArgs.ConfigFile = args[i + 1];
                i++;
            }
            else
            {
                Console.WriteLine($"Unknown argument or missing value: {args[i]}");
            }
        }

        return appArgs;
    }
}