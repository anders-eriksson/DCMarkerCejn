namespace DCMarkerEF
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("LaserData")]
    public partial class LaserData
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string F1 { get; set; }

        [StringLength(1)]
        public string Kant { get; set; }

        [StringLength(100)]
        public string Avdelning { get; set; }

        [StringLength(50)]
        public string F2 { get; set; }

        [StringLength(50)]
        public string F3 { get; set; }

        [StringLength(50)]
        public string F4 { get; set; }

        [StringLength(50)]
        public string F5 { get; set; }

        [StringLength(50)]
        public string F6 { get; set; }

        [StringLength(50)]
        public string F7 { get; set; }

        [StringLength(50)]
        public string F8 { get; set; }

        [StringLength(50)]
        public string F9 { get; set; }

        [StringLength(50)]
        public string F10 { get; set; }

        [StringLength(50)]
        public string BC1 { get; set; }

        [StringLength(50)]
        public string BC2 { get; set; }

        [StringLength(50)]
        public string Template { get; set; }

        [StringLength(50)]
        public string P1 { get; set; }

        [StringLength(50)]
        public string P2 { get; set; }

        [StringLength(50)]
        public string P3 { get; set; }

        [StringLength(50)]
        public string P4 { get; set; }

        [StringLength(50)]
        public string P5 { get; set; }

        [StringLength(50)]
        public string P6 { get; set; }

        [StringLength(50)]
        public string FixtureId { get; set; }

        public bool? ExternTest { get; set; }

        public bool? EnableTO { get; set; }

        public bool? Careful { get; set; }

        [StringLength(50)]
        public string TOnr { get; set; }
    }
}