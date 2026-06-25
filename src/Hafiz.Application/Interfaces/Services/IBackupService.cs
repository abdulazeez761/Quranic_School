using System.Threading;
using System.Threading.Tasks;

namespace Hafiz.Application.Interfaces.Services
{
    public interface IBackupService
    {
        /// <summary>
        /// ينشئ نسخة احتياطية كاملة لقاعدة البيانات، يرفعها إلى Google Drive،
        /// ثم يحذف النسخة المحلية. يُرجع اسم ملف النسخة الاحتياطية.
        /// </summary>
        Task<string> RunBackupAsync(CancellationToken ct = default);
    }
}
