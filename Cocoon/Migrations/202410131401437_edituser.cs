namespace G4Fit.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class edituser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "IDNumber", c => c.String());
            AddColumn("dbo.AspNetUsers", "QR", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "QR");
            DropColumn("dbo.AspNetUsers", "IDNumber");
        }
    }
}
