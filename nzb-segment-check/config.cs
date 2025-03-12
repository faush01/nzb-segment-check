
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace check_headers;

public class NntpServer
{
    public string Name { get; set; } = "";
    public string Host { get; set; } = "";
    public int Port { get; set; } = 563;
    public bool UseSsl { get; set; } = true;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public int NumberOfThreads { get; set; } = 10;
}

public class AppConfig
{
    public List<NntpServer> NntpServers { get; set; } = new List<NntpServer>();
    public bool Verbose { get; set; } = false;
}

public class Config
{
    public Config()
    {
        // Default constructor
    }

    public AppConfig LoadConfig(string configFilePath)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        using (var reader = new StreamReader(configFilePath))
        {
            var yaml = reader.ReadToEnd();
            return deserializer.Deserialize<AppConfig>(yaml);
        }
    }

    public void SaveConfig(string configFilePath, AppConfig config)
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        using (var writer = new StreamWriter(configFilePath))
        {
            var yaml = serializer.Serialize(config);
            writer.Write(yaml);
        }
    }

    public AppConfig DefaultConfig()
    {
        return new AppConfig
        {
            NntpServers = new List<NntpServer>
            {
                new NntpServer
                {
                    Name = "server_name",
                    Host = "server_host",
                    Port = 563,
                    UseSsl = true,
                    Username = "username",
                    Password = "password",
                    NumberOfThreads = 10
                }
            },
            Verbose = false
        };
    }

}
