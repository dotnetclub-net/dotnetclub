using System;
using FluentMigrator;

namespace Discussion.Migrations
{
    [Migration(23)]
    public class RemoveAvatarFileIdFromWeChatAccountTable : Migration
    {
        public override void Up()
        {
            Delete.Column("AvatarFileId").FromTable(CreateWeChatAccountTable.TABLE_NAME);
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
