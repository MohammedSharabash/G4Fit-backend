namespace G4Fit.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tamara : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "TamaraPaymentId", c => c.String());
            AddColumn("dbo.Orders", "Tamara_reference_id", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Orders", "Tamara_reference_id");
            DropColumn("dbo.Orders", "TamaraPaymentId");
        }
    }
}
