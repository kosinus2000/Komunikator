﻿using System.Text.Json.Serialization;

namespace KomunikatorClient.Models;

public class IdentityErrorResponse
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}