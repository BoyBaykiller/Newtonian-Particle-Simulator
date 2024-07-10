using System;
using System.Windows;
using System.Windows.Controls;

namespace ParticleSimulation
{
    public partial class MainWindow : Window
    {
        private TextBox particleCountTextBox;
        private Button restartButton;

        public MainWindow()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            // Create TextBox for entering the number of particles
            particleCountTextBox = new TextBox
            {
                Width = 100,
                Margin = new Thickness(10)
            };

            // Create Button to restart the simulation
            restartButton = new Button
            {
                Content = "Restart Simulation",
                Margin = new Thickness(10)
            };
            restartButton.Click += RestartButton_Click;

            // Add TextBox and Button to the layout
            var stackPanel = new StackPanel();
            stackPanel.Children.Add(particleCountTextBox);
            stackPanel.Children.Add(restartButton);

            this.Content = stackPanel;
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(particleCountTextBox.Text, out int particleCount))
            {
                RestartSimulation(particleCount);
            }
            else
            {
                MessageBox.Show("Please enter a valid number of particles.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RestartSimulation(int particleCount)
        {
            // Logic to restart the simulation with the specified number of particles
            // This method should be implemented in the remaining files
            Console.WriteLine($"Simulation restarted with {particleCount} particles.");
        }
    }
}