namespace G4Fit.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class orderEdit : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Services", "SpecialPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Services", "SpecialOfferPrice", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.Orders", "UserType", c => c.Int(nullable: false));
            AddColumn("dbo.Orders", "UserTypeImageUrl", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Orders", "UserTypeImageUrl");
            DropColumn("dbo.Orders", "UserType");
            DropColumn("dbo.Services", "SpecialOfferPrice");
            DropColumn("dbo.Services", "SpecialPrice");
        }
    }
}
