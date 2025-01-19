namespace G4Fit.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConnectedCategoryId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SubCategories", "ConnectedToAnotherCategory", c => c.Boolean(nullable: false));
            AddColumn("dbo.SubCategories", "ConnectedCategoryId", c => c.Long());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SubCategories", "ConnectedCategoryId");
            DropColumn("dbo.SubCategories", "ConnectedToAnotherCategory");
        }
    }
}
