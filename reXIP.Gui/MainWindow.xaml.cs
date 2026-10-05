using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace ReXipGui;

public partial class MainWindow : Window
{
    private readonly CliRunner _cli = new();
    private readonly ObservableCollection<string> _maps = new();
    private CancellationTokenSource? _cts;
    private readonly string _verText;

    public MainWindow()
    {
        InitializeComponent();

        var ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        _verText = ver is null ? "?" : $"{ver.Major}.{ver.Minor}.{ver.Build}";

        LstMaps.ItemsSource = _maps;

        // Remember the last used paths. On first run the working folder defaults to
        // Documents\reXIP-Gui (not bin\, which is wiped on rebuild and would take keyFiles with it).
        var saved = LoadSettings();
        Loc.SetLanguage(saved?.Language ?? Loc.English);
        CmbLang.SelectedIndex = Loc.Lang == Loc.ChineseSimplified ? 1 : 0;

        var guess = Path.Combine(AppContext.BaseDirectory, "reXIP.exe");
        TxtExe.Text = !string.IsNullOrWhiteSpace(saved?.ExePath) ? saved!.ExePath! : (File.Exists(guess) ? guess : "");
        TxtWork.Text = !string.IsNullOrWhiteSpace(saved?.WorkDir)
            ? saved!.WorkDir!
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "reXIP-Gui");
        TxtGame.Text = saved?.GameDir ?? "";
        TxtExtractDir.Text = Path.Combine(TxtWork.Text, "patch");

        ApplyLanguage();                       // also refreshes the key status line
        CmbLang.SelectionChanged += CmbLang_SelectionChanged;   // hooked after init on purpose

        Log($"reXIP GUI v{_verText}");
        RefreshPaks();
        RefreshProcesses();
    }

    // ───────────────────────── Localization ─────────────────────────

    /// <summary>Assigns every user-visible string for the current language.</summary>
    private void ApplyLanguage()
    {
        Title = Loc.T("app.title", _verText);

        GrpSettings.Header = Loc.T("grp.settings");
        LblExe.Text = Loc.T("lbl.exe");
        LblWork.Text = Loc.T("lbl.work");
        LblGame.Text = Loc.T("lbl.game");
        LblLang.Text = Loc.T("lbl.lang");

        var browse = Loc.T("btn.browse");
        foreach (var b in new[] { BtnExe, BtnWork, BtnGame, BtnPakBrowse, BtnExtractDir, BtnPatchDir, BtnCrcIn })
            b.Content = browse;

        TabKeys.Header = Loc.T("tab.keys");
        TabBrowse.Header = Loc.T("tab.browse");
        TabPack.Header = Loc.T("tab.pack");
        TabCrc.Header = Loc.T("tab.crc");

        // keys tab
        TxtKeysIntro.Text = Loc.T("keys.intro");
        LblProc.Text = Loc.T("lbl.process");
        BtnRefreshProc.Content = Loc.T("btn.refreshProc");
        BtnKeys.Content = Loc.T("btn.keys");
        BtnUseKeys.Content = Loc.T("btn.useKeys");

        // browse tab
        LblPak.Text = Loc.T("lbl.pak");
        BtnPakRefresh.Content = Loc.T("btn.refresh");
        LblFilter.Text = Loc.T("lbl.filter");
        BtnList.Content = Loc.T("btn.list");
        BtnVerify.Content = Loc.T("btn.verify");
        LblOutTo.Text = Loc.T("lbl.outTo");
        BtnExtractAll.Content = Loc.T("btn.extractAll");
        BtnExtractSel.Content = Loc.T("btn.extractSel");

        // pack tab
        TxtPackIntro.Text = Loc.T("pack.intro");
        LblPatchDir.Text = Loc.T("lbl.patchDir");
        GrpMap.Header = Loc.T("grp.map");
        LblInner.Text = Loc.T("lbl.inner");
        BtnLocal.Content = Loc.T("btn.pickFile");
        BtnAddMap.Content = Loc.T("btn.add");
        BtnDelMap.Content = Loc.T("btn.removeSel");
        LblOutName.Text = Loc.T("lbl.outName");
        BtnAutoName.Content = Loc.T("btn.autoName");
        TxtNameHint.Text = Loc.T("pack.nameHint");
        ChkInstall.Content = Loc.T("chk.install");
        BtnCreate.Content = Loc.T("btn.build");

        // CRC tab
        TxtCrcWarn.Text = Loc.T("crc.warn");
        LblCrcIn.Text = Loc.T("lbl.crcIn");
        LblCrcOut.Text = Loc.T("lbl.crcOut");
        BtnCrc.Content = Loc.T("btn.crc");

        // log
        LblLog.Text = Loc.T("lbl.log");
        BtnClearLog.Content = Loc.T("btn.clear");
        BtnCancel.Content = Loc.T("btn.cancel");

        RefreshKeyStatus();
    }

    private void CmbLang_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CmbLang.SelectedItem is ComboBoxItem { Tag: string code })
        {
            Loc.SetLanguage(code);
            ApplyLanguage();
            SaveSettings();
        }
    }

    // ───────────────────────── Settings persistence ─────────────────────────

    private sealed record AppSettings(string? ExePath, string? WorkDir, string? GameDir, string? Language = null);

    private static string SettingsPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "reXIP.Gui", "settings.json");

    private static AppSettings? LoadSettings()
    {
        try
        {
            return File.Exists(SettingsPath)
                ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(SettingsPath))
                : null;
        }
        catch { return null; }
    }

    private void SaveSettings()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(
                new AppSettings(TxtExe.Text.Trim(), TxtWork.Text.Trim(), TxtGame.Text.Trim(), Loc.Lang)));
        }
        catch { /* failing to save must not get in the way */ }
    }

    protected override void OnClosed(EventArgs e)
    {
        SaveSettings();
        base.OnClosed(e);
    }

    // ───────────────────────── Common ─────────────────────────

    private void Log(string s) => Dispatcher.BeginInvoke(() =>
    {
        TxtLog.AppendText(s + Environment.NewLine);
        TxtLog.ScrollToEnd();
    });

    private void SetBusy(bool busy)
    {
        MainTabs.IsEnabled = !busy;
        BtnCancel.IsEnabled = busy;
    }

    private void Warn(string msg) =>
        MessageBox.Show(this, msg, "reXIP GUI", MessageBoxButton.OK, MessageBoxImage.Information);

    private static string Quote(string a) => a.Contains(' ') ? $"\"{a}\"" : a;

    private static string? PickFile(Window owner, string filter)
    {
        var d = new OpenFileDialog { Filter = filter };
        return d.ShowDialog(owner) == true ? d.FileName : null;
    }

    private static string? PickFolder(Window owner)
    {
        var d = new OpenFolderDialog();
        return d.ShowDialog(owner) == true ? d.FolderName : null;
    }

    private bool PrepareCli()
    {
        var exe = TxtExe.Text.Trim();
        if (!File.Exists(exe)) { Warn(Loc.T("msg.needExe")); return false; }

        var work = TxtWork.Text.Trim();
        if (work.Length == 0) { Warn(Loc.T("msg.needWork")); return false; }
        Directory.CreateDirectory(work);

        var game = TxtGame.Text.Trim();
        _cli.ExePath = exe;
        _cli.WorkDir = work;
        _cli.GameDir = Directory.Exists(game) ? game : null;
        SaveSettings();
        return true;
    }

    /// <summary>Runs one reXIP command; returns null if it was cancelled or failed to start.</summary>
    private async Task<(int Code, List<string> Lines)?> RunAsync(params string[] args)
    {
        if (!PrepareCli()) return null;

        _cts = new CancellationTokenSource();
        SetBusy(true);
        Log("> reXIP " + string.Join(' ', args.Select(Quote)));
        try
        {
            var r = await _cli.RunAsync(args, Log, _cts.Token);
            Log(Loc.T("log.exit", r.ExitCode));
            return (r.ExitCode, r.Lines);
        }
        catch (OperationCanceledException) { Log(Loc.T("log.cancelled")); return null; }
        catch (Exception ex) { Log(Loc.T("log.error", ex.Message)); return null; }
        finally
        {
            SetBusy(false);
            _cts?.Dispose();
            _cts = null;
        }
    }

    // ───────────────────────── Settings panel ─────────────────────────

    private void BtnExe_Click(object sender, RoutedEventArgs e)
    {
        if (PickFile(this, Loc.T("filter.exe")) is { } f) { TxtExe.Text = f; RefreshKeyStatus(); }
    }

    private void BtnWork_Click(object sender, RoutedEventArgs e)
    {
        if (PickFolder(this) is { } d) { TxtWork.Text = d; RefreshKeyStatus(); }
    }

    private void BtnGame_Click(object sender, RoutedEventArgs e)
    {
        if (PickFolder(this) is { } d) { TxtGame.Text = d; RefreshPaks(); }
    }

    // ───────────────────────── 1. Keys ─────────────────────────

    private void RefreshProcesses()
    {
        var procs = Process.GetProcesses();
        var names = procs.Select(p => p.ProcessName)
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                         .ToList();
        foreach (var p in procs) p.Dispose();

        CmbProc.ItemsSource = names;
        // Prefer the game itself ("DJMax", i.e. DJMax.exe), then anything containing "djmax"
        // that is not a launcher.
        CmbProc.SelectedItem =
            names.FirstOrDefault(n => n.Equals("DJMax", StringComparison.OrdinalIgnoreCase))
            ?? names.FirstOrDefault(n => n.Contains("djmax", StringComparison.OrdinalIgnoreCase)
                                         && !n.Contains("launcher", StringComparison.OrdinalIgnoreCase));
    }

    private void RefreshKeyStatus()
    {
        var dirs = new List<string>();
        if (TxtWork.Text.Trim().Length > 0) dirs.Add(Path.Combine(TxtWork.Text.Trim(), "keyFiles"));
        if (Path.GetDirectoryName(TxtExe.Text.Trim()) is { Length: > 0 } exeDir) dirs.Add(Path.Combine(exeDir, "keyFiles"));

        var found = dirs.FirstOrDefault(d => Directory.Exists(d) && Directory.EnumerateFileSystemEntries(d).Any());
        TxtKeyStatus.Text = found != null
            ? Loc.T("key.found", found)
            : Loc.T("key.missing");
    }

    private void BtnRefreshProc_Click(object sender, RoutedEventArgs e) => RefreshProcesses();

    private async void BtnKeys_Click(object sender, RoutedEventArgs e)
    {
        if (CmbProc.SelectedItem is not string proc) { Warn(Loc.T("msg.needProc")); return; }

        if (proc.Contains("launcher", StringComparison.OrdinalIgnoreCase)
            && MessageBox.Show(this, Loc.T("msg.launcherBody", proc),
                   Loc.T("msg.launcherTitle"), MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;

        var r1 = await RunAsync("dump", proc, "dump.bin");
        if (r1 is not { Code: 0 }) return;

        await RunAsync("keys", "dump.bin");
        RefreshKeyStatus();
    }

    /// <summary>
    /// For when the keys were already dumped: pick an existing keyFiles folder and copy its files
    /// (top level only) into the working folder's keyFiles subfolder, so reXIP finds them as if just dumped
    /// (reXIP only looks for keyFiles\ next to itself and in the current directory).
    /// </summary>
    private void BtnUseKeys_Click(object sender, RoutedEventArgs e)
    {
        var work = TxtWork.Text.Trim();
        if (work.Length == 0) { Warn(Loc.T("msg.needWork")); return; }

        if (PickFolder(this) is not { } src) return;

        var dest = Path.Combine(work, "keyFiles");
        static string Norm(string p) => Path.TrimEndingDirectorySeparator(Path.GetFullPath(p));
        var srcN = Norm(src);
        var destN = Norm(dest);
        const StringComparison cmp = StringComparison.OrdinalIgnoreCase;

        if (srcN.Equals(destN, cmp))
        {
            Log(Loc.T("keys.alreadyUsing"));
            RefreshKeyStatus();
            return;
        }

        // Picking the working folder itself (or a parent) would put the destination inside the
        // chosen folder, i.e. copy a whole directory tree into itself.
        if (destN.StartsWith(srcN + Path.DirectorySeparatorChar, cmp))
        {
            Warn(Loc.T("keys.containsWork"));
            return;
        }

        var files = Directory.GetFiles(src);   // top level only, no recursion
        if (files.Length == 0)
        {
            Warn(Loc.T("keys.noFiles"));
            return;
        }
        if (files.Any(f => Path.GetExtension(f).ToLowerInvariant() is ".exe" or ".dll" or ".pdb" or ".pak"))
        {
            Warn(Loc.T("keys.looksWrong"));
            return;
        }

        var names = string.Join("\n", files.Take(10).Select(f => "  " + Path.GetFileName(f)));
        if (files.Length > 10) names += "\n" + Loc.T("keys.more", files.Length);
        var overwrite = Directory.Exists(dest) && Directory.EnumerateFileSystemEntries(dest).Any();
        var msg = Loc.T("keys.confirmBody", files.Length, dest, names)
                + (overwrite ? Loc.T("keys.overwrite") : "")
                + Loc.T("msg.continue");
        if (MessageBox.Show(this, msg, Loc.T("keys.importTitle"), MessageBoxButton.YesNo, MessageBoxImage.Question)
            != MessageBoxResult.Yes) return;

        try
        {
            Directory.CreateDirectory(dest);
            foreach (var f in files)
                File.Copy(f, Path.Combine(dest, Path.GetFileName(f)), overwrite: true);
            Log(Loc.T("keys.imported", files.Length, src, dest));
        }
        catch (Exception ex)
        {
            Log(Loc.T("keys.importFailed", ex.Message));
        }
        RefreshKeyStatus();
    }

    // ───────────────────────── 2. Browse / extract ─────────────────────────

    private void RefreshPaks()
    {
        var dir = TxtGame.Text.Trim();
        if (!Directory.Exists(dir)) return;

        var files = Directory.GetFiles(dir, "system*.pak")
                             .Select(f => Path.GetFileName(f))
                             .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                             .ToList();
        CmbPak.ItemsSource = files;
        if (files.Count > 0 && string.IsNullOrWhiteSpace(CmbPak.Text)) CmbPak.SelectedIndex = 0;
    }

    private string ResolvePak()
    {
        var t = CmbPak.Text.Trim();
        if (t.Length == 0) return "";
        if (File.Exists(t)) return t;

        var g = TxtGame.Text.Trim();
        if (Directory.Exists(g))
        {
            var p = Path.Combine(g, t);
            if (File.Exists(p)) return p;
        }
        return t;
    }

    private void BtnPakRefresh_Click(object sender, RoutedEventArgs e) => RefreshPaks();

    private void BtnPakBrowse_Click(object sender, RoutedEventArgs e)
    {
        if (PickFile(this, Loc.T("filter.pak")) is { } f) CmbPak.Text = f;
    }

    private void BtnExtractDir_Click(object sender, RoutedEventArgs e)
    {
        if (PickFolder(this) is { } d) TxtExtractDir.Text = d;
    }

    private async void BtnList_Click(object sender, RoutedEventArgs e)
    {
        var pak = ResolvePak();
        if (pak.Length == 0) { Warn(Loc.T("msg.needPak")); return; }

        var args = new List<string> { "list", pak };
        if (TxtFilter.Text.Trim().Length > 0) args.Add(TxtFilter.Text.Trim());

        var r = await RunAsync(args.ToArray());
        if (r is null) return;
        LstEntries.ItemsSource = r.Value.Lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
    }

    private async void BtnVerify_Click(object sender, RoutedEventArgs e)
    {
        var pak = ResolvePak();
        if (pak.Length == 0) { Warn(Loc.T("msg.needPak")); return; }
        await RunAsync("verify", pak);
    }

    private async void BtnExtractAll_Click(object sender, RoutedEventArgs e)
    {
        var pak = ResolvePak();
        var outDir = TxtExtractDir.Text.Trim();
        if (pak.Length == 0 || outDir.Length == 0) { Warn(Loc.T("msg.needPakOut")); return; }

        Directory.CreateDirectory(outDir);
        await RunAsync("extract", pak, "*", outDir);
    }

    private async void BtnExtractSel_Click(object sender, RoutedEventArgs e)
    {
        var pak = ResolvePak();
        var outDir = TxtExtractDir.Text.Trim();
        if (pak.Length == 0 || outDir.Length == 0) { Warn(Loc.T("msg.needPakOut")); return; }

        var paths = LstEntries.SelectedItems.Cast<string>()
                              .Select(EntryParser.TryGetPath)
                              .OfType<string>()
                              .Distinct()
                              .ToList();
        if (paths.Count == 0) { Warn(Loc.T("msg.needSel")); return; }

        foreach (var p in paths)
        {
            // Single-file extract: output path = output folder + in-archive relative path
            // (keeps the directory structure).
            var dest = Path.Combine(outDir, p.TrimStart('\\', '/'));
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);

            var r = await RunAsync("extract", pak, p, dest);
            if (r is not { Code: 0 }) return;
        }
        Log(Loc.T("log.extracted", paths.Count, outDir));
    }

    // ───────────────────────── 3. Build patch ─────────────────────────

    /// <summary>Finds the highest system_NNNN.pak number in a folder and returns the next file name.</summary>
    private static string NextPakName(string dir)
    {
        var max = 0;
        foreach (var f in Directory.GetFiles(dir, "system*.pak"))
        {
            var m = Regex.Match(Path.GetFileNameWithoutExtension(f), @"^system_(\d+)$", RegexOptions.IgnoreCase);
            if (m.Success) max = Math.Max(max, int.Parse(m.Groups[1].Value));
        }
        return $"system_{max + 1:D4}.pak";
    }

    private void BtnPatchDir_Click(object sender, RoutedEventArgs e)
    {
        if (PickFolder(this) is { } d) TxtPatchDir.Text = d;
    }

    private void BtnLocal_Click(object sender, RoutedEventArgs e)
    {
        if (PickFile(this, Loc.T("filter.all")) is { } f) TxtLocal.Text = f;
    }

    private void BtnAddMap_Click(object sender, RoutedEventArgs e)
    {
        var inner = TxtInner.Text.Trim();
        var local = TxtLocal.Text.Trim();
        if (inner.Length == 0 || !File.Exists(local)) { Warn(Loc.T("msg.needMap")); return; }

        _maps.Add($"{inner}={local}");
        TxtInner.Clear();
        TxtLocal.Clear();
    }

    private void BtnDelMap_Click(object sender, RoutedEventArgs e)
    {
        if (LstMaps.SelectedItem is string s) _maps.Remove(s);
    }

    private void BtnAutoName_Click(object sender, RoutedEventArgs e)
    {
        var g = TxtGame.Text.Trim();
        if (!Directory.Exists(g)) { Warn(Loc.T("msg.needGame")); return; }
        TxtOutName.Text = NextPakName(g);
    }

    private async void BtnCreate_Click(object sender, RoutedEventArgs e)
    {
        var name = TxtOutName.Text.Trim();
        if (name.Length == 0) { Warn(Loc.T("msg.needOutName")); return; }

        var sources = new List<string>();
        var patch = TxtPatchDir.Text.Trim();
        if (patch.Length > 0)
        {
            if (!Directory.Exists(patch)) { Warn(Loc.T("msg.patchMissing")); return; }
            sources.Add(Path.TrimEndingDirectorySeparator(patch));
        }
        sources.AddRange(_maps);
        if (sources.Count == 0) { Warn(Loc.T("msg.needSources")); return; }

        var buildDir = Path.Combine(TxtWork.Text.Trim(), "build");
        Directory.CreateDirectory(buildDir);
        var outPath = Path.Combine(buildDir, name);

        var args = new List<string> { "create", outPath };
        args.AddRange(sources);

        var r = await RunAsync(args.ToArray());
        if (r is not { Code: 0 }) return;

        Log(Loc.T("log.built", outPath));
        if (ChkInstall.IsChecked == true) InstallToGame(outPath);
    }

    private void InstallToGame(string pakPath)
    {
        var g = TxtGame.Text.Trim();
        if (!Directory.Exists(g)) { Log(Loc.T("install.skip")); return; }

        var fileName = Path.GetFileName(pakPath);
        var dest = Path.Combine(g, fileName);
        var exists = File.Exists(dest);
        var expected = NextPakName(g);

        var msg = exists ? Loc.T("install.exists", dest) : Loc.T("install.new", dest);
        if (!exists && !fileName.Equals(expected, StringComparison.OrdinalIgnoreCase))
            msg += Loc.T("install.note", expected);

        if (MessageBox.Show(this, msg, Loc.T("install.title"), MessageBoxButton.YesNo, MessageBoxImage.Question)
            != MessageBoxResult.Yes) return;

        try
        {
            if (exists)
            {
                var backupDir = Path.Combine(TxtWork.Text.Trim(), "backup");
                Directory.CreateDirectory(backupDir);
                var bak = Path.Combine(backupDir, $"{fileName}.{DateTime.Now:yyyyMMdd_HHmmss}.bak");
                File.Copy(dest, bak, overwrite: false);
                Log(Loc.T("log.backedUp", bak));
            }
            File.Copy(pakPath, dest, overwrite: true);
            Log(Loc.T("log.copied", dest));
        }
        catch (Exception ex)
        {
            Log(Loc.T("log.copyFailed", ex.Message));
        }
    }

    // ───────────────────────── 4. CRC ─────────────────────────

    private void BtnCrcIn_Click(object sender, RoutedEventArgs e)
    {
        if (PickFile(this, Loc.T("filter.pak")) is { } f) TxtCrcIn.Text = f;
    }

    private async void BtnCrc_Click(object sender, RoutedEventArgs e)
    {
        var input = TxtCrcIn.Text.Trim();
        var output = TxtCrcOut.Text.Trim();
        if (input.Length == 0 || output.Length == 0) { Warn(Loc.T("msg.needCrc")); return; }
        await RunAsync("crc", input, output);
    }

    // ───────────────────────── Log ─────────────────────────

    private void BtnClearLog_Click(object sender, RoutedEventArgs e) => TxtLog.Clear();
    private void BtnCancel_Click(object sender, RoutedEventArgs e) => _cts?.Cancel();
}
