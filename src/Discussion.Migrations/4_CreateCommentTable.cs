using FluentMigrator;
using FluentMigrator.Builders.Create.Table;

namespace Discussion.Migrations
{
    [Migration(4)]
    public class CreateCommentTable: CreateEntityTableMigration
    {
        protected override string TableName()
        {
            return "Comment";
        }

        protected override void CreateEntityColumns(ICreateTableWithColumnSyntax entityTable)
        {
            entityTable
                .WithColumn("TopicId").AsInt32()
                .WithColumn("CreatedBy").AsInt32()
                .WithColumn("Content").AsString(int.MaxValue);
        }
    }
}