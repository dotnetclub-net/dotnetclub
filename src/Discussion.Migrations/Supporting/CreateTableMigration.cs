using FluentMigrator;
using FluentMigrator.Builders.Create.Table;

namespace Discussion.Migrations
{
    public abstract class CreateTableMigration : Migration
    {
        protected abstract string TableName();
        protected abstract void CreateColumns(ICreateTableWithColumnOrSchemaOrDescriptionSyntax table);
        
        public override void Up()
        {
            var tableSyntax = Create.Table(TableName());
            CreateColumns(tableSyntax);
        }

        public override void Down()
        {
            Delete.Table(TableName());
        }
    }
    
    public abstract class CreateEntityTableMigration : CreateTableMigration
    {
        protected override void CreateColumns(ICreateTableWithColumnOrSchemaOrDescriptionSyntax table)
        {
            var tableSyntax = table.WithEntityBaseColumes();
            CreateEntityColumns(tableSyntax);
        }

        protected abstract void CreateEntityColumns(ICreateTableWithColumnSyntax entityTable);

    }
}