using System.Collections;
using System.Text.Json;
using LuaManagement;
using Newtonsoft.Json;
using NLua;

namespace LuaPlugins
{
    class JsonManager
    {
        private DiscordLua _discordLua;
        public JsonManager(DiscordLua discordLua)
        {
            _discordLua = discordLua;
        }


        public string JsonToTable(string json)
        {
            return "";
        }

        public string TableToJson(LuaTable table)
        {
            Dictionary<object, object> dictionary = _discordLua.GetDictFromTable(table);
            dictionary = FindTables(dictionary);

            string json = JsonConvert.SerializeObject(dictionary);
            Console.WriteLine(json);

            return json;
        }

        private Dictionary<object, object> FindTables(Dictionary<object, object> testdict)
        {
            foreach (KeyValuePair<object, object> de in testdict)
            {
                if (de.Value.GetType() == typeof(LuaTable))
                {
                    Dictionary<object, object> testdict2 = _discordLua.GetDictFromTable((LuaTable)de.Value);
                    testdict[de.Key] = FindTables(testdict2);
                }
            }

            return testdict;
        }
    }
}