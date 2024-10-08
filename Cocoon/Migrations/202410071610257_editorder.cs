namespace G4Fit.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class editorder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OrderItems", "StartDate", c => c.DateTime());
            AlterColumn("dbo.Services", "ServiceDays", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Services", "ServiceDays", c => c.Long(nullable: false));
            DropColumn("dbo.OrderItems", "StartDate");
        }
    }
}
