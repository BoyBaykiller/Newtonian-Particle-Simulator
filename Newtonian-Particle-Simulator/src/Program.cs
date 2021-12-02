namespace Newtonian_Particle_Simulator
{
    class Program
    {
        static void Main()
        {
            MainWindow gameWindow = new MainWindow();
            gameWindow.Run(System.Math.Min(OpenTK.DisplayDevice.Default.RefreshRate, 144));
        }
    }
}
