﻿using Microsoft.EntityFrameworkCore;
using PieShop.Models.Repositories.Interfaces;

namespace PieShop.Models
{
    public class ShoppingCart : IShoppingCart
    {
        private readonly PieShopDbContext _context;
        public string? ShoppingCartId { get; set; }
        public List<ShoppingCartItem> shoppingCartItems { get; set; } = default!;

        public ShoppingCart(PieShopDbContext context)
        {
            _context = context;
        }

        public static ShoppingCart GetCart(IServiceProvider services)
        {
            ISession? session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext?.Session;

            PieShopDbContext context = services.GetService<PieShopDbContext>() ?? throw new Exception("Error initializing");

            string cartId = session?.GetString("CartId") ?? Guid.NewGuid().ToString();

            session?.SetString("CartId", cartId);

            return new ShoppingCart(context) { ShoppingCartId = cartId };
        }

        public void AddToCart(Pie pie)
        {
            var shoppingCartItem = _context.ShoppingCartItems.SingleOrDefault(s => s.Pie.PieId == pie.PieId && s.ShoppingCartId == ShoppingCartId);

            if (shoppingCartItem == null)
            {
                shoppingCartItem = new ShoppingCartItem
                {
                    ShoppingCartId = ShoppingCartId,
                    Pie = pie,
                    Amount = 1
                };
                _context.ShoppingCartItems.Add(shoppingCartItem);
            } else
            {
                shoppingCartItem.Amount++;
            }

            _context.SaveChanges();
        }

        public void ClearCart()
        {
            var cartItems = _context.ShoppingCartItems.Where(c => c.ShoppingCartId == ShoppingCartId);

            _context.ShoppingCartItems.RemoveRange(cartItems);
            _context.SaveChanges();
        }

        public List<ShoppingCartItem> GetAll()
        {
            return shoppingCartItems ??= _context.ShoppingCartItems.Where(c => c.ShoppingCartId == ShoppingCartId).Include(s => s.Pie).ToList();
        }

        public decimal GetTotalPrice()
        {
            return _context.ShoppingCartItems.Where(c => c.ShoppingCartId == ShoppingCartId).Select(i => i.Amount * i.Pie.Price).Sum();
        }

        public int RemoveFromCart(Pie pie)
        {
            var shoppingCartItem = _context.ShoppingCartItems.SingleOrDefault(s => s.Pie.PieId == pie.PieId && s.ShoppingCartId == ShoppingCartId);

            var localAmount = 0;

            if (shoppingCartItem != null)
            {
                if (shoppingCartItem.Amount > 1){
                    shoppingCartItem.Amount --;
                    localAmount = shoppingCartItem.Amount;
                }
                else
                {
                    _context.ShoppingCartItems.Remove(shoppingCartItem);
                }
                _context.SaveChanges();
            }

            return localAmount;
        }
    }
}
