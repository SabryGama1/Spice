using Spice.Models;
using System;

namespace Spice.Utility
{
    public class StaticDefintion
    {
        public const string ManagerUser = "Manager";
        public const string KitchenUser = "Kitchen";
        public const string FrontDeskUser = "Front Desk";
        public const string CustomerEndUser = "Customer";
        public const string ShoppingCartCount = "ShoppingCartCount";
        public const string ssCouponCode = "CouponCode";

        public const string StatusSubmitted = "Submitted";
        public const string StatusInProccess = "Begin Prepared";
        public const string StatusReady = "Ready For Pickup";
        public const string StatusCompleted = "Completed";
        public const string StatusCancalled = "Cancalled";

        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Completed";
        public const string PaymentStatusRejected = "Rejected";




        public static string ConvertToRawHtml(string source)
        {
            char[] array = new char[source.Length];
            int arrayIndex = 0;
            bool inside = false;

            for (int i = 0; i < source.Length; i++)
            {
                char let = source[i];
                if (let == '<')
                {
                    inside = true;
                    continue;
                }
                if (let == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }

        public static double DiscountPrice(Coupon coupon, double OrderTotalOrginal)
        {
            if (coupon == null)
            {
                return Math.Round(OrderTotalOrginal, 2);
            }
            else
            {
                if (coupon.MinimumAmount > OrderTotalOrginal)
                {
                    return Math.Round(OrderTotalOrginal, 2);

                }
                else
                {
                    if (int.Parse(coupon.CouponType) == (int)Coupon.ECouponType.Doller)
                    {
                        return Math.Round(OrderTotalOrginal - coupon.Discount, 2);
                    }
                    else
                    {
                        return Math.Round(OrderTotalOrginal - (OrderTotalOrginal * (coupon.Discount / 100)), 2);

                    }
                }
            }
        }

    }
}
