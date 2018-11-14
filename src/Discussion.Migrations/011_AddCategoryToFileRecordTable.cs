using FluentMigrator;

namespace Discussion.Migrations
{
    [Migration(11)]
    public class AddCategoryToFileRecordTable: Migration
    {
        public override void Up()
        {
            Alter.Table(CreateFileRecordTable.TABLE_NAME)
                .AddColumn("Category").AsString(255).Nullable();
        }

        public override void Down()
        {
            Delete.Column("Category").FromTable(CreateFileRecordTable.TABLE_NAME);
        }
    }
}