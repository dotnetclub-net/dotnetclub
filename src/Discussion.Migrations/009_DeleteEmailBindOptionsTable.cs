using System;
using FluentMigrator;

namespace Discussion.Migrations
{
    [Migration(9)]
    public class DeleteEmailBindOptionsTable : Migration
    {
        public override void Up()
        {
            Delete.Table(CreateEmailBindOptionsTable.TABLE_NAME);
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}