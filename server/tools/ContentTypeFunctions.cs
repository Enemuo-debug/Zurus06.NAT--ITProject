using System;

namespace server.tools
{
    public class ContentTypeFunctions
    {
        public static ContentTypes MapContentTypes(int index)
        {
            ContentTypes[] contentTypes =
            {
                ContentTypes.Text,
                ContentTypes.Image,
                ContentTypes.NATSimulation,
            };

            // Validate index range (expecting 1-based index)
            if (index < 1 || index > contentTypes.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index),
                    $"Invalid content type index: {index}. Expected a value between 1 and {contentTypes.Length}.");
            }

            return contentTypes[index - 1]; // safe now
        }
    }
}
