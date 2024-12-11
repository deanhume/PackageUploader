﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PackageUploader.ClientApi.Client.Xfus.Models.Internal;
using System.Text.Json.Serialization;

namespace PackageUploader.ClientApi.Client.Xfus.Uploader;

[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true
)]
[JsonSerializable(typeof(UploadProperties))]
[JsonSerializable(typeof(UploadProgress))]
internal partial class XfusJsonSerializerContext : JsonSerializerContext
{ }