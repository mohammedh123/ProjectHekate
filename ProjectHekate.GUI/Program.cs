using ProjectHekate.Scripting;

namespace ProjectHekate.GUI
{
    class Program
    {
        public static void Main()
        {
            var se = new ScriptEngine();
            se.Run();

            using (var game = new MainGame()) {
                game.Run(60);
            }
        }
    }
}