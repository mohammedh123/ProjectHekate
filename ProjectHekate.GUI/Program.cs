namespace ProjectHekate.GUI
{
    class Program
    {
        public static void Main()
        {
            using (var game = new MainGame()) {
                game.Run(60);
            }
        }
    }
}