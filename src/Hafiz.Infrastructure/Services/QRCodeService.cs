using System;
using System.Text;
using Hafiz.Application.Interfaces.Services;
using QRCoder;

namespace Hafiz.Infrastructure.Services;

/// <summary>
/// QR generation backed by the QRCoder library, which implements ISO/IEC 18004 in full:
/// byte-mode encoding, Reed-Solomon error correction, mask selection, and both format and
/// version information blocks.
///
/// Every renderer's output is handed back exactly as the library produced it. Nothing here
/// rewrites the markup: a hand-edited SVG that stops being well-formed fails to decode as an
/// image at all, which looks like a missing picture rather than a bad QR code.
///
/// Output is a plain SVG string or a <c>data:</c> URI, so any consumer can render it without
/// referencing this assembly or the Razor layer - a Razor partial today, an Angular
/// <c>&lt;img&gt;</c> or a server-side PDF pipeline later.
/// </summary>
public class QRCodeService : IQRCodeService
{
    /// <summary>
    /// Error correction level. M (~15% recovery) tolerates the smudges and handling a printed
    /// certificate collects, without inflating the module count the way Q or H would at this size.
    /// </summary>
    private const QRCodeGenerator.ECCLevel EccLevel = QRCodeGenerator.ECCLevel.M;

    /// <summary>
    /// Raster resolution floor. A certificate prints this symbol at roughly 20mm, so each module
    /// needs enough pixels to survive print; a caller's <c>pixelSize</c> can only raise this.
    /// </summary>
    private const int MinPixelsPerModule = 10;

    public string GenerateSvg(string content, int pixelSize = 200)
    {
        if (string.IsNullOrEmpty(content))
            return string.Empty;

        using var generator = new QRCodeGenerator();
        using var qrData = generator.CreateQrCode(content, EccLevel);

        // Vector art carrying its own viewBox, so it has no meaningful pixel size of its own and
        // pixelSize is deliberately not applied - sizing belongs to whichever element embeds it.
        // The quiet zone QRCoder draws by default is required by the spec; do not disable it.
        return new SvgQRCode(qrData).GetGraphic(MinPixelsPerModule);
    }

    public string GenerateDataUri(string content, int pixelSize = 200)
    {
        if (string.IsNullOrEmpty(content))
            return string.Empty;

        using var generator = new QRCodeGenerator();
        using var qrData = generator.CreateQrCode(content, EccLevel);

        // ModuleMatrix excludes the quiet zone the renderer adds, so scale against the module
        // count alone and treat pixelSize as a target width for the raster.
        var pixelsPerModule = Math.Max(
            MinPixelsPerModule,
            (int)Math.Ceiling(pixelSize / (double)qrData.ModuleMatrix.Count));

        var png = new PngByteQRCode(qrData).GetGraphic(pixelsPerModule);
        return $"data:image/png;base64,{Convert.ToBase64String(png)}";
    }
}
