namespace DCMarkerEF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("WeekCode")]
    public partial class WeekCode
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short WeekNo { get; set; }

        [StringLength(1)]
        public string Code { get; set; }
    }
}