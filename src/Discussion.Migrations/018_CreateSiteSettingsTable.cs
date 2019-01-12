using FluentMigrator;
using FluentMigrator.Builders.Create.Table;

namespace Discussion.Migrations
{
    [Migration(18)]
    public class CreateSiteSettingsTable : CreateEntityTableMigration
    {
        private const string TABLE_NAME = "SiteSettings";

        protected override string TableName()
        {
            return TABLE_NAME;
        }

        protected override void CreateEntityColumns(ICreateTableWithColumnSyntax entityTable)
        {
            entityTable.WithColumn("RequireUserPhoneNumberVerified").AsBoolean()
                .WithColumn("PublicHostName").AsString()

                .WithColumn("EnableNewUserRegistration").AsBoolean()
                .WithColumn("EnableNewTopicCreation").AsBoolean()
                .WithColumn("EnableNewReplyCreation").AsBoolean()
                .WithColumn("IsReadonly").AsBoolean()

                .WithColumn("FooterNoticeLeft").AsString().Nullable()
                .WithColumn("FooterNoticeRight").AsString().Nullable()

                .WithColumn("HeaderLink1Text").AsString().Nullable()
                .WithColumn("HeaderLink1Url").AsString().Nullable()

                .WithColumn("HeaderLink2Text").AsString().Nullable()
                .WithColumn("HeaderLink2Url").AsString().Nullable()

                .WithColumn("HeaderLink3Text").AsString().Nullable()
                .WithColumn("HeaderLink3Url").AsString().Nullable()

                .WithColumn("HeaderLink4Text").AsString().Nullable()
                .WithColumn("HeaderLink4Url").AsString().Nullable();
        }
    }
}