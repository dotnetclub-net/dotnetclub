using FluentMigrator;
using FluentMigrator.Builders.Create.Table;

namespace Discussion.Migrations
{
    [Migration(2)]
    public class CreateUserTable: CreateEntityTableMigration
    {
        public const string TABLE_NAME = "User";
        
        protected override string TableName()
        {
            return TABLE_NAME;
        }

        protected override void CreateEntityColumns(ICreateTableWithColumnSyntax entityTable)
        {
           entityTable.WithColumn("UserName").AsString(50)
               .WithColumn("DisplayName").AsString(50).Nullable()
               .WithColumn("HashedPassword").AsString(2000).Nullable()
               .WithColumn("LastSeenAt").AsDateTime().Nullable()
               .WithColumn("EmailAddress").AsString(50).Nullable();
        }

    }
}