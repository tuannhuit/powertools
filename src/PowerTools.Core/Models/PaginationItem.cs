namespace PowerTools.Core.Models
{
    public class PaginationItem<T>
    {
        #region NextId
        private static readonly object _lock = new object();
        private static long _nextId = -1;
        /// <summary>
        /// Gets next ID using for new adding Item into ItemSource
        /// </summary>
        public static long NextId
        {
            get
            {
                lock (_lock)
                {
                    return ++_nextId;
                }
            }
        }
        #endregion

        public long Id { get; }
        public T Value { get; set; }

        public PaginationItem(T value)
        {
            Id = NextId;
            Value = value;
        }
    }
}
