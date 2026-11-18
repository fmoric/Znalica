namespace Znalica.Domain
{
    /// <summary>What a Brojevna crta item asks the child to do.</summary>
    public enum NumberLineTask
    {
        /// <summary>Click where a number lies on the 0-20 line.</summary>
        Place,

        /// <summary>Type the number before the one named.</summary>
        Before,

        /// <summary>Type the number after the one named.</summary>
        After,

        /// <summary>Type where a jump along the line lands.</summary>
        Jump,
    }
}
