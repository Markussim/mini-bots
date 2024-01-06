using Watson.ORM.Core;
using DatabaseWrapper.Core;
using Watson.ORM.Sqlite;
using ExpressionTree;

namespace MiniBots
{
    // Apply attributes to your class
    [Table("miniBots")]
    public class MiniBot
    {
        [Column("id", true, DataTypes.Int, false)]
        public int Id { get; set; }

        [Column("name", false, DataTypes.Nvarchar, 64, false)]
        public string Name { get; set; }

        [Column("code", false, DataTypes.Nvarchar, 64, false)]
        public string Code { get; set; }

        [Column("storage", false, DataTypes.Nvarchar, 64, false)]
        public string Storage { get; set; }

        // Parameter-less constructor is required
        public MiniBot()
        {
        }
    }

    public class DatabaseManager
    {
        private WatsonORM Orm { get; }

        public DatabaseManager()
        {
            string databasePath = "./Database/mini-bots.db";
            DatabaseSettings settings = new DatabaseSettings(databasePath);
            Orm = new WatsonORM(settings);
            Orm.InitializeDatabase();
            Orm.InitializeTable(typeof(MiniBot));
        }

        public MiniBot? GetMiniBotByName(string name)
        {
            Expr selectFilter = new Expr(
                Orm.GetColumnName<MiniBot>(nameof(MiniBot.Name)),
                OperatorEnum.Equals,
                name);
            // Select all records
            List<MiniBot> miniBots = Orm.SelectMany<MiniBot>(null, null, selectFilter);

            if (miniBots.Count > 1)
            {
                // This should never happen
                Console.WriteLine("More than 1 bot with name: " + name);
            }
            if (miniBots.Count <= 0)
            {
                return null;
            }

            return miniBots[0];
        }

        public List<MiniBot> GetMiniBots()
        {
            // Select all records
            List<MiniBot> miniBots = Orm.SelectMany<MiniBot>();

            return miniBots;
        }

        public void CreateMiniBotIfNotExists(string name, string code)
        {
            MiniBot? miniBot = GetMiniBotByName(name);
            if (miniBot != null)
            {
                miniBot.Code = code;
                miniBot.Storage = ""; // Clear storage on update
                Orm.Update(miniBot);
                return;
            }

            MiniBot newMiniBot = new MiniBot
            {
                Name = name,
                Code = code,
                Storage = ""
            };
            Orm.Insert(newMiniBot);
        }

        public string GetStorage(int id)
        {
            MiniBot? miniBot = Orm.SelectByPrimaryKey<MiniBot>(id);
            if (miniBot == null)
            {
                return "";
            }

            return miniBot.Storage;
        }

        /// <summary>
        ///   Set the storage of a bot.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <param name="append"> If true, append the data to the existing storage. </param>
        public void SetStorage(int id, string data, bool append = false)
        {
            MiniBot? miniBot = Orm.SelectByPrimaryKey<MiniBot>(id);
            if (miniBot == null)
            {
                return;
            }

            if (append)
            {
                miniBot.Storage += data;
            }
            else
            {
                miniBot.Storage = data;
            }

            Orm.Update(miniBot);
        }

        public void DeleteMiniBot(int id)
        {
            MiniBot? miniBot = Orm.SelectByPrimaryKey<MiniBot>(id);
            if (miniBot == null)
            {
                return;
            }

            Orm.Delete(miniBot);
        }
    }
}