namespace G4Fit.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ddddd : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SubCategories", "HardDelete", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SubCategories", "HardDelete");
        }
    }
}
