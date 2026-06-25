using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hafiz.Application.Interfaces.Services
{
    public interface IGoogleDriveUploader
    {
        Task<string> UploadAsync(string filePath, CancellationToken ct = default);
    }
}
