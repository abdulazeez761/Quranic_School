using Hafiz.Application.Interfaces.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Hafiz.Infrastructure.Services
{
    public class BackupService : IBackupService
    {
        private readonly string _connectionString;
        private readonly string _localBackupFolder;
        private readonly IGoogleDriveUploader _driveUploader;
        private readonly ILogger<BackupService> _logger;
        private readonly bool _isUsingDifferentImage = false; // the server might have more than one image for the applicaion and another one for the database

        public BackupService(
            IConfiguration config,
            IGoogleDriveUploader driveUploader,
            ILogger<BackupService> logger
        )
        {
            _connectionString =
                config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection غير مضبوط.");
            _localBackupFolder = config["Backup:LocalFolder"] ?? @"C:\SqlBackups";
            _driveUploader = driveUploader;
            _logger = logger;
            _isUsingDifferentImage = config["Backup:UseDifferentImage"]?.ToLower() == "true";
        }

        public async Task<string> RunBackupAsync(CancellationToken ct = default)
        {
            var databaseName = new SqlConnectionStringBuilder(_connectionString).InitialCatalog;
            if (string.IsNullOrWhiteSpace(databaseName))
                throw new InvalidOperationException(
                    "اسم قاعدة البيانات غير موجود في سلسلة الاتصال."
                );

            Directory.CreateDirectory(_localBackupFolder);

            var fileName = $"{databaseName}_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
            var fullPath = Path.Combine(_localBackupFolder, fileName);

            // ملاحظة: أمر BACKUP ينفّذه حساب خدمة SQL Server، فلا بد أن يكون
            // مجلد الحفظ قابلاً للكتابة من ذلك الحساب.
            // تم تجنّب COMPRESSION لأنها غير مدعومة على SQL Server Express.
            var sql =
                $@"BACKUP DATABASE [{databaseName}]
                   TO DISK = @path
                   WITH FORMAT, INIT, NAME = N'{databaseName} Full Backup';";

            await using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync(ct);
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@path", fullPath);
                cmd.CommandTimeout = 0; // النسخ الاحتياطي قد يستغرق وقتًا
                await cmd.ExecuteNonQueryAsync(ct);
            }

            _logger.LogInformation("تم إنشاء النسخة الاحتياطية محليًا: {Path}", fullPath);

            try
            {
                if (_isUsingDifferentImage == true)
                {
                    var process = new System.Diagnostics.Process();
                    process.StartInfo.FileName = "docker";
                    process.StartInfo.Arguments = $"cp sql_server:{fullPath} {fullPath}";
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.UseShellExecute = false;
                    process.Start();
                    process.WaitForExit();
                }

                var driveFileId = await _driveUploader.UploadAsync(fullPath, ct);
                _logger.LogInformation(
                    "تم رفع النسخة الاحتياطية إلى Google Drive (Id: {Id})",
                    driveFileId
                );
            }
            finally
            {
                // نظّف النسخة المحلية بعد الرفع (أو حتى لو فشل الرفع لتجنّب امتلاء القرص)
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
            }

            return fileName;
        }
    }
}
