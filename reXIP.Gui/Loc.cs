using System.Globalization;

namespace ReXipGui;

/// <summary>
/// Minimal UI localization: English ("en") and Simplified Chinese ("zh-CN").
/// Add a language by extending the tuple and the switch in <see cref="T"/>.
/// </summary>
internal static class Loc
{
    public const string English = "en";
    public const string ChineseSimplified = "zh-CN";

    public static string Lang { get; private set; } = English;

    public static void SetLanguage(string? code) =>
        Lang = code == ChineseSimplified ? ChineseSimplified : English;

    /// <summary>Chinese UI culture -> zh-CN, anything else -> English.</summary>
    public static string DetectSystemLanguage() =>
        CultureInfo.CurrentUICulture.Name.StartsWith("zh", StringComparison.OrdinalIgnoreCase)
            ? ChineseSimplified
            : English;

    /// <summary>Look up a string; optional args are applied with string.Format.</summary>
    public static string T(string key, params object[] args)
    {
        if (!Strings.TryGetValue(key, out var v)) return key;
        var s = Lang == ChineseSimplified ? v.Zh : v.En;
        return args.Length == 0 ? s : string.Format(s, args);
    }

    private static readonly Dictionary<string, (string En, string Zh)> Strings = new()
    {
        // ── window / settings ──
        ["app.title"] = ("reXIP GUI v{0} — DJMAX Online PAK tool", "reXIP GUI v{0} — DJMAX Online PAK 工具"),
        ["grp.settings"] = ("Settings", "基本设置"),
        ["lbl.exe"] = ("reXIP.exe location", "reXIP.exe 位置"),
        ["lbl.work"] = ("Working folder (keyFiles, dump.bin)", "工作目录（keyFiles、dump.bin）"),
        ["lbl.game"] = ("Game FILES folder (optional)", "游戏 FILES 目录（可选）"),
        ["lbl.lang"] = ("Language", "语言"),
        ["btn.browse"] = ("Browse…", "浏览…"),

        // ── tabs ──
        ["tab.keys"] = ("1. Get keys", "1. 获取密钥"),
        ["tab.browse"] = ("2. Browse / Extract", "2. 浏览 / 提取"),
        ["tab.pack"] = ("3. Build patch", "3. 打包补丁"),
        ["tab.crc"] = ("Advanced: Rebuild CRC", "高级：重建 CRC"),

        // ── keys tab ──
        ["keys.intro"] = (
            "Start DJMAX first (no login needed; leaving it at the \"cannot connect to server\" pop-up is fine), then select the game process below. The PAK files are encrypted and the keys only exist in the running process.",
            "先启动 DJMAX（不用登录，停在“无法连接服务器”的提示窗口即可），然后在下面选中游戏进程。PAK 文件是加密的，密钥只存在于运行中的进程里。"),
        ["lbl.process"] = ("Process", "进程"),
        ["btn.refreshProc"] = ("Refresh processes", "刷新进程"),
        ["btn.keys"] = ("Get keys (dump + keys)", "一键获取密钥（dump + keys）"),
        ["btn.useKeys"] = ("Use an existing keyFiles folder…", "使用已有的密钥文件夹…"),
        ["key.found"] = ("✔ Keys found: {0}", "✔ 已找到密钥：{0}"),
        ["key.missing"] = ("✘ keyFiles\\ not found yet — get the keys first.", "✘ 还没有找到 keyFiles\\，需要先获取密钥。"),
        ["msg.needProc"] = (
            "Please select the DJMAX process first (the game must be running).",
            "请先选中 DJMAX 的进程（需要游戏正在运行）。"),
        ["msg.launcherBody"] = (
            "\"{0}\" looks like the launcher, not the game itself.\nThe keys live in the game process DJMax; dumping the launcher will not give correct keys.\n\nContinue anyway?",
            "选中的“{0}”看起来是启动器，不是游戏本体。\n密钥在游戏进程 DJMax 里，dump 启动器得不到正确的密钥。\n\n仍然继续吗？"),
        ["msg.launcherTitle"] = ("Wrong process?", "进程可能选错了"),
        ["keys.alreadyUsing"] = ("Already using that keyFiles folder.", "已经在使用该密钥文件夹。"),
        ["keys.containsWork"] = (
            "The folder you chose contains the working folder, so it is not the keyFiles folder itself.\n\nPlease choose the keyFiles folder that directly holds the key files.",
            "你选的文件夹包含了工作目录，不是 keyFiles 文件夹本身。\n\n请选择里面直接放着密钥文件的那个 keyFiles 文件夹。"),
        ["keys.noFiles"] = (
            "There are no files at the top level of this folder.\nPlease choose the keyFiles folder that directly holds the key files.",
            "这个文件夹这一层里没有文件。\n请选择直接放着密钥文件的 keyFiles 文件夹。"),
        ["keys.looksWrong"] = (
            "This folder contains exe / dll / pak files and does not look like a key folder; cancelled.\nPlease choose the keyFiles folder itself.",
            "这个文件夹里有 exe / dll / pak 文件，看起来不是密钥文件夹，已取消。\n请选择 keyFiles 文件夹本身。"),
        ["keys.more"] = ("  … {0} files in total", "  … 共 {0} 个"),
        ["keys.confirmBody"] = (
            "These {0} files will be copied to:\n{1}\n\n{2}\n\n",
            "将把这 {0} 个文件复制到：\n{1}\n\n{2}\n\n"),
        ["keys.overwrite"] = (
            "keyFiles already exists at the destination; files with the same name will be overwritten.\n\n",
            "目标里已有 keyFiles，同名文件会被覆盖。\n\n"),
        ["msg.continue"] = ("Continue?", "继续吗？"),
        ["keys.importTitle"] = ("Import keys", "导入密钥"),
        ["keys.imported"] = ("Imported {0} key files: {1} → {2}", "已导入 {0} 个密钥文件：{1} → {2}"),
        ["keys.importFailed"] = ("[import failed] {0}", "[导入失败] {0}"),

        // ── browse tab ──
        ["lbl.pak"] = ("PAK", "PAK"),
        ["btn.refresh"] = ("Refresh", "刷新"),
        ["lbl.filter"] = ("Filter", "过滤"),
        ["btn.list"] = ("List", "列出"),
        ["btn.verify"] = ("Verify", "校验"),
        ["lbl.outTo"] = ("Output to", "输出到"),
        ["btn.extractAll"] = ("Extract all", "全部提取"),
        ["btn.extractSel"] = ("Extract selected", "提取选中"),
        ["msg.needPak"] = ("Please choose a PAK file first.", "请先选择一个 PAK 文件。"),
        ["msg.needPakOut"] = ("Please choose a PAK file and an output folder first.", "请先选择 PAK 和输出目录。"),
        ["msg.needSel"] = (
            "Click \"List\" first, then select the entries you want to extract.",
            "请先点“列出”，再在列表里选中要提取的条目。"),
        ["log.extracted"] = ("Extracted {0} files to {1}", "已提取 {0} 个文件到 {1}"),

        // ── pack tab ──
        ["pack.intro"] = (
            "Put only the files you changed in the patch folder, keeping their relative paths (e.g. System\\shop\\ItemStock.csv). The new PAK overrides entries with the same name; system.pak itself is not touched.",
            "补丁文件夹里只放你改过的文件（保持相对路径，如 System\\shop\\ItemStock.csv）。新 PAK 会覆盖同名条目，不需要改动 system.pak。"),
        ["lbl.patchDir"] = ("Patch folder", "补丁文件夹"),
        ["grp.map"] = ("Single-file mappings (path in archive = local file)", "单个文件映射（归档内路径 = 本地文件）"),
        ["lbl.inner"] = ("Path in archive", "归档内路径"),
        ["btn.pickFile"] = ("Choose file…", "选文件…"),
        ["btn.add"] = ("Add", "添加"),
        ["btn.removeSel"] = ("Remove selected", "移除选中"),
        ["lbl.outName"] = ("Output file name", "输出文件名"),
        ["btn.autoName"] = ("Use next number", "自动取下一个编号"),
        ["pack.nameHint"] = (
            "The file name must be the highest existing number in the game folder + 1. For example, if system_0005.pak exists, name it system_0006.pak.",
            "文件名必须是游戏目录里现有最大编号 +1，例如已有 system_0005.pak，就要叫 system_0006.pak。"),
        ["chk.install"] = (
            "After a successful build, copy to the game folder (an existing file with the same name is backed up to <working folder>\\backup first)",
            "打包成功后复制到游戏目录（同名文件会先备份到 工作目录\\backup）"),
        ["btn.build"] = ("Build", "开始打包"),
        ["msg.needMap"] = (
            "Enter a path inside the archive and choose an existing local file.",
            "请填写归档内路径，并选择一个存在的本地文件。"),
        ["msg.needGame"] = (
            "Please set the game FILES folder in Settings first.",
            "请先在“基本设置”里指定游戏 FILES 目录。"),
        ["msg.needOutName"] = ("Please enter an output file name.", "请填写输出文件名。"),
        ["msg.patchMissing"] = ("The patch folder does not exist.", "补丁文件夹不存在。"),
        ["msg.needSources"] = (
            "Specify a patch folder, or add at least one single-file mapping.",
            "请指定补丁文件夹，或至少添加一条单文件映射。"),
        ["log.built"] = ("Created: {0}", "已生成：{0}"),
        ["install.skip"] = ("No game folder set; skipping copy.", "未设置游戏目录，跳过复制。"),
        ["install.exists"] = (
            "Target already exists:\n{0}\n\nThe original will be backed up and then overwritten. Continue?",
            "目标已存在：\n{0}\n\n将先备份原文件再覆盖，继续吗？"),
        ["install.new"] = ("Will be copied to:\n{0}\n\nContinue?", "将复制到：\n{0}\n\n继续吗？"),
        ["install.note"] = (
            "\n\nNote: the game loads archives by number; the next one should be {0}. The current file name may not be loaded by the game.",
            "\n\n注意：游戏目录里按编号，下一个应该是 {0}，当前文件名可能不会被游戏加载。"),
        ["install.title"] = ("Copy to game folder", "复制到游戏目录"),
        ["log.backedUp"] = ("Backed up original: {0}", "已备份原文件：{0}"),
        ["log.copied"] = ("Copied to game folder: {0}", "已复制到游戏目录：{0}"),
        ["log.copyFailed"] = ("[copy failed] {0}", "[复制失败] {0}"),

        // ── CRC tab ──
        ["crc.warn"] = (
            "Only needed if you modify original archives such as system.pak directly. Adding a new system_000N.pak does not require touching crc.pak. Back up first.",
            "只有当你直接修改了原版 system.pak 等文件时才需要。仅新增 system_000N.pak 不用碰 crc.pak。请务必先备份。"),
        ["lbl.crcIn"] = ("crc.pak (input)", "crc.pak（输入）"),
        ["lbl.crcOut"] = ("Output pak", "输出 pak"),
        ["btn.crc"] = ("Rebuild system.crc", "重建 system.crc"),
        ["msg.needCrc"] = ("Please fill in the input and output files.", "请填写输入和输出文件。"),

        // ── log / common ──
        ["lbl.log"] = ("Log", "日志"),
        ["btn.clear"] = ("Clear", "清空"),
        ["btn.cancel"] = ("Cancel current operation", "取消当前操作"),
        ["msg.needExe"] = (
            "Please set the location of reXIP.exe in Settings first.",
            "请先在“基本设置”里指定 reXIP.exe 的位置。"),
        ["msg.needWork"] = ("Please set a working folder first.", "请先指定工作目录。"),
        ["log.exit"] = ("[exit code {0}]", "[退出码 {0}]"),
        ["log.cancelled"] = ("[cancelled]", "[已取消]"),
        ["log.error"] = ("[error] {0}", "[错误] {0}"),

        // ── file dialog filters ──
        ["filter.exe"] = ("reXIP|reXIP.exe|Executable|*.exe", "reXIP|reXIP.exe|可执行文件|*.exe"),
        ["filter.pak"] = ("PAK archive|*.pak|All files|*.*", "PAK 归档|*.pak|所有文件|*.*"),
        ["filter.all"] = ("All files|*.*", "所有文件|*.*"),
    };
}
