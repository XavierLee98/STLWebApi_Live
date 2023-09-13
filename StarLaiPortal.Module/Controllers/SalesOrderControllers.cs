using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using CrystalDecisions.Shared.Json;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp.Web;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Web;
using StarLaiPortal.Module.BusinessObjects;
using StarLaiPortal.Module.BusinessObjects.Load;
using StarLaiPortal.Module.BusinessObjects.Sales_Order;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

// 2023-08-22 add cancel and close button ver 1.0.9

namespace StarLaiPortal.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class SalesOrderControllers : ViewController
    {
        GeneralControllers genCon;
        public SalesOrderControllers()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            // Perform various tasks depending on the target View.
            this.PreviewSO.Active.SetItemValue("Enabled", false);
            // Start ver 1.0.9
            this.CancelSO.Active.SetItemValue("Enabled", false);
            this.CloseSO.Active.SetItemValue("Enabled", false);
            // End ver 1.0.9
        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // Access and customize the target View control.

            genCon = Frame.GetController<GeneralControllers>();
            if (View.Id == "SalesOrder_DetailView")
            {
                if (((DetailView)View).ViewEditMode == ViewEditMode.View)
                {
                    this.PreviewSO.Active.SetItemValue("Enabled", true);
                    // Start ver 1.0.9
                    this.CancelSO.Active.SetItemValue("Enabled", true);
                    this.CloseSO.Active.SetItemValue("Enabled", true);
                    // End ver 1.0.9
                }
            }
            else
            {
                this.PreviewSO.Active.SetItemValue("Enabled", false);
                // Start ver 1.0.9
                this.CancelSO.Active.SetItemValue("Enabled", false);
                this.CloseSO.Active.SetItemValue("Enabled", false);
                // End ver 1.0.9
            }
        }
        protected override void OnDeactivated()
        {
            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
        }

        public void openNewView(IObjectSpace os, object target, ViewEditMode viewmode)
        {
            ShowViewParameters svp = new ShowViewParameters();
            DetailView dv = Application.CreateDetailView(os, target);
            dv.ViewEditMode = viewmode;
            dv.IsRoot = true;
            svp.CreatedView = dv;

            Application.ShowViewStrategy.ShowView(svp, new ShowViewSource(null, null));

        }
        public void showMsg(string caption, string msg, InformationType msgtype)
        {
            MessageOptions options = new MessageOptions();
            options.Duration = 3000;
            //options.Message = string.Format("{0} task(s) have been successfully updated!", e.SelectedObjects.Count);
            options.Message = string.Format("{0}", msg);
            options.Type = msgtype;
            options.Web.Position = InformationPosition.Right;
            options.Win.Caption = caption;
            options.Win.Type = WinMessageType.Flyout;
            Application.ShowViewStrategy.ShowMessage(options);
        }

        private void PreviewSO_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            string strServer;
            string strDatabase;
            string strUserID;
            string strPwd;
            string filename;

            SqlConnection conn = new SqlConnection(genCon.getConnectionString());
            SalesOrder so = (SalesOrder)View.CurrentObject;
            ApplicationUser user = (ApplicationUser)SecuritySystem.CurrentUser;

            try
            {
                ReportDocument doc = new ReportDocument();
                strServer = ConfigurationManager.AppSettings.Get("SQLserver").ToString();
                doc.Load(HttpContext.Current.Server.MapPath("~\\Reports\\SalesOrder.rpt"));
                strDatabase = conn.Database;
                strUserID = ConfigurationManager.AppSettings.Get("SQLID").ToString();
                strPwd = ConfigurationManager.AppSettings.Get("SQLPass").ToString();
                doc.DataSourceConnections[0].SetConnection(strServer, strDatabase, strUserID, strPwd);
                doc.Refresh();

                doc.SetParameterValue("dockey@", so.Oid);
                doc.SetParameterValue("dbName@", conn.Database);

                filename = ConfigurationManager.AppSettings.Get("ReportPath").ToString() + conn.Database
                    + "_" + so.Oid + "_" + user.UserName + "_SO_"
                    + DateTime.Parse(so.DocDate.ToString()).ToString("yyyyMMdd") + ".pdf";

                doc.ExportToDisk(ExportFormatType.PortableDocFormat, filename);
                doc.Close();
                doc.Dispose();

                string url = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority +
                    ConfigurationManager.AppSettings.Get("PrintPath").ToString() + conn.Database
                    + "_" + so.Oid + "_" + user.UserName + "_SO_"
                    + DateTime.Parse(so.DocDate.ToString()).ToString("yyyyMMdd") + ".pdf";
                var script = "window.open('" + url + "');";

                WebWindow.CurrentRequestWindow.RegisterStartupScript("DownloadFile", script);
            }
            catch (Exception ex)
            {
                showMsg("Fail", ex.Message, InformationType.Error);
            }
        }

        // Start ver 1.0.9
        private void CancelSO_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            SalesOrder selectedObject = (SalesOrder)e.CurrentObject;
            SqlConnection conn = new SqlConnection(genCon.getConnectionString());
            StringParameters p = (StringParameters)e.PopupWindow.View.CurrentObject;
            if (p.IsErr) return;
            string sapdb = null;
            int docentry = 0;
            string error = null;

            foreach (SalesOrderDetails dtl in selectedObject.SalesOrderDetails)
            {
                docentry = dtl.SAPDocEntry;
                break;
            }

            string getcompany = "SELECT SAPDB FROM [" + ConfigurationManager.AppSettings.Get("CommonTable").ToString() + "]..ODBC WHERE " +
              "DBName = '" + conn.Database + "'";
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
            conn.Open();
            SqlCommand cmd = new SqlCommand(getcompany, conn);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                sapdb = reader.GetString(0);
            }
            conn.Close();

            if (sapdb != null && docentry > 0)
            {
                string getpicklist = "EXEC sp_SAPChecking 'SalesOrders', 'Cancel', '" + sapdb + "', " + docentry;
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                conn.Open();
                SqlCommand cmd1 = new SqlCommand(getpicklist, conn);
                SqlDataReader reader1 = cmd1.ExecuteReader();
                while (reader1.Read())
                {
                    error = reader1.GetString(0);
                    break;
                }
                conn.Close();
            }

            if (error == null)
            {
                selectedObject.PendingCancel = true;
                selectedObject.Status = DocStatus.Cancelled;
                SalesOrderDocStatus ds = ObjectSpace.CreateObject<SalesOrderDocStatus>();
                ds.DocStatus = DocStatus.Cancelled;
                ds.DocRemarks = p.ParamString;
                selectedObject.SalesOrderDocStatus.Add(ds);

                ObjectSpace.CommitChanges();
                ObjectSpace.Refresh();

                IObjectSpace os = Application.CreateObjectSpace();
                SalesOrder trx = os.FindObject<SalesOrder>(new BinaryOperator("Oid", selectedObject.Oid));
                openNewView(os, trx, ViewEditMode.View);
                showMsg("Successful", "Cancel Done.", InformationType.Success);
            }
            else
            {
                showMsg("Error", error, InformationType.Error);
            }
        }

        private void CancelSO_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            var os = Application.CreateObjectSpace(typeof(StringParameters));
            StringParameters message = os.CreateObject<StringParameters>();

            DetailView dv = Application.CreateDetailView(os, message);
            dv.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
            ((StringParameters)dv.CurrentObject).IsErr = false;
            ((StringParameters)dv.CurrentObject).ActionMessage = "Press OK to CONFIRM the action and SAVE, else press Cancel.";

            e.View = dv;
        }

        private void CloseSO_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            SalesOrder selectedObject = (SalesOrder)e.CurrentObject;
            SqlConnection conn = new SqlConnection(genCon.getConnectionString());
            StringParameters p = (StringParameters)e.PopupWindow.View.CurrentObject;
            if (p.IsErr) return;
            string sapdb = null;
            int docentry = 0;
            string error = null;

            foreach (SalesOrderDetails dtl in selectedObject.SalesOrderDetails)
            {
                docentry = dtl.SAPDocEntry;
                break;
            }

            string getcompany = "SELECT SAPDB FROM [" + ConfigurationManager.AppSettings.Get("CommonTable").ToString() + "]..ODBC WHERE " +
              "DBName = '" + conn.Database + "'";
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
            conn.Open();
            SqlCommand cmd = new SqlCommand(getcompany, conn);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                sapdb = reader.GetString(0);
            }
            conn.Close();

            if (sapdb != null && docentry > 0)
            {
                string getpicklist = "EXEC sp_SAPChecking 'SalesOrders', 'Close', '" + sapdb + "', " + docentry;
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                conn.Open();
                SqlCommand cmd1 = new SqlCommand(getpicklist, conn);
                SqlDataReader reader1 = cmd1.ExecuteReader();
                while (reader1.Read())
                {
                    error = reader1.GetString(0);
                    break;
                }
                conn.Close();
            }

            if (error == null)
            {
                selectedObject.PendingClose = true;
                selectedObject.Status = DocStatus.Cancelled;
                SalesOrderDocStatus ds = ObjectSpace.CreateObject<SalesOrderDocStatus>();
                ds.DocStatus = DocStatus.Closed;
                ds.DocRemarks = p.ParamString;
                selectedObject.SalesOrderDocStatus.Add(ds);

                ObjectSpace.CommitChanges();
                ObjectSpace.Refresh();

                IObjectSpace os = Application.CreateObjectSpace();
                SalesOrder trx = os.FindObject<SalesOrder>(new BinaryOperator("Oid", selectedObject.Oid));
                openNewView(os, trx, ViewEditMode.View);
                showMsg("Successful", "Close Done.", InformationType.Success);
            }
            else
            {
                showMsg("Error", error, InformationType.Error);
            }
        }

        private void CloseSO_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            var os = Application.CreateObjectSpace(typeof(StringParameters));
            StringParameters message = os.CreateObject<StringParameters>();

            DetailView dv = Application.CreateDetailView(os, message);
            dv.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
            ((StringParameters)dv.CurrentObject).IsErr = false;
            ((StringParameters)dv.CurrentObject).ActionMessage = "Press OK to CONFIRM the action and SAVE, else press Cancel.";

            e.View = dv;
        }
        // End ver 1.0.9
    }
}
