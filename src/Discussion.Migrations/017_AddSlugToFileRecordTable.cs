using System;
using System.Collections.Generic;
using FluentMigrator;

namespace Discussion.Migrations
{
    [Migration(17)]
    public class AddSlugToFileRecordTable: Migration
    {
        public override void Up()
        {
            Alter.Table(CreateFileRecordTable.TABLE_NAME)
                .AddColumn("Slug").AsString()
                .Nullable();
            
            Execute.WithConnection((conn, trans) =>
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT Id FROM {CreateFileRecordTable.TABLE_NAME}";

                    var idList = new List<int>();
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        idList.Add(reader.GetInt32(0));
                    }

                    reader.Close();

                    idList.ForEach(id =>
                    {
                        // ReSharper disable AccessToDisposedClosure
                        var updateCmd = $"UPDATE {CreateFileRecordTable.TABLE_NAME} SET Slug='{Guid.NewGuid():N}' WHERE Id={id}";
                        cmd.CommandText = updateCmd;
                        cmd.ExecuteNonQuery();
                    });
                }
            });
        }

        public override void Down()
        {
            Delete.Column("Slug").FromTable(CreateFileRecordTable.TABLE_NAME);
        }
    }
}
