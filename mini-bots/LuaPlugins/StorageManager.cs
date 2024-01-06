using MiniBots;
using Watson.ORM.Sqlite;


namespace LuaPlugins
{
    public class StorageManager
    {
        private WatsonORM Orm { get; }
        private readonly int _id;
        /// <summary>
        ///   Replace the data in the storage with the given data.
        /// </summary>
        /// <param name="data"></param>
        /// <remarks>
        ///  This will overwrite the data in the storage.
        /// </remarks>
        public void ReplaceData(string data)
        {
            MiniBot miniBot = Orm.SelectByPrimaryKey<MiniBot>(_id);

            miniBot.Storage = data;
            Orm.Update<MiniBot>(miniBot);
        }
        /// <summary>
        ///  Append the given data to the storage.
        /// </summary>
        /// <param name="data"></param>
        public void AppendData(string data)
        {
            MiniBot miniBot = Orm.SelectByPrimaryKey<MiniBot>(_id);

            miniBot.Storage += data;
            Orm.Update<MiniBot>(miniBot);
        }
        /// <summary>
        ///  Get the data in the storage.
        ///  </summary>
        ///  <param name="data"></param>
        public string GetData()
        {
            MiniBot miniBot = Orm.SelectByPrimaryKey<MiniBot>(_id);

            return miniBot.Storage;
        }

        public StorageManager(WatsonORM orm, int id)
        {
            Orm = orm;
            _id = id;
        }
    }
}
