using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using server.tools;

namespace server.dtos
{
    public class EditContentDto : IValidatableObject
    {
        [Required]
        public string type { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public int? NATSimulationId { get; set; } = null;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // --- ENUM VALIDATION ---
            if (!Enum.TryParse<ContentTypes>(type, true, out var result))
            {
                yield return new ValidationResult(
                    "Invalid Content Type was passed in",
                    new[] { nameof(type) }
                );
                yield break; // stop further checks if type is invalid
            }

            var mappedType = result;

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
                        "Content must be at least 10 characters when type is Text.",
                        new[] { nameof(Content) });
                }
            }

            // --- NAT SIMULATION TYPE VALIDATION ---
            if (mappedType == ContentTypes.NATSimulation)
            {
                if (!NATSimulationId.HasValue || NATSimulationId <= 0)
                {
                    yield return new ValidationResult(
                        "NATSimulationId must be provided and positive when type is NATSimulation.",
                        new[] { nameof(NATSimulationId) });
                }
            }
        }
    }
}
