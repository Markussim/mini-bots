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


        public LuaTable JsonToTable(string json)
        {
            Console.WriteLine("Tjo");
            LuaTable table = _discordLua.CreateLuaTable();
            Console.WriteLine(table.ToString());
    
            Console.WriteLine(table.Keys);
            if (table.Keys.ToString() == null){
                Console.WriteLine("test");
            }
            Console.WriteLine("tablehej: " + table["hej"]);

            Dictionary<object, object>? dictionary = JsonConvert.DeserializeObject<Dictionary<object, object>>(json);
            if (dictionary != null)
            {
                foreach (KeyValuePair<object, object> de in dictionary)
                {
                    Console.WriteLine(de.Key.ToString() + ", " + de.Value.ToString());
                    Console.WriteLine(de.Value.GetType());

                    if (de.Value.GetType() == typeof(JObject))
                    {
                        JObject? jObject = (JObject)de.Value;
                        Dictionary<object, object>? subDictionary = jObject.ToObject<Dictionary<object, object>>();
                        if (subDictionary != null)
                        {
                            dictionary[de.Key] = subDictionary;
                        }
                    }
                    else
                    {
                        //table = "hgej";
                        table[de.Key] = de.Value;
                    }
                    // TODO: Make sure keys like 1 are returned as int and not string
                }
            }
            return table;
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
    }
}