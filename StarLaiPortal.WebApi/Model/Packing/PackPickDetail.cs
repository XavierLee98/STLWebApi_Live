namespace StarLaiPortal.WebApi.Model.Packing
{
    public class PackPickDetail
    {
        public int OID { get; set; }
        public string DocNum { get; set; }

        public string ToBin { get; set; }
        public DateTime DocDate { get; set; }
        public decimal PickQty { get; set; }
        public string ItemCode { get; set; }
        public string ItemDesc { get; set; }
        public string UOM { get; set; }
        public string CatalogNo { get; set; }
        public string LegacyCode { get; set; }
        public string Warehouse { get; set; }
        public string SOTransporter { get; set; }
        public string SOTransporterID { get; set; }
        public string SlpName { get; set; }
        public string SOBaseDoc { get; set; }
        public int SOBaseId { get; set; }
        public decimal PackQty { get; set; }

        public int PickListOID { get; set; }
        public int PickDetailOID { get; set; }
        public string PickListDocNum { get; set; }
        public string Customer { get; set; }
        public List<PackedBundle> packedBundles { get; set; }
        public string BundleListStr { get; set; }
    }
}
