using FluentMigrator;
using FluentMigrator.Builders.Create.Table;

namespace Discussion.Migrations
{
    [Migration(4)]
    public class CreateReplyTable: CreateEntityTableMigration
    {
        public const string TABLE_NAME = "Reply";

        protected override string TableName()
        {
            return TABLE_NAME;
        }

        protected override void CreateEntityColumns(ICreateTableWithColumnSyntax entityTable)
        {
            entityTable
                .WithColumn("TopicId").AsInt32()
                .WithColumn("CreatedBy").AsInt32().Nullable()
                .WithColumn("Content").AsString(int.MaxValue);
        }
    }
}