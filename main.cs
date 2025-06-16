using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Text;

namespace VeloskiddyFixer {
  public partial class MainWindow: Window {
      private bool _isClosingAnimationCompleted = false;
      private string currentVersion = "0.0.5";
      public MainWindow() {
        InitializeComponent();
        LocationVerification();
        Loaded += OnWindowLoaded;
        Closing += OnWindowClosing;
      }

      public static void AnimateOpacity(UIElement element, double targetOpacity, double durationSeconds = 0.25) {
        var animation = new DoubleAnimation {
          To = targetOpacity,
            Duration = TimeSpan.FromSeconds(durationSeconds),
            EasingFunction = new QuadraticEase {
              EasingMode = EasingMode.EaseInOut
            }
        };
        element.BeginAnimation(UIElement.OpacityProperty, animation);
      }

      private async Task Unzip() {
        NoticeFixer.Text = "Extracting Dependencies..";
        FixerInfo.Text = "This could take a while depending on your PC..";
        NoiceIcon.Source = new BitmapImage(new Uri("Resources/Folder.png", UriKind.Relative));
        string zipPath = @ "C:\Windows\Temp\aio.zip";
        string extractPath = @ "C:\Windows\Temp\aio";
        ZipFile.ExtractToDirectory(zipPath, extractPath);
      }

      private async Task cleanUp() {
        string zipPath = @ "C:\Windows\Temp\aio.zip";
        string extractPath = @ "C:\Windows\Temp\aio";

        NoticeFixer.Text = "Cleaning up..";
        FixerInfo.Text = "Cleaning up the files VelocityFixer downloaded....";
        NoiceIcon.Source = new BitmapImage(new Uri("Resources/recyclebin.png", UriKind.Relative));
        try {
          if (File.Exists(zipPath)) {
            File.Delete(zipPath);
          }
          if (Directory.Exists(extractPath)) {
            Directory.Delete(extractPath, true);
          }
        } catch (Exception ex) {
          //MessageBox.Show(extractPath + " could not be deleted, please delete it manually.\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
      }

      public async Task SetDNS() {
        NoticeFixer.Text = "Setting DNS servers...";
        FixerInfo.Text = "Please wait...";
        NoiceIcon.Source = new BitmapImage(new Uri("Resources/wifi.png", UriKind.Relative));
        string[] ipv4Dns = {
          "8.8.8.8",
          "8.8.4.4"
        };
        string[] ipv6Dns = {
          "2001:4860:4860::8888",
          "2001:4860:4860::8844"
        };

        string psScript = @ "
        $ipv4dns = @('8.8.8.8', '8.8.4.4')
        $ipv6dns = @('2001:4860:4860::8888', '2001:4860:4860::8844')

        $adapters = Get - NetAdapter - Physical | Where - Object {
          $_.Status - eq 'Up'
        }

        foreach($adapter in $adapters) {
          $ifIndex = $adapter.ifIndex
          Set - DnsClientServerAddress - InterfaceIndex $ifIndex - ServerAddresses $ipv4dns
          Set - DnsClientServerAddress - InterfaceIndex $ifIndex - ServerAddresses $ipv6dns
        }
        "; // thanks gpt - Korabi

        var startInfo = new ProcessStartInfo {
          FileName = "powershell",
            Arguments = $ "-NoProfile -ExecutionPolicy Bypass -Command \"{psScript}\"",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try {
          Process.Start(startInfo);
        } catch (Exception ex) {
          Console.WriteLine("Error launching PowerShell: " + ex.Message);
        }
      }
      private async Task GetVersion() {
        NoticeFixer.Text = "Checking for latest version...";
        FixerInfo.Text = "Please wait...";
        NoiceIcon.Source = new BitmapImage(new Uri("Resources/Meteor-HQ-NOR.png", UriKind.Relative));

        string versionUrl = "https://download.getvelocity.live/api/fixer/version";

        try {
          using(HttpClient client = new HttpClient()) {
            string version = await client.GetStringAsync(versionUrl);
            version = version.Trim();

            if (version != currentVersion) {
              MessageBox.Show($"A new version of VelocityFixer is available! Please download it from the official website.\n\nCurrent Version: {currentVersion}\nLatest Version: {version}\n\ngetvelocity.live/fixer", "Update Available", MessageBoxButton.OK, MessageBoxImage.Information);
              Environment.Exit(0);
            }
          }
        } catch (Exception ex) {
          MessageBox.Show($"Failed to check version! You CAN click OK to proceed, but re-install the fixer just in case from getvelocity.live/fixer\n\n {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
          // Environment.Exit(0);
        }
      }

      private async Task DownloadNET() {
        NoticeFixer.Text = "Downloading .NET";
        FixerInfo.Text = "This could take a while depending on your PC..";
        NoiceIcon.Source = new BitmapImage(new Uri("Resources/NET.png", UriKind.Relative));
        string psCommand = "irm https://builds.dotnet.microsoft.com/dotnet/scripts/v1/dotnet-install.ps1 | iex"; // thanks micropoop - Korabi

        try {
          ProcessStartInfo startInfo = new ProcessStartInfo() {
            FileName = "powershell.exe",
              Arguments = $ "-NoProfile -ExecutionPolicy Bypass -Command \"{psCommand}\"",
              UseShellExecute = false,
              CreateNoWindow = true,
              RedirectStandardOutput = true,
              RedirectStandardError = true
          };

          Process.Start(startInfo);
        } catch (Exception ex) {
          MessageBox.Show($"Failed to download .NET exclusions:\n{ex.Message}");
        }
      }

      private async Task Download() {
        NoticeFixer.Text = "Downloading Dependencies";
        FixerInfo.Text = "This could take a while depending on your PC..";
        NoiceIcon.Source = new BitmapImage(new Uri("Resources/Download.png", UriKind.Relative));

        string url = "https://github.com/Korabi-dev/VelocityFixer/releases/download/Production/aio.zip";
        string filePath = @ "C:\Windows\Temp\aio.zip";

        using(HttpClient client = new HttpClient())
        using(HttpResponseMessage response = await client.GetAsync(url))
        using(Stream contentStream = await response.Content.ReadAsStreamAsync(),
          fileStream = File.Create(filePath)) {
          response.EnsureSuccessStatusCode();
          await contentStream.CopyToAsync(fileStream);
          Console.WriteLine("Download complete!");
        }
      }

      private async Task KillProcesses() {
        string[] processNames = {
          "Velocity",
          "Decompiler",
          "erto3e4rortoergn",
          "RobloxPlayerBeta"
        };

        int killedCount = 0;

        NoticeFixer.Text = "Closing Velocity..";
        FixerInfo.Text = "This should take a few seconds..";
        NoiceIcon.Source = new BitmapImage(new Uri("Resources/Meteor-HQ-NOR.png", UriKind.Relative));
        foreach(string name in processNames) {
          Process[] found = Process.GetProcessesByName(name);
          foreach(Process proc in found) {
            try {
              proc.Kill();
              killedCount++;
            } catch {
              NoticeFixer.Text = "Unable to find process";
              FixerInfo.Text = "Velocity does not appear to be running, skipping..";

            }
          }
        }
      }

      private void ExitApplication(object sender, MouseButtonEventArgs e) {
        OnWindowClosing(this, new System.ComponentModel.CancelEventArgs());
      }

      private async Task AddDefenderExclusions() {
        NoticeFixer.Text = "Adding Exclusions..";
        FixerInfo.Text = "This should take a few seconds..";
        NoiceIcon.Source = new BitmapImage(new Uri("Resources/ShittyAntiVirus.png", UriKind.Relative));
        string[] filesToExclude = {
          "Velocity.exe",
          "VelocityAPI.dll",
          "Decompiler.exe",
          "erto3e4rortoergn.exe"
        };

        string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string psCommand = "";

        foreach(string fileName in filesToExclude) {
          psCommand += $ "Add-MpPreference -ExclusionPath '{fileName}'; ";
        }

        psCommand += @ "Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios' -Name 'HypervisorEnforcedCodeIntegrity' -Value 0 -Type DWord;";
        psCommand += $ "Add-MpPreference -ExclusionPath '{exeDirectory}';";
        NoticeFixer.Text = "Adding Exclusions..";
        FixerInfo.Text = "We are automatically adding Windows Defender exclusions for Velocity and all it's files, please wait.";
        NoiceIcon.Source = new BitmapImage(new Uri("Resources/ShittyAntiVirus.png", UriKind.Relative));

        try {
          ProcessStartInfo startInfo = new ProcessStartInfo() {
            FileName = "powershell.exe",
              Arguments = $ "-NoProfile -ExecutionPolicy Bypass -Command \"{psCommand}\"",
              UseShellExecute = false,
              CreateNoWindow = true,
              RedirectStandardOutput = true,
              RedirectStandardError = true
          };

          Process.Start(startInfo);
        } catch (Exception ex) {
          MessageBox.Show($"Failed to add exclusions:\n{ex.Message}");
        }
      }

      public async Task scanNow() {
        NoticeFixer.Text = "Fixing System Files";
        FixerInfo.Text = "Performing a scan of your system files, this may take a while...";
        NoiceIcon.Source = new BitmapImage(new Uri("Resources/wrench.png", UriKind.Relative));

        try {
          var psi = new ProcessStartInfo {
            FileName = "cmd.exe",
              Arguments = "/c sfc /scannow",
              RedirectStandardOutput = true,
              RedirectStandardError = true,
              UseShellExecute = false,
              CreateNoWindow = true
          };

          using(var process = new Process {
            StartInfo = psi
          }) {
            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (!string.IsNullOrWhiteSpace(error)) {
              MessageBox.Show(error, "SFC Errors");
            }
          }
        } catch (Exception ex) {
          MessageBox.Show(ex.Message, "Error");
        }
      }
      private CancellationTokenSource _cts;
      public async Task DeleteFoldersAsync() {
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        NoticeFixer.Text = "Deleting Files & Folders";
        FixerInfo.Text = "Clearing temporary files and Roblox data…";
        NoiceIcon.Source = new BitmapImage(new Uri("Resources/delete-folder.png", UriKind.Relative));

        var tempPath = Path.GetTempPath();
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        var foldersToClear = new [] {
          tempPath
        };

        var robloxRoot = Path.Combine(localAppData, "Roblox");
        var foldersToDelete = new [] {
          Path.Combine(robloxRoot, "crashpad"),
            Path.Combine(robloxRoot, "logs"),
            Path.Combine(robloxRoot, "downloads")
        };

        var report = await Task.Run(() => {
          var sb = new StringBuilder();

          void TryDeleteFile(string file) {
            try {
              File.SetAttributes(file, FileAttributes.Normal);
              File.Delete(file);
            } catch (Exception ex) {
              sb.AppendLine($"[File]   {file} → {ex.Message}");
            }
          }

          void TryDeleteDirectory(string dir, bool recursive) {
            try {
              Directory.Delete(dir, recursive);
            } catch (Exception ex) {
              sb.AppendLine($"[Folder] {dir} → {ex.Message}");
            }
          }

          foreach(var folder in foldersToClear) {
            if (token.IsCancellationRequested) break;

            if (!Directory.Exists(folder)) {
              sb.AppendLine($"[Skipped] Folder not found: {folder}");
              continue;
            }

            foreach(var entry in Directory.EnumerateFileSystemEntries(folder)) {
              if (token.IsCancellationRequested) break;

              if (File.Exists(entry))
                TryDeleteFile(entry);
              else if (Directory.Exists(entry))
                TryDeleteDirectory(entry, true);
            }
          }

          foreach(var folder in foldersToDelete) {
            if (token.IsCancellationRequested) break;

            if (Directory.Exists(folder))
              TryDeleteDirectory(folder, true);
            else
              sb.AppendLine($"[Skipped] Folder not found: {folder}");
          }

          return sb.ToString();
        }, token);

        _cts.Dispose();
        _cts = null;
      }

      private async void ShowFixer(object sender, MouseEventArgs e) {
          AnimateOpacity(IntroductionBorder, 0);
          await Task.Delay(600);
          IntroductionBorder.Margin = new Thickness(910, 46, -910, 0);

          MainBorder.Opacity = 0;
          MainBorder.Margin = new Thickness(0, 46, 0, 0);
          AnimateOpacity(MainBorder, 1);

          AnimateOpacity(RebootPC, 0, 0);
          RebootPC.IsEnabled = false;
          await Task.Delay(2200);
          await SetDNS();
          await Task.Delay(2000);
          await GetVersion();
          await Task.Delay(2000);
          if (Directory.Exists(@ "C:\Windows\Temp\aio\")) {
                Directory.Delete(@ "C:\Windows\Temp\aio\", true);
                }
                if (File.Exists(@ "C:\Windows\Temp\aio.zip")) {
                  File.Delete(@ "C:\Windows\Temp\aio.zip");
                }
                await KillProcesses();
                await Task.Delay(2000);
                await DeleteFoldersAsync();
                await Task.Delay(2000);
                await DownloadNET();
                await Task.Delay(2000);
                await Download();
                await Task.Delay(2000);
                await Unzip();
                string filepath = @ "C:\Windows\Temp\aio\install_all.bat";
                if (File.Exists(filepath)) {
                  Process process = new Process();
                  process.StartInfo.FileName = filepath;
                  process.StartInfo.UseShellExecute = false;
                  process.StartInfo.CreateNoWindow = true;
                  process.Start();
                  process.WaitForExit();

                  Process process2 = new Process();
                  process2.StartInfo.FileName = @ "C:\Windows\Temp\aio\dxwebsetup.exe";
                  process2.StartInfo.Arguments = "/Q";
                  process2.StartInfo.UseShellExecute = false;
                  process2.StartInfo.CreateNoWindow = true;
                  process2.Start();

                  await AddDefenderExclusions();
                  await cleanUp();
                  await Task.Delay(2000);
                  await scanNow();
                  NoticeFixer.Text = "All done!";
                  FixerInfo.Text = "Please restart your PC to apply the full fixes onto your system!";
                  NoiceIcon.Source = new BitmapImage(new Uri("Resources/ShittyAntiVirus.png", UriKind.Relative));
                  AnimateOpacity(RebootPC, 1);
                  RebootPC.IsEnabled = true;

                } else {
                  MessageBox.Show("The required files were not found. Please check your internet connection, and restart the fixer.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                  Environment.Exit(0);
                }
              }

              private void RebootButton_Click(object sender, MouseEventArgs e) {
                var result = MessageBox.Show(
                  "Do you want to reboot the computer?",
                  "Reboot Confirmation",
                  MessageBoxButton.YesNo,
                  MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes) {
                  try {
                    Process.Start(new ProcessStartInfo("shutdown", "/r /t 0") {
                      CreateNoWindow = true,
                        UseShellExecute = false
                    });
                  } catch (Exception ex) {
                    MessageBox.Show($"Failed to reboot: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                  }
                } else {
                  MessageBox.Show("Reboot canceled.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                  Environment.Exit(0);
                }
              }

              private void ChromeDrag(object sender, MouseEventArgs e) {

                if (e.LeftButton == MouseButtonState.Pressed) {
                  try {
                    this.DragMove();
                  } catch (InvalidOperationException) {

                  }
                }
              }

              private void LocationVerification() {
                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string velocityPath = System.IO.Path.Combine(exeDirectory, "Velocity.exe");
                if (!File.Exists(velocityPath)) {
                  UserGreet.Text = "Oh no..";
                  WindowInfo.Text = "Please put this executable in the same directory where your Velocity installation is located.";
                  Notice.Text = "You can't proceed with the fixing process without doing this step! ";
                  ChromeTitle.Text = "Velocity Fixer - Failed!";
                  ContinueToInstaller.Opacity = 0.2;
                  ContinueToInstaller.IsEnabled = false;
                  IntroImage.Source = new BitmapImage(new Uri("Resources/Error.png", UriKind.Relative));
                }
              }

              private void MinimiseWindow(object sender, MouseButtonEventArgs e) {
                this.WindowState = WindowState.Minimized;
              }

              private async void OnWindowLoaded(object sender, RoutedEventArgs e) {
                Storyboard ? WindowOpenAnimation = FindResource("WindowOpenAnimation") as Storyboard;
              }

              private void OnWindowClosing(object ? sender, System.ComponentModel.CancelEventArgs e) {
                if (!_isClosingAnimationCompleted) {
                  e.Cancel = true;

                  if (FindResource("WindowCloseAnimation") is Storyboard windowCloseAnimation) {
                    windowCloseAnimation.Completed += OnWindowCloseAnimationCompleted;
                    windowCloseAnimation.Begin(this);
                  }
                }
              }

              private void OnWindowCloseAnimationCompleted(object ? sender, EventArgs e) {
                _isClosingAnimationCompleted = true;
                this.Close();

              }
            }
          }
