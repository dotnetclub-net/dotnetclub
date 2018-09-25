using FluentMigrator;
using FluentMigrator.Builders.Create.Table;

namespace Discussion.Migrations
{
    [Migration(4)]
    public class CreateReplyTable: CreateEntityTableMigration
    {
        protected override string TableName()
        {
            return "Reply";
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