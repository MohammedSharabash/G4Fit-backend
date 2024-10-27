namespace G4Fit.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PurposeOfSubscription : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "PurposeOfSubscription", c => c.Int(nullable: false));
            AddColumn("dbo.AspNetUsers", "weight", c => c.Double());
            AddColumn("dbo.AspNetUsers", "length", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "length");
            DropColumn("dbo.AspNetUsers", "weight");
            DropColumn("dbo.Orders", "PurposeOfSubscription");
        }
    }
}
