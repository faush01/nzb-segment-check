// https://github.com/keimpema/Usenet
// http://www.tcpipguide.com/free/t_NNTPStatusResponsesandResponseCodes-3.htm

namespace check_headers;

class Program
{
    static void Main(string[] args)
    {
        // extract command line args
        AppArgs appArgs = ArgsExtract.ParseArgs(args);
        Console.WriteLine($"nzb file    : ({appArgs.NzbFile})");
        Console.WriteLine($"config file : ({appArgs.ConfigFile})");

        // if no nzb file is provided, exit
        if(string.IsNullOrEmpty(appArgs.NzbFile))
        {
            Console.WriteLine("No nzb file provided, exiting.");
            return;
        }

        // nzb file does not exist
        if(File.Exists(appArgs.NzbFile) == false)
        {
            Console.WriteLine($"nzb file does not exist : {appArgs.NzbFile}");
            return;
        }

        // load the config file
        Config config = new Config();

        // if the config does not exist, create a default one and exit
        if(File.Exists(appArgs.ConfigFile) == false)
        {
            config.SaveConfig(appArgs.ConfigFile, config.DefaultConfig());
            Console.WriteLine($"Default config file created : {appArgs.ConfigFile}\nPlease edit it and run again.");
            return;
        }

        // load the config file
        AppConfig app_config = config.LoadConfig(appArgs.ConfigFile);
        if(app_config.NntpServers.Count == 0 || 
           app_config.NntpServers[0].Host == "server_host" ||
           app_config.NntpServers[0].Username == "username" ||
           app_config.NntpServers[0].Password == "password")
        {
            Console.WriteLine($"Config is still blank, Please edit it and run again.");
            return;
        }   

        // load the nzb file
        NzbTools nzbTools = new NzbTools();
        HashSet<string> message_ids = nzbTools.GetSegments(appArgs.NzbFile);
        Console.WriteLine("Segment Count : " + message_ids.Count);

        // if no message ids are found, exit
        if(message_ids.Count == 0)
        {
            Console.WriteLine("No message ids found, exiting.");
            return;
        }

        // split message_ids into work sets
        int number_of_threads = 10;
        int messages_per_thread = (message_ids.Count / number_of_threads) + 1;
        List<List<string>> message_ids_split = new List<List<string>>();
        List<string> new_worker_list = new List<string>();
        foreach (string message_id in message_ids)
        {
            if(new_worker_list.Count >= messages_per_thread)
            {
                message_ids_split.Add(new_worker_list);
                new_worker_list = new List<string>();
            }
            new_worker_list.Add(message_id);
        }
        if(new_worker_list.Count > 0)
        {
            message_ids_split.Add(new_worker_list);
        }

        // select the first NNTP server configured
        NntpServer nntpServer = app_config.NntpServers[0];

        // create all the workers and start them
        Dictionary<string, int> header_resp_all = new Dictionary<string, int>();
        List<NntpWorker> nntpWorkers = new List<NntpWorker>();
        int worker_id = 1;
        foreach(List<string> worker_id_list in message_ids_split)
        {
            NntpWorker nntpWorker = new NntpWorker(nntpServer.Host, nntpServer.Port, nntpServer.UseSsl, nntpServer.Username, nntpServer.Password, worker_id++);
            nntpWorkers.Add(nntpWorker);
            nntpWorker.SetMessageIds(worker_id_list);
            nntpWorker.RunWorker();
        }

        // wait for all the workers to finish
        while(true)
        {
            int finished_count = 0;
            int running_workers = 0;
            foreach(NntpWorker nntpWorker in nntpWorkers)
            {
                finished_count += nntpWorker.GetCurrentItem();
                if(nntpWorker.IsRunning())
                {
                    running_workers++;
                }
                else
                {
                    nntpWorker.Dispose();
                }
            }
            if(running_workers == 0)
            {
                break;
            }
            Console.WriteLine($"Finished {finished_count} of {message_ids.Count} - Running Workers : {running_workers}");
            Thread.Sleep(1000);
        }

        // merge all the results
        foreach(NntpWorker nntpWorker in nntpWorkers)
        {
            nntpWorker.GetHeaderResponse().ToList().ForEach(x => header_resp_all.Add(x.Key, x.Value));
        }

        // results
        Dictionary<int, int> header_resp_count = new Dictionary<int, int>();
        foreach(var key in header_resp_all.Keys)
        {
            if(header_resp_count.ContainsKey(header_resp_all[key]))
            {
                header_resp_count[header_resp_all[key]]++;
            }
            else
            {
                header_resp_count.Add(header_resp_all[key], 1);
            }
        }
        Console.WriteLine("Results:");
        foreach(var key in header_resp_count.Keys)
        {
            string message = NntpCodes.GetResponseMessage(key);
            Console.WriteLine($"   {header_resp_count[key]} \t {key}:{message}");
        }

        // save the results
        string result_file = appArgs.NzbFile + "." + nntpServer.Name.ToLower() + ".tsv";
        using (StreamWriter writer = new StreamWriter(result_file))
        {
            foreach(var key in header_resp_all.Keys)
            {
                writer.WriteLine($"{key}\t{header_resp_all[key]}");
            }
        }

    }
}
