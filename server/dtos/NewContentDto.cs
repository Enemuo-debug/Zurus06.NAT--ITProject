using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using server.tools;

namespace server.dtos
{
    public class NewContentDto : IValidatableObject
    {
        [Required]
        public string type { get; set; }
        public string Content { get; set; } = string.Empty;
        public IFormFile? File { get; set; } = null;
        public int? NATSimulationId { get; set; } = null;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var mappedType = Enum.Parse<ContentTypes>(type);

            // --- TEXT TYPE VALIDATION ---
            if (mappedType == ContentTypes.Text)
            {
                if (string.IsNullOrWhiteSpace(Content))
                {
                    yield return new ValidationResult(
                        "Content cannot be empty when type is Text.",
                        new[] { nameof(Content) });
                }
                else if (Content.Length < 10)
                {
                    yield return new ValidationResult(
                        "Content cannot be less than 10 characters when type is Text.",
                        new[] { nameof(Content) });
                }

                if (File != null)
                {
                    yield return new ValidationResult(
                        "File must not be provided when type is Text.",
                        new[] { nameof(File) });
                }
            }

            // --- IMAGE TYPE VALIDATION ---
            if (mappedType == ContentTypes.Image)
            {
                if (File == null || string.IsNullOrWhiteSpace(File.FileName))
                {
                    yield return new ValidationResult(
                        "An image file must be provided when type is Image.",
                        new[] { nameof(File) });
                }
                else
                {
                    const long maxFileSize = 5 * 1024 * 1024; // 2 MB
                    if (File.Length > maxFileSize)
                    {
                        yield return new ValidationResult(
                            "Image file size cannot exceed 5MB.",
                            new[] { nameof(File) });
                    }
                }

                if (!string.IsNullOrEmpty(Content) && Content.Length > 50)
                {
                    yield return new ValidationResult(
                        "Content cannot exceed 50 characters when type is Image.",
                        new[] { nameof(Content) });
                }
            }

            // --- NAT SIMULATION TYPE VALIDATION ---
            if (mappedType == ContentTypes.NATSimulation)
            {
                if (!NATSimulationId.HasValue || NATSimulationId <= 0)
                {
                    yield return new ValidationResult(
                        "NATSimulationId must be provided when type is NATSimulation.",
                        new[] { nameof(NATSimulationId) });
                }

                if (!string.IsNullOrWhiteSpace(Content) || File != null)
                {
                    yield return new ValidationResult(
                        "Content and File must be empty when type is NATSimulation.",
                        new[] { nameof(Content), nameof(File) });
                }

                if (Enum.TryParse<ContentTypes>(type, true, out var result))
                {
                    yield return new ValidationResult(
                        "Invalid Content Type was passed in", new[] { nameof(type) }
                    );
                }
            }
        }
    }
}