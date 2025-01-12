namespace G4Fit.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserInBodyOperation : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserInBodyOperations",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        UserId = c.String(maxLength: 128),
                        OrderId = c.Long(nullable: false),
                        ImageUrl = c.String(),
                        Note = c.String(),
                        Confirmed = c.Boolean(nullable: false),
                        ConfirmationCode = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsModified = c.Boolean(nullable: false),
                        RestoredOn = c.DateTime(),
                        DeletedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Orders", t => t.OrderId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.OrderId);
            
            AddColumn("dbo.Services", "InBodyCount", c => c.Int(nullable: false));
            AddColumn("dbo.Orders", "InBodyCount", c => c.Int(nullable: false));
            AddColumn("dbo.Orders", "InBodyUsedCount", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserInBodyOperations", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserInBodyOperations", "OrderId", "dbo.Orders");
            DropIndex("dbo.UserInBodyOperations", new[] { "OrderId" });
            DropIndex("dbo.UserInBodyOperations", new[] { "UserId" });
            DropColumn("dbo.Orders", "InBodyUsedCount");
            DropColumn("dbo.Orders", "InBodyCount");
            DropColumn("dbo.Services", "InBodyCount");
            DropTable("dbo.UserInBodyOperations");
        }
    }
}
