using FluentMigrator;
using FluentMigrator.Builders.Create.Table;

namespace Discussion.Migrations
{
    [Migration(10)]
    public class CreateFileRecordTable: CreateEntityTableMigration
    {
        protected override string TableName()
        {
            return "FileRecord";
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