using System.Globalization;
using System.Text;
using ELearning.Application.Common.Interfaces;
using ELearning.Domain.Aggregates.CertificateAggregate;

namespace ELearning.Infrastructure.Certificates;

public sealed class SimpleCertificatePdfService : ICertificatePdfService
{
    public byte[] Generate(Certificate certificate)
    {
        var lines = new[]
        {
            "CERTIFICATE OF COMPLETION",
            "This certifies that",
            certificate.LearnerName,
            "has successfully completed",
            certificate.CourseTitle,
            $"Issued: {certificate.IssuedAt:yyyy-MM-dd}",
            $"Certificate No: {certificate.CertificateNumber}",
            $"Verification Code: {certificate.VerificationCode}",
            $"Attendance: {certificate.AttendancePercent.ToString("0.##", CultureInfo.InvariantCulture)}%  Progress: {certificate.ProgressPercent.ToString("0.##", CultureInfo.InvariantCulture)}%"
        };

        var content = BuildPageContent(lines);
        var objects = new List<string>
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 842 595] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            $"<< /Length {Encoding.ASCII.GetByteCount(content)} >>\nstream\n{content}\nendstream"
        };

        return BuildPdf(objects);
    }

    private static string BuildPageContent(IReadOnlyList<string> lines)
    {
        var sb = new StringBuilder();
        sb.AppendLine("BT");
        sb.AppendLine("/F1 30 Tf");
        sb.AppendLine("220 500 Td");
        sb.AppendLine($"({Escape(lines[0])}) Tj");
        sb.AppendLine("/F1 16 Tf");
        sb.AppendLine("80 -70 Td");
        sb.AppendLine($"({Escape(lines[1])}) Tj");
        sb.AppendLine("/F1 28 Tf");
        sb.AppendLine("0 -45 Td");
        sb.AppendLine($"({Escape(lines[2])}) Tj");
        sb.AppendLine("/F1 16 Tf");
        sb.AppendLine("0 -45 Td");
        sb.AppendLine($"({Escape(lines[3])}) Tj");
        sb.AppendLine("/F1 24 Tf");
        sb.AppendLine("0 -40 Td");
        sb.AppendLine($"({Escape(lines[4])}) Tj");
        sb.AppendLine("/F1 12 Tf");

        for (var i = 5; i < lines.Count; i++)
        {
            sb.AppendLine("0 -28 Td");
            sb.AppendLine($"({Escape(lines[i])}) Tj");
        }

        sb.AppendLine("ET");
        return sb.ToString();
    }

    private static byte[] BuildPdf(IReadOnlyList<string> objects)
    {
        var sb = new StringBuilder();
        var offsets = new List<int> { 0 };

        sb.AppendLine("%PDF-1.4");
        for (var i = 0; i < objects.Count; i++)
        {
            offsets.Add(Encoding.ASCII.GetByteCount(sb.ToString()));
            sb.AppendLine($"{i + 1} 0 obj");
            sb.AppendLine(objects[i]);
            sb.AppendLine("endobj");
        }

        var xrefOffset = Encoding.ASCII.GetByteCount(sb.ToString());
        sb.AppendLine("xref");
        sb.AppendLine($"0 {objects.Count + 1}");
        sb.AppendLine("0000000000 65535 f ");
        foreach (var offset in offsets.Skip(1))
            sb.AppendLine($"{offset:0000000000} 00000 n ");

        sb.AppendLine("trailer");
        sb.AppendLine($"<< /Size {objects.Count + 1} /Root 1 0 R >>");
        sb.AppendLine("startxref");
        sb.AppendLine(xrefOffset.ToString(CultureInfo.InvariantCulture));
        sb.AppendLine("%%EOF");

        return Encoding.ASCII.GetBytes(sb.ToString());
    }

    private static string Escape(string value)
        => value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
}
