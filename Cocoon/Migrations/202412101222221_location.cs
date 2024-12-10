namespace G4Fit.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class location : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CompanyDatas", "Latitude", c => c.Double());
            AddColumn("dbo.CompanyDatas", "Longitude", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.CompanyDatas", "Longitude");
            DropColumn("dbo.CompanyDatas", "Latitude");
        }
    }
}
