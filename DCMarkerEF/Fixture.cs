namespace DCMarkerEF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Fixture")]
    public partial class Fixture
    {
        [Key]
        [StringLength(10)]
        public string FixturId { get; set; }

        public double? Xdistance { get; set; }

        public double? Ydistance { get; set; }

        public double? Xcount { get; set; }

        public double? Ycount { get; set; }
    }
}
