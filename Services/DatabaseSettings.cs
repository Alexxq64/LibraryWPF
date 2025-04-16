namespace LibraryWPF
{
    public class DatabaseSettings
    {
        private static DatabaseSettings _instance;

        public static DatabaseSettings Instance => _instance ??= new DatabaseSettings();

        public string SelectedDbName { get; set; } = "LibraryDB";

        public bool IsCreateNewDb { get; set; } = false;

        private DatabaseSettings() { }
    }
}
