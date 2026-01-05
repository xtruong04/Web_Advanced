using System.ComponentModel.DataAnnotations;

namespace ClothesShop.Models
{
    public class Inventory
    {
        #region ChangeLog Types
        public enum ChangeLogType
        {
            Addition=1,
            Removal=2,
            Cancellation = 3,
            Adjustment =4
        }
        #endregion
        [Key]
        public int Id { get; set; }
        public int LogId { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public ChangeLogType ChangeType { get; set; }
        public int QuantityChanged { get; set; }
        public DateTime ChangeDate { get; set; } = DateTime.Now;
        public string Notes { get; set; } = string.Empty;
    }
}
