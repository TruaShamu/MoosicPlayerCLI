using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

namespace ConsoleMediaPlayer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = new ServiceProvider();
            var player = serviceProvider.GetService<IMusicPlayer>();
            var userInterface = serviceProvider.GetService<IUserInterface>();
            
            Console.WriteLine($"Using UI: {userInterface.GetType().Name}");
            await userInterface.StartAsync(player);
        }
    }
}