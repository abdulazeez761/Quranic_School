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
            try
            {
                credential = await AuthorizeInternalAsync(ct);
            }
            catch (Exception ex) when (ex.ToString().Contains("invalid_grant") || 
                                       ex.ToString().Contains("expired") || 
                                       ex.ToString().Contains("revoked"))
            {
                // Delete the token folder contents to reset authentication state
                try
                {
                    if (System.IO.Directory.Exists(_tokenFolder))
                    {
                        foreach (var file in System.IO.Directory.GetFiles(_tokenFolder))
                        {
                            System.IO.File.Delete(file);
                        }
                    }
                }
                catch
                {
                    // Ignore folder/file deletion failures
                }

                // Retry authorization flow, which will prompt user in browser since token is deleted
                credential = await AuthorizeInternalAsync(ct);
            }

            return new DriveService(
                new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Hafiz backup",
                }
            );
        }

        private async Task<UserCredential> AuthorizeInternalAsync(CancellationToken ct)
        {
            await using var stream = System.IO.File.OpenRead(_credentialsFilePath);
            return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                new[] { DriveService.Scope.DriveFile },
                "user",
                ct,
                new FileDataStore(_tokenFolder, true)
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
