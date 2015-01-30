using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Xps.Packaging;
using System.Windows.Xps.Serialization;

namespace CACCCheckIn.Modules.Admin.Views
{
    /// <summary>
    /// 
    /// </summary>
    public static class FlowDocumentExtentions
    {
        public static List<Block> CloneDeep(this IEnumerable<Block> blocks)
        {
            List<Block> lb = new List<Block>();
            foreach (var b in blocks) lb.Add(b.Clone());
            return lb;
        }

        public static T Clone<T>(this T element)
            where T : TextElement
        {
            MemoryStream ms = new MemoryStream();
            XamlWriter.Save(element, ms);
            ms.Position = 0;
            return (T)XamlReader.Load(ms);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class FlowDocumentHelpers
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="paginator"></param>
        public static void CreateXPSDocument(string path, DocumentPaginator paginator)
        {
            using (Package container = Package.Open(path, FileMode.Create))
            {
                using (XpsDocument xpsDoc = new XpsDocument(container, CompressionOption.Maximum))
                {
                    XpsSerializationManager xpsSM = new XpsSerializationManager(
                        new XpsPackagingPolicy(xpsDoc), false);
                    xpsSM.SaveAsXaml(paginator);
                    xpsSM.Commit();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void SaveReport(FlowDocument flowDoc)
        {
            if (null == flowDoc) return;

            const double DesiredPageWidth = 96 * 8.5;
            const double DesiredPageHeight = 96 * 11;

            FlowDocument clonedFlowDoc = new FlowDocument();
            List<Block> sourceBlocks = flowDoc.Blocks.CloneDeep();
            foreach (Block block in sourceBlocks)
            {
                clonedFlowDoc.Blocks.Add(block);
            }

            // Modify the FlowDocument to print to normal page
            // with one column
            clonedFlowDoc.PageWidth = DesiredPageWidth;
            clonedFlowDoc.PageHeight = DesiredPageHeight;
            clonedFlowDoc.PagePadding = new Thickness(50);
            clonedFlowDoc.ColumnGap = 0;
            clonedFlowDoc.ColumnWidth = clonedFlowDoc.PageWidth - clonedFlowDoc.PagePadding.Left - clonedFlowDoc.PagePadding.Right;

            using (System.Windows.Forms.SaveFileDialog saveDialog = new System.Windows.Forms.SaveFileDialog())
            {
                // Default file extension
                saveDialog.DefaultExt = "xps";
                // Available file extensions
                saveDialog.Filter = "XPS file (*.xps)|*.xps";
                saveDialog.AddExtension = true;
                // Restores the selected directory, next time
                saveDialog.RestoreDirectory = true;
                saveDialog.AutoUpgradeEnabled = true;
                saveDialog.OverwritePrompt = true;
                saveDialog.Title = "Save the Report to File";

                if (System.Windows.Forms.DialogResult.OK == saveDialog.ShowDialog())
                {
                    DocumentPaginator paginator = ((IDocumentPaginatorSource)clonedFlowDoc).DocumentPaginator;
                    paginator.PageSize = new Size(DesiredPageWidth, DesiredPageHeight);
                    CreateXPSDocument(saveDialog.FileName, paginator);
                }
            }
        }
    }
}
