namespace MySQLCrossServerTransferTool.Models
{
    public class ProgramOptions
    {
        public string FromConnectionString { get; set; }
        public string ToConnectionString { get; set; }
        public string ScriptFile { get; set; }
        public string Engine { get; set; }
    }
}
