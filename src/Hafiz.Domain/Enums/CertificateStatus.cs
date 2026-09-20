namespace Hafiz.Domain.Enums;

public enum CertificateStatus
{
    Active = 1,     // سارية ومعتمدة
    Revoked = 2,    // ملغاة رسمياً
    Expired = 3     // منتهية الصلاحية
}
