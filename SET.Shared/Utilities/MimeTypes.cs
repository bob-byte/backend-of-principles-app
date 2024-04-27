// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace SET.Shared.Utilities;

public static class MimeTypes
{
    public static class Application
    {
        public const string JSON = "application/json";
        public const string OCTET_BINARY = "application/octet-binary";
        public const string OCTET_STREAM = "application/octet-stream";
        public const string PDF = "application/pdf";
    }

    public static class Text
    {
        public const string PLAIN = "text/plain";
        public const string HTML = "text/html";
        public const string HTML_UTF_8 = "text/html; charset=UTF-8";
        public const string CSS = "text/css";
    }

    public static class Image
    {
        public const string PNG = "image/png";
        public const string JPG = "image/jpeg";
        public const string SVG = "image/svg+xml";
        public const string BMP = "image/bmp";
        public const string TIF = "image/tiff";
    }
    public static string GetMimeMapping( string ext )
    {
        return ext.ToLowerInvariant() switch
        {
            ".png" => Image.PNG,
            ".svg" => Image.SVG,
            ".bmp" => Image.BMP,
            ".tif" or ".tiff" => Image.TIF,
            ".jpg" or ".jpeg" or ".jpe" => Image.JPG,
            _ => throw new ArgumentOutOfRangeException( paramName: $"Invalid Mime for {ext}" )
        };
    }

    public static bool IsImage( string? mime )
    {
        return mime != null && mime.StartsWith( value: "image", StringComparison.OrdinalIgnoreCase );
    }
}
