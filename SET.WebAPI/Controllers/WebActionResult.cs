// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

using SET.Shared.Utilities;

namespace SET.WebAPI.Controllers;

public class WebActionResult : IActionResult
{
    private readonly List<String> m_data = new();
    private readonly Dictionary<String, String> m_headers = new();
    private readonly String _contentType;

    public WebActionResult(String data, String contentType = MimeTypes.Application.JSON)
    {
        if (data != null)
            m_data.Add(data);
        _contentType = contentType;
    }

    public void AddHeader(String name, String value)
    {
        m_headers.Add(name, value);
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var resp = context.HttpContext.Response;
        resp.ContentType = _contentType;

        foreach (var (k, v) in m_headers)
        {
            resp.Headers.Remove(k);
            resp.Headers.Add(k, v);
        }

        for (int i = 0; i < m_data.Count; i++)
            await resp.WriteAsync(m_data[i], Encoding.UTF8);
    }
}

public class WebBinaryActionResult : IActionResult
{
    private readonly List<Byte[]> _data = new();
    private readonly Dictionary<String, String> _headers = new();
    private readonly String _contentType;

    Boolean _cache;

    public WebBinaryActionResult(Byte[] data, String contentType = MimeTypes.Application.JSON)
    {
        if (data != null)
            _data.Add(data);
        _contentType = contentType;
    }

    public WebBinaryActionResult EnableCache(Boolean bEnable = true)
    {
        _cache = bEnable;
        return this;
    }

    public WebBinaryActionResult AddHeader(String name, String value)
    {
        _headers.Add(name, value);
        return this;
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var resp = context.HttpContext.Response;
        resp.ContentType = _contentType;
        foreach (var (k, v) in _headers)
        {
            resp.Headers.Remove(k);
            resp.Headers.Add(k, v);
        }

        if (_cache)
        {
            resp.GetTypedHeaders().CacheControl =
            new CacheControlHeaderValue()
            {
                Private = true,
                MaxAge = TimeSpan.FromDays(30)
            };
        }

        for (int i = 0; i < _data.Count; i++)
            await resp.BodyWriter.WriteAsync(_data[i]);
    }
}

public class WebExceptionResult : IActionResult
{
    private readonly Int32 _errorCode;
    private readonly String _message;

    public WebExceptionResult(Int32 errorCode, String? message)
    {
        _errorCode = errorCode;
        _message = message ?? String.Empty;
    }

    public Task ExecuteResultAsync(ActionContext context)
    {
        var resp = context.HttpContext.Response;
        resp.ContentType = MimeTypes.Text.PLAIN;
        resp.StatusCode = _errorCode;
        return resp.WriteAsync(_message, Encoding.UTF8);
    }
}
