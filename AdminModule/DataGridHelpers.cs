using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace CACCCheckIn.Modules.Admin
{
    public static class DataGridHelpers
    {
        #region GetCell

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public static DataGridCell GetCell(DataGrid dataGrid, int row, int column)
        {
            DataGridRow rowContainer = GetRow(dataGrid, row);
            if (rowContainer != null)
            {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);

                // try to get the cell but it may possibly be virtualized
                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                if (cell == null)
                {
                    // now try to bring into view and retreive the cell
                    dataGrid.ScrollIntoView(rowContainer, dataGrid.Columns[column]);

                    cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                }

                return cell;
            }

            return null;
        }

        #endregion GetCell

        #region GetRow

        /// <summary>
        /// Gets the DataGridRow based on the given index
        /// </summary>
        /// <param name="index">the index of the container to get</param>
        public static DataGridRow GetRow(DataGrid dataGrid, int index)
        {
            if (index < 0) return null;

            DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                // may be virtualized, bring into view and try again
                dataGrid.ScrollIntoView(dataGrid.Items[index]);
                dataGrid.UpdateLayout();

                row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(index);
            }

            return row;
        }

        #endregion GetRow

        #region GetVisualChild

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }

            return child;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static T GetVisualChild<T>(Visual parent, int index) where T : Visual
        {
            T child = default(T);

            int encounter = 0;
            Queue<Visual> queue = new Queue<Visual>();
            queue.Enqueue(parent);
            while (queue.Count > 0)
            {
                Visual v = queue.Dequeue();
                child = v as T;
                if (child != null)
                {
                    if (encounter == index)
                        break;
                    encounter++;
                }
                else
                {
                    int numVisuals = VisualTreeHelper.GetChildrenCount(v);
                    for (int i = 0; i < numVisuals; i++)
                    {
                        queue.Enqueue((Visual)VisualTreeHelper.GetChild(v, i));
                    }
                }
            }

            return child;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="visualToFind"></param>
        /// <returns></returns>
        public static bool VisualChildExists(Visual parent, DependencyObject visualToFind)
        {
            Queue<Visual> queue = new Queue<Visual>();
            queue.Enqueue(parent);
            while (queue.Count > 0)
            {
                Visual v = queue.Dequeue();
                DependencyObject child = v as DependencyObject;
                if (child != null)
                {
                    if (child == visualToFind)
                        return true;
                }
                else
                {
                    int numVisuals = VisualTreeHelper.GetChildrenCount(v);
                    for (int i = 0; i < numVisuals; i++)
                    {
                        queue.Enqueue((Visual)VisualTreeHelper.GetChild(v, i));
                    }
                }
            }

            return false;
        }

        #endregion GetVisualChild

        #region FindVisualParent

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element"></param>
        /// <returns></returns>
        public static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }

            return null;
        }

        #endregion FindVisualParent
    }
}
