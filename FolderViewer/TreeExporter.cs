using System.IO;
using System.Text;
using System.Windows.Forms;

namespace DesktopKit.FolderViewer
{
    /// <summary>
    /// 展開済みノードからツリー形式テキスト＋フルパス一覧を生成する。
    /// </summary>
    public static class TreeExporter
    {
        /// <summary>
        /// TreeViewの内容をテキストファイルに書き出す。
        /// ツリー構造セクションとフルパス一覧セクションの2部構成。
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
                sb.AppendLine(rootPath + @"\");
                BuildTreeText(rootNode, "", true, sb, fullPaths);
            }

            sb.AppendLine();
            sb.AppendLine("--- フルパス一覧 ---");
            foreach (var path in fullPaths)
            {
                sb.AppendLine(path);
            }

            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
        }

        private static void BuildTreeText(TreeNode parentNode, string indent, bool isRoot, StringBuilder sb, List<string> fullPaths)
        {
            for (int i = 0; i < parentNode.Nodes.Count; i++)
            {
                var node = parentNode.Nodes[i];
                bool isLast = (i == parentNode.Nodes.Count - 1);
                var connector = isLast ? "└── " : "├── ";
                var childIndent = indent + (isLast ? "    " : "│   ");

                sb.AppendLine(indent + connector + node.Text);

                var tag = node.Tag as string;
                if (tag != null)
                {
                    // ファイルのみフルパス一覧に追加（末尾に\がないもの）
                    if (!node.Text.EndsWith(@"\"))
                    {
                        fullPaths.Add(tag);
                    }
                }

                if (node.Nodes.Count > 0)
                {
                    BuildTreeText(node, childIndent, false, sb, fullPaths);
                }
            }
        }
    }
}
