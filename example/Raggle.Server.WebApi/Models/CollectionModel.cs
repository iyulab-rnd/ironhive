﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raggle.Server.WebApi.Models;

[Table("Collections")]
public class CollectionModel
{
    // Primary Key
    public Guid CollectionId { get; set; } = Guid.NewGuid();

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public string EmbedServiceKey { get; set; } = string.Empty;

    [Required]
    public string EmbedModelName { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUpdatedAt { get; set; }

    public IDictionary<string, object>? HandlerOptions { get; set; }

    // 네비게이션 속성
    public ICollection<DocumentModel> Documents { get; set; } = [];
}
