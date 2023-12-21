namespace LogReaderApi.Controllers
{
    using LogReaderApi.Models;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;
    using Nest;
    using System;
    using System.Linq;

    [ApiController]
    [Route("[controller]")]
    [EnableCors]
    public class LogController : ControllerBase
    {
        private readonly IElasticClient _elasticClient;

        public LogController(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }
        public class LogDto
        {
            public DateTime Timestamp { get; set; }
            public string Source { get; set; }
            public string Message { get; set; }
            public string LogName { get; set; }
        }

        [HttpGet]
        public IActionResult GetAllLogs()
        {
            try
            {

                var searchResponse = _elasticClient.Search<LogDocument>(s => s
                    .Index("windows-logs")
                    .Query(q => q
                        .MatchAll()
                    ).Size(100)
                );

                if (!searchResponse.IsValid)
                {
                    return StatusCode(500, "Error occurred while retrieving logs");
                }

                var logDtos = searchResponse.Documents.Select(doc => new LogDto
                {
                    Timestamp = doc.Timestamp,
                    Source = doc.Source,
                    Message = doc.Message,
                    LogName = doc.LogName
                }).ToList();
                return Ok(logDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

    }

}
