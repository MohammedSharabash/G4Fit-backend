namespace G4Fit.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class first : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        NameAr = c.String(),
                        NameEn = c.String(),
                        ImageUrl = c.String(),
                        SortingNumber = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsModified = c.Boolean(nullable: false),
                        RestoredOn = c.DateTime(),
                        DeletedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SubCategories",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        NameAr = c.String(),
                        NameEn = c.String(),
                        ImageUrl = c.String(),
                        CategoryId = c.Long(nullable: false),
                        SortingNumber = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsModified = c.Boolean(nullable: false),
                        RestoredOn = c.DateTime(),
                        DeletedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categories", t => t.CategoryId, cascadeDelete: true)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.Services",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        NameAr = c.String(),
                        NameEn = c.String(),
                        DescriptionAr = c.String(),
                        DescriptionEn = c.String(),
                        OriginalPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        OfferPrice = c.Decimal(precision: 18, scale: 2),
                        IsHidden = c.Boolean(nullable: false),
                        SellCounter = c.Int(nullable: false),
                        Inventory = c.Long(nullable: false),
                        SortingNumber = c.Int(nullable: false),
                        SubCategoryId = c.Long(nullable: false),
                        SupplierId = c.Long(),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsModified = c.Boolean(nullable: false),
                        RestoredOn = c.DateTime(),
                        DeletedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Suppliers", t => t.SupplierId)
                .ForeignKey("dbo.SubCategories", t => t.SubCategoryId, cascadeDelete: true)
                .Index(t => t.SubCategoryId)
                .Index(t => t.SupplierId);
            
            CreateTable(
                "dbo.ServiceColors",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Color = c.String(),
                        ServiceId = c.Long(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsModified = c.Boolean(nullable: false),
                        RestoredOn = c.DateTime(),
                        DeletedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Services", t => t.ServiceId, cascadeDelete: true)
                .Index(t => t.ServiceId);
            
            CreateTable(
                "dbo.OrderItems",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        OrderId = c.Long(nullable: false),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ServiceId = c.Long(nullable: false),
                        ColorId = c.Long(),
                        SizeId = c.Long(),
                        Quantity = c.Int(nullable: false),
                        SubTotal = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsModified = c.Boolean(nullable: false),
                        RestoredOn = c.DateTime(),
                        DeletedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ServiceColors", t => t.ColorId)
                .ForeignKey("dbo.Orders", t => t.OrderId, cascadeDelete: true)
                .ForeignKey("dbo.Services", t => t.ServiceId, cascadeDelete: true)
                .ForeignKey("dbo.ServiceSizes", t => t.SizeId)
                .Index(t => t.OrderId)
                .Index(t => t.ServiceId)
                .Index(t => t.ColorId)
                .Index(t => t.SizeId);
            
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        UnknownUserKeyIdentifier = c.String(),
                        UserId = c.String(maxLength: 128),
                        OrderStatus = c.Int(nullable: false),
                        Code = c.String(),
                        Address = c.String(),
                        CityId = c.Long(),
                        DeliveryFees = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PackageDiscount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PromoDiscount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        WalletDiscount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SubTotal = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Total = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PaymentMethod = c.Int(nullable: false),
                        IsPaid = c.Boolean(nullable: false),
                        PackageId = c.Long(),
                        SMSNotificationsCount = c.Int(nullable: false),
                        LastSMSNotificationDateSent = c.DateTime(),
                        PromoId = c.Long(),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsModified = c.Boolean(nullable: false),
                        RestoredOn = c.DateTime(),
                        DeletedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .ForeignKey("dbo.UserPackages", t => t.PackageId)
                .ForeignKey("dbo.PromoCodes", t => t.PromoId)
                .ForeignKey("dbo.Cities", t => t.CityId)
                .Index(t => t.UserId)
                .Index(t => t.CityId)
                .Index(t => t.PackageId)
                .Index(t => t.PromoId);
            
            CreateTable(
                "dbo.Cities",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        NameAr = c.String(),
                        NameEn = c.String(),
                        DeliveryFees = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CountryId = c.Long(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsModified = c.Boolean(nullable: false),
                        RestoredOn = c.DateTime(),
                        DeletedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Countries", t => t.CountryId, cascadeDelete: true)
                .Index(t => t.CountryId);
            
            CreateTable(
                "dbo.Countries",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        NameAr = c.String(),
                        NameEn = c.String(),
                        ImageUrl = c.String(),
                        TimeZoneId = c.String(),
                        PhoneCode = c.String(),
                        CurrencyAr = c.String(),
                        CurrencyEn = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsModified = c.Boolean(nullable: false),
                        RestoredOn = c.DateTime(),
                        DeletedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                        ImageUrl = c.String(),
                        PhoneNumberCountryCode = c.String(),
                        VerificationCode = c.Int(nullable: false),
                        ForgotPasswordGUID = c.String(),
                        Address = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        LoginType = c.Int(nullable: false),
                        CountryId = c.Long(),
                        CityId = c.Long(),
                        Wallet = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsDeleted = c.Boolean(nullable: false),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Cities", t => t.CityId)
                .ForeignKey("dbo.Countries", t => t.CountryId)
                .Index(t => t.CountryId)
                .Index(t => t.CityId)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.ServiceFavourites",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        UnknownUserKeyIdentifier = c.String(),
                        ServiceId = c.Long(nullable: false),
                        UserId = c.String(maxLength: 128),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsModified = c.Boolean(nullable: false),
                        RestoredOn = c.DateTime(),
                        DeletedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Services", t => t.ServiceId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.ServiceId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Title = c.String(),
                        Body = c.String(),
                        IsSeen = c.Boolean(nullable: false),
                        UserId = c.String(maxLength: 128),
                        NotificationType = c.Int(nullable: false),
                        RequestId = c.Long(),
                        NotificationLink = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsModified = c.Boolean(nullable: false),
                        RestoredOn = c.DateTime(),
                        DeletedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.UserPackages",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        UserId = c.String(maxLength: 128),
                        PackageId = c.Long(nullable: false),
                        StartOn = c.DateTime(nullable: false),
                        FinishOn = c.DateTime(nullable: false),
                        IsPaid = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        NumberOfTimesUsed = c.Int(nullable: false),
                        TrackId = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsModified = c.Boolean(nullable: false),
                        RestoredOn = c.DateTime(),
                        DeletedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Packages", t => t.PackageId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.PackageId);
            
            CreateTable(
                "dbo.Packages",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        NameAr = c.String(),
                        NameEn = c.String(),
                        DescriptionAr = c.String(),
                        DescriptionEn = c.String(),
                        DiscountPercentage = c.Int(nullable: false),
                        MonthlyPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        YearlyPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsModified = c.Boolean(nullable: false),
                        RestoredOn = c.DateTime(),
                        DeletedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PaymentTransactionHistories",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        OrderId = c.Long(),
                        PackageId = c.Long(),
                        UserId = c.String(maxLength: 128),
                        TransactionType = c.Int(nullable: false),
                        PaymentId = c.String(),
                        TranId = c.String(),
                        ECI = c.String(),
                        Result = c.String(),
                        TrackId = c.String(),
                        ResponseCode = c.String(),
                        AuthCode = c.String(),
                        RRN = c.String(),
                        responseHash = c.String(),
                        amount = c.String(),
                        cardBrand = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsModified = c.Boolean(nullable: false),
                        RestoredOn = c.DateTime(),
                        DeletedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Orders", t => t.OrderId)
                .ForeignKey("dbo.Packages", t => t.PackageId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.OrderId)
                .Index(t => t.PackageId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.PromoCodes",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        UserId = c.String(maxLength: 128),
                        Text = c.String(),
                        DiscountMoney = c.Decimal(precision: 18, scale: 2),
                        DiscountPercentage = c.Int(),
                        NumberOfUse = c.Int(nullable: false),
                        CouponQuantity = c.Int(),
                        NumberOfAllowedUsingTimes = c.Int(nullable: false),
                        MinimumOrderCost = c.Decimal(precision: 18, scale: 2),
                        MaximumDiscountMoney = c.Decimal(precision: 18, scale: 2),
                        IsFinished = c.Boolean(nullable: false),
                        FinishOn = c.DateTime(),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsModified = c.Boolean(nullable: false),
                        RestoredOn = c.DateTime(),
                        DeletedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.PromoCodeUsers",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        UserId = c.String(maxLength: 128),
                        PromoId = c.Long(nullable: false),
                        UsedOn = c.DateTime(nullable: false),
                        DiscountGot = c.Decimal(nullable: false, precision: 18, scale: 2),
                        OrderId = c.Long(),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsModified = c.Boolean(nullable: false),
                        RestoredOn = c.DateTime(),
                        DeletedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Orders", t => t.OrderId)
                .ForeignKey("dbo.PromoCodes", t => t.PromoId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.PromoId)
                .Index(t => t.OrderId);
            
            CreateTable(
                "dbo.UserPushTokens",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        PushToken = c.String(),
                        OS = c.Int(nullable: false),
                        UserId = c.String(maxLength: 128),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsModified = c.Boolean(nullable: false),
                        RestoredOn = c.DateTime(),
                        DeletedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Suppliers",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        UserId = c.String(maxLength: 128),
                        Type = c.Int(nullable: false),
                        FirstName = c.String(),
                        LastName = c.String(),
                        StoreName = c.String(),
                        IsAccepted = c.Boolean(),
                        TaxNumber = c.String(),
                        TaxNumberFileUrl = c.String(),
                        CommercialRegister = c.String(),
                        CommercialRegisterFileUrl = c.String(),
                        BankAccount = c.String(),
                        IBAN = c.String(),
                        IdentityFileUrl = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsModified = c.Boolean(nullable: false),
                        RestoredOn = c.DateTime(),
                        DeletedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.UserWallets",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        UserId = c.String(maxLength: 128),
                        TransactionAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TransactionType = c.Int(nullable: false),
                        AttachmentUrl = c.String(),
                        TransactionWay = c.String(),
                        TransactionId = c.String(),
                        OrderId = c.Long(),
                        OrderCode = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsModified = c.Boolean(nullable: false),
                        RestoredOn = c.DateTime(),
                        DeletedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.ServiceSizes",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Size = c.String(),
                        ServiceId = c.Long(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsModified = c.Boolean(nullable: false),
                        RestoredOn = c.DateTime(),
                        DeletedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Services", t => t.ServiceId, cascadeDelete: true)
                .Index(t => t.ServiceId);
            
            CreateTable(
                "dbo.ServiceImages",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ImageUrl = c.String(),
                        ServiceId = c.Long(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsModified = c.Boolean(nullable: false),
                        RestoredOn = c.DateTime(),
                        DeletedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Services", t => t.ServiceId, cascadeDelete: true)
                .Index(t => t.ServiceId);
            
            CreateTable(
                "dbo.ServiceOffers",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Percentage = c.Int(nullable: false),
                        OriginalPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AfterPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ServiceId = c.Long(nullable: false),
                        NumberOfUse = c.Int(nullable: false),
                        IsFinished = c.Boolean(nullable: false),
                        FinishOn = c.DateTime(),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsModified = c.Boolean(nullable: false),
                        RestoredOn = c.DateTime(),
                        DeletedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Services", t => t.ServiceId, cascadeDelete: true)
                .Index(t => t.ServiceId);
            
            CreateTable(
                "dbo.PromotionalSectionServices",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        PromotionalSectionId = c.Long(nullable: false),
                        ServiceId = c.Long(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsModified = c.Boolean(nullable: false),
                        RestoredOn = c.DateTime(),
                        DeletedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Services", t => t.ServiceId, cascadeDelete: true)
                .ForeignKey("dbo.PromotionalSections", t => t.PromotionalSectionId, cascadeDelete: true)
                .Index(t => t.PromotionalSectionId)
                .Index(t => t.ServiceId);
            
            CreateTable(
                "dbo.PromotionalSections",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        NameAr = c.String(),
                        NameEn = c.String(),
                        SortingNumber = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsModified = c.Boolean(nullable: false),
                        RestoredOn = c.DateTime(),
                        DeletedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CompanyDatas",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        VideoUrl = c.String(),
                        DescriptionAr = c.String(),
                        DescriptionEn = c.String(),
                        VisionAr = c.String(),
                        VisionEn = c.String(),
                        MessageAr = c.String(),
                        MessageEn = c.String(),
                        ValuesAr = c.String(),
                        ValuesEn = c.String(),
                        OtherNotesAr = c.String(),
                        OtherNotesEn = c.String(),
                        FooterTextAr = c.String(),
                        FooterTextEn = c.String(),
                        WhatsApp = c.String(),
                        Twitter = c.String(),
                        Instagram = c.String(),
                        Facebook = c.String(),
                        SnapChat = c.String(),
                        TikTok = c.String(),
                        YouTube = c.String(),
                        Website = c.String(),
                        Hotline = c.String(),
                        AddressAr = c.String(),
                        AddressEn = c.String(),
                        Email = c.String(),
                        PrivacyPolicyAr = c.String(),
                        PrivacyPolicyEn = c.String(),
                        TermsConditionsAr = c.String(),
                        TermsConditionsEn = c.String(),
                        DeliveringConditionsAr = c.String(),
                        DeliveringConditionsEn = c.String(),
                        ReturnAndExchangePolicyAr = c.String(),
                        ReturnAndExchangePolicyEn = c.String(),
                        CustomerServiceAr = c.String(),
                        CustomerServiceEn = c.String(),
                        HowToOrderAr = c.String(),
                        HowToOrderEn = c.String(),
                        InformationAr = c.String(),
                        InformationEn = c.String(),
                        SellWithUsAr = c.String(),
                        SellWithUsEn = c.String(),
                        SupplierSellingPolicyAr = c.String(),
                        SupplierSellingPolicyEn = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsModified = c.Boolean(nullable: false),
                        RestoredOn = c.DateTime(),
                        DeletedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.FAQs",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        QuestionAr = c.String(),
                        QuestionEn = c.String(),
                        AnswerAr = c.String(),
                        AnswerEn = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsModified = c.Boolean(nullable: false),
                        RestoredOn = c.DateTime(),
                        DeletedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.Sliders",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ImageUrl = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(),
                        IsModified = c.Boolean(nullable: false),
                        RestoredOn = c.DateTime(),
                        DeletedOn = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Services", "SubCategoryId", "dbo.SubCategories");
            DropForeignKey("dbo.PromotionalSectionServices", "PromotionalSectionId", "dbo.PromotionalSections");
            DropForeignKey("dbo.PromotionalSectionServices", "ServiceId", "dbo.Services");
            DropForeignKey("dbo.ServiceOffers", "ServiceId", "dbo.Services");
            DropForeignKey("dbo.ServiceImages", "ServiceId", "dbo.Services");
            DropForeignKey("dbo.ServiceColors", "ServiceId", "dbo.Services");
            DropForeignKey("dbo.ServiceSizes", "ServiceId", "dbo.Services");
            DropForeignKey("dbo.OrderItems", "SizeId", "dbo.ServiceSizes");
            DropForeignKey("dbo.OrderItems", "ServiceId", "dbo.Services");
            DropForeignKey("dbo.OrderItems", "OrderId", "dbo.Orders");
            DropForeignKey("dbo.Orders", "CityId", "dbo.Cities");
            DropForeignKey("dbo.UserWallets", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Suppliers", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Services", "SupplierId", "dbo.Suppliers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserPushTokens", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.PromoCodeUsers", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.PromoCodeUsers", "PromoId", "dbo.PromoCodes");
            DropForeignKey("dbo.PromoCodeUsers", "OrderId", "dbo.Orders");
            DropForeignKey("dbo.PromoCodes", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Orders", "PromoId", "dbo.PromoCodes");
            DropForeignKey("dbo.UserPackages", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserPackages", "PackageId", "dbo.Packages");
            DropForeignKey("dbo.PaymentTransactionHistories", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.PaymentTransactionHistories", "PackageId", "dbo.Packages");
            DropForeignKey("dbo.PaymentTransactionHistories", "OrderId", "dbo.Orders");
            DropForeignKey("dbo.Orders", "PackageId", "dbo.UserPackages");
            DropForeignKey("dbo.Orders", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Notifications", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ServiceFavourites", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ServiceFavourites", "ServiceId", "dbo.Services");
            DropForeignKey("dbo.AspNetUsers", "CountryId", "dbo.Countries");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUsers", "CityId", "dbo.Cities");
            DropForeignKey("dbo.Cities", "CountryId", "dbo.Countries");
            DropForeignKey("dbo.OrderItems", "ColorId", "dbo.ServiceColors");
            DropForeignKey("dbo.SubCategories", "CategoryId", "dbo.Categories");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.PromotionalSectionServices", new[] { "ServiceId" });
            DropIndex("dbo.PromotionalSectionServices", new[] { "PromotionalSectionId" });
            DropIndex("dbo.ServiceOffers", new[] { "ServiceId" });
            DropIndex("dbo.ServiceImages", new[] { "ServiceId" });
            DropIndex("dbo.ServiceSizes", new[] { "ServiceId" });
            DropIndex("dbo.UserWallets", new[] { "UserId" });
            DropIndex("dbo.Suppliers", new[] { "UserId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.UserPushTokens", new[] { "UserId" });
            DropIndex("dbo.PromoCodeUsers", new[] { "OrderId" });
            DropIndex("dbo.PromoCodeUsers", new[] { "PromoId" });
            DropIndex("dbo.PromoCodeUsers", new[] { "UserId" });
            DropIndex("dbo.PromoCodes", new[] { "UserId" });
            DropIndex("dbo.PaymentTransactionHistories", new[] { "UserId" });
            DropIndex("dbo.PaymentTransactionHistories", new[] { "PackageId" });
            DropIndex("dbo.PaymentTransactionHistories", new[] { "OrderId" });
            DropIndex("dbo.UserPackages", new[] { "PackageId" });
            DropIndex("dbo.UserPackages", new[] { "UserId" });
            DropIndex("dbo.Notifications", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.ServiceFavourites", new[] { "UserId" });
            DropIndex("dbo.ServiceFavourites", new[] { "ServiceId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUsers", new[] { "CityId" });
            DropIndex("dbo.AspNetUsers", new[] { "CountryId" });
            DropIndex("dbo.Cities", new[] { "CountryId" });
            DropIndex("dbo.Orders", new[] { "PromoId" });
            DropIndex("dbo.Orders", new[] { "PackageId" });
            DropIndex("dbo.Orders", new[] { "CityId" });
            DropIndex("dbo.Orders", new[] { "UserId" });
            DropIndex("dbo.OrderItems", new[] { "SizeId" });
            DropIndex("dbo.OrderItems", new[] { "ColorId" });
            DropIndex("dbo.OrderItems", new[] { "ServiceId" });
            DropIndex("dbo.OrderItems", new[] { "OrderId" });
            DropIndex("dbo.ServiceColors", new[] { "ServiceId" });
            DropIndex("dbo.Services", new[] { "SupplierId" });
            DropIndex("dbo.Services", new[] { "SubCategoryId" });
            DropIndex("dbo.SubCategories", new[] { "CategoryId" });
            DropTable("dbo.Sliders");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.FAQs");
            DropTable("dbo.CompanyDatas");
            DropTable("dbo.PromotionalSections");
            DropTable("dbo.PromotionalSectionServices");
            DropTable("dbo.ServiceOffers");
            DropTable("dbo.ServiceImages");
            DropTable("dbo.ServiceSizes");
            DropTable("dbo.UserWallets");
            DropTable("dbo.Suppliers");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.UserPushTokens");
            DropTable("dbo.PromoCodeUsers");
            DropTable("dbo.PromoCodes");
            DropTable("dbo.PaymentTransactionHistories");
            DropTable("dbo.Packages");
            DropTable("dbo.UserPackages");
            DropTable("dbo.Notifications");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.ServiceFavourites");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Countries");
            DropTable("dbo.Cities");
            DropTable("dbo.Orders");
            DropTable("dbo.OrderItems");
            DropTable("dbo.ServiceColors");
            DropTable("dbo.Services");
            DropTable("dbo.SubCategories");
            DropTable("dbo.Categories");
        }
    }
}
