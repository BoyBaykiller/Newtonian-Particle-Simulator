namespace Newtonian_Particle_Simulator
{
    class Program
    {
        static void Main()
        {
            MainWindow gameWindow = new MainWindow();
            gameWindow.Run(OpenTK.DisplayDevice.Default.RefreshRate);
        }
    }
}
