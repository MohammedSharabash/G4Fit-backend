namespace G4Fit.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class edit : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Services", newName: "Services");
            RenameTable(name: "dbo.ServiceColors", newName: "ServiceColors");
            RenameTable(name: "dbo.ServiceFavourites", newName: "ServiceFavourites");
            RenameTable(name: "dbo.ServiceSizes", newName: "ServiceSizes");
            RenameTable(name: "dbo.ServiceImages", newName: "ServiceImages");
            RenameTable(name: "dbo.ServiceOffers", newName: "ServiceOffers");
            RenameTable(name: "dbo.PromotionalSectionServices", newName: "PromotionalSectionServices");
            RenameColumn(table: "dbo.ServiceColors", name: "ServiceId", newName: "ServiceId");
            RenameColumn(table: "dbo.ServiceFavourites", name: "ServiceId", newName: "ServiceId");
            RenameColumn(table: "dbo.ServiceImages", name: "ServiceId", newName: "ServiceId");
            RenameColumn(table: "dbo.ServiceOffers", name: "ServiceId", newName: "ServiceId");
            RenameColumn(table: "dbo.OrderItems", name: "ServiceId", newName: "ServiceId");
            RenameColumn(table: "dbo.PromotionalSectionServices", name: "ServiceId", newName: "ServiceId");
            RenameColumn(table: "dbo.ServiceSizes", name: "ServiceId", newName: "ServiceId");
            RenameIndex(table: "dbo.ServiceColors", name: "IX_ServiceId", newName: "IX_ServiceId");
            RenameIndex(table: "dbo.OrderItems", name: "IX_ServiceId", newName: "IX_ServiceId");
            RenameIndex(table: "dbo.ServiceFavourites", name: "IX_ServiceId", newName: "IX_ServiceId");
            RenameIndex(table: "dbo.ServiceSizes", name: "IX_ServiceId", newName: "IX_ServiceId");
            RenameIndex(table: "dbo.ServiceImages", name: "IX_ServiceId", newName: "IX_ServiceId");
            RenameIndex(table: "dbo.ServiceOffers", name: "IX_ServiceId", newName: "IX_ServiceId");
            RenameIndex(table: "dbo.PromotionalSectionServices", name: "IX_ServiceId", newName: "IX_ServiceId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.PromotionalSectionServices", name: "IX_ServiceId", newName: "IX_ServiceId");
            RenameIndex(table: "dbo.ServiceOffers", name: "IX_ServiceId", newName: "IX_ServiceId");
            RenameIndex(table: "dbo.ServiceImages", name: "IX_ServiceId", newName: "IX_ServiceId");
            RenameIndex(table: "dbo.ServiceSizes", name: "IX_ServiceId", newName: "IX_ServiceId");
            RenameIndex(table: "dbo.ServiceFavourites", name: "IX_ServiceId", newName: "IX_ServiceId");
            RenameIndex(table: "dbo.OrderItems", name: "IX_ServiceId", newName: "IX_ServiceId");
            RenameIndex(table: "dbo.ServiceColors", name: "IX_ServiceId", newName: "IX_ServiceId");
            RenameColumn(table: "dbo.ServiceSizes", name: "ServiceId", newName: "ServiceId");
            RenameColumn(table: "dbo.PromotionalSectionServices", name: "ServiceId", newName: "ServiceId");
            RenameColumn(table: "dbo.OrderItems", name: "ServiceId", newName: "ServiceId");
            RenameColumn(table: "dbo.ServiceOffers", name: "ServiceId", newName: "ServiceId");
            RenameColumn(table: "dbo.ServiceImages", name: "ServiceId", newName: "ServiceId");
            RenameColumn(table: "dbo.ServiceFavourites", name: "ServiceId", newName: "ServiceId");
            RenameColumn(table: "dbo.ServiceColors", name: "ServiceId", newName: "ServiceId");
            RenameTable(name: "dbo.PromotionalSectionServices", newName: "PromotionalSectionServices");
            RenameTable(name: "dbo.ServiceOffers", newName: "ServiceOffers");
            RenameTable(name: "dbo.ServiceImages", newName: "ServiceImages");
            RenameTable(name: "dbo.ServiceSizes", newName: "ServiceSizes");
            RenameTable(name: "dbo.ServiceFavourites", newName: "ServiceFavourites");
            RenameTable(name: "dbo.ServiceColors", newName: "ServiceColors");
            RenameTable(name: "dbo.Services", newName: "Services");
        }
    }
}
