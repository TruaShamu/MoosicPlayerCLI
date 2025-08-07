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
        private MenuBar _menuBar = null!;
        private Window _mainWindow = null!;
        private Window _playlistWindow = null!;
        private Window _statusWindow = null!;
        private Label _statusLabel = null!;
        private Label _trackInfoLabel = null!;
        private Label _timeLabel = null!;
        private ProgressBar _progressBar = null!;
        private ListView _playlistView = null!;
        private Label _subtitleLabel = null!;
        private Window _helpWindow = null!;
        private bool _helpVisible = false;
        
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
            
            // Create menu bar
            CreateMenuBar();
            
            // Create main windows
            CreateStatusWindow();
            CreatePlaylistWindow();
            CreateMainWindow();
            
            // Add all components to top
            _top.Add(_menuBar, _statusWindow, _playlistWindow, _mainWindow);

            // Create help window (initially hidden)
            CreateHelpWindow();

            // Set up key bindings
            SetupKeyBindings();
        }

        private void CreateHelpWindow()
        {
            _helpWindow = new Window("Help - Keyboard Shortcuts")
            {
                X = Pos.Center(),
                Y = Pos.Center(),
                Width = 50,
                Height = 16,
                Visible = false
            };

            var helpText = new Label(@"
 PLAYBACK CONTROLS:
 ─────────────────
 Space       Toggle Play/Pause
 →           Next Track
 ←           Previous Track
 ↑           Increase Volume
 ↓           Decrease Volume

 PLAYLIST CONTROLS:
 ─────────────────
 L           Load Directory
 R           Toggle Loop Current Track
 S           Shuffle Playlist
 Enter       Play Selected Track

 OTHER:
 ─────
 H           Toggle This Help
 Q/Esc       Quit

 Press H again to close this help.")
            {
                X = 1,
                Y = 1,
                Width = Dim.Fill() - 2,
                Height = Dim.Fill() - 2
            };

            _helpWindow.Add(helpText);
            _top.Add(_helpWindow);
        }

        private void CreateMenuBar()
        {
            _menuBar = new MenuBar(new MenuBarItem[] {
                new MenuBarItem("_File", new MenuItem[] {
                    new MenuItem("_Load Directory", "Load music directory", () => LoadDirectoryAction()),
                    new MenuItem("_Quit", "Exit application", () => QuitAction())
                }),
                new MenuBarItem("_Playback", new MenuItem[] {
                    new MenuItem("_Play/Pause", "Toggle playback", () => _player.TogglePlayPause()),
                    new MenuItem("_Next Track", "Skip to next track", () => _player.NextTrack()),
                    new MenuItem("_Previous Track", "Go to previous track", () => _player.PreviousTrack()),
                    new MenuItem("_Loop Current", "Toggle loop current track", () => _player.LoopCurrentTrack()),
                    new MenuItem("_Shuffle", "Shuffle playlist", () => _player.ShufflePlaylist())
                }),
                new MenuBarItem("_Volume", new MenuItem[] {
                    new MenuItem("_Increase", "Increase volume", () => _player.IncreaseVolume()),
                    new MenuItem("_Decrease", "Decrease volume", () => _player.DecreaseVolume())
                }),
                new MenuBarItem("_Help", new MenuItem[] {
                    new MenuItem("_Keyboard Shortcuts", "Show keyboard shortcuts", () => ToggleHelp()),
                    new MenuItem("_About", "About this application", () => ShowAbout())
                })
            });
        }

        private void CreateStatusWindow()
        {
            _statusWindow = new Window("Status")
            {
                X = 0,
                Y = 1, // Below menu bar
                Width = Dim.Fill(),
                Height = 6
            };

            _statusLabel = new Label("⏸️ Stopped")
            {
                X = 1,
                Y = 0,
                Width = Dim.Fill() - 2,
                Height = 1
            };

            _trackInfoLabel = new Label("No track loaded")
            {
                X = 1,
                Y = 1,
                Width = Dim.Fill() - 2,
                Height = 1
            };

            _timeLabel = new Label("00:00 / 00:00 | Press F1 for help")
            {
                X = 1,
                Y = 2,
                Width = Dim.Fill() - 2,
                Height = 1
            };

            _progressBar = new ProgressBar()
            {
                X = 1,
                Y = 3,
                Width = Dim.Fill() - 2,
                Height = 1
            };

            _statusWindow.Add(_statusLabel, _trackInfoLabel, _timeLabel, _progressBar);
        }

        private void CreatePlaylistWindow()
        {
            _playlistWindow = new Window("Playlist")
            {
                X = 0,
                Y = 7, // Below status window
                Width = Dim.Fill(),
                Height = Dim.Fill() - 10 // Leave space for main window
            };

            _playlistView = new ListView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            _playlistView.OpenSelectedItem += OnPlaylistItemSelected;
            _playlistWindow.Add(_playlistView);
        }

        private void CreateMainWindow()
        {
            _mainWindow = new Window("Subtitles")
            {
                X = 0,
                Y = Pos.AnchorEnd(3),
                Width = Dim.Fill(),
                Height = 3
            };

            _subtitleLabel = new Label("")
            {
                X = 1,
                Y = 0,
                Width = Dim.Fill() - 2,
                Height = 1,
                TextAlignment = TextAlignment.Centered
            };

            _mainWindow.Add(_subtitleLabel);
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
                    case (Key)'h':
                    case (Key)'H':
                        ToggleHelp();
                        args.Handled = true;
                        break;
                    case Key.Esc:
                        if (_helpVisible)
                        {
                            ToggleHelp();
                            args.Handled = true;
                        }
                        else
                        {
                            QuitAction();
                            args.Handled = true;
                        }
                        break;
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
                        UpdateProgressBar();
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
                _timeLabel.Text = "00:00 / 00:00 | Press 'H' for help, 'L' to load directory, 'Q' to quit";
            }
        }

        private void UpdateProgressBar()
        {
            if (_player.CurrentTrack != null && _player.TotalDuration.TotalSeconds > 0)
            {
                var progress = (float)(_player.CurrentPosition.TotalSeconds / _player.TotalDuration.TotalSeconds);
                _progressBar.Fraction = Math.Max(0, Math.Min(1, progress));
            }
            else
            {
                _progressBar.Fraction = 0;
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

        private void ToggleHelp()
        {
            _helpVisible = !_helpVisible;
            _helpWindow.Visible = _helpVisible;
            
            if (_helpVisible)
            {
                _helpWindow.SetFocus();
            }
        }

        private void ShowAbout()
        {
            MessageBox.Query("About box here...", "OK");
        }
    }
}
