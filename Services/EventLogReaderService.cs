using System.Diagnostics;
using Nest;

public class EventLogReaderService : IHostedService
{
    private readonly IElasticClient _elasticClient;
    private readonly IConfiguration _configuration;
    private Timer _timer;

    public EventLogReaderService(IElasticClient elasticClient, IConfiguration configuration)
    {
        _elasticClient = elasticClient;
        _configuration = configuration;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero,
            TimeSpan.FromSeconds(_configuration.GetValue<double>("LogReadInterval")));
        return Task.CompletedTask;
    }

    private void DoWork(object state)
    {
        // Fetch log names from configuration
        var logNames = _configuration.GetSection("EventLogs").Get<string[]>();

        foreach (var logName in logNames)
        {
            EventLog eventLog = new EventLog(logName);
            foreach (EventLogEntry entry in eventLog.Entries)
            {
                var logDocument = new
                {
                    Timestamp = entry.TimeGenerated,
                    Source = entry.Source,
                    Message = entry.Message,
                    LogName = logName
                };
                var indexResponse = _elasticClient.IndexDocument(logDocument);
                if (indexResponse.IsValid)
                {
                    Console.WriteLine("The document was successfully inserted");
                }
                else
                {
                    Console.WriteLine($"Failed to insert document: {indexResponse.OriginalException}");
                }
            }
        }
    }


    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }
}
