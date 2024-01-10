using System.Text;
using DSharpPlus.Entities;
using LuaPlugins;
using MiniBots;
using NLua;


namespace LuaManagement
{
    public class DiscordLua
    {
        private Lua _lua;

        public DiscordLua()
        {
            _lua = new Lua();
        }

        public string Run(int id, string code, DiscordMessage message, DatabaseManager databaseManager)
        {
            _lua["messageManager"] = new MessageManager(message);
            _lua["timeManager"] = new TimeManager();
            _lua["storageManager"] = new StorageManager(databaseManager, id);

            Byte[] luaIn = Encoding.UTF8.GetBytes(code);

            _lua.State.Encoding = Encoding.UTF8;
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