namespace DCMarkerEF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("QuarterCode")]
    public partial class QuarterCode
    {
        [Key]
        [StringLength(4)]
        public string QYear { get; set; }

        [StringLength(2)]
        public string Q1 { get; set; }

        [StringLength(2)]
        public string Q2 { get; set; }

        [StringLength(2)]
        public string Q3 { get; set; }

        [StringLength(2)]
        public string Q4 { get; set; }
    }
}