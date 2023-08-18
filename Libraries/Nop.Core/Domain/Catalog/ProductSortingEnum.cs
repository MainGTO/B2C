namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents the product sorting
    /// </summary>
    public enum ProductSortingEnum
    {
        /// <summary>
        /// Position (display order)
        /// </summary>
        Position = 0,

        /// <summary>
        /// Name: A to Z
        /// </summary>
        NameAsc = 5,

        /// <summary>
        /// Name: Z to A
        /// </summary>
        NameDesc = 6,

        /// <summary>
        /// Price: Low to High
        /// </summary>
        PriceAsc = 10,

        /// <summary>
        /// Price: High to Low
        /// </summary>
        PriceDesc = 11,

        /// <summary>
        /// Product creation date
        /// </summary>
        CreatedOn = 15,

        /// <summary>
        /// Product creation date descending
        /// </summary>
        CreatedOnDesc = 16,

        /// <summary>
        /// Product update date descending
        /// </summary>
        UpdatedOnDesc = 17,

        /// <summary>
        /// Product Id descending (정렬)
        /// </summary>
        IdDesc = 18, 
    }
}