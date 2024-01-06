using MiniBots;


namespace LuaPlugins
{
    public class StorageManager
    {
        private DatabaseManager _databaseManager;
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
            _databaseManager.SetStorage(_id, data, false);
        }
        /// <summary>
        ///  Append the given data to the storage.
        /// </summary>
        /// <param name="data"></param>
        public void AppendData(string data)
        {
            _databaseManager.SetStorage(_id, data, true);
        }
        /// <summary>
        ///  Get the data in the storage.
        ///  </summary>
        ///  <param name="data"></param>
        public string GetData()
        {
            return _databaseManager.GetStorage(_id);
        }

        public StorageManager(DatabaseManager databaseManager, int id)
        {
            _databaseManager = databaseManager;
            _id = id;
        }
    }
}
