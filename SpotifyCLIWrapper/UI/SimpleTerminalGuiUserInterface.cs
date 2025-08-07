using System;
using System.Threading;
using System.Threading.Tasks;
using Terminal.Gui;
using ConsoleMediaPlayer;

namespace Player.UI
{
    public class SimpleTerminalGuiUserInterface : IUserInterface
    {
        private bool _isRunning;
        private CancellationTokenSource _cancellationTokenSource = null!;
        private IMusicPlayer _player = null!;
        
        // UI Components
        private Toplevel _top = null!;
        private Label _statusLabel = null!;
        private Label _trackInfoLabel = null!;
        private Label _timeLabel = null!;
        private ListView _playlistView = null!;
        private Label _subtitleLabel = null!;
        
        public async Task StartAsync(IMusicPlayer player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            _player = player;
            _isRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();

            // Try Terminal.Gui first, fall back to console on failure
            try
            {
                Application.Init();
                CreateSimpleUI();
                
                // Start the display update task
                var displayTask = UpdateDisplayAsync(_cancellationTokenSource.Token);
                
                // Run the application
                Application.Run(_top);
                
                _isRunning = false;
                _cancellationTokenSource.Cancel();
                await displayTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Terminal.Gui initialization failed: {ex.Message}");
                Console.WriteLine("Falling back to console interface...");
                
                // Clean up Terminal.Gui
                try { Application.Shutdown(); } catch { }
                
                // Fall back to console interface
                var fallbackUI = new ConsoleUserInterface();
                await fallbackUI.StartAsync(player);
            }
            finally
            {
                try
                {
                    Application.Shutdown();
                }
                catch
                {
                    // Ignore shutdown errors
                }
            }
        }

        private void CreateSimpleUI()
        {
            _top = Application.Top;
            
            // Create status panel
            _statusLabel = new Label("⏸️ Stopped")
            {
                X = 1,
                Y = 1,
                Width = Dim.Fill() - 2,
                Height = 1
            };

            _trackInfoLabel = new Label("No track loaded")
            {
                X = 1,
                Y = 2,
                Width = Dim.Fill() - 2,
                Height = 1
            };

            _timeLabel = new Label("00:00 / 00:00")
            {
                X = 1,
                Y = 3,
                Width = Dim.Fill() - 2,
                Height = 1
            };

            // Create playlist view
            _playlistView = new ListView()
            {
                X = 1,
                Y = 5,
                Width = Dim.Fill() - 2,
                Height = Dim.Fill() - 8
            };

            _playlistView.OpenSelectedItem += OnPlaylistItemSelected;

            // Create subtitle area
            _subtitleLabel = new Label("")
            {
                X = 1,
                Y = Pos.AnchorEnd(2),
                Width = Dim.Fill() - 2,
                Height = 1
            };

            // Add components
            _top.Add(_statusLabel, _trackInfoLabel, _timeLabel, _playlistView, _subtitleLabel);

            // Set up key bindings
            SetupKeyBindings();
        }

        private void SetupKeyBindings()
        {
            _top.KeyPress += (args) =>
            {
                switch (args.KeyEvent.Key)
                {
                    case Key.Space:
                        _player.TogglePlayPause();
                        args.Handled = true;
                        break;
                    case Key.CursorRight:
                        _player.NextTrack();
                        args.Handled = true;
                        break;
                    case Key.CursorLeft:
                        _player.PreviousTrack();
                        args.Handled = true;
                        break;
                    case Key.CursorUp:
                        _player.IncreaseVolume();
                        args.Handled = true;
                        break;
                    case Key.CursorDown:
                        _player.DecreaseVolume();
                        args.Handled = true;
                        break;
                    case (Key)'l':
                    case (Key)'L':
                        LoadDirectoryAction();
                        args.Handled = true;
                        break;
                    case (Key)'r':
                    case (Key)'R':
                        _player.LoopCurrentTrack();
                        args.Handled = true;
                        break;
                    case (Key)'s':
                    case (Key)'S':
                        _player.ShufflePlaylist();
                        args.Handled = true;
                        break;
                    case Key.Esc:
                    case (Key)'q':
                    case (Key)'Q':
                        QuitAction();
                        args.Handled = true;
                        break;
                }
            };
        }

        private async Task UpdateDisplayAsync(CancellationToken cancellationToken)
        {
            const int updateIntervalMs = 500;

            while (!cancellationToken.IsCancellationRequested && _isRunning)
            {
                try
                {
                    Application.MainLoop.Invoke(() =>
                    {
                        UpdateStatus();
                        UpdateTrackInfo();
                        UpdateTimeDisplay();
                        UpdateSubtitles();
                        UpdatePlaylistHighlight();
                    });

                    await Task.Delay(updateIntervalMs, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // Log error but continue
                    System.Diagnostics.Debug.WriteLine($"Error in UpdateDisplayAsync: {ex.Message}");
                }
            }
        }

        private void UpdateStatus()
        {
            if (_player.CurrentTrack != null)
            {
                string status = _player.IsPlaying ? "▶️ Playing" : "⏸️ Paused";
                
                if (_player.IsLoopingCurrentTrack)
                    status += " (Loop)";
                
                if (_player.IsShuffling)
                    status += " (Shuffle)";

                _statusLabel.Text = status + $" | Volume: {(int)(_player.Volume * 100)}%";
            }
            else
            {
                _statusLabel.Text = "⏹️ Stopped";
            }
        }

        private void UpdateTrackInfo()
        {
            if (_player.CurrentTrack != null)
            {
                string trackInfo = _player.CurrentTrack.FileName;
                if (_player.CurrentTrack.HasSubtitles)
                    trackInfo += " [CC]";
                
                _trackInfoLabel.Text = trackInfo;
            }
            else
            {
                _trackInfoLabel.Text = "No track loaded";
            }
        }

        private void UpdateTimeDisplay()
        {
            if (_player.CurrentTrack != null)
            {
                _timeLabel.Text = $"{_player.CurrentPosition:mm\\:ss} / {_player.TotalDuration:mm\\:ss}";
            }
            else
            {
                _timeLabel.Text = "00:00 / 00:00 | Press 'L' to load directory, 'Q' to quit";
            }
        }

        private void UpdateSubtitles()
        {
            if (_player.CurrentTrack?.HasSubtitles == true && _player.CurrentSubtitle != null)
            {
                _subtitleLabel.Text = _player.CurrentSubtitle.Text ?? "";
            }
            else
            {
                _subtitleLabel.Text = "";
            }
        }

        private void UpdatePlaylistHighlight()
        {
            // Update playlist to reflect current playing track
            if (_player.CurrentPlaylist?.Count > 0)
            {
                var items = new string[_player.CurrentPlaylist.Count];
                for (int i = 0; i < _player.CurrentPlaylist.Count; i++)
                {
                    var file = _player.CurrentPlaylist[i];
                    string prefix = "";
                    
                    // Mark current track
                    if (_player.CurrentTrack != null)
                    {
                        for (int j = 0; j < _player.CurrentPlaylist.Count; j++)
                        {
                            if (_player.CurrentPlaylist[j] == _player.CurrentTrack && j == i)
                            {
                                prefix = "► ";
                                break;
                            }
                        }
                    }
                    
                    items[i] = $"{prefix}{i + 1:D3}. {file.FileName}";
                }
                
                _playlistView.SetSource(items);
            }
        }

        private void LoadDirectoryAction()
        {
            var dialog = new OpenDialog("Load Music Directory", "Select a directory containing music files:")
            {
                DirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
                CanChooseDirectories = true,
                CanChooseFiles = false,
                AllowsMultipleSelection = false
            };

            Application.Run(dialog);

            if (!dialog.Canceled && dialog.FilePaths.Count > 0)
            {
                var selectedPath = dialog.FilePaths[0];
                Task.Run(async () =>
                {
                    try
                    {
                        await _player.LoadPlaylistFromDirectoryAsync(selectedPath);
                        
                        Application.MainLoop.Invoke(() =>
                        {
                            MessageBox.Query("Success", $"Loaded {_player.CurrentPlaylist.Count} audio files.", "OK");
                            
                            if (_player.CurrentPlaylist.Count > 0)
                            {
                                _player.PlayCurrentTrack();
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Application.MainLoop.Invoke(() =>
                        {
                            MessageBox.ErrorQuery("Error", $"Failed to load directory:\n{ex.Message}", "OK");
                        });
                    }
                });
            }
        }

        private void OnPlaylistItemSelected(ListViewItemEventArgs args)
        {
            if (args.Item >= 0 && args.Item < _player.CurrentPlaylist.Count)
            {
                // Play the selected track
                Task.Run(() =>
                {
                    try
                    {
                        _player.PlayTrackAtIndex(args.Item);
                    }
                    catch (Exception ex)
                    {
                        Application.MainLoop.Invoke(() =>
                        {
                            MessageBox.ErrorQuery("Error", $"Failed to play track:\n{ex.Message}", "OK");
                        });
                    }
                });
            }
        }

        private void QuitAction()
        {
            _isRunning = false;
            Application.RequestStop();
        }
    }
}
