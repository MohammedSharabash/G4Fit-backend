namespace G4Fit.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class nowdata : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Services", "ServiceFreezingDays", c => c.Int(nullable: false));
            AddColumn("dbo.Services", "ServiceFreezingTimes", c => c.Int(nullable: false));
            AddColumn("dbo.OrderItems", "FreezingDays", c => c.Int(nullable: false));
            AddColumn("dbo.OrderItems", "FreezingTimes", c => c.Int(nullable: false));
            AddColumn("dbo.OrderItems", "RemainServiceDays", c => c.Int(nullable: false));
            AddColumn("dbo.OrderItems", "RemainFreezingDays", c => c.Int(nullable: false));
            AddColumn("dbo.OrderItems", "RemainFreezingTimes", c => c.Int(nullable: false));
            AddColumn("dbo.Orders", "Frezzed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Orders", "Frezzed");
            DropColumn("dbo.OrderItems", "RemainFreezingTimes");
            DropColumn("dbo.OrderItems", "RemainFreezingDays");
            DropColumn("dbo.OrderItems", "RemainServiceDays");
            DropColumn("dbo.OrderItems", "FreezingTimes");
            DropColumn("dbo.OrderItems", "FreezingDays");
            DropColumn("dbo.Services", "ServiceFreezingTimes");
            DropColumn("dbo.Services", "ServiceFreezingDays");
        }
    }
}
