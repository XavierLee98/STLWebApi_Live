using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using StarLaiPortal.Module.BusinessObjects;
using StarLaiPortal.Module.BusinessObjects.Purchase_Return;
using StarLaiPortal.Module.BusinessObjects.Sales_Quotation;
using StarLaiPortal.Module.BusinessObjects.Stock_Count;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using Admiral.ImportData;
using StarLaiPortal.Module.BusinessObjects.Warehouse_Transfer;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using DevExpress.ExpressApp.Web;
using System.Configuration;
using System.Web;

namespace StarLaiPortal.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class StockCountSheetControllers : ViewController
    {
        GeneralControllers genCon;
        public StockCountSheetControllers()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            // Perform various tasks depending on the target View.
            this.SubmitSCS.Active.SetItemValue("Enabled", false);
            this.CancelSCS.Active.SetItemValue("Enabled", false);
            this.CloseSCS.Active.SetItemValue("Enabled", false);
            this.ExportSheetTargetItems.Active.SetItemValue("Enabled", false);
            this.ImportSheetTargetItems.Active.SetItemValue("Enabled", false);
            this.ExportSheetCountedItems.Active.SetItemValue("Enabled", false);
            this.ImportSheetCountedItems.Active.SetItemValue("Enabled", false);
        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // Access and customize the target View control.
            genCon = Frame.GetController<GeneralControllers>();

            if (View.Id == "StockCountSheet_DetailView")
            {
                //this.BackToInquiry.Active.SetItemValue("Enabled", true);
                if (((DetailView)View).ViewEditMode == ViewEditMode.View)
                {
                    this.SubmitSCS.Active.SetItemValue("Enabled", true);
                    this.CancelSCS.Active.SetItemValue("Enabled", true);
                    this.CloseSCS.Active.SetItemValue("Enabled", true);
                }
                else
                {
                    this.SubmitSCS.Active.SetItemValue("Enabled", false);
                    this.CancelSCS.Active.SetItemValue("Enabled", false);
                    this.CloseSCS.Active.SetItemValue("Enabled", false);
                }

                if (((DetailView)View).ViewEditMode == ViewEditMode.Edit)
                {
                    this.ExportSheetTargetItems.Active.SetItemValue("Enabled", true);
                    this.ImportSheetTargetItems.Active.SetItemValue("Enabled", true);
                    this.ExportSheetCountedItems.Active.SetItemValue("Enabled", true);
                    this.ImportSheetCountedItems.Active.SetItemValue("Enabled", true);
                }
                else
                {
                    this.ExportSheetTargetItems.Active.SetItemValue("Enabled", false);
                    this.ImportSheetTargetItems.Active.SetItemValue("Enabled", false);
                    this.ExportSheetCountedItems.Active.SetItemValue("Enabled", false);
                    this.ImportSheetCountedItems.Active.SetItemValue("Enabled", false);
                }
            }
            else
            {
                this.SubmitSCS.Active.SetItemValue("Enabled", false);
                this.CancelSCS.Active.SetItemValue("Enabled", false);
                this.CloseSCS.Active.SetItemValue("Enabled", false);
                this.ExportSheetTargetItems.Active.SetItemValue("Enabled", false);
                this.ImportSheetTargetItems.Active.SetItemValue("Enabled", false);
                this.ExportSheetCountedItems.Active.SetItemValue("Enabled", false);
                this.ImportSheetCountedItems.Active.SetItemValue("Enabled", false);
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

        private void SubmitSCS_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            StockCountSheet selectedObject = (StockCountSheet)e.CurrentObject;
            StringParameters p = (StringParameters)e.PopupWindow.View.CurrentObject;
            if (p.IsErr) return;
            SqlConnection conn = new SqlConnection(genCon.getConnectionString());

            if (selectedObject.IsValid1 == true)
            {
                showMsg("Error", "No Content.", InformationType.Error);
                return;
            }

            selectedObject.Status = DocStatus.Submitted;
            StockCountSheetDocTrail ds = ObjectSpace.CreateObject<StockCountSheetDocTrail>();
            ds.DocStatus = DocStatus.Submitted;
            ds.DocRemarks = p.ParamString;
            selectedObject.StockCountSheetDocTrail.Add(ds);

            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();

            IObjectSpace os = Application.CreateObjectSpace();
            StockCountSheet trx = os.FindObject<StockCountSheet>(new BinaryOperator("Oid", selectedObject.Oid));
            openNewView(os, trx, ViewEditMode.View);
            showMsg("Successful", "Submit Done.", InformationType.Success);
        }

        private void SubmitSCS_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            var os = Application.CreateObjectSpace(typeof(StringParameters));
            StringParameters message = os.CreateObject<StringParameters>();

            DetailView dv = Application.CreateDetailView(os, message);
            dv.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
            ((StringParameters)dv.CurrentObject).IsErr = false;
            ((StringParameters)dv.CurrentObject).ActionMessage = "Press OK to CONFIRM the action and SAVE, else press Cancel.";

            e.View = dv;
        }

        private void CancelSCS_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            StockCountSheet selectedObject = (StockCountSheet)e.CurrentObject;
            StringParameters p = (StringParameters)e.PopupWindow.View.CurrentObject;
            if (p.IsErr) return;

            selectedObject.Status = DocStatus.Cancelled;
            StockCountSheetDocTrail ds = ObjectSpace.CreateObject<StockCountSheetDocTrail>();
            ds.DocStatus = DocStatus.Cancelled;
            ds.DocRemarks = p.ParamString;
            selectedObject.StockCountSheetDocTrail.Add(ds);

            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();

            IObjectSpace os = Application.CreateObjectSpace();
            StockCountSheet trx = os.FindObject<StockCountSheet>(new BinaryOperator("Oid", selectedObject.Oid));
            openNewView(os, trx, ViewEditMode.View);
            showMsg("Successful", "Cancel Done.", InformationType.Success);
        }

        private void CancelSCS_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            var os = Application.CreateObjectSpace(typeof(StringParameters));
            StringParameters message = os.CreateObject<StringParameters>();

            DetailView dv = Application.CreateDetailView(os, message);
            dv.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
            ((StringParameters)dv.CurrentObject).IsErr = false;
            ((StringParameters)dv.CurrentObject).ActionMessage = "Press OK to CONFIRM the action and SAVE, else press Cancel.";

            e.View = dv;
        }

        private void CloseSCS_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            StockCountSheet selectedObject = (StockCountSheet)e.CurrentObject;
            StringParameters p = (StringParameters)e.PopupWindow.View.CurrentObject;
            if (p.IsErr) return;

            selectedObject.Status = DocStatus.Cancelled;
            StockCountSheetDocTrail ds = ObjectSpace.CreateObject<StockCountSheetDocTrail>();
            ds.DocStatus = DocStatus.Cancelled;
            ds.DocRemarks = p.ParamString;
            selectedObject.StockCountSheetDocTrail.Add(ds);

            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();

            IObjectSpace os = Application.CreateObjectSpace();
            StockCountSheet trx = os.FindObject<StockCountSheet>(new BinaryOperator("Oid", selectedObject.Oid));
            openNewView(os, trx, ViewEditMode.View);
            showMsg("Successful", "Cancel Done.", InformationType.Success);
        }

        private void CloseSCS_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            var os = Application.CreateObjectSpace(typeof(StringParameters));
            StringParameters message = os.CreateObject<StringParameters>();

            DetailView dv = Application.CreateDetailView(os, message);
            dv.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
            ((StringParameters)dv.CurrentObject).IsErr = false;
            ((StringParameters)dv.CurrentObject).ActionMessage = "Press OK to CONFIRM the action and SAVE, else press Cancel.";

            e.View = dv;
        }

        private void ImportSheetTargetItems_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();
        }

        private void ImportSheetTargetItems_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            StockCountSheet trx = (StockCountSheet)View.CurrentObject;

            var os = Application.CreateObjectSpace();
            var solution = os.CreateObject<ImportData>();
            solution.Option = new ImportOption();

            solution.Option.UpdateProgress = (x) => solution.Progress = x;
            solution.Option.DocNum = trx.DocNum;
            solution.Option.ConnectionString = genCon.getConnectionString();
            solution.Option.Type = "StockCountSheetTarget";

            solution.Option.MainTypeInfo = (this.View as DetailView).Model.ModelClass;
            var view = Application.CreateDetailView(os, solution, false);

            view.Closed += (sss, eee) =>
            {
                this.Frame.GetController<RefreshController>().RefreshAction.DoExecute();
            };

            e.DialogController.CancelAction.Active["NothingToCancel"] = false;
            e.DialogController.AcceptAction.ActionMeaning = ActionMeaning.Unknown;
            //e.Maximized = true;

            e.View = view;
        }

        private void ImportSheetCountedItems_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();
        }

        private void ImportSheetCountedItems_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            StockCountSheet trx = (StockCountSheet)View.CurrentObject;

            var os = Application.CreateObjectSpace();
            var solution = os.CreateObject<ImportData>();
            solution.Option = new ImportOption();

            solution.Option.UpdateProgress = (x) => solution.Progress = x;
            solution.Option.DocNum = trx.DocNum;
            solution.Option.ConnectionString = genCon.getConnectionString();
            solution.Option.Type = "StockCountSheetCounted";

            solution.Option.MainTypeInfo = (this.View as DetailView).Model.ModelClass;
            var view = Application.CreateDetailView(os, solution, false);

            view.Closed += (sss, eee) =>
            {
                this.Frame.GetController<RefreshController>().RefreshAction.DoExecute();
            };

            e.DialogController.CancelAction.Active["NothingToCancel"] = false;
            e.DialogController.AcceptAction.ActionMeaning = ActionMeaning.Unknown;
            //e.Maximized = true;

            e.View = view;
        }

        private void ExportSheetTargetItems_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            string strServer;
            string strDatabase;
            string strUserID;
            string strPwd;
            string filename;

            SqlConnection conn = new SqlConnection(genCon.getConnectionString());
            StockCountSheet sheettarget = (StockCountSheet)View.CurrentObject;
            ApplicationUser user = (ApplicationUser)SecuritySystem.CurrentUser;

            try
            {
                ReportDocument doc = new ReportDocument();
                strServer = ConfigurationManager.AppSettings.Get("SQLserver").ToString();
                doc.Load(HttpContext.Current.Server.MapPath("~\\Reports\\SheetTargetImportFormat.rpt"));
                strDatabase = conn.Database;
                strUserID = ConfigurationManager.AppSettings.Get("SQLID").ToString();
                strPwd = ConfigurationManager.AppSettings.Get("SQLPass").ToString();
                doc.DataSourceConnections[0].SetConnection(strServer, strDatabase, strUserID, strPwd);
                doc.Refresh();

                doc.SetParameterValue("DocNum", sheettarget.DocNum);
                doc.SetParameterValue("Type", "StarLaiPortal.Module.BusinessObjects.Stock_Count.StockCountSheetTarget");

                filename = ConfigurationManager.AppSettings.Get("ReportPath").ToString() + conn.Database
                    + "_" + sheettarget.DocNum + "_" + user.UserName + "_SheetTargetImport_" + ".xls";

                doc.ExportToDisk(ExportFormatType.Excel, filename);
                doc.Close();
                doc.Dispose();

                string url = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority +
                    ConfigurationManager.AppSettings.Get("PrintPath").ToString() + conn.Database
                    + "_" + sheettarget.DocNum + "_" + user.UserName + "_SheetTargetImport_" + ".xls";
                var script = "window.open('" + url + "');";

                WebWindow.CurrentRequestWindow.RegisterStartupScript("DownloadFile", script);
            }
            catch (Exception ex)
            {
                showMsg("Fail", ex.Message, InformationType.Error);
            }
        }

        private void ExportSheetCountedItems_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            string strServer;
            string strDatabase;
            string strUserID;
            string strPwd;
            string filename;

            SqlConnection conn = new SqlConnection(genCon.getConnectionString());
            StockCountSheet sheetcounted = (StockCountSheet)View.CurrentObject;
            ApplicationUser user = (ApplicationUser)SecuritySystem.CurrentUser;

            try
            {
                ReportDocument doc = new ReportDocument();
                strServer = ConfigurationManager.AppSettings.Get("SQLserver").ToString();
                doc.Load(HttpContext.Current.Server.MapPath("~\\Reports\\SheetCountedImportFormat.rpt"));
                strDatabase = conn.Database;
                strUserID = ConfigurationManager.AppSettings.Get("SQLID").ToString();
                strPwd = ConfigurationManager.AppSettings.Get("SQLPass").ToString();
                doc.DataSourceConnections[0].SetConnection(strServer, strDatabase, strUserID, strPwd);
                doc.Refresh();

                doc.SetParameterValue("DocNum", sheetcounted.DocNum);
                doc.SetParameterValue("Type", "StarLaiPortal.Module.BusinessObjects.Stock_Count.StockCountSheetCounted");

                filename = ConfigurationManager.AppSettings.Get("ReportPath").ToString() + conn.Database
                    + "_" + sheetcounted.DocNum + "_" + user.UserName + "_SheetCountedImport_" + ".xls";

                doc.ExportToDisk(ExportFormatType.Excel, filename);
                doc.Close();
                doc.Dispose();

                string url = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority +
                    ConfigurationManager.AppSettings.Get("PrintPath").ToString() + conn.Database
                    + "_" + sheetcounted.DocNum + "_" + user.UserName + "_SheetCountedImport_" + ".xls";
                var script = "window.open('" + url + "');";

                WebWindow.CurrentRequestWindow.RegisterStartupScript("DownloadFile", script);
            }
            catch (Exception ex)
            {
                showMsg("Fail", ex.Message, InformationType.Error);
            }
        }
    }
}
