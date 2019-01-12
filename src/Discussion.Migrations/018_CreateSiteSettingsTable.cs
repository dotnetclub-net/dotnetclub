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

                .WithColumn("FooterNoticeLeft").AsString()
                .WithColumn("FooterNoticeRight").AsString()

                .WithColumn("HeaderLink1Text").AsString()
                .WithColumn("HeaderLink1Url").AsString()

                .WithColumn("HeaderLink2Text").AsString()
                .WithColumn("HeaderLink2Url").AsString()

                .WithColumn("HeaderLink3Text").AsString()
                .WithColumn("HeaderLink3Url").AsString()

                .WithColumn("HeaderLink4Text").AsString()
                .WithColumn("HeaderLink4Url").AsString();
        }
    }
}