namespace DCHistory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.HistoryData",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        F1 = c.String(maxLength: 50),
                        Kant = c.String(maxLength: 1, fixedLength: true, unicode: false),
                        F2 = c.String(maxLength: 50),
                        F3 = c.String(maxLength: 50),
                        F4 = c.String(maxLength: 50),
                        F5 = c.String(maxLength: 50),
                        F6 = c.String(maxLength: 50),
                        F7 = c.String(maxLength: 50),
                        F8 = c.String(maxLength: 50),
                        F9 = c.String(maxLength: 50),
                        F10 = c.String(maxLength: 50),
                        BC1 = c.String(maxLength: 50),
                        BC2 = c.String(maxLength: 50),
                        Template = c.String(maxLength: 50),
                        P1 = c.String(maxLength: 50),
                        P2 = c.String(maxLength: 50),
                        P3 = c.String(maxLength: 50),
                        P4 = c.String(maxLength: 50),
                        P5 = c.String(maxLength: 50),
                        P6 = c.String(maxLength: 50),
                        FixtureId = c.String(maxLength: 50),
                        ExternTest = c.Boolean(),
                        EnableTO = c.Boolean(),
                        TOnr = c.String(maxLength: 50),
                        Snr = c.String(maxLength: 30),
                        Issued = c.DateTime(),
                        DateMark = c.String(maxLength: 4, fixedLength: true, unicode: false),
                        DateMarkLong = c.String(maxLength: 4, fixedLength: true, unicode: false),
                        DateMarkShort = c.String(maxLength: 4, fixedLength: true, unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.HistoryData");
        }
    }
}
