namespace G4Fit.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class harddelete : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Services", "HardDelete", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Services", "HardDelete");
        }
    }
}
