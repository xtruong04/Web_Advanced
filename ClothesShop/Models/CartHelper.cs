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
                var cart = new List<CartItem>();
                SaveCart(session, cart);
                return cart;
            }

            return JsonSerializer.Deserialize<List<CartItem>>(json)
                   ?? new List<CartItem>();
        }

        public static void SaveCart(ISession session, List<CartItem> cart)
        {
            session.SetString(
                CartSessionKey,
                JsonSerializer.Serialize(cart)
            );
        }

        public static void AddToCart(ISession session, CartItem item)
        {
            var cart = GetCart(session);
            var existing = cart.FirstOrDefault(c => c.ProductId == item.ProductId);

            if (existing != null)
                existing.Quantity += item.Quantity;
            else
                cart.Add(item);

            SaveCart(session, cart);
        }

        public static void UpdateQuantity(ISession session, int productId, int quantity)
        {
            var cart = GetCart(session);
            var item = cart.FirstOrDefault(c => c.ProductId == productId);

            if (item != null)
            {
                if (quantity > 0)
                    item.Quantity = quantity;
                else
                    cart.Remove(item);

                SaveCart(session, cart);
            }
        }

        public static void RemoveFromCart(ISession session, int productId)
        {
            var cart = GetCart(session);
            var item = cart.FirstOrDefault(c => c.ProductId == productId);

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
