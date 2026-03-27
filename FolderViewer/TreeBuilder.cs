using System.IO;
using System.Windows.Forms;

namespace DesktopKit.FolderViewer
{
    /// <summary>
    /// 指定フォルダからTreeViewノードを構築する。階層制限対応。
    /// </summary>
    public static class TreeBuilder
    {
        /// <summary>
        /// 指定パスを起点にTreeViewを構築する。
        /// </summary>
        /// <param name="treeView">対象のTreeView</param>
        /// <param name="rootPath">起点フォルダのパス</param>
        /// <param name="maxDepth">最大階層の深さ</param>
        /// <returns>フォルダ数とファイル数のタプル</returns>
        public static (int folders, int files) Build(TreeView treeView, string rootPath, int maxDepth)
        {
            treeView.BeginUpdate();
            treeView.Nodes.Clear();

            var rootNode = new TreeNode(Path.GetFileName(rootPath) + @"\")
            {
                Tag = rootPath
            };
            treeView.Nodes.Add(rootNode);

            int folders = 0;
            int files = 0;
            AddChildren(rootNode, rootPath, 1, maxDepth, ref folders, ref files);

            rootNode.ExpandAll();
            treeView.EndUpdate();

            return (folders, files);
        }

        private static void AddChildren(TreeNode parentNode, string dirPath, int currentDepth, int maxDepth, ref int folders, ref int files)
        {
            // フォルダを追加
            try
            {
                foreach (var dir in Directory.GetDirectories(dirPath))
                {
                    var dirName = Path.GetFileName(dir) + @"\";
                    var node = new TreeNode(dirName) { Tag = dir };
                    parentNode.Nodes.Add(node);
                    folders++;

                    if (currentDepth < maxDepth)
                    {
                        AddChildren(node, dir, currentDepth + 1, maxDepth, ref folders, ref files);
                    }
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (IOException) { }

            // ファイルを追加
            try
            {
                foreach (var file in Directory.GetFiles(dirPath))
                {
                    var fileName = Path.GetFileName(file);
                    var node = new TreeNode(fileName) { Tag = file };
                    parentNode.Nodes.Add(node);
                    files++;
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (IOException) { }
        }
    }
}
