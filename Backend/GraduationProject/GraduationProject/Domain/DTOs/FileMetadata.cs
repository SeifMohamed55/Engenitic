﻿namespace GraduationProject.Domain.DTOs
{
    public class FileMetadata
    {
        public string Name { get; set; } = null!;
        public string ImageURL { get; set; } = null!;
        public ulong Hash { get; set; }
        public string Version { get; set; } = null!;
    }
}
