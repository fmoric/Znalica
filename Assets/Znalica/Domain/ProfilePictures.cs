using System;
using System.Collections.Generic;

namespace Znalica.Domain
{
    /// <summary>
    /// The bundled set of profile pictures, by name. The name is what a profile stores; how each is drawn is the
    /// screen's business (placeholder art until the art style is chosen, functional spec open question 6).
    /// </summary>
    public static class ProfilePictures
    {
        private static readonly string[] Names = { "Fox", "Bear", "Owl", "Cat", "Rabbit", "Frog" };

        public static IReadOnlyList<string> Ids => Names;

        public static bool Contains(string id)
        {
            return IndexOf(id) >= 0;
        }

        /// <summary>The picture's place in <see cref="Ids"/>, or -1 when it is not a bundled picture.</summary>
        public static int IndexOf(string id)
        {
            return id == null ? -1 : Array.IndexOf(Names, id);
        }
    }
}
