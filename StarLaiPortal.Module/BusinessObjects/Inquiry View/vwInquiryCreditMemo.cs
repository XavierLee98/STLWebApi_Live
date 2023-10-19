using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;

// 2023-10-16 - add row total and doc total - ver 1.0.11

namespace StarLaiPortal.Module.BusinessObjects.Inquiry_View
{
    [XafDisplayName("A/R Credit Memo Inquiry")]
    [NavigationItem("Sales Return")]
    [DefaultProperty("PortalNo")]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("hideSave", AppearanceItemType = "Action", TargetItems = "Save", Context = "Any", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]
    //[Appearance("HideResetViewSetting", AppearanceItemType.Action, "True", TargetItems = "ResetViewSettings", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    //[Appearance("HideExport", AppearanceItemType.Action, "True", TargetItems = "Export", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideRefresh", AppearanceItemType.Action, "True", TargetItems = "Refresh", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]

    public class vwInquiryCreditMemo : XPLiteObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument113146.aspx).
        // Use CodeRush to create XPO classes and properties with a few keystrokes.
        // https://docs.devexpress.com/CodeRushForRoslyn/118557
        public vwInquiryCreditMemo(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument112834.aspx).
        }

        [Key]
        [Browsable(true)]
        [XafDisplayName("PriKey")]
        [Appearance("PriKey", Enabled = false)]
        [Index(0), VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(false)]
        public string PriKey
        {
            get; set;
        }

        [XafDisplayName("SAP No")]
        [Appearance("SAPNo", Enabled = false)]
        [Index(3)]
        public string SAPNo
        {
            get; set;
        }

        [XafDisplayName("Create DT")]
        [Appearance("CreateDT", Enabled = false)]
        [Index(5)]
        public string CreateDT
        {
            get; set;
        }

        [XafDisplayName("Doc Date")]
        [Appearance("DocDate", Enabled = false)]
        [Index(8)]
        public string DocDate
        {
            get; set;
        }

        [XafDisplayName("Due Date")]
        [Appearance("DueDate", Enabled = false)]
        [Index(10)]
        public string DueDate
        {
            get; set;
        }

        [XafDisplayName("Status")]
        [Appearance("Status", Enabled = false)]
        [Index(13)]
        public string Status
        {
            get; set;
        }

        [XafDisplayName("Card Code")]
        [Appearance("CardCode", Enabled = false)]
        [Index(15)]
        public string CardCode
        {
            get; set;
        }


        [XafDisplayName("Card Name")]
        [Appearance("CardName", Enabled = false)]
        [Index(18)]
        public string CardName
        {
            get; set;
        }


        [XafDisplayName("Item Code")]
        [Appearance("ItemCode", Enabled = false)]
        [Index(20)]
        public string ItemCode
        {
            get; set;
        }


        [XafDisplayName("Item Name")]
        [Appearance("ItemName", Enabled = false)]
        [Index(23)]
        public string ItemName
        {
            get; set;
        }


        [XafDisplayName("Old Code")]
        [Appearance("OldCode", Enabled = false)]
        [Index(25)]
        public string OldCode
        {
            get; set;
        }


        [XafDisplayName("Catalog No")]
        [Appearance("CatalogNo", Enabled = false)]
        [Index(28)]
        public string CatalogNo
        {
            get; set;
        }

        [XafDisplayName("Model")]
        [Appearance("Model", Enabled = false)]
        [Index(30)]
        public string Model
        {
            get; set;
        }

        [XafDisplayName("Brand")]
        [Appearance("Brand", Enabled = false)]
        [Index(33)]
        public string Brand
        {
            get; set;
        }

        [XafDisplayName("Quantity")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Appearance("Quantity", Enabled = false)]
        [Index(35)]
        public decimal Quantity
        {
            get; set;
        }

        [XafDisplayName("UOM")]
        [Appearance("UOM", Enabled = false)]
        [Index(38)]
        public string UOM
        {
            get; set;
        }

        [XafDisplayName("Warehouse")]
        [Appearance("Warehouse", Enabled = false)]
        [Index(40)]
        public string Warehouse
        {
            get; set;
        }

        [XafDisplayName("Remark")]
        [Appearance("Remark", Enabled = false)]
        [Index(43)]
        public string Remark
        {
            get; set;
        }

        [XafDisplayName("Portal SO No")]
        [Appearance("PortalSONo", Enabled = false)]
        [Index(45)]
        public string PortalSONo
        {
            get; set;
        }

        [XafDisplayName("SO Series")]
        [Appearance("SOSeries", Enabled = false)]
        [Index(48)]
        public string SOSeries
        {
            get; set;
        }

        [XafDisplayName("Loading No")]
        [Appearance("LoadingNo", Enabled = false)]
        [Index(50)]
        public string LoadingNo
        {
            get; set;
        }

        [XafDisplayName("Portal Inv No")]
        [Appearance("PortalInvNo", Enabled = false)]
        [Index(53)]
        public string PortalInvNo
        {
            get; set;
        }

        [XafDisplayName("SAP Inv No")]
        [Appearance("SAPInvNo", Enabled = false)]
        [Index(55)]
        public string SAPInvNo
        {
            get; set;
        }

        [XafDisplayName("Portal DO No")]
        [Appearance("PortalDONo", Enabled = false)]
        [Index(58)]
        public string PortalDONo
        {
            get; set;
        }

        [XafDisplayName("SAP DO No")]
        [Appearance("SAPDONo", Enabled = false)]
        [Index(60)]
        public string SAPDONo
        {
            get; set;
        }

        [XafDisplayName("Transporter")]
        [Appearance("Transporter", Enabled = false)]
        [Index(63)]
        public string Transporter
        {
            get; set;
        }

        [XafDisplayName("DocKey")]
        [Appearance("DocKey", Enabled = false)]
        [Index(65), VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(false)]
        public string DocKey
        {
            get; set;
        }

        // Start ver 1.0.11
        [XafDisplayName("Row Total")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Appearance("RowTotal", Enabled = false)]
        [Index(68)]
        public decimal RowTotal
        {
            get; set;
        }

        [XafDisplayName("Doc. Total")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Appearance("DocTotal", Enabled = false)]
        [Index(70)]
        public decimal DocTotal
        {
            get; set;
        }
        // End ver 1.0.11
    }
}