using Usenet.Nntp;
using Usenet.Nntp.Models;
using Usenet.Nntp.Responses;

namespace check_headers;

public class NntpWorker : IDisposable
{
    private NntpClient? client = null;
    private bool worker_running = false;
    private Dictionary<string, int> header_resp = new Dictionary<string, int>();
    private List<string> message_ids = new List<string>();
    private int current_item = 0;
    private string worker_id = "---";

    public NntpWorker(string host, int port, bool useSsl, string username, string password, int id)
    {
        this.worker_id = id.ToString("D3");
        client = new NntpClient(new NntpConnection());
        var connected = client.ConnectAsync(host, port, useSsl).Result;
        if(!connected)
        {
            client = null;
            throw new Exception($"{worker_id} - Failed to connect to {host}:{port}");
        }
        bool authenticated = client.Authenticate(username, password);
        if(!authenticated)
        {
            client.Quit();
            client = null;
            throw new Exception($"{worker_id} - Failed to authenticate with {username}");
        }
        Console.WriteLine($"{worker_id} - NNTP Client Connected and Authenticated");
    }
    public void SetMessageIds(List<string> message_ids)
    {
        this.message_ids = message_ids;
    }

    public Dictionary<string, int> GetHeaderResponse()
    {
        return this.header_resp;
    }

    public bool IsRunning()
    {
        return this.worker_running;
    }

    public int GetCurrentItem()
    {
        return this.current_item;
    }

    private int GetHeader(string message_id)
    {
        if (client == null)
        {
            throw new Exception($"{worker_id} - NNTP Client is not initialized");
        }
        NntpMessageId messageId = new NntpMessageId(message_id);
        NntpArticleResponse articleByMessageId = client.Head(messageId);

        /*
        Console.WriteLine("Code : " + articleByMessageId.Code);
        if (articleByMessageId.Code == 221)
        {
            foreach(var key in articleByMessageId.Article.Headers.Keys)
            {
                Console.WriteLine($"\t- {key} = {string.Join("|", articleByMessageId.Article.Headers[key])}");
            }
        }
        */

        return articleByMessageId.Code;        
    }

    public void RunWorker()
    {
        if (client == null)
        {
            throw new Exception($"{worker_id} - NNTP Client is not initialized");
        }

        Thread workerThread = new Thread(() =>
        {
            try
            {
                CheckHeaders();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{worker_id} - Error in worker thread: {ex.Message}");
            }
            this.worker_running = false;
        });
        workerThread.Start();
        this.worker_running = true;
        Console.WriteLine($"{worker_id} - Worker thread started");
    }

    private void CheckHeaders()
    {
        if (client == null)
        {
            throw new Exception($"{worker_id} - NNTP Client is not initialized");
        }

        string thread_id = Thread.CurrentThread.ManagedThreadId.ToString();
        int count = 1;
        foreach (string message_id in this.message_ids)
        {
            int code = GetHeader(message_id);
            header_resp[message_id] = code;
            current_item = count;
            //Console.WriteLine($"{worker_id} - {count} of {this.message_ids.Count} - Message ID: {message_id} - Code: {code}");
            count++;
        }
    }

    public void Dispose()
    {
        if (client != null)
        {
            client.Quit();
            client = null;
            Console.WriteLine($"{worker_id} - NNTP Client Closed");
        }
    }
}