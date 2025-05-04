    public class ConsoleUserInterface : IUserInterface
    {
        private bool _isRunning;
        private CancellationTokenSource _cancellationTokenSource;

        public async Task StartAsync(IMusicPlayer player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            _isRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();

            DisplayWelcomeMessage();
            
            // Start the display update task
            var displayTask = UpdateDisplayAsync(player, _cancellationTokenSource.Token);

            while (_isRunning)
            {
                DisplayMenu();
                var key = Console.ReadKey(true);
                await HandleKeyPressAsync(key, player);
            }

            _cancellationTokenSource.Cancel();
            await displayTask;
        }

        private void DisplayWelcomeMessage()
        {
            Console.Clear();
            Console.WriteLine("welcome message");
            Console.WriteLine("Press 'L' to load a music directory.");
        }

        private void DisplayMenu()
        {
            Console.WriteLine("\nCommands:");
            Console.WriteLine("  L: Load music directory");
            Console.WriteLine("  P: Play/Pause");
            Console.WriteLine("  >: Next track");
            Console.WriteLine("  <: Previous track");
            Console.WriteLine("  V: View playlist");
            Console.WriteLine("  Q: Quit");
            Console.WriteLine("  .: Loop current track");
            Console.Write("\nEnter command: ");
        }

        private async Task HandleKeyPressAsync(ConsoleKeyInfo key, IMusicPlayer player)
        {
            switch (char.ToUpper(key.KeyChar))
            {
                case 'L':
                    await LoadDirectoryAsync(player);
                    break;
                case 'P':
                    player.TogglePlayPause();
                    break;
                case '>':
                    player.NextTrack();
                    break;
                case '<':
                    player.PreviousTrack();
                    break;
                case 'V':
                    DisplayPlaylist(player);
                    break;
                case 'Q':
                    _isRunning = false;
                    break;
                case '.':
                    player.LoopCurrentTrack();
                    break;
            }
        }

        private async Task LoadDirectoryAsync(IMusicPlayer player)
        {
            Console.Write("\nEnter directory path: ");
            var path = Console.ReadLine();

            try
            {
                await player.LoadPlaylistFromDirectoryAsync(path);
                Console.WriteLine($"Loaded {player.CurrentPlaylist.Count} audio files.");
                
                if (player.CurrentPlaylist.Count > 0)
                {
                    player.PlayCurrentTrack();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading directory: {ex.Message}");
            }
        }

        private void DisplayPlaylist(IMusicPlayer player)
        {
            Console.WriteLine("\nPlaylist:");
            
            if (player.CurrentPlaylist.Count == 0)
            {
                Console.WriteLine("  No files loaded.");
                return;
            }

            for (int i = 0; i < player.CurrentPlaylist.Count; i++)
            {
                var file = player.CurrentPlaylist[i];
                Console.WriteLine($"{i+1}. {file.FileName}");
                
            }
        }

        private async Task UpdateDisplayAsync(IMusicPlayer player, CancellationToken cancellationToken)
        {
            const int updateIntervalMs = 1000;
            var statusLine = Console.CursorTop;

            while (!cancellationToken.IsCancellationRequested)
            {
                if (player.CurrentTrack != null)
                {
                    int originalTop = Console.CursorTop;
                    int originalLeft = Console.CursorLeft;

                    // Save current cursor position
                    Console.SetCursorPosition(0, 0);
                    
                    // Clear the status line
                    Console.Write(new string(' ', Console.WindowWidth));
                    
                    // Display playing status
                    Console.SetCursorPosition(0, 0);
                    string status = player.IsPlaying ? "► Playing" : "❚❚ Paused";
                    string trackInfo = $"{player.CurrentTrack.FileName}";
                    string timeInfo = $"{player.CurrentPosition:mm\\:ss} / {player.TotalDuration:mm\\:ss}";

                    // Display whether the track is looping
                    if (player.IsLoopingCurrentTrack)
                    {
                        status += " (Looping)";
                    }
                    
                    Console.Write($"{status} | {trackInfo} | {timeInfo}");
                    
                    // Restore cursor position
                    Console.SetCursorPosition(originalLeft, originalTop);
                }

                await Task.Delay(updateIntervalMs, cancellationToken);
            }
        }
    }