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


        public string JsonToString(LuaTable luaTable)
        {
            return "";
        }

        public Dictionary<object, object> FindTables(Dictionary<object, object> testdict)
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

        public void Testfuction(LuaTable test)
        {
            Dictionary<object, object> testdict = _discordLua.GetDictFromTable(test);
            testdict = FindTables(testdict);

            Console.WriteLine(testdict);

            foreach (KeyValuePair<object, object> de in testdict)
            {
                Console.WriteLine(de.Value.GetType());
                Console.WriteLine("{0} {1}", de.Key.ToString(), de.Value.ToString());
                //if (de.Value.GetType() == typeof(LuaTable))
                //{
                //    Dictionary<object, object> testdict2 = _discordLua.GetDictFromTable(test);
                //}
            }

            Console.WriteLine(JsonConvert.SerializeObject(testdict));
        }
    }
}