namespace Hafiz.Application.DTO.Certificate;

/// <summary>Counts behind the tiles at the top of the certificates register.</summary>
public class CertificateStats
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Revoked { get; set; }
    public int Expired { get; set; }

    /// <summary>Issued since the start of the current month in the viewer's time zone.</summary>
    public int IssuedThisMonth { get; set; }

    /// <summary>Share of the register that is still valid, 0-100.</summary>
    public int ActivePercent => Total == 0 ? 0 : (int)Math.Round(Active * 100.0 / Total);

    /// <summary>Share of the register that has been revoked, 0-100.</summary>
    public int RevokedPercent => Total == 0 ? 0 : (int)Math.Round(Revoked * 100.0 / Total);
}
