namespace Ksi
{
    /// <summary>
    /// An enum indicating a slot state for open addressing hash tables with lazy deletion.
    /// </summary>
    public enum KsiHashTableSlotState
    {
        /// <summary>
        /// Slot is not used.
        /// </summary>
        Empty,

        /// <summary>
        /// Slot stores a value.
        /// </summary>
        Occupied,

        /// <summary>
        /// Slot is empty but required for linear probing.
        /// </summary>
        Deleted,
    }
}