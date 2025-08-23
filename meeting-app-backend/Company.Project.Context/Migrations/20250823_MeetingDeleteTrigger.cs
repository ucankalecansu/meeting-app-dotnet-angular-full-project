using Microsoft.EntityFrameworkCore.Migrations;

namespace Company.Project.Context.Migrations
{
    public partial class MeetingDeleteTrigger : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Silinen toplantıları loglamak için trigger oluştur
            migrationBuilder.Sql(@"
                CREATE TRIGGER TR_Meetings_Delete
                ON Meetings
                AFTER DELETE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    INSERT INTO DeletedMeetingLogs (
                        MeetingId, 
                        Title, 
                        StartAt, 
                        EndAt, 
                        Description, 
                        WasCancelled, 
                        CancelledAt, 
                        DeletedAt,
                        DeletedBy
                    )
                    SELECT 
                        d.Id,
                        d.Title,
                        d.StartAt,
                        d.EndAt,
                        d.Description,
                        d.IsCancelled,
                        d.CancelledAt,
                        GETUTCDATE(),
                        SYSTEM_USER
                    FROM deleted d;
                    
                    PRINT 'Silinen toplantı log tablosuna kaydedildi';
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Trigger'ı kaldır
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TR_Meetings_Delete");
        }
    }
}
