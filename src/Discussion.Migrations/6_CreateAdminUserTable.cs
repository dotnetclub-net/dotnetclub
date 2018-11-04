using FluentMigrator;
using FluentMigrator.Builders.Create.Table;

namespace Discussion.Migrations
{
    [Migration(6)]
    public class CreateAdminUserTable: CreateEntityTableMigration
    {
        public const string TABLE_NAME = "AdminUser";

        protected override string TableName()
        {
            return TABLE_NAME;
        }

        protected override void CreateEntityColumns(ICreateTableWithColumnSyntax entityTable)
        {
            entityTable
                .WithColumn("Username").AsString(50)
                .WithColumn("HashedPassword").AsString(2000).Nullable();
        }
    }
}