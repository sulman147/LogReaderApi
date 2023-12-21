namespace LogReaderApi.Models
{
    public class LogDocument
    {
        public DateTime Timestamp { get; set; }
        public string Source { get; set; }
        public string Message { get; set; }
        public string LogName { get; set; }
    }

}
