﻿namespace sReportsV2.Domain.Sql.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTimezoneToOrganization : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Organizations", "TimeZone", c => c.String());
            AddColumn("dbo.Organizations", "TimeZoneOffset", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Organizations", "TimeZoneOffset");
            DropColumn("dbo.Organizations", "TimeZone");
        }
    }
}
