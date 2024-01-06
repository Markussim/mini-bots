using System.Text;
using DSharpPlus.Entities;
using LuaPlugins;
using NLua;
using Watson.ORM.Sqlite;


namespace LuaManagement
{
    public class DiscordLua
    {
        private Lua _lua;

        public DiscordLua()
        {
            _lua = new Lua();
        }

        public string Run(int id, string code, DiscordMessage message, WatsonORM orm)
        {
            _lua["messageManager"] = new MessageManager(message);
            _lua["timeManager"] = new TimeManager();
            _lua["storageManager"] = new StorageManager(orm, id);

            Byte[] luaIn = Encoding.UTF8.GetBytes(code);

            // TODO: Handle utf8 output
            object[] luaOutput = _lua.DoString(luaIn);

            if (luaOutput.Length > 0)
            {
                var stringOut = luaOutput[0].ToString();
                if (stringOut != null)
                {
                    return stringOut;
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }

        }
    }
}