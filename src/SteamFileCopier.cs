using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;

namespace SteamFileCopier
{
    public class MainForm : Form
    {
        private TextBox txtSource, txtLua, txtManifest;
        private CheckBox chkSkip, chkRec;
        private ProgressBar progressBar;
        private RichTextBox logBox;
        private Button btnCopy;

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Steam File Copier";
            this.Size = new Size(695, 560);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10);
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            try { this.Icon = Icon.ExtractAssociatedIcon(Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\imageres.dll"); } catch { }

            var header = new Label
            {
                Text = "Копирование .lua и .manifest файлов в папки Steam",
                Location = new Point(15, 10),
                Size = new Size(650, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };

            // Source
            var lblSrc = CreateLabel("Исходная папка:", 15, 45);
            txtSource = CreateTextBox("D:\\кряки стим", 135, 45);
            var btnSrc = CreateBrowseButton(540, 44, () => BrowseFolder(txtSource));

            // Lua
            var lblLua = CreateLabel("Папка для .lua:", 15, 85);
            txtLua = CreateTextBox(@"C:\Program Files (x86)\Steam\config\stplug-in", 135, 85);
            var btnLua = CreateBrowseButton(540, 84, () => BrowseFolder(txtLua));

            // Manifest
            var lblMan = CreateLabel("Папка для .manifest:", 15, 125);
            txtManifest = CreateTextBox(@"C:\Program Files (x86)\Steam\depotcache", 135, 125);
            var btnMan = CreateBrowseButton(540, 124, () => BrowseFolder(txtManifest));

            // Checkboxes
            chkSkip = new CheckBox
            {
                Text = "Пропускать дубликаты (не перезаписывать)",
                Location = new Point(15, 165),
                Size = new Size(350, 25),
                Checked = true
            };
            chkRec = new CheckBox
            {
                Text = "Искать рекурсивно (во всех подпапках)",
                Location = new Point(15, 190),
                Size = new Size(350, 25),
                Checked = true
            };

            // Copy button
            btnCopy = new Button
            {
                Text = "Начать копирование",
                Location = new Point(170, 225),
                Size = new Size(250, 40),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCopy.Click += BtnCopy_Click;

            // Progress bar
            progressBar = new ProgressBar
            {
                Location = new Point(15, 275),
                Size = new Size(645, 25),
                Style = ProgressBarStyle.Continuous
            };

            // Log
            logBox = new RichTextBox
            {
                Location = new Point(15, 310),
                Size = new Size(645, 195),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.Lime,
                Font = new Font("Consolas", 9),
                ReadOnly = true,
                WordWrap = false
            };

            this.Controls.AddRange(new Control[] {
                header,
                lblSrc, txtSource, btnSrc,
                lblLua, txtLua, btnLua,
                lblMan, txtManifest, btnMan,
                chkSkip, chkRec,
                btnCopy,
                progressBar,
                logBox
            });
        }

        private Label CreateLabel(string text, int x, int y)
        {
            return new Label { Text = text, Location = new Point(x, y), Size = new Size(120, 25) };
        }

        private TextBox CreateTextBox(string text, int x, int y)
        {
            return new TextBox { Text = text, Location = new Point(x, y), Size = new Size(400, 25) };
        }

        private Button CreateBrowseButton(int x, int y, Action action)
        {
            var btn = new Button
            {
                Text = "Обзор...",
                Location = new Point(x, y),
                Size = new Size(120, 28)
            };
            btn.Click += (s, e) => action();
            return btn;
        }

        private void BrowseFolder(TextBox target)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.SelectedPath = target.Text;
                if (dialog.ShowDialog() == DialogResult.OK)
                    target.Text = dialog.SelectedPath;
            }
        }

        private void BtnCopy_Click(object sender, EventArgs e)
        {
            var srcPath = txtSource.Text.Trim();
            var luaDest = txtLua.Text.Trim();
            var manifestDest = txtManifest.Text.Trim();
            var skipDups = chkSkip.Checked;
            var recursive = chkRec.Checked;

            if (string.IsNullOrEmpty(srcPath) || !Directory.Exists(srcPath))
            {
                MessageBox.Show("Укажите существующую исходную папку.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnCopy.Enabled = false;
            btnCopy.Text = "Копирую...";
            progressBar.Value = 0;
            logBox.Clear();

            try
            {
                if (!Directory.Exists(luaDest))
                    Directory.CreateDirectory(luaDest);
            }
            catch
            {
                MessageBox.Show("Не удалось создать папку:\n" + luaDest +
                    "\n\nЗапустите программу от имени администратора.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnCopy.Enabled = true;
                btnCopy.Text = "Начать копирование";
                return;
            }

            try
            {
                if (!Directory.Exists(manifestDest))
                    Directory.CreateDirectory(manifestDest);
            }
            catch
            {
                MessageBox.Show("Не удалось создать папку:\n" + manifestDest +
                    "\n\nЗапустите программу от имени администратора.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnCopy.Enabled = true;
                btnCopy.Text = "Начать копирование";
                return;
            }

            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            var luaFiles = Directory.GetFiles(srcPath, "*.lua", searchOption);
            var manifestFiles = Directory.GetFiles(srcPath, "*.manifest", searchOption);

            var total = luaFiles.Length + manifestFiles.Length;
            var copied = 0;
            var skipped = 0;
            var current = 0;

            Log("=== Steam File Copier ===");
            Log("Время: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            Log("Откуда: " + srcPath);
            Log("Куда .lua: " + luaDest);
            Log("Куда .manifest: " + manifestDest);
            Log("Найдено: " + luaFiles.Length + " .lua, " + manifestFiles.Length + " .manifest");
            Log("Всего: " + total + " файлов");
            Log("================================");
            Log("");

            if (total == 0)
            {
                Log("Файлы .lua и .manifest не найдены.");
                Log("Проверьте путь: " + srcPath);
                btnCopy.Enabled = true;
                btnCopy.Text = "Начать копирование";
                return;
            }

            // Copy .lua
            if (luaFiles.Length > 0)
            {
                Log("--- .lua файлы ---");
                foreach (var file in luaFiles)
                {
                    current++;
                    var fileName = Path.GetFileName(file);
                    var destPath = Path.Combine(luaDest, fileName);

                    if (skipDups && File.Exists(destPath))
                    {
                        Log("  [" + current + "/" + total + "] " + fileName + " - уже есть, пропущен");
                        skipped++;
                    }
                    else
                    {
                        try
                        {
                            File.Copy(file, destPath, true);
                            Log("  [" + current + "/" + total + "] " + fileName);
                            copied++;
                        }
                        catch (Exception ex)
                        {
                            Log("  [" + current + "/" + total + "] " + fileName + " - ОШИБКА: " + ex.Message);
                            skipped++;
                        }
                    }
                    progressBar.Value = Math.Min((int)((double)current / total * 100), 99);
                    Application.DoEvents();
                }
            }

            // Copy .manifest
            if (manifestFiles.Length > 0)
            {
                Log("");
                Log("--- .manifest файлы ---");
                foreach (var file in manifestFiles)
                {
                    current++;
                    var fileName = Path.GetFileName(file);
                    var destPath = Path.Combine(manifestDest, fileName);

                    if (skipDups && File.Exists(destPath))
                    {
                        Log("  [" + current + "/" + total + "] " + fileName + " - уже есть, пропущен");
                        skipped++;
                    }
                    else
                    {
                        try
                        {
                            File.Copy(file, destPath, true);
                            Log("  [" + current + "/" + total + "] " + fileName);
                            copied++;
                        }
                        catch (Exception ex)
                        {
                            Log("  [" + current + "/" + total + "] " + fileName + " - ОШИБКА: " + ex.Message);
                            skipped++;
                        }
                    }
                    progressBar.Value = Math.Min((int)((double)current / total * 100), 99);
                    Application.DoEvents();
                }
            }

            progressBar.Value = 100;

            Log("");
            Log("================================");
            Log("ГОТОВО!");
            Log("Скопировано: " + copied + "  |  Пропущено: " + skipped);

            if (copied > 0)
            {
                MessageBox.Show("Готово!\nСкопировано: " + copied + "\nПропущено: " + skipped,
                    "Steam File Copier", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            btnCopy.Enabled = true;
            btnCopy.Text = "Начать копирование";
        }

        private void Log(string text)
        {
            logBox.AppendText(text + "\n");
            logBox.SelectionStart = logBox.TextLength;
            logBox.ScrollToCaret();
        }
    }

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
}
