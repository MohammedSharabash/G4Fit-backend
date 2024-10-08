namespace G4Fit.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class editservice0 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Services", "ServiceDays", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Services", "ServiceDays", c => c.Long());
        }
    }
}
