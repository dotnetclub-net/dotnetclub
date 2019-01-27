using FluentMigrator;
using FluentMigrator.Builders.Create.Table;

namespace Discussion.Migrations
{
    [Migration(19)]
    public class CreateWeChatAccountTable : CreateEntityTableMigration
    {
        private const string TABLE_NAME = "WeChatAccount";

        protected override string TableName()
        {
            return TABLE_NAME;
        }

        protected override void CreateEntityColumns(ICreateTableWithColumnSyntax entityTable)
        {
            entityTable.WithColumn("WxId").AsString()
                .WithColumn("NickName").AsString()
                .WithColumn("WxAccount").AsString().Nullable()
                .WithColumn("AvatarFileId").AsInt32().Nullable();
        }
    }
}