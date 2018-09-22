using FluentMigrator;
using FluentMigrator.Builders.Create.Table;

namespace Discussion.Migrations
{
    [Migration(1)]
    public class CreateArticleTable: CreateEntityTableMigration
    {
        protected override string TableName()
        {
            // Entity Framework Core 2 不再默认使用复数表名
            // 详细情况请参阅：https://stackoverflow.com/a/37502978/1817042
            return "Article";
        }

        protected override void CreateEntityColumns(ICreateTableWithColumnSyntax entityTable)
        {
            entityTable.WithColumn("Title").AsString(255);
        }
    }
}