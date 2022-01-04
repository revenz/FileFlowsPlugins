using FileFlows.Plugin.Attributes;
using NPoco;
using System.ComponentModel.DataAnnotations;
using System.Data.SQLite;

namespace CollectionNodes
{
    public class DataCollection : Node
    {
        public override int Inputs => 1;
        public override int Outputs => 3;
        public override FlowElementType Type => FlowElementType.Logic;
        public override string Icon => "fas fa-database";
        private Dictionary<string, object> _Variables;
        public override Dictionary<string, object> Variables => _Variables;
        public DataCollection()
        {
            _Variables = new Dictionary<string, object>()
            {
                { "dc.Value", "ValueFromDataCollection" }
            };
        }


        [Required]
        [Folder(1)]
        public string DatabaseFolder { get; set; }

        [Required]
        [Text(2)]
        public string DatabaseFile { get; set; }

        [TextVariable(3)]
        public string Key { get; set; }

        [Required]
        [TextVariable(4)]
        public string Value { get; set; }

        [Boolean(5)]
        public bool UpdateIfDifferent { get; set; }

        public override int Execute(NodeParameters args)
        {
            string key = args.ReplaceVariables(Key ?? string.Empty, true);
            string value = args.ReplaceVariables(Value ?? string.Empty, true);
            if (string.IsNullOrEmpty(key))
            {
                args.Logger?.ELog("Key not set using filename");
                key = args.FileName;
            }
            if (string.IsNullOrEmpty(value))
            {
                args.Logger?.ELog("Value not set");
                return -1;
            }

            if (args.Variables.ContainsKey(value))
            {
                value = args.Variables[value]?.ToString() ?? string.Empty;
            }

            string dbFile = Path.Combine(
                    args.ReplaceVariables(this.DatabaseFolder, stripMissing: true),
                    args.ReplaceVariables(this.DatabaseFile, stripMissing: true)
            );
            CreateDatabaseIfNotExists(dbFile);


            var db = new Database($"Data Source={dbFile};Version=3;", null, SQLiteFactory.Instance);

            string result = db.ExecuteScalar<string>($"select {nameof(DbObject.Value)} from {nameof(DbObject)} where {nameof(DbObject.Key)} = @0", key);
            if(result == null)
            {
                args.Logger?.ILog("Key not in database");
                db.Execute($"insert into {nameof(DbObject)} values (@0, @1)", key, value);
                args.UpdateVariables(new Dictionary<string, object>
                {
                    { "dc.Value", value },
                });
                return 1;
            }
            if (result == value)
            {
                args.Logger?.ILog("Value matches what was already in database");
                args.UpdateVariables(new Dictionary<string, object>
                {
                    { "dc.Value", value },
                });
                return 2;
            }
            args.UpdateVariables(new Dictionary<string, object>
            {
                { "dc.Value", result },
            });
            args.Logger?.ILog("Key value did not match what was in database");
            if (UpdateIfDifferent)
            {
                db.Execute($"update {nameof(DbObject)} set {nameof(DbObject.Value)} = @1 where {nameof(DbObject.Key)} = @0", key, value);
            }
            return 3;
        }

        private void CreateDatabaseIfNotExists(string dbFile)
        {
            if (System.IO.File.Exists(dbFile) == false)
                SQLiteConnection.CreateFile(dbFile);
            else
            {
                // create backup 
                File.Copy(dbFile, dbFile + ".backup", true);
            }

            using (var con = new SQLiteConnection($"Data Source={dbFile};Version=3;"))
            {
                con.Open();
                using (var cmd = new SQLiteCommand($"SELECT name FROM sqlite_master WHERE type='table' AND name='{nameof(DbObject)}'", con))
                {
                    if (cmd.ExecuteScalar() != null)
                        return;// tables exist, all good                    
                }
                using (var cmd = new SQLiteCommand(CreateDbScript, con))
                {
                    cmd.ExecuteNonQuery();
                }
                con.Close();
            }
        }


        private const string CreateDbScript = @$"CREATE TABLE {nameof(DbObject)}(
                Key             VARCHAR(1024)      NOT NULL          PRIMARY KEY,
                Value           TEXT               NOT NULL
            );";

        private class DbObject
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }
    }
}
