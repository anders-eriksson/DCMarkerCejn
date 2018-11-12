namespace DCHistory.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("HistoryData")]
    public partial class HistoryData
    {
        public long Id { get; set; }

        [StringLength(50)]
        public string F1 { get; set; }

        [StringLength(1)]
        public string Kant { get; set; }

        [StringLength(50)]
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

        [StringLength(50)]
        public string TOnr { get; set; }

        public bool? Careful { get; set; }

        [StringLength(30)]
        public string Snr { get; set; }

        public DateTime? Issued { get; set; }

        [StringLength(4)]
        public string DateMark { get; set; }

        [StringLength(4)]
        public string DateMarkLong { get; set; }

        [StringLength(4)]
        public string DateMarkShort { get; set; }
    }
}