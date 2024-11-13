namespace G4Fit.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class about : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CompanyDatas", "aboutConditionsAr", c => c.String());
            AddColumn("dbo.CompanyDatas", "aboutConditionsEn", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.CompanyDatas", "aboutConditionsEn");
            DropColumn("dbo.CompanyDatas", "aboutConditionsAr");
        }
    }
}
