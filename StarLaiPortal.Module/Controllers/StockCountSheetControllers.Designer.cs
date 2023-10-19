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
            // StockCountSheetControllers
            // 
            this.Actions.Add(this.SubmitSCS);
            this.Actions.Add(this.CancelSCS);
            this.Actions.Add(this.CloseSCS);

        }

        #endregion

        private DevExpress.ExpressApp.Actions.PopupWindowShowAction SubmitSCS;
        private DevExpress.ExpressApp.Actions.PopupWindowShowAction CancelSCS;
        private DevExpress.ExpressApp.Actions.PopupWindowShowAction CloseSCS;
    }
}
