using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace GoDingtalkUi
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    public sealed class MainForm : Form
    {
        private readonly string appDir;
        private readonly string downloaderPath;
        private readonly string defaultSaveDir;

        private TextBox urlBox;
        private TextBox saveBox;
        private NumericUpDown threadBox;
        private TextBox logBox;
        private Label statusLabel;
        private ProgressBar progressBar;
        private Button downloadButton;
        private Button cancelButton;
        private Process currentProcess;

        public MainForm()
        {
            appDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            downloaderPath = Path.Combine(appDir, "GoDingtalk.exe");
            defaultSaveDir = Path.Combine(appDir, "video");

            if (!Directory.Exists(defaultSaveDir))
            {
                Directory.CreateDirectory(defaultSaveDir);
            }

            BuildUi();
        }

        private void BuildUi()
        {
            Text = "GoDingtalk 直播下载";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(820, 560);
            Size = new Size(920, 640);
            Font = new Font("Microsoft YaHei UI", 9F);

            var main = new TableLayoutPanel();
            main.Dock = DockStyle.Fill;
            main.Padding = new Padding(16);
            main.ColumnCount = 1;
            main.RowCount = 6;
            main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            main.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Controls.Add(main);

            var title = new Label();
            title.Text = "钉钉直播回放下载";
            title.Font = new Font("Microsoft YaHei UI", 16F, FontStyle.Bold);
            title.AutoSize = true;
            main.Controls.Add(title, 0, 0);

            var urlLabel = new Label();
            urlLabel.Text = "直播链接";
            urlLabel.AutoSize = true;
            urlLabel.Margin = new Padding(0, 14, 0, 4);
            main.Controls.Add(urlLabel, 0, 1);

            urlBox = new TextBox();
            urlBox.Dock = DockStyle.Top;
            urlBox.Height = 28;
            main.Controls.Add(urlBox, 0, 2);

            var savePanel = new TableLayoutPanel();
            savePanel.Dock = DockStyle.Top;
            savePanel.ColumnCount = 3;
            savePanel.RowCount = 2;
            savePanel.Margin = new Padding(0, 12, 0, 8);
            savePanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            savePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            savePanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var saveLabel = new Label();
            saveLabel.Text = "保存目录";
            saveLabel.AutoSize = true;
            saveLabel.Anchor = AnchorStyles.Left;
            saveLabel.Margin = new Padding(0, 0, 12, 0);
            savePanel.Controls.Add(saveLabel, 0, 0);

            saveBox = new TextBox();
            saveBox.Text = defaultSaveDir;
            saveBox.Dock = DockStyle.Fill;
            savePanel.Controls.Add(saveBox, 1, 0);

            var browseButton = new Button();
            browseButton.Text = "选择...";
            browseButton.Width = 92;
            browseButton.Height = 30;
            browseButton.Click += BrowseButton_Click;
            savePanel.Controls.Add(browseButton, 2, 0);

            var threadLabel = new Label();
            threadLabel.Text = "线程数";
            threadLabel.AutoSize = true;
            threadLabel.Anchor = AnchorStyles.Left;
            threadLabel.Margin = new Padding(0, 10, 12, 0);
            savePanel.Controls.Add(threadLabel, 0, 1);

            threadBox = new NumericUpDown();
            threadBox.Minimum = 1;
            threadBox.Maximum = 32;
            threadBox.Value = 10;
            threadBox.Width = 80;
            threadBox.Margin = new Padding(0, 8, 0, 0);
            savePanel.Controls.Add(threadBox, 1, 1);

            main.Controls.Add(savePanel, 0, 3);

            logBox = new TextBox();
            logBox.Dock = DockStyle.Fill;
            logBox.Multiline = true;
            logBox.ReadOnly = true;
            logBox.ScrollBars = ScrollBars.Vertical;
            logBox.BackColor = Color.FromArgb(250, 250, 250);
            logBox.Font = new Font("Consolas", 9F);
            main.Controls.Add(logBox, 0, 4);

            var bottom = new TableLayoutPanel();
            bottom.Dock = DockStyle.Bottom;
            bottom.ColumnCount = 6;
            bottom.RowCount = 1;
            bottom.Margin = new Padding(0, 12, 0, 0);
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            statusLabel = new Label();
            statusLabel.Text = "就绪";
            statusLabel.AutoSize = true;
            statusLabel.Anchor = AnchorStyles.Left;
            bottom.Controls.Add(statusLabel, 0, 0);

            progressBar = new ProgressBar();
            progressBar.Width = 180;
            progressBar.Height = 22;
            progressBar.Style = ProgressBarStyle.Blocks;
            progressBar.Margin = new Padding(0, 0, 12, 0);
            bottom.Controls.Add(progressBar, 1, 0);

            var openFolderButton = new Button();
            openFolderButton.Text = "打开目录";
            openFolderButton.Width = 92;
            openFolderButton.Height = 32;
            openFolderButton.Margin = new Padding(0, 0, 8, 0);
            openFolderButton.Click += OpenFolderButton_Click;
            bottom.Controls.Add(openFolderButton, 2, 0);

            var loginButton = new Button();
            loginButton.Text = "登录/刷新";
            loginButton.Width = 92;
            loginButton.Height = 32;
            loginButton.Margin = new Padding(0, 0, 8, 0);
            loginButton.Click += LoginButton_Click;
            bottom.Controls.Add(loginButton, 3, 0);

            cancelButton = new Button();
            cancelButton.Text = "取消";
            cancelButton.Width = 86;
            cancelButton.Height = 32;
            cancelButton.Enabled = false;
            cancelButton.Margin = new Padding(0, 0, 8, 0);
            cancelButton.Click += CancelButton_Click;
            bottom.Controls.Add(cancelButton, 4, 0);

            downloadButton = new Button();
            downloadButton.Text = "开始下载";
            downloadButton.Width = 110;
            downloadButton.Height = 32;
            downloadButton.Click += DownloadButton_Click;
            bottom.Controls.Add(downloadButton, 5, 0);

            main.Controls.Add(bottom, 0, 5);
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "选择视频保存目录";
                dialog.SelectedPath = saveBox.Text;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    saveBox.Text = dialog.SelectedPath;
                }
            }
        }

        private void OpenFolderButton_Click(object sender, EventArgs e)
        {
            var dir = saveBox.Text.Trim();
            if (dir.Length == 0)
            {
                dir = defaultSaveDir;
            }

            Directory.CreateDirectory(dir);
            Process.Start("explorer.exe", QuoteForExplorer(dir));
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            if (!File.Exists(downloaderPath))
            {
                MessageBox.Show(this, "未找到 GoDingtalk.exe，请把界面程序放在 GoDingtalk.exe 同一目录。", "找不到程序", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            logBox.Clear();
            AppendLog("正在启动登录流程...");

            var psi = new ProcessStartInfo();
            psi.FileName = downloaderPath;
            psi.WorkingDirectory = appDir;
            psi.Arguments = "-login";
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = true;
            psi.StandardOutputEncoding = Encoding.UTF8;
            psi.StandardErrorEncoding = Encoding.UTF8;

            var ffmpegBin = FindFFmpegBin();
            if (ffmpegBin != null)
            {
                psi.EnvironmentVariables["PATH"] = ffmpegBin + ";" + psi.EnvironmentVariables["PATH"];
            }

            var process = new Process();
            process.StartInfo = psi;
            process.EnableRaisingEvents = true;
            process.OutputDataReceived += delegate(object s, DataReceivedEventArgs args)
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    AppendLog(args.Data);
                }
            };
            process.ErrorDataReceived += delegate(object s, DataReceivedEventArgs args)
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    AppendLog(args.Data);
                }
            };
            process.Exited += delegate
            {
                var exitCode = process.ExitCode;
                BeginInvoke((Action)delegate
                {
                    if (exitCode == 0)
                    {
                        statusLabel.Text = "登录完成";
                        AppendLog("登录流程完成。");
                    }
                    else
                    {
                        statusLabel.Text = "登录失败，退出码 " + exitCode.ToString();
                        AppendLog("登录失败，退出码 " + exitCode.ToString() + "。");
                    }

                    process.Dispose();
                });
            };

            try
            {
                statusLabel.Text = "正在登录...";
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                statusLabel.Text = "登录启动失败";
                MessageBox.Show(this, ex.Message, "启动失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DownloadButton_Click(object sender, EventArgs e)
        {
            var url = urlBox.Text.Trim();
            var saveDir = saveBox.Text.Trim();

            if (url.Length == 0)
            {
                MessageBox.Show(this, "请先输入钉钉直播链接。", "缺少链接", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!File.Exists(downloaderPath))
            {
                MessageBox.Show(this, "未找到 GoDingtalk.exe，请把界面程序放在 GoDingtalk.exe 同一目录。", "找不到程序", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var ffmpegBin = FindFFmpegBin();
            if (ffmpegBin == null)
            {
                MessageBox.Show(this, "未找到 ffmpeg.exe，请先安装 FFmpeg，或放到当前目录的 tools 文件夹。", "缺少 FFmpeg", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (saveDir.Length == 0)
            {
                MessageBox.Show(this, "请选择保存目录。", "缺少保存目录", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Directory.CreateDirectory(saveDir);
            logBox.Clear();
            AppendLog("启动下载...");
            AppendLog("保存目录: " + saveDir);

            var psi = new ProcessStartInfo();
            psi.FileName = downloaderPath;
            psi.WorkingDirectory = appDir;
            psi.Arguments = "-url " + QuoteArgument(url) + " -saveDir " + QuoteArgument(saveDir) + " -thread " + ((int)threadBox.Value).ToString();
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = true;
            psi.StandardOutputEncoding = Encoding.UTF8;
            psi.StandardErrorEncoding = Encoding.UTF8;
            psi.EnvironmentVariables["PATH"] = ffmpegBin + ";" + psi.EnvironmentVariables["PATH"];

            var process = new Process();
            process.StartInfo = psi;
            process.EnableRaisingEvents = true;
            process.OutputDataReceived += delegate(object s, DataReceivedEventArgs args)
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    AppendLog(args.Data);
                }
            };
            process.ErrorDataReceived += delegate(object s, DataReceivedEventArgs args)
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    AppendLog(args.Data);
                }
            };
            process.Exited += delegate
            {
                var exitCode = process.ExitCode;
                BeginInvoke((Action)delegate
                {
                    SetRunning(false);
                    if (exitCode == 0)
                    {
                        statusLabel.Text = "下载完成";
                        AppendLog("所有任务完成。");
                    }
                    else
                    {
                        statusLabel.Text = "下载失败，退出码 " + exitCode.ToString();
                        AppendLog("下载失败，退出码 " + exitCode.ToString() + "。");
                    }

                    process.Dispose();
                    currentProcess = null;
                });
            };

            try
            {
                SetRunning(true);
                currentProcess = process;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                SetRunning(false);
                currentProcess = null;
                MessageBox.Show(this, ex.Message, "启动失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (currentProcess != null && !currentProcess.HasExited)
            {
                currentProcess.Kill();
                AppendLog("已取消下载。");
                statusLabel.Text = "已取消";
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (currentProcess != null && !currentProcess.HasExited)
            {
                var result = MessageBox.Show(this, "下载仍在进行，确定要退出吗？", "确认退出", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }

                currentProcess.Kill();
            }

            base.OnFormClosing(e);
        }

        private void SetRunning(bool running)
        {
            downloadButton.Enabled = !running;
            cancelButton.Enabled = running;
            progressBar.Style = running ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks;
            if (!running)
            {
                progressBar.Value = 0;
            }
            else
            {
                statusLabel.Text = "正在下载...";
            }
        }

        private void AppendLog(string message)
        {
            if (logBox.InvokeRequired)
            {
                logBox.BeginInvoke((Action)delegate { AppendLog(message); });
                return;
            }

            logBox.AppendText(message + Environment.NewLine);
            logBox.SelectionStart = logBox.TextLength;
            logBox.ScrollToCaret();
        }

        private string FindFFmpegBin()
        {
            var toolsDir = Path.Combine(appDir, "tools");
            if (Directory.Exists(toolsDir))
            {
                var files = Directory.GetFiles(toolsDir, "ffmpeg.exe", SearchOption.AllDirectories);
                if (files.Length > 0)
                {
                    return Path.GetDirectoryName(files[0]);
                }
            }

            var path = Environment.GetEnvironmentVariable("PATH") ?? "";
            foreach (var entry in path.Split(Path.PathSeparator))
            {
                try
                {
                    if (entry.Length == 0)
                    {
                        continue;
                    }

                    var candidate = Path.Combine(entry.Trim('"'), "ffmpeg.exe");
                    if (File.Exists(candidate))
                    {
                        return entry.Trim('"');
                    }
                }
                catch
                {
                }
            }

            return null;
        }

        private static string QuoteArgument(string value)
        {
            return "\"" + value.Replace("\"", "\\\"") + "\"";
        }

        private static string QuoteForExplorer(string value)
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}
