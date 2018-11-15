using FluentMigrator;
using FluentMigrator.Builders.Create.Table;

namespace Discussion.Migrations
{
    [Migration(10)]
    public class CreateFileRecordTable: CreateEntityTableMigration
    {
        internal const string TABLE_NAME = "FileRecord";
        protected override string TableName()
        {
            return TABLE_NAME;
        }

        protected override void CreateEntityColumns(ICreateTableWithColumnSyntax entityTable)
        {
            entityTable
                .WithColumn("UploadedBy").AsInt32()
                .WithColumn("Size").AsInt64()
                .WithColumn("OriginalName").AsString()
                .WithColumn("StoragePath").AsString(2000);
        }
    }
}