namespace G4Fit.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tabby : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "Tabby_PaymentId", c => c.String());
            AddColumn("dbo.Orders", "Tabby_reference_id", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Orders", "Tabby_reference_id");
            DropColumn("dbo.Orders", "Tabby_PaymentId");
        }
    }
}
