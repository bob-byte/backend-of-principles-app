using System.IO.Compression;
using System.Text;
using BusinessLogic;
using ImageMagick;

namespace SET.UnitTests.Services;

public class AiServiceAttachmentsTests
{
    [Fact]
    public void SanitizeAttachments_HeicMime_ConvertsToJpeg()
    {
        // Magick.NET can decode HEIC but not encode it in this package build, so the
        // fixture is a tiny JPEG labeled as HEIC — Sanitize still runs the convert path
        // (Magick detects format from bytes) and re-encodes to JPEG for OpenAI.
        byte[] source = CreateSampleJpeg();
        IReadOnlyList<AiChatAttachment> result = AiService.SanitizeAttachments(
            new[] { new AiChatAttachment( "IMG_001.HEIC", "image/heic", source ) } );

        Assert.Single( result );
        Assert.Equal( "image/jpeg", result[0].MimeType );
        Assert.Equal( "IMG_001.jpg", result[0].FileName );
        Assert.True( result[0].Data.Length > 0 );
        Assert.Equal( 0xFF, result[0].Data[0] );
        Assert.Equal( 0xD8, result[0].Data[1] );
        Assert.Contains(
            MagickNET.SupportedFormats,
            f => f.Format == MagickFormat.Heic && f.SupportsReading );
    }

    [Fact]
    public void SanitizeAttachments_InvalidHeic_IsDropped()
    {
        IReadOnlyList<AiChatAttachment> result = AiService.SanitizeAttachments(
            new[] { new AiChatAttachment( "broken.heic", "image/heic", new byte[] { 1, 2, 3 } ) } );

        Assert.Empty( result );
    }

    [Fact]
    public void SanitizeAttachments_Jpeg_Unchanged()
    {
        byte[] jpeg = CreateSampleJpeg();

        IReadOnlyList<AiChatAttachment> result = AiService.SanitizeAttachments(
            new[] { new AiChatAttachment( "shot.jpg", "image/jpeg", jpeg ) } );

        Assert.Single( result );
        Assert.Equal( "image/jpeg", result[0].MimeType );
        Assert.Equal( "shot.jpg", result[0].FileName );
        Assert.Equal( jpeg, result[0].Data );
    }

    [Fact]
    public void WithJpegExtension_ReplacesHeicSuffix()
    {
        Assert.Equal( "photo.jpg", AiService.WithJpegExtension( "photo.heic" ) );
        Assert.Equal( "scan.jpg", AiService.WithJpegExtension( "scan.HEIF" ) );
        Assert.Equal( "attachment.jpg", AiService.WithJpegExtension( "attachment" ) );
    }

    [Fact]
    public void SanitizeAttachments_Docx_ExtractsPlainText()
    {
        byte[] docx = CreateSampleDocx( "Works at Edvantis" );

        IReadOnlyList<AiChatAttachment> result = AiService.SanitizeAttachments(
            new[]
            {
                new AiChatAttachment(
                    "Bohdan_Bats_Resume.docx",
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    docx )
            } );

        Assert.Single( result );
        Assert.Equal( "text/plain", result[0].MimeType );
        Assert.Equal( "Bohdan_Bats_Resume.docx", result[0].FileName );
        string text = Encoding.UTF8.GetString( result[0].Data );
        Assert.Contains( "Works at Edvantis", text );
    }

    [Fact]
    public void SanitizeAttachments_InvalidDocx_KeepsFilenameOnly()
    {
        IReadOnlyList<AiChatAttachment> result = AiService.SanitizeAttachments(
            new[]
            {
                new AiChatAttachment(
                    "broken.docx",
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    new byte[] { 1, 2, 3, 4 } )
            } );

        Assert.Single( result );
        Assert.Equal( "broken.docx", result[0].FileName );
        Assert.Empty( result[0].Data );
    }

    [Fact]
    public void TryExtractDocxText_ReadsParagraphs()
    {
        byte[] docx = CreateSampleDocx( "Line one", "Line two" );

        string? text = AiService.TryExtractDocxText( docx );

        Assert.NotNull( text );
        Assert.Contains( "Line one", text );
        Assert.Contains( "Line two", text );
    }

    private static byte[] CreateSampleJpeg()
    {
        using MagickImage image = new( MagickColors.Blue, 4, 4 );
        image.Format = MagickFormat.Jpeg;
        return image.ToByteArray( MagickFormat.Jpeg );
    }

    private static byte[] CreateSampleDocx( params string[] paragraphs )
    {
        StringBuilder body = new();
        foreach (string paragraph in paragraphs)
        {
            body.Append( "<w:p><w:r><w:t>" )
                .Append( System.Security.SecurityElement.Escape( paragraph ) )
                .Append( "</w:t></w:r></w:p>" );
        }

        string documentXml =
            """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
              <w:body>
            """
            + body
            + """
              </w:body>
            </w:document>
            """;

        using MemoryStream ms = new();
        using (ZipArchive zip = new( ms, ZipArchiveMode.Create, leaveOpen: true ))
        {
            ZipArchiveEntry entry = zip.CreateEntry( "word/document.xml" );
            using StreamWriter writer = new( entry.Open(), new UTF8Encoding( encoderShouldEmitUTF8Identifier: false ) );
            writer.Write( documentXml );
        }

        return ms.ToArray();
    }
}
