using SolidBoxGE.Core;

namespace Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Application app = new Sandbox();

            NLua.Lua lua= new();

            lua.DoFile("./Data/Scripts/init.lua");

            app.Run();
        }
    }
}