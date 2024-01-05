using Watson.ORM.Sqlite;

namespace MiniBots
{
    public class StorageManager
    {
        private WatsonORM Orm { get; }
        private readonly int Id;
        /// <summary>
        ///   Replace the data in the storage with the given data.
        /// </summary>
        /// <param name="data"></param>
        /// <remarks>
        ///  This will overwrite the data in the storage.
        /// </remarks>
        public void ReplaceData(string data)
        {
            MiniBot miniBot = Orm.SelectByPrimaryKey<MiniBot>(Id);

            miniBot.Storage = data;
            Orm.Update<MiniBot>(miniBot);
        }
        /// <summary>
        ///  Append the given data to the storage.
        /// </summary>
        /// <param name="data"></param>
        public void AppendData(string data)
        {
            MiniBot miniBot = Orm.SelectByPrimaryKey<MiniBot>(Id);

            miniBot.Storage += data;
            Orm.Update<MiniBot>(miniBot);
        }
        /// <summary>
        ///  Get the data in the storage.
        ///  </summary>
        ///  <param name="data"></param>
        public string GetData()
        {
            MiniBot miniBot = Orm.SelectByPrimaryKey<MiniBot>(Id);

            return miniBot.Storage;
        }

        public StorageManager(WatsonORM orm, int id)
        {
            Orm = orm;
            Id = id;
        }
    }
}
