using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;

namespace ClothesShop.Models
{
    public static class CartHelper
    {
        private const string CartSessionKey = "Cart";

        public static List<CartItem> GetCart(ISession session)
        {
            var json = session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(json))
            {
                return new List<CartItem>();
            }
            return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
        }

        public static void SaveCart(ISession session, List<CartItem> cart)
        {
            session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
        }

        // SỬA: So khớp bằng ProductId VÀ Size
        public static void AddToCart(ISession session, CartItem item)
        {
            var cart = GetCart(session);
            // Một item được gọi là trùng nếu cùng ID và cùng Size
            var existing = cart.FirstOrDefault(c => c.ProductId == item.ProductId && c.Size == item.Size);

            if (existing != null)
                existing.Quantity += item.Quantity;
            else
                cart.Add(item);

            SaveCart(session, cart);
        }

        // SỬA: Cần thêm tham số size để biết chính xác dòng nào cần update
        public static void UpdateQuantity(ISession session, int productId, string size, int quantity)
        {
            var cart = GetCart(session);
            var item = cart.FirstOrDefault(c => c.ProductId == productId && c.Size == size);

            if (item != null)
            {
                if (quantity > 0)
                    item.Quantity = quantity;
                else
                    cart.Remove(item);

                SaveCart(session, cart);
            }
        }

        // SỬA: Xóa dựa trên cả ID và Size
        public static void RemoveFromCart(ISession session, int productId, string size)
        {
            var cart = GetCart(session);
            var item = cart.FirstOrDefault(c => c.ProductId == productId && c.Size == size);

            if (item != null)
            {
                cart.Remove(item);
                SaveCart(session, cart);
            }
        }

        public static void ClearCart(ISession session)
        {
            session.Remove(CartSessionKey);
        }

        public static decimal GetTotal(ISession session)
        {
            var cart = GetCart(session);
            return cart.Sum(c => c.Total);
        }

        public static int GetCartCount(ISession session)
        {
            return GetCart(session).Count;
        }
    }
}