namespace OneSTools.FileDatabase.HighLevel
{
    public enum FieldType
    {
        /// <summary>
        /// Fixed-length binary data
        /// </summary>
        Binary,
        /// <summary>
        /// Boolean value
        /// </summary>
        Logical,
        /// <summary>
        /// Fixed point decimal
        /// </summary>
        Numeric,
        /// <summary>
        /// Fixed-length Unicode string
        /// </summary>
        NChar, // Unicode string
        /// <summary>
        /// Variable-length Unicode string
        /// </summary>
        NVarChar,
        /// <summary>
        /// Version of the row
        /// </summary>
        RowVersion,
        /// <summary>
        /// Unlimited-length Unicode string (UTF-16)
        /// </summary>
        NText,
        /// <summary>
        /// Unlimited-length binary data
        /// </summary>
        Image,
        /// <summary>
        /// Date and time
        /// </summary>
        DateTime
    }
}
