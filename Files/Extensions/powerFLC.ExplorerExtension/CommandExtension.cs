using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Autodesk.Connectivity.Explorer.Extensibility;
using Autodesk.Connectivity.WebServices;
using Autodesk.DataManagement.Client.Framework.Vault.Currency.Connections;
using Connectivity.Explorer.Framework;
using Connectivity.Explorer.Item;
using Connectivity.Request.Foundation.Currency;
using DevExpress.XtraGrid.Views.Grid;


[assembly: Autodesk.Connectivity.Extensibility.Framework.ExtensionId("65516a26-2cbd-4f10-a6d5-a77a135370e5")]
#if Vault2019
[assembly: Autodesk.Connectivity.Extensibility.Framework.ApiVersion("12.0")]
#endif
#if Vault2020
[assembly: Autodesk.Connectivity.Extensibility.Framework.ApiVersion("13.0")]
#endif
#if Vault2021
[assembly: Autodesk.Connectivity.Extensibility.Framework.ApiVersion("14.0")]
#endif

namespace powerFLC.ExplorerExtension
{
    public class CommandExtension : IExplorerExtension
    {
        private const string JobType = "Sample.TransferItemBOMs";
        private const string AttributeNs = "FLC.ITEM";
        private const string AttributeName = "Urn";
        private Connection _connection;

        #region IExtension Members
        public IEnumerable<CommandSite> CommandSites()
        {
            var sites = new List<CommandSite>();

            var gotoItem = new CommandItem("Command.powerFLC.GoToFlcItem", "Go To Fusion Lifecycle Item...")
            {
                NavigationTypes = new SelectionTypeId[] { },
                MultiSelectEnabled = false,
                Image = Properties.Resources.favicon_fusion.ToBitmap()
            };
            gotoItem.Execute += GoToFlcItem;

            var queueItem = new CommandItem("Command.powerFLC.QueueItemJob", "Publish Fusion Lifecycle Item")
            {
                NavigationTypes = new SelectionTypeId[] { },
                MultiSelectEnabled = true,
                Image = Properties.Resources.favicon_fusion.ToBitmap()
            };
            queueItem.Execute += QueueItemJob;

            var itemSite = new CommandSite("Menu.powerFLC.Item.Context", "powerFLC.Item.Context")
            {
                Location = CommandSiteLocation.ItemContextMenu,
                DeployAsPulldownMenu = false
            };
            itemSite.AddCommand(gotoItem);
            itemSite.AddCommand(queueItem);

            var bomSite = new CommandSite("Menu.powerFLC.BOM.Context", "powerFLC.Item.Context")
            {
                Location = CommandSiteLocation.ItemBomToolbar,
                DeployAsPulldownMenu = false
            };
            bomSite.AddCommand(queueItem);

            sites.Add(itemSite);
            sites.Add(bomSite);

            return sites;
        }

        public IEnumerable<DetailPaneTab> DetailTabs()
        {
            return new List<DetailPaneTab>();
        }

        public void OnLogOn(IApplication application)
        {
            _connection = application.Connection;
            application.CommandEnd += Application_CommandEnd;
        }


        public void OnLogOff(IApplication application)
        {
        }

        public void OnShutdown(IApplication application)
        {
        }

        public void OnStartup(IApplication application)
        {
        }

        public IEnumerable<string> HiddenCommands()
        {
            return null;
        }

        public IEnumerable<CustomEntityHandler> CustomEntityHandlers()
        {
            return null;
        }
        #endregion

        #region Private Members
        private void GoToFlcItem(object s, CommandItemEventArgs e)
        {
            var selection = e.Context.CurrentSelectionSet.First();
            if (selection.TypeId != SelectionTypeId.Item) return;

            var item = _connection.WebServiceManager.ItemService.GetLatestItemByItemNumber(selection.Label);
            var entAttrs = _connection.WebServiceManager.PropertyService.GetEntityAttributes(item.MasterId, AttributeNs);
            var entAttr = entAttrs?.SingleOrDefault(a => a.Attr.Equals(AttributeName));
            if (entAttr == null)
            {
                //TODO: connect to FLC and query item by number
                return;
            }

            var attribute = new EntAttrEx(entAttr, AttributeNs);
            var urn = attribute.Val;

            if (string.IsNullOrEmpty(urn))
                return;

            var url = $"https://{attribute.Tenant}.autodeskplm360.net/plm/workspaces/{attribute.Workspace}/items/itemDetails?view=full&tab=details&mode=view&itemId={urn.Replace(":", "%60").Replace(".", ",")}";
            Process.Start(url);
        }


        internal static string GetValue(string settingsName)
        {
            try
            {
                string configPath = Assembly.GetExecutingAssembly().Location + ".config";

                var map = new ExeConfigurationFileMap { ExeConfigFilename = configPath };
                var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

                var section = config.GetSection("appSettings") as AppSettingsSection;
                if (section == null)
                    return String.Empty;

                return section.Settings[settingsName].Value;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void QueueItemJob(object s, CommandItemEventArgs e)
        {
            var errorItems = new List<string>();
            var successItems = new List<string>();
            foreach (var selection in e.Context.CurrentSelectionSet.Reverse())
            {
                if (selection.TypeId.EntityClassId != "ITEM") return;
                var item = _connection.WebServiceManager.ItemService.GetLatestItemByItemNumber(selection.Label);

                try
                {
                    var jobParams = new List<JobParam>
                    {
                        new JobParam { Name = "EntityClassId", Val = "ITEM" },
                        new JobParam { Name = "EntityId", Val = item.Id.ToString() }
                    };

                    var job = _connection.WebServiceManager.JobService.AddJob(JobType,
                        $"Transfers item {item.ItemNum} to Fusion Lifecycle", jobParams.ToArray(), 100);

                    if (job != null)
                        successItems.Add(item.ItemNum);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    errorItems.Add(item.ItemNum);
                }
            }


            if (successItems.Count > 0)
            {
                MessageBox.Show($@"Job '{JobType}' successfully triggered for item{(successItems.Count > 1 ? "s": "")} {string.Join(", ", successItems)}!",
                    @"Add Job",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            if (errorItems.Count > 0)
            {
                MessageBox.Show($@"Error while triggering job '{JobType}' for item{(errorItems.Count > 1 ? "s" : "")} {string.Join(", ", errorItems)}!",
                    @"Add Job",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Vault UI
        private void Application_CommandEnd(object sender, CommandEndEventArgs e)
        {
            if (ExplorerApp.Application.ExplorerForm.ExplorerViewPanel.ExplorerControl is ItemMasterControl)
                Subscribe(ExplorerApp.Application.ExplorerForm.ExplorerViewPanel.ExplorerControl);

            ExplorerApp.Application.ExplorerForm.ExplorerViewPanel.ExplorerControlChanged += (o, args) => Subscribe(args.CurrentControl);
        }

        private bool _isSubscribed;
        private EntAttr[] _attributes;
        private void Subscribe(IExplorerControl control)
        {
            _attributes = _connection.WebServiceManager.PropertyService.FindEntityAttributes(AttributeNs, AttributeName);
            if (_isSubscribed) return;

            if (control is ItemMasterControl visibleControl)
            {
                var gridView = visibleControl.CurrentGridView;
                gridView.CustomDrawCell += GridView_CustomDrawCell;
                _isSubscribed = true;
            }
        }

        private void GridView_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            var gridView = sender as GridView;
            if (gridView == null)
                return;

            IExplorerObject explorerObject = null;
            PropertyDefinition propDef = null;

            if (!ExplorerUtils.GetCellInfo(e.RowHandle, e.Column, ref explorerObject, ref propDef))
                return;

            ItemRevisionExplorerObject itemRevisionExplorerObject = explorerObject as ItemRevisionExplorerObject;
            if (itemRevisionExplorerObject == null)
                return;

            if (e.Column.FieldName == "File!VaultStatus")
            {
                e.DefaultDraw();

                var attributes = _attributes?.Where(a => a.EntityId == itemRevisionExplorerObject.ItemRevision.MasterId);
                if (attributes != null && attributes.Any())
                {
                    e.Graphics.DrawIcon(new Icon(Properties.Resources.favicon_fusion, new Size(16, 16)), e.Bounds.Location.X + e.Bounds.Width - 16, e.Bounds.Location.Y);
                    //e.Cache.DrawIcon(new Icon(Properties.Resources.favicon_fusion, new Size(16, 16)), e.Bounds.Location.X + e.Bounds.Width - 16, e.Bounds.Location.Y);
                }
            }
        }
        #endregion
    }
}