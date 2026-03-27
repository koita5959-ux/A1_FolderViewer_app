using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using DesktopKit.Common;

namespace DesktopKit.FolderViewer
{
    /// <summary>
    /// FolderViewer（フォルダ構造ビューア）のメインフォーム。
    /// </summary>
    public class MainForm : BaseForm
    {
        private Button btnSelectFolder = null!;
        private TextBox txtFolderPath = null!;
        private Label lblDepth = null!;
        private NumericUpDown nudDepth = null!;
        private TreeView tvFolderTree = null!;
        private Button btnExport = null!;
        private ContextMenuStrip contextMenu = null!;

        private string _currentRootPath = "";
        private static readonly string SettingsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DesktopKit");
        private static readonly string SettingsFile = Path.Combine(SettingsDir, "FolderViewer.json");

        public MainForm()
        {
            ComponentName = "FolderViewer";
            InitializeControls();
        }

        private void InitializeControls()
        {
            // --- 上部パネル: フォルダ選択 + 階層の深さ ---
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                Padding = new Padding(10, 10, 10, 5)
            };

            btnSelectFolder = new Button
            {
                Text = "フォルダを選択",
                Location = new Point(10, 8),
                Size = new Size(120, 28)
            };
            btnSelectFolder.Click += BtnSelectFolder_Click;

            txtFolderPath = new TextBox
            {
                ReadOnly = true,
                Location = new Point(140, 10),
                Size = new Size(620, 23),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            lblDepth = new Label
            {
                Text = "階層の深さ:",
                Location = new Point(10, 42),
                AutoSize = true
            };

            nudDepth = new NumericUpDown
            {
                Value = 5,
                Minimum = 1,
                Maximum = 20,
                Location = new Point(110, 40),
                Size = new Size(60, 23)
            };
            nudDepth.ValueChanged += NudDepth_ValueChanged;

            topPanel.Controls.AddRange(new Control[] { btnSelectFolder, txtFolderPath, lblDepth, nudDepth });

            // --- 中央: TreeView ---
            tvFolderTree = new TreeView
            {
                Dock = DockStyle.Fill
            };

            // コンテキストメニュー
            contextMenu = new ContextMenuStrip();
            var menuItem = new ToolStripMenuItem("ここを起点にする");
            menuItem.Click += MenuSetRoot_Click;
            contextMenu.Items.Add(menuItem);
            tvFolderTree.MouseDown += TvFolderTree_MouseDown;

            // --- 下部パネル: 書き出しボタン（右寄せ） ---
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 45,
                Padding = new Padding(10, 5, 10, 5)
            };

            btnExport = new Button
            {
                Text = "書き出し",
                Size = new Size(100, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnExport.Location = new Point(bottomPanel.ClientSize.Width - bottomPanel.Padding.Right - btnExport.Width, 8);
            btnExport.Click += BtnExport_Click;

            bottomPanel.Controls.Add(btnExport);

            // --- フォームに追加（順序重要: Fill は最後に追加） ---
            Controls.Add(tvFolderTree);
            Controls.Add(topPanel);
            Controls.Add(bottomPanel);
        }

        // --- イベントハンドラ ---

        private void BtnSelectFolder_Click(object? sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "表示するフォルダを選択してください"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SetRootAndBuild(dialog.SelectedPath);
            }
        }

        private void NudDepth_ValueChanged(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentRootPath) && Directory.Exists(_currentRootPath))
            {
                BuildTree();
            }
        }

        private void TvFolderTree_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitNode = tvFolderTree.GetNodeAt(e.X, e.Y);
                if (hitNode != null && hitNode.Tag is string path && Directory.Exists(path))
                {
                    tvFolderTree.SelectedNode = hitNode;
                    contextMenu.Show(tvFolderTree, e.Location);
                }
            }
        }

        private void MenuSetRoot_Click(object? sender, EventArgs e)
        {
            if (tvFolderTree.SelectedNode?.Tag is string path && Directory.Exists(path))
            {
                SetRootAndBuild(path);
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            if (tvFolderTree.Nodes.Count == 0)
            {
                MessageBox.Show("先にフォルダを選択してください。", "FolderViewer", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var folderName = Path.GetFileName(_currentRootPath);
            var defaultFileName = $"{folderName}_構造_{DateTime.Now:yyyyMMdd}.txt";

            using var dialog = new SaveFileDialog
            {
                Filter = "テキストファイル (*.txt)|*.txt|すべてのファイル (*.*)|*.*",
                FileName = defaultFileName
            };

            // 前回の保存先を復元
            var lastDir = LoadLastSaveDirectory();
            if (!string.IsNullOrEmpty(lastDir) && Directory.Exists(lastDir))
            {
                dialog.InitialDirectory = lastDir;
            }

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                TreeExporter.Export(tvFolderTree, dialog.FileName);
                SaveLastSaveDirectory(Path.GetDirectoryName(dialog.FileName) ?? "");
                StatusLabel.Text = $"書き出し完了: {dialog.FileName}";
            }
        }

        // --- ヘルパー ---

        private void SetRootAndBuild(string rootPath)
        {
            _currentRootPath = rootPath;
            txtFolderPath.Text = rootPath;
            BuildTree();
        }

        private void BuildTree()
        {
            var (folders, files) = TreeBuilder.Build(tvFolderTree, _currentRootPath, (int)nudDepth.Value);
            StatusLabel.Text = $"{folders}フォルダ、{files}ファイルを表示中";
        }

        // --- 設定の保存・読み込み ---

        private string? LoadLastSaveDirectory()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    var json = File.ReadAllText(SettingsFile);
                    var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    if (dict != null && dict.TryGetValue("LastSaveDirectory", out var dir))
                        return dir;
                }
            }
            catch { }
            return null;
        }

        private void SaveLastSaveDirectory(string directory)
        {
            try
            {
                Directory.CreateDirectory(SettingsDir);
                var dict = new Dictionary<string, string> { ["LastSaveDirectory"] = directory };
                File.WriteAllText(SettingsFile, JsonSerializer.Serialize(dict));
            }
            catch { }
        }
    }
}
