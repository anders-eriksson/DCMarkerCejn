namespace DCMarkerEF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("AssemblyCode")]
    public partial class AssemblyCode
    {
        [Key]
        [StringLength(3)]
        public string Unit { get; set; }

        [StringLength(1)]
        public string Code { get; set; }
    }
}
