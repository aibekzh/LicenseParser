namespace license
{
    public class Configuration
    {
        public string DbHost { get; set; }
        public string DbPort { get; set; }
        public string DbUserName { get; set; }
        public string DbPassword { get; set; }
        public string DbName { get; set; }
        public string DbScheme { get; set; }
        public string ProxyApiKey { get; set; }
        public string BinTable { get; set; }
        public string BinColumn { get; set; }
        public int WorkersNumber { get; set; }
    }
}