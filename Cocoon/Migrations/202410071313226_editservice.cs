namespace G4Fit.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class editservice : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Services", "IsTimeBoundService", c => c.Boolean(nullable: false));
            AddColumn("dbo.Services", "ServiceDays", c => c.Long());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Services", "ServiceDays");
            DropColumn("dbo.Services", "IsTimeBoundService");
        }
    }
}
