using System;
using System.Collections.Generic;
using System.Text;
using Hafiz.Application.Interfaces.Services;

namespace Hafiz.Infrastructure.Services;

/// <summary>
/// Lightweight, zero-dependency QR Code generator producing crisp SVG output.
/// Implements ISO/IEC 18004 specification for byte-mode QR codes with error correction.
/// </summary>
public class QRCodeService : IQRCodeService
{
    public string GenerateSvg(string content, int pixelSize = 200)
    {
        if (string.IsNullOrEmpty(content))
            return string.Empty;

        var matrix = GenerateQrMatrix(content);
        int moduleCount = matrix.GetLength(0);
        int margin = 4;
        int totalSize = moduleCount + (margin * 2);

        var sb = new StringBuilder();
        sb.Append($"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {totalSize} {totalSize}\" width=\"{pixelSize}\" height=\"{pixelSize}\" shape-rendering=\"crispEdges\">");
        sb.Append($"<rect width=\"100%\" height=\"100%\" fill=\"#ffffff\"/>");
        sb.Append("<path fill=\"#000000\" d=\"");

        for (int y = 0; y < moduleCount; y++)
        {
            for (int x = 0; x < moduleCount; x++)
            {
                if (matrix[y, x])
                {
                    sb.Append($"M{x + margin},{y + margin}h1v1h-1z ");
                }
            }
        }

        sb.Append("\"/></svg>");
        return sb.ToString();
    }

    public string GenerateDataUri(string content, int pixelSize = 200)
    {
        var svg = GenerateSvg(content, pixelSize);
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(svg));
        return $"data:image/svg+xml;base64,{base64}";
    }

    #region QR Code Matrix Generation

    private static bool[,] GenerateQrMatrix(string text)
    {
        byte[] data = Encoding.UTF8.GetBytes(text);
        int version = GetBestVersion(data.Length);
        int size = 17 + 4 * version;
        var modules = new bool[size, size];
        var isFunction = new bool[size, size];

        // 1. Finder patterns
        DrawFinderPattern(modules, isFunction, 0, 0);
        DrawFinderPattern(modules, isFunction, size - 7, 0);
        DrawFinderPattern(modules, isFunction, 0, size - 7);

        // 2. Alignment patterns
        DrawAlignmentPatterns(modules, isFunction, version);

        // 3. Timing patterns
        for (int i = 8; i < size - 8; i++)
        {
            bool val = (i % 2 == 0);
            SetModule(modules, isFunction, 6, i, val);
            SetModule(modules, isFunction, i, 6, val);
        }

        // 4. Dark module & format placeholders
        SetModule(modules, isFunction, 8, 4 * version + 9, true);

        for (int i = 0; i < 9; i++)
        {
            if (i != 6)
            {
                isFunction[8, i] = true;
                isFunction[i, 8] = true;
            }
        }
        for (int i = size - 8; i < size; i++)
        {
            isFunction[8, i] = true;
            isFunction[i, 8] = true;
        }

        // 5. Encode data codewords
        byte[] codewords = EncodeData(data, version);

        // 6. Draw data codewords with mask pattern 0: (row + col) % 2 == 0
        int bitIndex = 0;
        int totalBits = codewords.Length * 8;
        int right = size - 1;

        while (right > 0)
        {
            if (right == 6) right--; // Skip vertical timing pattern

            for (int vert = 0; vert < size; vert++)
            {
                for (int j = 0; j < 2; j++)
                {
                    int x = right - j;
                    bool upward = ((right + 1) & 2) == 0;
                    int y = upward ? size - 1 - vert : vert;

                    if (!isFunction[y, x])
                    {
                        bool bit = false;
                        if (bitIndex < totalBits)
                        {
                            bit = ((codewords[bitIndex >> 3] >> (7 - (bitIndex & 7))) & 1) != 0;
                            bitIndex++;
                        }

                        // Mask 0: (x + y) % 2 == 0
                        bool mask = ((x + y) % 2 == 0);
                        modules[y, x] = bit ^ mask;
                    }
                }
            }
            right -= 2;
        }

        // 7. Write format information (Mask 0, ECC Level L: 01 000 -> format bits with BCH: 0x77C4)
        // ECC L + Mask 0 = 0b01000 -> BCH 15,5 = 0x77C4
        int formatInfo = 0x77C4;
        for (int i = 0; i < 15; i++)
        {
            bool bit = ((formatInfo >> i) & 1) != 0;
            if (i < 6) modules[8, i] = bit;
            else if (i < 8) modules[8, i + 1] = bit;
            else modules[14 - i, 8] = bit;

            if (i < 8) modules[size - 1 - i, 8] = bit;
            else modules[8, size - 15 + i] = bit;
        }

        return modules;
    }

    private static void DrawFinderPattern(bool[,] modules, bool[,] isFunc, int startX, int startY)
    {
        for (int dy = -1; dy <= 7; dy++)
        {
            for (int dx = -1; dx <= 7; dx++)
            {
                int x = startX + dx;
                int y = startY + dy;
                if (x >= 0 && x < modules.GetLength(0) && y >= 0 && y < modules.GetLength(1))
                {
                    isFunc[y, x] = true;
                    if (dx >= 0 && dx <= 6 && dy >= 0 && dy <= 6)
                    {
                        bool isBlack = (dx == 0 || dx == 6 || dy == 0 || dy == 6 || (dx >= 2 && dx <= 4 && dy >= 2 && dy <= 4));
                        modules[y, x] = isBlack;
                    }
                    else
                    {
                        modules[y, x] = false; // Quiet separator
                    }
                }
            }
        }
    }

    private static void DrawAlignmentPatterns(bool[,] modules, bool[,] isFunc, int version)
    {
        if (version < 2) return;

        int[] pos = GetAlignmentPatternPositions(version);
        for (int i = 0; i < pos.Length; i++)
        {
            for (int j = 0; j < pos.Length; j++)
            {
                int x = pos[i];
                int y = pos[j];
                if (isFunc[y, x]) continue; // Skip finder patterns

                for (int dy = -2; dy <= 2; dy++)
                {
                    for (int dx = -2; dx <= 2; dx++)
                    {
                        isFunc[y + dy, x + dx] = true;
                        bool isBlack = (Math.Abs(dx) == 2 || Math.Abs(dy) == 2 || (dx == 0 && dy == 0));
                        modules[y + dy, x + dx] = isBlack;
                    }
                }
            }
        }
    }

    private static int[] GetAlignmentPatternPositions(int version)
    {
        if (version == 1) return Array.Empty<int>();
        int numPatterns = (version / 7) + 2;
        int step = (version == 32) ? 26 : (int)Math.Ceiling((version * 4 + 4.0) / (numPatterns * 2 - 2)) * 2;
        var result = new int[numPatterns];
        result[0] = 6;
        for (int i = numPatterns - 1, val = 17 + 4 * version - 7; i > 0; i--, val -= step)
        {
            result[i] = val;
        }
        return result;
    }

    private static void SetModule(bool[,] modules, bool[,] isFunc, int x, int y, bool value)
    {
        modules[y, x] = value;
        isFunc[y, x] = true;
    }

    private static int GetBestVersion(int dataLen)
    {
        // Capacities for Byte mode with ECC Level L:
        int[] capacities = { 17, 32, 53, 78, 106, 134, 154, 192, 230, 271, 321, 367, 425, 458 };
        for (int v = 1; v <= capacities.Length; v++)
        {
            if (dataLen + 3 <= capacities[v - 1])
                return v;
        }
        return 6; // Reasonable default for typical URLs
    }

    private static byte[] EncodeData(byte[] data, int version)
    {
        int[] totalCodewordsTable = { 26, 44, 70, 100, 134, 172, 196, 242, 292, 346, 404, 466, 532, 581 };
        int[] ecCodewordsTable = { 7, 10, 15, 20, 26, 18, 20, 24, 30, 18, 20, 24, 26, 30 };
        int[] numBlocksTable = { 1, 1, 1, 1, 1, 2, 2, 2, 2, 4, 4, 4, 4, 4 };

        int totalDataCodewords = totalCodewordsTable[version - 1] - ecCodewordsTable[version - 1] * numBlocksTable[version - 1];
        var bitBuffer = new List<bool>();

        // 1. Mode indicator: Byte mode = 0100
        AppendBits(bitBuffer, 0b0100, 4);

        // 2. Character count indicator (8 bits for versions 1-9)
        int countBits = (version <= 9) ? 8 : 16;
        AppendBits(bitBuffer, data.Length, countBits);

        // 3. Data bits
        foreach (byte b in data)
        {
            AppendBits(bitBuffer, b, 8);
        }

        // 4. Terminator (up to 4 zeroes)
        int padTerminator = Math.Min(4, totalDataCodewords * 8 - bitBuffer.Count);
        for (int i = 0; i < padTerminator; i++) bitBuffer.Add(false);

        // 5. Pad to multiple of 8
        while (bitBuffer.Count % 8 != 0) bitBuffer.Add(false);

        // 6. Convert to byte array and add pad bytes 0xEC, 0x11
        var dataBytes = new byte[totalDataCodewords];
        for (int i = 0; i < bitBuffer.Count / 8; i++)
        {
            byte b = 0;
            for (int bit = 0; bit < 8; bit++)
            {
                if (bitBuffer[i * 8 + bit]) b |= (byte)(1 << (7 - bit));
            }
            dataBytes[i] = b;
        }

        byte[] padBytes = { 0xEC, 0x11 };
        for (int i = bitBuffer.Count / 8; i < totalDataCodewords; i++)
        {
            dataBytes[i] = padBytes[(i - bitBuffer.Count / 8) % 2];
        }

        // 7. Error correction using Reed-Solomon
        int numBlocks = numBlocksTable[version - 1];
        int ecCount = ecCodewordsTable[version - 1];
        int dataPerBlock = totalDataCodewords / numBlocks;

        var result = new List<byte>();
        var ecBlocks = new List<byte[]>();
        var dataBlocks = new List<byte[]>();

        for (int b = 0; b < numBlocks; b++)
        {
            var blockData = new byte[dataPerBlock];
            Array.Copy(dataBytes, b * dataPerBlock, blockData, 0, dataPerBlock);
            dataBlocks.Add(blockData);
            ecBlocks.Add(CalculateReedSolomon(blockData, ecCount));
        }

        // Interleave data
        for (int i = 0; i < dataPerBlock; i++)
        {
            for (int b = 0; b < numBlocks; b++)
            {
                result.Add(dataBlocks[b][i]);
            }
        }

        // Interleave EC
        for (int i = 0; i < ecCount; i++)
        {
            for (int b = 0; b < numBlocks; b++)
            {
                result.Add(ecBlocks[b][i]);
            }
        }

        return result.ToArray();
    }

    private static void AppendBits(List<bool> buffer, int value, int count)
    {
        for (int i = count - 1; i >= 0; i--)
        {
            buffer.Add(((value >> i) & 1) != 0);
        }
    }

    private static byte[] CalculateReedSolomon(byte[] data, int ecLength)
    {
        byte[] generator = BuildGeneratorPolynomial(ecLength);
        byte[] info = new byte[data.Length + ecLength];
        Array.Copy(data, info, data.Length);

        for (int i = 0; i < data.Length; i++)
        {
            byte coef = info[i];
            if (coef != 0)
            {
                for (int j = 0; j < generator.Length; j++)
                {
                    info[i + j] ^= GfMultiply(generator[j], coef);
                }
            }
        }

        byte[] ec = new byte[ecLength];
        Array.Copy(info, data.Length, ec, 0, ecLength);
        return ec;
    }

    private static byte[] BuildGeneratorPolynomial(int degree)
    {
        byte[] poly = { 1 };
        for (int i = 0; i < degree; i++)
        {
            byte[] next = new byte[poly.Length + 1];
            byte factor = GfExp(i);
            for (int j = 0; j < poly.Length; j++)
            {
                next[j] ^= poly[j];
                next[j + 1] ^= GfMultiply(poly[j], factor);
            }
            poly = next;
        }
        return poly;
    }

    private static byte GfMultiply(byte x, byte y)
    {
        if (x == 0 || y == 0) return 0;
        return GfExp((GfLog(x) + GfLog(y)) % 255);
    }

    private static readonly byte[] ExpTable = new byte[256];
    private static readonly byte[] LogTable = new byte[256];

    static QRCodeService()
    {
        int x = 1;
        for (int i = 0; i < 255; i++)
        {
            ExpTable[i] = (byte)x;
            LogTable[x] = (byte)i;
            x <<= 1;
            if ((x & 0x100) != 0) x ^= 0x11D;
        }
        ExpTable[255] = ExpTable[0];
    }

    private static byte GfExp(int power) => ExpTable[(power % 255 + 255) % 255];
    private static byte GfLog(byte val) => LogTable[val];

    #endregion
}
