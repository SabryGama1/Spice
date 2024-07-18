using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace Spice.Models
{
    public class OrderHeader
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }
        [Required]
        public DateTime OrderDate { get; set; }
        [Required]
        public double OrderTotalOrginal { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public double OrderTotal { get; set; }
        [Required]
        [Display(Name = "Pickup Time")]
        public DateTime PickUpTime { get; set; }
        [NotMapped]
        [Required]
        public DateTime PickUpDate { get; set; }


        [Display(Name = "Coupon Code")]
        public string CouponCode { get; set; }
        public double CouponCodeDiscount { get; set; }
        public string Status { get; set; }

        public string PaymentStatus { get; set; }
        public string Comments { get; set; }
        [Display(Name = "Pickup Name")]
        public string PickUpName { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
        public string TrasactionId { get; set; }

    }
}
