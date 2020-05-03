namespace ProcessTracker.Model
{
    public class ProcessModel
    {
        public int Id { get; set; }
        public string ProcessName { get; set; }
        public string ActivatorFilePath { get; set; }
        public string DeActivatorFilePath { get; set; }
        public int ProcessId { get; set; }
        public long CpuUsage { get; set; }
        public long MemoryUsage { get; set; }
        public bool Activate { get; set; }

    }
}
