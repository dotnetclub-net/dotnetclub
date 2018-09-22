using FluentMigrator;

namespace Discussion.Migrations
{
    [Migration(1)]
    public class CreateUserTable: Migration
    {
        public override void Up()
        {
            Create.Table("Users")
                .WithColumn("abcd")
                .AsString();
        }

        public override void Down()
        {
            Delete.Table("Users");
        }
    }
}