using LuaManagement;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public string TableToJson(LuaTable table)
        {
            Dictionary<object, object> dictionary = _discordLua.GetDictFromTable(table);
            dictionary = FindTables(dictionary);

            string json = JsonConvert.SerializeObject(dictionary);
            Console.WriteLine(json);

            return json;
        }

        private Dictionary<object, object> FindTables(Dictionary<object, object> dictionary)
        {
            foreach (KeyValuePair<object, object> de in dictionary)
            {
                if (de.Value.GetType() == typeof(LuaTable))
                {
                    Dictionary<object, object> tmpDictionary = _discordLua.GetDictFromTable((LuaTable)de.Value);
                    dictionary[de.Key] = FindTables(tmpDictionary);
                }
            }

            return dictionary;
        }

        public LuaTable JsonToTable(string json)
        {
            LuaTable table = _discordLua.CreateLuaTable();

            Dictionary<object, object>? dictionary = JsonConvert.DeserializeObject<Dictionary<object, object>>(json);
            if (dictionary != null)
            {
                table = FindJObject(dictionary, table);
            }
            return table;
        }

        private LuaTable FindJObject(Dictionary<object, object> dictionary, LuaTable luaTable)
        {
            foreach (KeyValuePair<object, object> de in dictionary)
            {
                if (de.Value.GetType() == typeof(JObject))
                {
                    JObject? jObject = (JObject)de.Value;
                    Dictionary<object, object>? subDictionary = jObject.ToObject<Dictionary<object, object>>();

                    if (subDictionary != null)
                    {
                        Console.WriteLine("going deeper");
                        luaTable[de.Key] = FindJObject(subDictionary, _discordLua.CreateLuaTable());
                    }
                }
                else
                {
                    luaTable[de.Key] = de.Value;
                }
            }
            return luaTable;
        }
    }
}