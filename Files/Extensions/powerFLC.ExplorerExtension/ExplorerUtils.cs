using Common.Reflection;
using Connectivity.Explorer.Framework;
using Connectivity.Explorer.Item;
using DevExpress.XtraGrid.Columns;
using PropertyDefinition = Connectivity.Request.Foundation.Currency.PropertyDefinition;

namespace powerFLC.ExplorerExtension
{
    public class ExplorerUtils
    {
        public static bool GetCellInfo(int rowHandle, GridColumn col, ref IExplorerObject explorerObject, ref PropertyDefinition propDef)
        {
            if (ExplorerApp.Application.ExplorerForm.ExplorerViewPanel.ExplorerControl is ItemMasterControl itemMasterControl)
            {
                var mcBinder = new InternalBinder(itemMasterControl);
                var prop = mcBinder.GetField("m_itemsGridOptions");
                GridDriver gd = prop as GridDriver;
                return gd.GetCellInfo(rowHandle, col, ref explorerObject, ref propDef);
            }
            return false;
        }
    }
}