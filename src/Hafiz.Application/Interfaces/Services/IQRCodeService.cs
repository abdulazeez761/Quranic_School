namespace Hafiz.Application.Interfaces.Services;

public interface IQRCodeService
{
    /// <summary>
    /// Generates a QR code SVG string for the specified text/URL.
    /// </summary>
    string GenerateSvg(string content, int pixelSize = 200);

    /// <summary>
    /// Generates a data URI suitable for an img src (e.g. data:image/svg+xml;utf8,...).
    /// </summary>
    string GenerateDataUri(string content, int pixelSize = 200);
}
