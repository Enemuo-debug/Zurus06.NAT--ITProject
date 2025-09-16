using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using server.tools;

namespace server.dtos
{
    public class NewContentDto : IValidatableObject
    {
        [Required]
        public string type { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public IFormFile? File { get; set; } = null;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Try to parse type safely
            if (!int.TryParse(type, out int typeValue))
            {
                yield return new ValidationResult(
                    "Invalid type value. It must be a number.",
                    new[] { nameof(type) });
                yield break; // Stop further validation if type is invalid
            }

            var mappedType = ContentTypeFunctions.MapContentTypes(typeValue);

            if (mappedType == ContentTypes.Text)
            {
                if (string.IsNullOrWhiteSpace(Content))
                {
                    yield return new ValidationResult(
                        "Content cannot be empty when type is Text.",
                        new[] { nameof(Content) });
                }
                else
                {
                    if (Content.Length > 500)
                    {
                        yield return new ValidationResult(
                            "Content cannot exceed 500 characters.",
                            new[] { nameof(Content) });
                    }
                    if (Content.Length < 10)
                    {
                        yield return new ValidationResult(
                            "Content cannot be less than 10 characters when type is Text.",
                            new[] { nameof(Content) });
                    }
                }
            }

            if (mappedType == ContentTypes.Image)
            {
                if (string.IsNullOrWhiteSpace(File?.FileName))
                {
                    yield return new ValidationResult(
                        "Img File must be provided and cannot be empty when type is Image.",
                        new[] { nameof(File) });
                }

                if (!string.IsNullOrEmpty(Content) && Content.Length > 50)
                {
                    yield return new ValidationResult(
                        "Content cannot exceed 50 characters when type is Image.",
                        new[] { nameof(Content) });
                }
            }

            if (mappedType == ContentTypes.NATSimulation)
            {
                if (!string.IsNullOrWhiteSpace(File?.FileName) || !string.IsNullOrWhiteSpace(Content))
                {
                    yield return new ValidationResult(
                        "ImgLink and Content must be empty when type is NATSimulation.",
                        new[] { nameof(File), nameof(Content) });
                }
            }
        }
    }
}
