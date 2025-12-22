namespace MyShop.Core.Models
{
    public enum UserRole
    {
        ADMIN,
        MANAGER,
        STAFF
    }

    public enum OrderStatus
    {
        PENDING,
        PROCESSING,
        COMPLETED,
        CANCELLED
    }

    public enum DiscountType
    {
        PERCENTAGE,
        FIXED_AMOUNT,
        BUY_X_GET_Y,
        MEMBER_DISCOUNT,
        WHOLESALE_DISCOUNT
    }
}
