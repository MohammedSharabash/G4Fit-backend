namespace G4Fit.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PurposeOfSubscription2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Orders", "PurposeOfSubscription", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Orders", "PurposeOfSubscription", c => c.Int(nullable: false));
        }
    }
}
