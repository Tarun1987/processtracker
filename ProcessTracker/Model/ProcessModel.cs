namespace ProcessTracker.Model
{
    public class ProcessModel
    {
        public string ProcessName { get; set; }
        public string ActivatorFilePath { get; set; }
        public string DeActivatorFilePath { get; set; }
        public bool Activate { get; set; }

    }
}
