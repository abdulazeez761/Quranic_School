using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Hafiz.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace Hafiz.Infrastructure.Services
{
    public class GoogleDriveUploader : IGoogleDriveUploader
    {
        private readonly string _credentialsFilePath; // credentials.json
        private readonly string _tokenFolder; // مكان حفظ الـ refresh token
        private readonly string _targetFolderId; // ID مجلد الوجهة في Drive

        public GoogleDriveUploader(IConfiguration config)
        {
            _credentialsFilePath =
                config["GoogleDrive:CredentialsFilePath"]
                ?? throw new InvalidOperationException(
                    "GoogleDrive:CredentialsFilePath غير مضبوط."
                );
            _tokenFolder =
                config["GoogleDrive:TokenFolder"]
                ?? throw new InvalidOperationException("GoogleDrive:TokenFolder غير مضبوط.");
            _targetFolderId =
                config["GoogleDrive:TargetFolderId"]
                ?? throw new InvalidOperationException("GoogleDrive:TargetFolderId غير مضبوط.");
        }

        private async Task<DriveService> GetServiceAsync(CancellationToken ct)
        {
            UserCredential credential;
            await using (var stream = File.OpenRead(_credentialsFilePath))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    new[] { DriveService.Scope.DriveFile },
                    "user",
                    ct,
                    new FileDataStore(_tokenFolder, true)
                );
            }
            return new DriveService(
                new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Hafiz backup",
                }
            );
        }

        public async Task<string> UploadAsync(string filePath, CancellationToken ct = default)
        {
            var service = await GetServiceAsync(ct);
            var fileMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = Path.GetFileName(filePath),
                Parents = new[] { _targetFolderId },
            };

            await using var fs = File.OpenRead(filePath);
            var request = service.Files.Create(fileMetadata, fs, "application/octet-stream");
            request.Fields = "id";
            var result = await request.UploadAsync(ct);
            if (result.Status != Google.Apis.Upload.UploadStatus.Completed)
                throw new Exception($"Drive upload failed: {result.Exception?.Message}");

            return request.ResponseBody.Id;
        }
    }
}
