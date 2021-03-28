namespace ESTest.Model.Models
{
    public class DocMgmt
    {
        public Mgmt mgmt { get; set; }
    }

    public class Mgmt
    {
        public int mgmtID { get; set; }
        public string name { get; set; }
        public string market { get; set; }
        public string state { get; set; }
    }
}
