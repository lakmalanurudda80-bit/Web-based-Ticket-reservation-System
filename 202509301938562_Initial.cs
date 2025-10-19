namespace EventTicketSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BookingDetails",
                c => new
                    {
                        BookingDetailId = c.Int(nullable: false, identity: true),
                        BookingId = c.Int(nullable: false),
                        TicketId = c.Int(nullable: false),
                        Quantity = c.Int(nullable: false),
                        UnitPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        QRCodeData = c.String(),
                        IsScanned = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.BookingDetailId)
                .ForeignKey("dbo.Bookings", t => t.BookingId)
                .ForeignKey("dbo.Tickets", t => t.TicketId)
                .Index(t => t.BookingId)
                .Index(t => t.TicketId);
            
            CreateTable(
                "dbo.Bookings",
                c => new
                    {
                        BookingId = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        BookingDate = c.DateTime(nullable: false),
                        TotalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Status = c.String(nullable: false),
                        PaymentIntentId = c.String(),
                        LoyaltyPointsEarned = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.BookingId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FirstName = c.String(nullable: false, maxLength: 100),
                        LastName = c.String(nullable: false, maxLength: 100),
                        Address = c.String(maxLength: 200),
                        LoyaltyPoints = c.Int(nullable: false),
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
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
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
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Tickets",
                c => new
                    {
                        TicketId = c.Int(nullable: false, identity: true),
                        EventId = c.Int(nullable: false),
                        TicketType = c.String(nullable: false, maxLength: 100),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        QuantityAvailable = c.Int(nullable: false),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.TicketId)
                .ForeignKey("dbo.Events", t => t.EventId)
                .Index(t => t.EventId);
            
            CreateTable(
                "dbo.Events",
                c => new
                    {
                        EventId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 200),
                        Description = c.String(nullable: false),
                        EventDate = c.DateTime(nullable: false),
                        EventTime = c.DateTime(nullable: false),
                        VenueId = c.Int(nullable: false),
                        CategoryId = c.Int(nullable: false),
                        OrganizerId = c.Int(nullable: false),
                        BasePrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ImagePath = c.String(),
                        TotalTickets = c.Int(nullable: false),
                        AvailableTickets = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.EventId)
                .ForeignKey("dbo.Categories", t => t.CategoryId)
                .ForeignKey("dbo.Organizers", t => t.OrganizerId)
                .ForeignKey("dbo.Venues", t => t.VenueId)
                .Index(t => t.VenueId)
                .Index(t => t.CategoryId)
                .Index(t => t.OrganizerId);
            
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        CategoryId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.CategoryId);
            
            CreateTable(
                "dbo.Organizers",
                c => new
                    {
                        OrganizerId = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        CompanyName = c.String(nullable: false, maxLength: 200),
                        Description = c.String(),
                        ContactEmail = c.String(),
                        ContactPhone = c.String(),
                        Address = c.String(),
                        IsApproved = c.Boolean(nullable: false),
                        RegistrationDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.OrganizerId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Venues",
                c => new
                    {
                        VenueId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 200),
                        Address = c.String(nullable: false),
                        City = c.String(nullable: false),
                        Capacity = c.Int(nullable: false),
                        Facilities = c.String(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.VenueId);
            
            CreateTable(
                "dbo.Promotions",
                c => new
                    {
                        PromotionId = c.Int(nullable: false, identity: true),
                        Code = c.String(nullable: false, maxLength: 100),
                        Description = c.String(nullable: false, maxLength: 200),
                        DiscountPercentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        EventId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.PromotionId)
                .ForeignKey("dbo.Events", t => t.EventId)
                .Index(t => t.EventId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Promotions", "EventId", "dbo.Events");
            DropForeignKey("dbo.BookingDetails", "TicketId", "dbo.Tickets");
            DropForeignKey("dbo.Tickets", "EventId", "dbo.Events");
            DropForeignKey("dbo.Events", "VenueId", "dbo.Venues");
            DropForeignKey("dbo.Events", "OrganizerId", "dbo.Organizers");
            DropForeignKey("dbo.Organizers", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Events", "CategoryId", "dbo.Categories");
            DropForeignKey("dbo.BookingDetails", "BookingId", "dbo.Bookings");
            DropForeignKey("dbo.Bookings", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.Promotions", new[] { "EventId" });
            DropIndex("dbo.Organizers", new[] { "UserId" });
            DropIndex("dbo.Events", new[] { "OrganizerId" });
            DropIndex("dbo.Events", new[] { "CategoryId" });
            DropIndex("dbo.Events", new[] { "VenueId" });
            DropIndex("dbo.Tickets", new[] { "EventId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.Bookings", new[] { "UserId" });
            DropIndex("dbo.BookingDetails", new[] { "TicketId" });
            DropIndex("dbo.BookingDetails", new[] { "BookingId" });
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Promotions");
            DropTable("dbo.Venues");
            DropTable("dbo.Organizers");
            DropTable("dbo.Categories");
            DropTable("dbo.Events");
            DropTable("dbo.Tickets");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Bookings");
            DropTable("dbo.BookingDetails");
        }
    }
}
