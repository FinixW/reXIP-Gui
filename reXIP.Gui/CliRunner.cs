using System.Diagnostics;
using System.Text;

namespace ReXipGui;

/// <summary>
/// Launches reXIP.exe in the background and streams its output back.
/// This is the only point of contact between the GUI and the reXIP command line tool.
/// </summary>
public sealed class CliRunner
{
    public string ExePath { get; set; } = "";
    public string WorkDir { get; set; } = "";   // keyFiles\ and dump.bin are resolved relative to this
    public string? GameDir { get; set; }        // passed to reXIP through DJMAX_FILES

    public async Task<(int ExitCode, List<string> Lines)> RunAsync(
        IEnumerable<string> args, Action<string>? onLine = null, CancellationToken ct = default)
    {
        var psi = new ProcessStartInfo(ExePath)
        {
            WorkingDirectory = WorkDir,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };
        foreach (var a in args) psi.ArgumentList.Add(a);   // handles spaces / quotes automatically
        if (!string.IsNullOrWhiteSpace(GameDir)) psi.Environment["DJMAX_FILES"] = GameDir;

        var lines = new List<string>();
        void Handle(string? s)
        {
            if (s == null) return;
            lock (lines) lines.Add(s);
            onLine?.Invoke(s);
        }

        using var p = new Process { StartInfo = psi, EnableRaisingEvents = true };
        p.OutputDataReceived += (_, e) => Handle(e.Data);
        p.ErrorDataReceived += (_, e) => Handle(e.Data);
        p.Start();
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();

        using var reg = ct.Register(() =>
        {
            try { if (!p.HasExited) p.Kill(true); } catch { /* already exited */ }
        });

        await p.WaitForExitAsync(ct);
        return (p.ExitCode, lines);
    }
}
