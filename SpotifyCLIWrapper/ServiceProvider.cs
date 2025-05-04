using ConsoleMediaPlayer;

public class ServiceProvider
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public ServiceProvider()
        {
            // Register services
            _services[typeof(IFileSystem)] = new FileSystem();
            _services[typeof(ISubtitleParser)] = new SrtSubtitleParser(GetService<IFileSystem>());
            _services[typeof(IAudioFileScanner)] = new AudioFileScanner(
                GetService<IFileSystem>(), 
                GetService<ISubtitleParser>());
            _services[typeof(IAudioPlayer)] = new NAudioPlayer();
            _services[typeof(IPlaylist)] = new Playlist();
            _services[typeof(IMusicPlayer)] = new MusicPlayer(
                GetService<IAudioFileScanner>(),
                GetService<IAudioPlayer>(),
                GetService<IPlaylist>());
            _services[typeof(IUserInterface)] = new ConsoleUserInterface();
        }

        public T GetService<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }
            
            throw new InvalidOperationException($"Service of type {typeof(T).Name} is not registered.");
        }
    }