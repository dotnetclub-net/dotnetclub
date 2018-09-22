using FluentMigrator.Builders.Create.Table;

namespace Discussion.Migrations
{
    public static class EntityBaseColumesExtensions
    {
        public static ICreateTableWithColumnSyntax WithEntityBaseColumes(this ICreateTableWithColumnSyntax table)
        {
            return table.WithColumn("Id").AsInt32().PrimaryKey().Identity().NotNullable()
                        .WithColumn("CreatedAtUtc").AsDateTime().NotNullable()
                        .WithColumn("ModifiedAtUtc").AsDateTime().NotNullable();
        }
    }
}