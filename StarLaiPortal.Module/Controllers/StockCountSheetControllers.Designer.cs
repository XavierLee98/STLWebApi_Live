namespace StarLaiPortal.Module.Controllers
{
    partial class StockCountSheetControllers
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
            this.SubmitSCS = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            this.CancelSCS = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            this.CloseSCS = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            this.ImportSheetTargetItems = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            this.ImportSheetCountedItems = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            this.ExportSheetTargetItems = new DevExpress.ExpressApp.Actions.SimpleAction(this.components);
            this.ExportSheetCountedItems = new DevExpress.ExpressApp.Actions.SimpleAction(this.components);
            // 
            // SubmitSCS
            // 
            this.SubmitSCS.AcceptButtonCaption = null;
            this.SubmitSCS.CancelButtonCaption = null;
            this.SubmitSCS.Caption = "Submit";
            this.SubmitSCS.Category = "ObjectsCreation";
            this.SubmitSCS.ConfirmationMessage = null;
            this.SubmitSCS.Id = "SubmitSCS";
            this.SubmitSCS.ToolTip = null;
            this.SubmitSCS.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.SubmitSCS_CustomizePopupWindowParams);
            this.SubmitSCS.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.SubmitSCS_Execute);
            // 
            // CancelSCS
            // 
            this.CancelSCS.AcceptButtonCaption = null;
            this.CancelSCS.CancelButtonCaption = null;
            this.CancelSCS.Caption = "Cancel";
            this.CancelSCS.Category = "ObjectsCreation";
            this.CancelSCS.ConfirmationMessage = null;
            this.CancelSCS.Id = "CancelSCS";
            this.CancelSCS.ToolTip = null;
            this.CancelSCS.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.CancelSCS_CustomizePopupWindowParams);
            this.CancelSCS.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.CancelSCS_Execute);
            // 
            // CloseSCS
            // 
            this.CloseSCS.AcceptButtonCaption = null;
            this.CloseSCS.CancelButtonCaption = null;
            this.CloseSCS.Caption = "Close";
            this.CloseSCS.Category = "ObjectsCreation";
            this.CloseSCS.ConfirmationMessage = null;
            this.CloseSCS.Id = "CloseSCS";
            this.CloseSCS.ToolTip = null;
            this.CloseSCS.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.CloseSCS_CustomizePopupWindowParams);
            this.CloseSCS.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.CloseSCS_Execute);
            // 
            // ImportSheetTargetItems
            // 
            this.ImportSheetTargetItems.AcceptButtonCaption = null;
            this.ImportSheetTargetItems.CancelButtonCaption = null;
            this.ImportSheetTargetItems.Caption = "Import Target Items";
            this.ImportSheetTargetItems.Category = "ListView";
            this.ImportSheetTargetItems.ConfirmationMessage = null;
            this.ImportSheetTargetItems.Id = "ImportSheetTargetItems";
            this.ImportSheetTargetItems.ToolTip = null;
            this.ImportSheetTargetItems.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.ImportSheetTargetItems_CustomizePopupWindowParams);
            this.ImportSheetTargetItems.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.ImportSheetTargetItems_Execute);
            // 
            // ImportSheetCountedItems
            // 
            this.ImportSheetCountedItems.AcceptButtonCaption = null;
            this.ImportSheetCountedItems.CancelButtonCaption = null;
            this.ImportSheetCountedItems.Caption = "Import Counted Items";
            this.ImportSheetCountedItems.Category = "ListView";
            this.ImportSheetCountedItems.ConfirmationMessage = null;
            this.ImportSheetCountedItems.Id = "ImportSheetCountedItems";
            this.ImportSheetCountedItems.ToolTip = null;
            this.ImportSheetCountedItems.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.ImportSheetCountedItems_CustomizePopupWindowParams);
            this.ImportSheetCountedItems.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.ImportSheetCountedItems_Execute);
            // 
            // ExportSheetTargetItems
            // 
            this.ExportSheetTargetItems.Caption = "Export Target (Format)";
            this.ExportSheetTargetItems.Category = "ListView";
            this.ExportSheetTargetItems.ConfirmationMessage = null;
            this.ExportSheetTargetItems.Id = "ExportSheetTargetItems";
            this.ExportSheetTargetItems.ToolTip = null;
            this.ExportSheetTargetItems.Execute += new DevExpress.ExpressApp.Actions.SimpleActionExecuteEventHandler(this.ExportSheetTargetItems_Execute);
            // 
            // ExportSheetCountedItems
            // 
            this.ExportSheetCountedItems.Caption = "Export Counted (Format)";
            this.ExportSheetCountedItems.Category = "ListView";
            this.ExportSheetCountedItems.ConfirmationMessage = null;
            this.ExportSheetCountedItems.Id = "ExportSheetCountedItems";
            this.ExportSheetCountedItems.ToolTip = null;
            this.ExportSheetCountedItems.Execute += new DevExpress.ExpressApp.Actions.SimpleActionExecuteEventHandler(this.ExportSheetCountedItems_Execute);
            // 
            // StockCountSheetControllers
            // 
            this.Actions.Add(this.SubmitSCS);
            this.Actions.Add(this.CancelSCS);
            this.Actions.Add(this.CloseSCS);
            this.Actions.Add(this.ImportSheetTargetItems);
            this.Actions.Add(this.ImportSheetCountedItems);
            this.Actions.Add(this.ExportSheetTargetItems);
            this.Actions.Add(this.ExportSheetCountedItems);

        }

        #endregion

        private DevExpress.ExpressApp.Actions.PopupWindowShowAction SubmitSCS;
        private DevExpress.ExpressApp.Actions.PopupWindowShowAction CancelSCS;
        private DevExpress.ExpressApp.Actions.PopupWindowShowAction CloseSCS;
        private DevExpress.ExpressApp.Actions.PopupWindowShowAction ImportSheetTargetItems;
        private DevExpress.ExpressApp.Actions.PopupWindowShowAction ImportSheetCountedItems;
        private DevExpress.ExpressApp.Actions.SimpleAction ExportSheetTargetItems;
        private DevExpress.ExpressApp.Actions.SimpleAction ExportSheetCountedItems;
    }
}
