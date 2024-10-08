namespace G4Fit.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class edit : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Products", newName: "Services");
            RenameTable(name: "dbo.ProductColors", newName: "ServiceColors");
            RenameTable(name: "dbo.ProductFavourites", newName: "ServiceFavourites");
            RenameTable(name: "dbo.ProductSizes", newName: "ServiceSizes");
            RenameTable(name: "dbo.ProductImages", newName: "ServiceImages");
            RenameTable(name: "dbo.ProductOffers", newName: "ServiceOffers");
            RenameTable(name: "dbo.PromotionalSectionProducts", newName: "PromotionalSectionServices");
            RenameColumn(table: "dbo.ServiceColors", name: "ProductId", newName: "ServiceId");
            RenameColumn(table: "dbo.ServiceFavourites", name: "ProductId", newName: "ServiceId");
            RenameColumn(table: "dbo.ServiceImages", name: "ProductId", newName: "ServiceId");
            RenameColumn(table: "dbo.ServiceOffers", name: "ProductId", newName: "ServiceId");
            RenameColumn(table: "dbo.OrderItems", name: "ProductId", newName: "ServiceId");
            RenameColumn(table: "dbo.PromotionalSectionServices", name: "ProductId", newName: "ServiceId");
            RenameColumn(table: "dbo.ServiceSizes", name: "ProductId", newName: "ServiceId");
            RenameIndex(table: "dbo.ServiceColors", name: "IX_ProductId", newName: "IX_ServiceId");
            RenameIndex(table: "dbo.OrderItems", name: "IX_ProductId", newName: "IX_ServiceId");
            RenameIndex(table: "dbo.ServiceFavourites", name: "IX_ProductId", newName: "IX_ServiceId");
            RenameIndex(table: "dbo.ServiceSizes", name: "IX_ProductId", newName: "IX_ServiceId");
            RenameIndex(table: "dbo.ServiceImages", name: "IX_ProductId", newName: "IX_ServiceId");
            RenameIndex(table: "dbo.ServiceOffers", name: "IX_ProductId", newName: "IX_ServiceId");
            RenameIndex(table: "dbo.PromotionalSectionServices", name: "IX_ProductId", newName: "IX_ServiceId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.PromotionalSectionServices", name: "IX_ServiceId", newName: "IX_ProductId");
            RenameIndex(table: "dbo.ServiceOffers", name: "IX_ServiceId", newName: "IX_ProductId");
            RenameIndex(table: "dbo.ServiceImages", name: "IX_ServiceId", newName: "IX_ProductId");
            RenameIndex(table: "dbo.ServiceSizes", name: "IX_ServiceId", newName: "IX_ProductId");
            RenameIndex(table: "dbo.ServiceFavourites", name: "IX_ServiceId", newName: "IX_ProductId");
            RenameIndex(table: "dbo.OrderItems", name: "IX_ServiceId", newName: "IX_ProductId");
            RenameIndex(table: "dbo.ServiceColors", name: "IX_ServiceId", newName: "IX_ProductId");
            RenameColumn(table: "dbo.ServiceSizes", name: "ServiceId", newName: "ProductId");
            RenameColumn(table: "dbo.PromotionalSectionServices", name: "ServiceId", newName: "ProductId");
            RenameColumn(table: "dbo.OrderItems", name: "ServiceId", newName: "ProductId");
            RenameColumn(table: "dbo.ServiceOffers", name: "ServiceId", newName: "ProductId");
            RenameColumn(table: "dbo.ServiceImages", name: "ServiceId", newName: "ProductId");
            RenameColumn(table: "dbo.ServiceFavourites", name: "ServiceId", newName: "ProductId");
            RenameColumn(table: "dbo.ServiceColors", name: "ServiceId", newName: "ProductId");
            RenameTable(name: "dbo.PromotionalSectionServices", newName: "PromotionalSectionProducts");
            RenameTable(name: "dbo.ServiceOffers", newName: "ProductOffers");
            RenameTable(name: "dbo.ServiceImages", newName: "ProductImages");
            RenameTable(name: "dbo.ServiceSizes", newName: "ProductSizes");
            RenameTable(name: "dbo.ServiceFavourites", newName: "ProductFavourites");
            RenameTable(name: "dbo.ServiceColors", newName: "ProductColors");
            RenameTable(name: "dbo.Services", newName: "Products");
        }
    }
}
