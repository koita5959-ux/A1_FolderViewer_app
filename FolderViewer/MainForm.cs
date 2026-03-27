using DesktopKit.Common;
using System.Windows.Forms;
using System.Drawing;

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

        /// <summary>
        /// MainFormのコンストラクタ。
        /// </summary>
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

            topPanel.Controls.AddRange(new Control[] { btnSelectFolder, txtFolderPath, lblDepth, nudDepth });

            // --- 中央: TreeView ---
            tvFolderTree = new TreeView
            {
                Dock = DockStyle.Fill
            };

            // --- 下部パネル: 書き出しボタン ---
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 45,
                Padding = new Padding(10, 5, 10, 5)
            };

            btnExport = new Button
            {
                Text = "書き出し",
                Location = new Point(10, 8),
                Size = new Size(100, 28)
            };

            bottomPanel.Controls.Add(btnExport);

            // --- フォームに追加（順序重要: Fill は最後に追加） ---
            Controls.Add(tvFolderTree);
            Controls.Add(topPanel);
            Controls.Add(bottomPanel);
        }
    }
}
