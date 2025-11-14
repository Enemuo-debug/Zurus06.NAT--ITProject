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
        public string simUUID { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!Enum.TryParse<ContentTypes>(type, true, out var mappedType))
            {
                yield return new ValidationResult(
                    "Invalid content type provided.",
                    new[] { nameof(type) }
                );
                yield break;
            }

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

                if (!string.IsNullOrEmpty(simUUID))
                {
                    yield return new ValidationResult(
                        "simUUID must be empty when type is Text.",
                        new[] { nameof(simUUID) });
                }
            }

            // --- IMAGE TYPE VALIDATION ---
            else if (mappedType == ContentTypes.Image)
            {
                if (File == null || string.IsNullOrWhiteSpace(File.FileName))
                {
                    yield return new ValidationResult(
                        "An image file must be provided when type is Image.",
                        new[] { nameof(File) });
                }
                else
                {
                    const long maxFileSize = 5 * 1024 * 1024; // 5 MB
                    if (File.Length > maxFileSize)
                    {
                        yield return new ValidationResult(
                            "Image file size cannot exceed 5MB.",
                            new[] { nameof(File) });
                    }
                }

                if (!string.IsNullOrEmpty(Content) && Content.Length > 150)
                {
                    yield return new ValidationResult(
                        "Caption (Content) cannot exceed 150 characters when type is Image.",
                        new[] { nameof(Content) });
                }

                if (!string.IsNullOrEmpty(simUUID))
                {
                    yield return new ValidationResult(
                        "simUUID must be empty when type is Image.",
                        new[] { nameof(simUUID) });
                }
            }

            // --- NAT SIMULATION TYPE VALIDATION ---
            else if (mappedType == ContentTypes.NATSimulation)
            {
                if (string.IsNullOrWhiteSpace(simUUID))
                {
                    yield return new ValidationResult(
                        "Simulation UUID must be provided when type is NATSimulation.",
                        new[] { nameof(simUUID) });
                }

                if (!string.IsNullOrEmpty(Content))
                {
                    yield return new ValidationResult(
                        "Content must be empty when type is NATSimulation.",
                        new[] { nameof(Content) });
                }

                if (File != null)
                {
                    yield return new ValidationResult(
                        "File must not be provided when type is NATSimulation.",
                        new[] { nameof(File) });
                }
            }
        }
    }
}
