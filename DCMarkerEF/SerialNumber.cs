namespace DCMarkerEF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SerialNumber")]
    public partial class SerialNumber
    {
        [Key]
        public int Snr { get; set; }

        public DateTime Issued { get; set; }
    }
}
