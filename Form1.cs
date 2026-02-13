using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Net.Http;
using System.Security.Cryptography;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using IWshRuntimeLibrary;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Reflection;

namespace SVLauncher
{
    public partial class Form1 : Form
    {
        private string defaultGamePath = @"C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\";
        private string settingsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.txt");
        private string? gamePath;
        private int progressValue = 0;
        private bool isGameRunning = false;
        private bool isUpdated = false;
        private CancellationTokenSource? downloadCts;
        private static readonly HttpClient httpClient = new HttpClient();
        private string configUrl = "https://sanchez4431.github.io/Chronos/config_sv.xml";
        private string launcherUpdateUrl = "https://sanchez4431.github.io/Chronos/SVHubUpdater.xml";
        private string currentLauncherVersion;
        private bool menuExpanded = false;
        private bool isDeveloperMode = false;
        private bool isDMode = false;
        private System.Windows.Forms.Timer menuAnimTimer = new System.Windows.Forms.Timer();
        private int menuStartX;
        private int menuEndX;
        private float uiScale = 1.0f;
        private float waveOffset = 0;
        private float colorOffset = 0;
        private LinearGradientBrush? backgroundBrush;
        private Color colorStart = Color.FromArgb(30, 15, 45);
        private Color colorEnd = Color.FromArgb(45, 25, 70);
        private enum Language { Russian, English }
        private Language currentLanguage = Language.Russian;

        private readonly Dictionary<string, (string ru, string en)> texts =
            new Dictionary<string, (string ru, string en)>
            {
                ["PLAY"] = ("ИГРАТЬ", "PLAY"),
                ["GAME_RUNNING"] = ("ИГРА ЗАПУЩЕНА", "GAME IS \nRUNNING"),
                ["STATUS_READY"] = ("Статус: Готов", "Status: Ready"),
                ["STATUS_IN_GAME"] = ("Статус: В игре", "Status: In game"),
                ["PREPARING"] = ("Подготовка...", "Preparing..."),
                ["LAUNCH_OK"] = ("Запуск выполнен!", "Game started!"),
                ["PATH_ERROR"] = ("Ошибка: проверьте путь к игре!", "Error: check game path!"),
                ["GAME_NOT_FOUND"] = ("Игра не найдена. Укажите папку с игрой.", "Game not found. Select game folder."),
                ["SELECT_FOLDER"] = ("Выберите папку с Stardew Valley.exe", "Select folder with Stardew Valley.exe"),
                ["CHECK_UPDATES"] = ("ПРОВЕРИТЬ ОБНОВЛЕНИЯ", "CHECK UPDATES"),
                ["UPDATED"] = ("ОБНОВЛЕНО", "UPDATED"),
                ["DOWNLOADING"] = ("Скачивание...", "Downloading..."),
                ["EXTRACTING"] = ("Распаковка...", "Extracting..."),
                ["CANCEL"] = ("ОТМЕНА", "CANCEL"),
                ["CONFIG_SET"] = ("Введите ссылку на config.xml", "Enter config.xml URL"),
                ["SHORTCUT_ASK"] = ("Добавить ярлык на рабочий стол?", "Create desktop shortcut?"),
                ["DONE"] = ("Готово! Запускайте лаунчер из папки игры.", "Done! Run launcher from game folder."),
                ["LAUNCH_MODE"] = ("РЕЖИМ ЗАПУСКА", "LAUNCH MODE"),
                ["VANILLA"] = ("Оригинальная версия", "Vanilla version"),
                ["MODS"] = ("SMAPI (с модами)", "SMAPI (with mods)"),
                ["LATEST_VERSION"] = ("У вас последняя версия лаунчера.", "You have the latest version of the launcher."),
                ["GAME_NOT_FOUND"] = ("Stardew Valley.exe не найден.", "Stardew Valley.exe not found."),
                ["DOWNLOAD_UPD"] = ("Загрузка обновления лаунчера...", "Loading launcher update..."),
                ["CHECK_SHA256"] = ("Проверка целостности...", "Checking file integrity..."),
                ["FILE_CORRUPTED"] = ("Ошибка проверки файла!\nХэш не совпадает.\nФайл повреждён или подменён.", "File verification error!\nThe hash does not match.\nThe file is damaged or substituted."),
                ["ERR_UPD_LAUNCHER"] = ("Ошибка обновления лаунчера:\n", "Launcher update error:\n"),
                ["UPD_ERR"] = ("Ошибка обновления:\n", "Update error:\n"),
                ["EXE_NOT_FOUND"] = ("Файл не найден: ", "File not found: "),
                ["SMAPI_NOT_FOUND"] = ("SMAPI не установлен.", "SMAPI is not installed."),
                ["CANCELED"] = ("Отменено", "Canceled"),
                ["DEV_CODE_REQ"] = ("Введите секретный код:", "Enter secret code:"),
                ["DEV_TITLE"] = ("Вход в режим разработчика", "Enter to developer mode"),
                ["WRONG_CODE"] = ("Неверный код доступа", "Wrong access code"),
                ["LANG"] = ("RU", "ENG")
            };

        private string T(string key)
        {
            return currentLanguage == Language.Russian ? texts[key].ru : texts[key].en;
        }

        private void ApplyLanguage()
        {
            btnLaunch.Text = T("PLAY");
            btnUpdate.Text = isUpdated ? T("UPDATED") : T("CHECK_UPDATES");
            btnCancel.Text = T("CANCEL");
            btnLang.Text = T("LANG");
            lblStatus.Text = T("STATUS_READY");
            lblTitle.Text = "STARDEW HUB";
            groupOptions.Text = currentLanguage == Language.Russian
                ? " РЕЖИМ ЗАПУСКА "
                : " LAUNCH MODE ";
            rbVanilla.Text = currentLanguage == Language.Russian
                ? "Оригинальная версия"
                : "Vanilla version";
            rbMods.Text = currentLanguage == Language.Russian
                ? "SMAPI (с модами)"
                : "SMAPI (with mods)";
            lblStatus.ForeColor = isGameRunning
                ? Color.SpringGreen
                : Color.White;
        }

        [DllImport("user32.dll")]
        private static extern void ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern void SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        [DllImport("gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect,
            int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;

        private enum DwmBackdropType
        {
            Auto = 0,
            None = 1,
            Mica = 2,
            Acrylic = 3,
            Tabbed = 4
        }

        [DllImport("kernel32.dll")]
        private static extern bool SetProcessWorkingSetSize(IntPtr proc, int min, int max);

        public void OptimizeMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 16, 16));

        }

        public Form1()
        {
            InitializeComponent();
            EnableMica();
            EnableGlass();
            this.Text = "Stardew Hub";
            Bitmap bmp = Properties.Resources.icon;
            this.Icon = Icon.FromHandle(bmp.GetHicon());
            this.BackColor = Color.Black;
            this.TransparencyKey = Color.Black;
            this.Opacity = 0.97;
            currentLauncherVersion = GetCurrentLauncherVersion();
            processCheckTimer.Interval = 5000;
            LoadSettings();
            CheckGamePath();
            ApplyLanguage();
            menuAnimTimer.Interval = 15;
            menuAnimTimer.Tick += MenuAnimTimer_Tick;
            SetDeveloperMode(true);
            menuEndX = btnLang.Left;
            menuStartX = topPanel.Width;
            HideMenuButtonsInstant();
            SetStyle(ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw, true);
            this.btnToggleAnim.Click += new System.EventHandler(this.btnToggleAnim_Click);
            this.btnResize.Click += new System.EventHandler(this.btnResize_Click);
            if (isLargeMode)
            {
                uiScale = 1.5f;
                float factor = 1.5f;
                this.Size = new Size((int)(this.Width * factor), (int)(this.Height * factor));
                ScaleUI(this, factor);
                this.StartPosition = FormStartPosition.CenterScreen;
            }
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
              ControlStyles.UserPaint |
              ControlStyles.DoubleBuffer |
              ControlStyles.OptimizedDoubleBuffer, true);
            uiTimer.Interval = 33;
            uiTimer.Start();
            UpdateStyles();
        }

        private void UpdateBrush()
        {
            backgroundBrush?.Dispose();
            backgroundBrush = new LinearGradientBrush(this.ClientRectangle, colorStart, colorEnd, 45f);
        }

        private void SaveSettings()
        {
            var lines = new List<string>
            {
                "Language=" + currentLanguage,
                "GamePath=" + gamePath,
                "ConfigUrl=" + configUrl,
                "LargeMode=" + isLargeMode
            };
            System.IO.File.WriteAllLines(settingsFile, lines);
        }

        private void LoadSettings()
        {
            if (!System.IO.File.Exists(settingsFile))
                return;
            var lines = System.IO.File.ReadAllLines(settingsFile);
            foreach (var line in lines)
            {
                if (line.StartsWith("Language="))
                {
                    string lang = line.Substring("Language=".Length);
                    if (Enum.TryParse(lang, out Language savedLang))
                        currentLanguage = savedLang;
                }
                else if (line.StartsWith("GamePath="))
                {
                    string path = line.Substring("GamePath=".Length);
                    if (!string.IsNullOrWhiteSpace(path) &&
                        System.IO.File.Exists(Path.Combine(path, "Stardew Valley.exe")))
                    {
                        gamePath = path;
                    }
                }
                else if (line.StartsWith("ConfigUrl="))
                {
                    string url = line.Substring("ConfigUrl=".Length);
                    if (!string.IsNullOrWhiteSpace(url))
                        configUrl = url;
                }
                else if (line.StartsWith("LargeMode="))
                {
                    string val = line.Substring("LargeMode=".Length);
                    if (bool.TryParse(val, out bool savedLargeMode))
                    {
                        isLargeMode = savedLargeMode;
                    }
                }
            }
        }

        private void CheckGamePath()
        {
            if (!string.IsNullOrEmpty(gamePath) &&
                System.IO.File.Exists(Path.Combine(gamePath, "Stardew Valley.exe")))
                return;
            string? found = FindStardewInSteam();
            if (!string.IsNullOrEmpty(found))
            {
                gamePath = found;
                SaveSettings();
                return;
            }
            if (System.IO.File.Exists(Path.Combine(defaultGamePath, "Stardew Valley.exe")))
            {
                gamePath = defaultGamePath;
                SaveSettings();
                return;
            }
            MessageBox.Show(T("GAME_NOT_FOUND"));
            using FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = T("SELECT_FOLDER");
            if (fbd.ShowDialog() != DialogResult.OK)
            {
                Application.Exit();
                return;
            }
            if (!System.IO.File.Exists(Path.Combine(fbd.SelectedPath, "Stardew Valley.exe")))
            {
                MessageBox.Show(T("GAME_NOT_FOUND"));
                Application.Exit();
                return;
            }
            gamePath = fbd.SelectedPath;
            SaveSettings();
        }

        private string? FindStardewInSteam()
        {
            try
            {
                string? steamPath = Registry.GetValue(
                    @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam",
                    "InstallPath", null)?.ToString();
                if (steamPath == null)
                    return null;
                string vdfPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
                if (!System.IO.File.Exists(vdfPath))
                    return null;
                string text = System.IO.File.ReadAllText(vdfPath);
                var matches = Regex.Matches(text, "\"path\"\\s*\"([^\"]+)\"");
                foreach (Match m in matches)
                {
                    string lib = m.Groups[1].Value.Replace(@"\\", @"\");
                    string candidate = Path.Combine(lib, "steamapps", "common", "Stardew Valley");
                    if (System.IO.File.Exists(Path.Combine(candidate, "Stardew Valley.exe")))
                        return candidate;
                }
            }
            catch { }
            return null;
        }

        private async Task<(Version version, string url, string sha256)> GetLatestLauncherInfo()
        {
            using var stream = await httpClient.GetStreamAsync(launcherUpdateUrl);
            XDocument doc = XDocument.Load(stream);
            var latest = doc.Descendants("item")
                .Select(x => new
                {
                    Version = Version.Parse(x.Attribute("version")!.Value),
                    Url = x.Attribute("url")!.Value,
                    Sha256 = x.Attribute("sha256")!.Value
                })
                .OrderByDescending(x => x.Version)
                .First();
            return (latest.Version, latest.Url, latest.Sha256);
        }

        private void CopyLauncherFiles(string targetPath)
        {
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            string[] files =
            {
                "SVLauncher.exe",
                "SVLauncher.dll",
                "SVLauncher.pdb",
                "SVLauncher.deps.json",
                "SVLauncher.runtimeconfig.json"
            };

            foreach (var file in files)
            {
                string src = Path.Combine(exeDir, file);
                string dst = Path.Combine(targetPath, file);
                if (System.IO.File.Exists(src))
                    System.IO.File.Copy(src, dst, true);
            }
        }

        private void CreateDesktopShortcut(string exePath)
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string shortcutPath = Path.Combine(desktop, "StardewHub.lnk");
            var shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = exePath;
            shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
            shortcut.Save();
        }
        private void btnLang_Click(object sender, EventArgs e)
        {
            currentLanguage = currentLanguage == Language.Russian
                ? Language.English
                : Language.Russian;
            SaveSettings();
            ApplyLanguage();
        }

        private void btnConfig_Click(object sender, EventArgs e)
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                T("CONFIG_SET"), "Config", configUrl);
            if (!string.IsNullOrWhiteSpace(input))
            {
                configUrl = input;
                SaveSettings();
            }
        }

        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                downloadCts = new CancellationTokenSource();
                btnCancel.Visible = true;
                btnCancel.Enabled = true;
                btnUpdate.Enabled = false;
                using var stream = await httpClient.GetStreamAsync(configUrl);
                XDocument doc = XDocument.Load(stream);
                var lastItem = doc.Descendants("item").Last();
                string url = lastItem.Attribute("Url")!.Value;
                string tempZip = Path.Combine(Path.GetTempPath(), "update.zip");
                lblStatus.Text = T("DOWNLOADING");
                await DownloadFileWithProgress(url, tempZip, downloadCts.Token);
                lblStatus.Text = T("EXTRACTING");
                ZipFile.ExtractToDirectory(tempZip, AppDomain.CurrentDomain.BaseDirectory, true);
                System.IO.File.Delete(tempZip);
                lblStatus.Text = T("UPDATED");
                isUpdated = true;
                btnCancel.Visible = false;
                SaveSettings();
            }
            catch (OperationCanceledException)
            {
                btnCancel.Visible = false;
                btnUpdate.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(T("UPD_ERR" + ex.Message));
                btnCancel.Visible = false;
                btnUpdate.Enabled = true;
            }
        }

        private async Task DownloadFileWithProgress(string url, string destinationPath, CancellationToken token)
        {
            using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();
            long totalBytes = response.Content.Headers.ContentLength ?? -1L;
            long receivedBytes = 0;
            using var contentStream = await response.Content.ReadAsStreamAsync(token);
            using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
            byte[] buffer = new byte[4096];
            int bytesRead;
            Stopwatch sw = Stopwatch.StartNew();
            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, token);
                receivedBytes += bytesRead;
                if (totalBytes > 0)
                {
                    int percent = (int)(receivedBytes * 100 / totalBytes);
                    progressFill.Width = (int)(progressBg.Width * (percent / 100f));
                    double speedMb = (receivedBytes / 1024d / 1024d) / sw.Elapsed.TotalSeconds;
                    double receivedKb = receivedBytes / 1024d;
                    double totalKb = totalBytes / 1024d;
                    lblStatus.Text = $"{receivedKb:F0}KB / {totalKb:F0}KB ({percent}%) - {speedMb:F2} MB/s";
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            downloadCts?.Cancel();
            lblStatus.Text = T("CANCELED");
            btnCancel.Visible = false;
            btnCancel.Enabled = false;
            btnUpdate.Enabled = true;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void processCheckTimer_Tick(object sender, EventArgs e)
        {
            bool running =
                Process.GetProcessesByName("Stardew Valley").Length > 0 ||
                Process.GetProcessesByName("StardewModdingAPI").Length > 0;
            if (running != isGameRunning)
            {
                isGameRunning = running;
                UpdateUIStatus(running);
            }
        }

        private void UpdateUIStatus(bool running)
        {
            btnLaunch.Enabled = !running;
            rbVanilla.Enabled = !running;
            rbMods.Enabled = !running;
            btnUpdate.Enabled = !running && !isUpdated;
            if (running)
            {
                btnLaunch.Text = T("GAME_RUNNING");
                lblStatus.Text = T("STATUS_IN_GAME");
                lblStatus.ForeColor = Color.SpringGreen;
                progressFill.Width = progressBg.Width;
            }
            else
            {
                btnLaunch.Text = T("PLAY");
                lblStatus.Text = T("STATUS_READY");
                lblStatus.ForeColor = Color.White;
                progressFill.Width = 0;
            }
            if (isUpdated)
            {
                btnUpdate.Text = T("UPDATED");
                btnUpdate.BackColor = Color.FromArgb(60, 120, 60);
            }
        }

        private void FinalizeLaunch()
        {
            if (gamePath == null) return;
            string exe = rbMods.Checked ? "StardewModdingAPI.exe" : "Stardew Valley.exe";
            string fullPath = Path.Combine(gamePath, exe);
            try
            {
                Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
                lblStatus.Text = T("LAUNCH_OK");
            }
            catch
            {
                MessageBox.Show(T("PATH_ERROR"));
                UpdateUIStatus(false);
            }
        }

        private void launchTimer_Tick(object sender, EventArgs e)
        {
            if (progressValue < 100)
            {
                progressValue += 5;
                progressFill.Width = (int)((float)progressBg.Width * (progressValue / 100f));
                lblStatus.Text = $"{T("PREPARING")} {progressValue}%";
            }
            else
            {
                launchTimer.Stop();
                FinalizeLaunch();
            }
        }

        private void btnLaunch_Click(object sender, EventArgs e)
        {
            if (gamePath == null) return;
            if (rbMods.Checked &&
                !System.IO.File.Exists(Path.Combine(gamePath, "StardewModdingAPI.exe")))
            {
                MessageBox.Show(T("SMAPI_NOT_FOUND"));
                return;
            }
            string exe = rbMods.Checked
                ? "StardewModdingAPI.exe"
                : "Stardew Valley.exe";
            string fullPath = Path.Combine(gamePath, exe);
            if (!System.IO.File.Exists(fullPath))
            {
                MessageBox.Show(T("EXE_NOT_FOUND" + exe));
                return;
            }
            Process.Start(new ProcessStartInfo(fullPath)
            {
                UseShellExecute = true,
                WorkingDirectory = gamePath
            });
        }

        private void topPanel_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void btnChangePath_Click(object sender, EventArgs e)
        {
            using FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = T("SELECT_FOLDER");

            if (fbd.ShowDialog() != DialogResult.OK)
                return;
            if (!System.IO.File.Exists(Path.Combine(fbd.SelectedPath, "Stardew Valley.exe")))
            {
                MessageBox.Show(T("GAME_NOT_FOUND"));
                return;
            }
            gamePath = fbd.SelectedPath;
            SaveSettings();
            MessageBox.Show(T("DONE"));
        }

        private string GetFileSha256(string filePath)
        {
            using var sha = SHA256.Create();
            using var stream = System.IO.File.OpenRead(filePath);
            byte[] hash = sha.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private async void btnUpdateLauncher_Click(object sender, EventArgs e)
        {
            try
            {
                using var stream = await httpClient.GetStreamAsync(launcherUpdateUrl);
                XDocument doc = XDocument.Load(stream);
                var latest = doc.Descendants("item")
                    .Select(x => new
                    {
                        Version = Version.Parse(x.Attribute("version")!.Value),
                        Url = x.Attribute("url")!.Value,
                        Sha256 = x.Attribute("sha256")!.Value.ToLowerInvariant()
                    })
                    .OrderByDescending(x => x.Version)
                    .First();
                Version current = Version.Parse(currentLauncherVersion);
                if (latest.Version <= current)
                {
                    MessageBox.Show(T("LATEST_VERSION"));
                    return;
                }
                string setupPath = Path.Combine(Path.GetTempPath(), "SVHubSetup.exe");
                lblStatus.Text = T("DOWNLOAD_UPD");
                await DownloadFileWithProgress(latest.Url, setupPath, CancellationToken.None);
                lblStatus.Text = T("CHECK_SHA256");
                string fileHash = GetFileSha256(setupPath);
                if (fileHash != latest.Sha256)
                {
                    MessageBox.Show(T("FILE_CORRUPTED"));
                    System.IO.File.Delete(setupPath);
                    return;
                }
                Process.Start(new ProcessStartInfo(setupPath)
                {
                    UseShellExecute = true
                });
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(T("ERR_UPD_LAUNCHER") + ex.Message);
            }
        }

        private void EnableMica()
        {
            if (Environment.OSVersion.Version.Build >= 22000)
            {
                int backdrop = (int)DwmBackdropType.Mica;
                DwmSetWindowAttribute(this.Handle, DWMWA_SYSTEMBACKDROP_TYPE, ref backdrop, sizeof(int));
            }
        }

        private void EnableGlass()
        {
            if (Environment.OSVersion.Version.Build >= 22000)
            {
                int backdrop = (int)DwmBackdropType.Mica;
                DwmSetWindowAttribute(this.Handle, DWMWA_SYSTEMBACKDROP_TYPE, ref backdrop, sizeof(int));
            }
            this.BackColor = Color.FromArgb(245, 245, 245);
        }

        private void lblStatus_Click(object sender, EventArgs e) { }

        private void rbMods_CheckedChanged(object sender, EventArgs e)
        {

        }

        private string GetCurrentLauncherVersion()
        {
            return Assembly.GetExecutingAssembly()
                           .GetName()
                           .Version!
                           .ToString();
        }

        private void ToggleMenu()
        {
            menuExpanded = !menuExpanded;
            menuAnimTimer.Start();
        }

        private void HideMenuButtonsInstant()
        {
            btnLang.Left = menuStartX;
            btnConfig.Left = menuStartX + 45;
            btnDevMode.Left = menuStartX + 45;
            btnUpdateLauncher.Left = menuStartX + 90;
            btnChangePath.Left = menuStartX + 135;
        }

        private void btnMenu_Click(object sender, EventArgs e)
        {
            if (!menuExpanded)
            {
                btnLang.Visible = true;
                btnUpdateLauncher.Visible = true;
                btnChangePath.Visible = true;
                if (isDeveloperMode)
                    btnConfig.Visible = true;
                else
                    btnDevMode.Visible = true;
                menuExpanded = true;
                menuAnimTimer.Start();
            }
            else
            {
                menuExpanded = false;
                menuAnimTimer.Start();
            }
        }

        private void MenuAnimTimer_Tick(object? sender, EventArgs e)
        {
            int speed = (int)(25 * uiScale);
            int gap = (int)(5 * uiScale);
            var buttons = new List<Button>();
            buttons.Add(btnLang);
            if (btnConfig.Visible)
                buttons.Add(btnConfig);
            else if (btnDevMode.Visible || !menuExpanded)
                buttons.Add(btnDevMode);
            buttons.Add(btnUpdateLauncher);
            buttons.Add(btnChangePath);
            if (menuExpanded)
            {
                bool allReached = true;
                int currentRightLimit = btnMenu.Left - gap;
                foreach (var btn in buttons)
                {
                    int targetX = currentRightLimit - btn.Width;
                    if (btn.Left > targetX)
                    {
                        btn.Left -= speed;
                        if (btn.Left < targetX) btn.Left = targetX;

                        allReached = false;
                    }
                    else
                    {
                        btn.Left = targetX;
                    }
                    currentRightLimit = btn.Left - gap;
                }
                if (allReached) menuAnimTimer.Stop();
            }
            else
            {
                bool allClosed = true;
                int targetX = btnMenu.Left;
                foreach (var btn in buttons)
                {
                    if (btn.Left < targetX)
                    {
                        btn.Left += speed;
                        if (btn.Left > targetX) btn.Left = targetX;
                        allClosed = false;
                    }
                }
                if (allClosed)
                {
                    menuAnimTimer.Stop();
                    foreach (var btn in buttons) btn.Visible = false;
                }
            }
        }

        private void SetDeveloperMode(bool active)
        {
            isDeveloperMode = active;
            if (active)
            {
                btnConfig.Location = btnDevMode.Location;
            }
            else
            {
                btnDevMode.Location = btnConfig.Location;
            }

            btnConfig.Visible = active;
            btnUpdate.Visible = active;
            btnDevMode.Visible = !active;

            UpdateLaunchButtonSize();
        }

        private void UpdateLaunchButtonSize()
        {
            int baseWidth = isDeveloperMode ? 160 : 400;
            btnLaunch.Width = (int)(baseWidth * uiScale);
            btnLaunch.Height = (int)(40 * uiScale);
        }

        private void btnDevMode_Click(object sender, EventArgs e)
        {
            string password = Microsoft.VisualBasic.Interaction.InputBox(
                T("DEV_CODE_REQ"),
                T("DEV_TITLE"),
                ""
            );

            if (password == "ChronosDev")
            {
                SetDeveloperMode(true);
            }
            else if (!string.IsNullOrEmpty(password))
            {
                MessageBox.Show(T("WRONG_CODE"));
            }
        }

        private void btnToggleAnim_Click(object? sender, EventArgs e)
        {
            isAnimationsEnabled = !isAnimationsEnabled;
            if (isAnimationsEnabled)
            {
                uiTimer.Start();
                btnToggleAnim.Text = "✨";
            }
            else
            {
                uiTimer.Stop();
                btnToggleAnim.Text = "🌑";
                this.Opacity = 1.0;
                btnLaunch.BackColor = Color.FromArgb(0, 120, 215);
            }
        }

        private void btnResize_Click(object? sender, EventArgs e)
        {
            float factor = isLargeMode ? 0.6667f : 1.5f;
            uiScale = isLargeMode ? 1.0f : 1.5f;
            isLargeMode = !isLargeMode;

            this.Size = new Size((int)(this.Width * factor), (int)(this.Height * factor));

            ScaleUI(this, factor);

            topPanel.Height = 45;

            UpdateLaunchButtonSize();

            this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 16, 16));

            this.CenterToScreen();

            if (menuExpanded)
            {
                menuExpanded = false;
                HideMenuButtonsInstant();
            }

            SaveSettings();
        }

        private void ScaleUI(Control parent, float factor)
        {
            foreach (Control c in parent.Controls)
            {
                bool isHeaderElement = (c == topPanel || c.Parent == topPanel);

                c.Left = (int)(c.Left * factor);
                c.Width = (int)(c.Width * factor);

                if (!isHeaderElement)
                {
                    c.Top = (int)(c.Top * factor);
                    c.Height = (int)(c.Height * factor);
                }
                float fontFactor = isHeaderElement ? 1.0f : factor;
                c.Font = new Font(c.Font.FontFamily, c.Font.Size * fontFactor, c.Font.Style);

                if (c.Controls.Count > 0)
                {
                    ScaleUI(c, factor);
                }
            }
        }
    }
}
