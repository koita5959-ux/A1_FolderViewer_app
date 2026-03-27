using System.IO;
using System.Text;
using System.Windows.Forms;

namespace DesktopKit.FolderViewer
{
    /// <summary>
    /// TreeViewからツリー形式テキスト＋フルパス一覧を生成する。
    /// チェックOFFのフォルダは名前のみ出力し、子の展開はしない。
    /// </summary>
    public static class TreeExporter
    {
        /// <summary>
        /// TreeViewの内容をテキストファイルに書き出す。
        /// </summary>
        public static void Export(TreeView treeView, string outputPath)
        {
            var sb = new StringBuilder();
            var fullPaths = new List<string>();

            sb.AppendLine("--- ツリー構造 ---");

            if (treeView.Nodes.Count > 0)
            {
                var rootNode = treeView.Nodes[0];
                var rootPath = rootNode.Tag as string ?? "";
                sb.AppendLine(rootPath + Path.DirectorySeparatorChar);
                BuildTreeText(rootNode, "", sb, fullPaths);
            }

            sb.AppendLine();
            sb.AppendLine("--- フルパス一覧 ---");
            foreach (var path in fullPaths)
            {
                sb.AppendLine(path);
            }

            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
        }

        private static void BuildTreeText(TreeNode parentNode, string indent, StringBuilder sb, List<string> fullPaths)
        {
            for (int i = 0; i < parentNode.Nodes.Count; i++)
            {
                var node = parentNode.Nodes[i];
                bool isLast = (i == parentNode.Nodes.Count - 1);
                var connector = isLast ? "└── " : "├── ";
                var childIndent = indent + (isLast ? "    " : "│   ");

                bool isFolder = TreeBuilder.IsFolder(node);

                sb.AppendLine(indent + connector + node.Text);

                if (isFolder)
                {
                    // チェックONのフォルダのみ子を出力
                    if (node.Checked && node.Nodes.Count > 0)
                    {
                        BuildTreeText(node, childIndent, sb, fullPaths);
                    }
                }
                else
                {
                    // ファイルはフルパス一覧に追加
                    if (node.Tag is string filePath)
                    {
                        fullPaths.Add(filePath);
                    }
                }
            }
        }
    }
}
