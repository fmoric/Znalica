namespace Znalica.Domain
{
    /// <summary>One thing the child is asked. Its key is its identity everywhere it is recorded.</summary>
    public abstract class Item
    {
        public abstract ItemKey Key { get; }

        public abstract Activity Activity { get; }

        public abstract Difficulty Difficulty { get; }

        /// <summary>
        /// The fact this item is one form of; items of one fact share it, and so share a <see cref="Difficulty"/>. An item
        /// that is not one form of a larger fact is its own fact.
        /// </summary>
        public virtual string FactId => Key.ToString();

        /// <summary>What a wrong answer shows and says.</summary>
        public abstract Explanation Explain();

        public abstract bool IsCorrect(Answer answer);
    }
}
