using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Freeze.Library
{
    public class UWF : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<string> CurrentSettings = new ObservableCollection<string>();
        public ObservableCollection<string> NextSettings = new ObservableCollection<string>();
        public ObservableCollection<string> CurrentExclusion = new ObservableCollection<string>();
        public ObservableCollection<string> NextExclusion = new ObservableCollection<string>();

        public UWF()
        {
            InitializeAsync().ConfigureAwait(false);
        }

        private async Task InitializeAsync()
        {
            var status = await GetFilterStatus();
            this.Active = status;
            this.Enabled = status;

            await RefreshStatus();
        }

        public async Task RefreshStatus()
        {
            CurrentSettings.Clear();
            NextSettings.Clear();
            CurrentExclusion.Clear();
            NextExclusion.Clear();

            foreach (var item in await GetCurrentSettings())
                CurrentSettings.Add(item);
            foreach (var item in await GetNextSettings())
                NextSettings.Add(item);
            foreach (var item in await GetCurrentExclusion())
                CurrentExclusion.Add(item);
            foreach (var item in await GetNextExclusion())
                NextExclusion.Add(item);
        }

        private static async Task<string> SendCmd(string cmd)
        {
            var startInfo = new ProcessStartInfo("uwfmgr", cmd)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.Unicode
            };
            var uwf = Process.Start(startInfo);
            var output = await uwf.StandardOutput.ReadToEndAsync();
            await uwf.WaitForExitAsync();
            return output;
        }

        private static async Task<IEnumerable<string>> GetCurrentSettings()
        {
            return (await SendCmd("get-config")).Get("Current Session Settings", "Next Session Settings");
        }

        private static async Task<IEnumerable<string>> GetNextSettings()
        {
            return (await SendCmd("get-config")).Get("Next Session Settings", untilEnd: true);
        }

        private static async Task<bool> GetFilterStatus()
        {
            var statusLine = (await SendCmd("get-config")).Get("FILTER SETTINGS").First();
            var status = Regex.Match(statusLine, "Filter state:\\s*(\\w+)").Groups[1].Value;

            if (status == "ON")
                return true;
            else if (status == "OFF")
                return false;
            else
                return false;
        }

        public async Task Enable()
        {
            if (HORM)
                await SendCmd("filter enable-HORM");
            else
                await SendCmd("filter enable");

            Enabled = true;
        }

        public async Task Disable()
        {
            if (HORM)
                await SendCmd("filter disable-HORM");
            else
                await SendCmd("filter disable");
            
            Enabled = false;
        }

        public async Task Protect()
        {
            await SendCmd("volume protect C");
            await SendCmd("overlay set-type Disk");
            await SendCmd("overlay set-passthrough on");
        }

        public async Task AddExclusion(string dir)
        {
            await SendCmd($"file add-exclusion \"{dir}\"");
        }

        public async Task RemoveExclusion(string dir)
        {
            await SendCmd($"file remove-exclusion \"{dir}\"");
        }

        public async Task<IEnumerable<string>> GetCurrentExclusion()
        {
            return (await SendCmd("file get-exclusions")).Get("Current Session Settings");
        }

        public async Task<IEnumerable<string>> GetNextExclusion()
        {
            return (await SendCmd("file get-exclusions")).Get("Next Session Settings");
        }

        private bool _enabled = false;
        public bool Enabled {
            set
            {
                _enabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Enabled)));
            }
            get => _enabled;
        }

        public bool Active { private set; get; }

        private bool _HORM = false;
        public bool HORM
        {
            set
            {
                _HORM = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HORM)));
            }
            get => _HORM;
        }
    }
}