namespace StarLaiPortal.Module.Controllers
{
    partial class StockCountConfirmControllers
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.SubmitSCC = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            this.CancelSCC = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            this.ExportConfirmCountItems = new DevExpress.ExpressApp.Actions.SimpleAction(this.components);
            this.ImportConfirmCountItems = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            // 
            // SubmitSCC
            // 
            this.SubmitSCC.AcceptButtonCaption = null;
            this.SubmitSCC.CancelButtonCaption = null;
            this.SubmitSCC.Caption = "Submit";
            this.SubmitSCC.Category = "ObjectsCreation";
            this.SubmitSCC.ConfirmationMessage = null;
            this.SubmitSCC.Id = "SubmitSCC";
            this.SubmitSCC.ToolTip = null;
            this.SubmitSCC.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.SubmitSCC_CustomizePopupWindowParams);
            this.SubmitSCC.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.SubmitSCC_Execute);
            // 
            // CancelSCC
            // 
            this.CancelSCC.AcceptButtonCaption = null;
            this.CancelSCC.CancelButtonCaption = null;
            this.CancelSCC.Caption = "Cancel";
            this.CancelSCC.Category = "ObjectsCreation";
            this.CancelSCC.ConfirmationMessage = null;
            this.CancelSCC.Id = "CancelSCC";
            this.CancelSCC.ToolTip = null;
            this.CancelSCC.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.CancelSCC_CustomizePopupWindowParams);
            this.CancelSCC.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.CancelSCC_Execute);
            // 
            // ExportConfirmCountItems
            // 
            this.ExportConfirmCountItems.Caption = "Export Count (Format)";
            this.ExportConfirmCountItems.Category = "ListView";
            this.ExportConfirmCountItems.ConfirmationMessage = null;
            this.ExportConfirmCountItems.Id = "ExportConfirmCountItems";
            this.ExportConfirmCountItems.ToolTip = null;
            this.ExportConfirmCountItems.Execute += new DevExpress.ExpressApp.Actions.SimpleActionExecuteEventHandler(this.ExportConfirmCountItems_Execute);
            // 
            // ImportConfirmCountItems
            // 
            this.ImportConfirmCountItems.AcceptButtonCaption = null;
            this.ImportConfirmCountItems.CancelButtonCaption = null;
            this.ImportConfirmCountItems.Caption = "Import Count Items";
            this.ImportConfirmCountItems.Category = "ListView";
            this.ImportConfirmCountItems.ConfirmationMessage = null;
            this.ImportConfirmCountItems.Id = "ImportConfirmCountItems";
            this.ImportConfirmCountItems.ToolTip = null;
            this.ImportConfirmCountItems.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.ImportConfirmCountItems_CustomizePopupWindowParams);
            this.ImportConfirmCountItems.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.ImportConfirmCountItems_Execute);
            // 
            // StockCountConfirmControllers
            // 
            this.Actions.Add(this.SubmitSCC);
            this.Actions.Add(this.CancelSCC);
            this.Actions.Add(this.ExportConfirmCountItems);
            this.Actions.Add(this.ImportConfirmCountItems);

        }

        #endregion

        private DevExpress.ExpressApp.Actions.PopupWindowShowAction SubmitSCC;
        private DevExpress.ExpressApp.Actions.PopupWindowShowAction CancelSCC;
        private DevExpress.ExpressApp.Actions.SimpleAction ExportConfirmCountItems;
        private DevExpress.ExpressApp.Actions.PopupWindowShowAction ImportConfirmCountItems;
    }
}
