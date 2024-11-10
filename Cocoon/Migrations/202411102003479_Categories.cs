namespace G4Fit.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Categories : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.SubCategories", "CategoryId", "dbo.Categories");
            DropIndex("dbo.SubCategories", new[] { "CategoryId" });
            RenameColumn(table: "dbo.SubCategories", name: "CategoryId", newName: "Category_Id");
            AlterColumn("dbo.SubCategories", "Category_Id", c => c.Long());
            CreateIndex("dbo.SubCategories", "Category_Id");
            AddForeignKey("dbo.SubCategories", "Category_Id", "dbo.Categories", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SubCategories", "Category_Id", "dbo.Categories");
            DropIndex("dbo.SubCategories", new[] { "Category_Id" });
            AlterColumn("dbo.SubCategories", "Category_Id", c => c.Long(nullable: false));
            RenameColumn(table: "dbo.SubCategories", name: "Category_Id", newName: "CategoryId");
            CreateIndex("dbo.SubCategories", "CategoryId");
            AddForeignKey("dbo.SubCategories", "CategoryId", "dbo.Categories", "Id", cascadeDelete: true);
        }
    }
}
