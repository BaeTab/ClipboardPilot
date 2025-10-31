using ClipboardPilot.Domain.Enums;
using System;

namespace ClipboardPilot.Domain.Entities;

public class ClipboardItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public ClipboardType Type { get; set; }
    
    public string? Text { get; set; }
    
    public string? Html { get; set; }
    
    public string? Rtf { get; set; }
    
    public byte[]? ImageBytes { get; set; }
    
    public string? ImagePath { get; set; }
    
    public string? FileList { get; set; }
    
    public string Preview { get; set; } = string.Empty;
    
    public string? SourceApp { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public bool Pinned { get; set; }
    
    public int FavoriteRank { get; set; }
    
    public string Tags { get; set; } = string.Empty;
    
    public ColorLabel Label { get; set; } = ColorLabel.None;
    
    public long Size { get; set; }
    
    public string Hash { get; set; } = string.Empty;
    
    public bool IsSensitive { get; set; }
}
