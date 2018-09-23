﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WingtipToys.Models;

namespace WingtipToys.Logic
{
    public class ShoppingCartActions : IDisposable
    {
        public string ShoppingCartId { get; set; }

        private ProductContext _db = new ProductContext();

        public const string CartSessionKey = "CartId";

        public void AddToCart(int id)
        {
            //Retrieve the product from database
            ShoppingCartId = GetCartId();

            var cartItem = _db.ShoppingCartItems.SingleOrDefault(
                c => c.CartId == ShoppingCartId
                && c.ProductId == id);
            if (cartItem == null)
            {
                //create a new cart item if no cart item exists
                cartItem = new CartItem
                {
                    ItemId = Guid.NewGuid().ToString(),
                    ProductId = id,
                    CartId = ShoppingCartId,
                    Product = _db.Products.SingleOrDefault(

                        p => p.ProductID == id),
                    Quantity = 1,
                    DateCreated = DateTime.Now
                };

                _db.ShoppingCartItems.Add(cartItem);
            }
            else
            {
                //if the item doest exist in the cart, then add one to the quantity
                cartItem.Quantity++;
            }
            _db.SaveChanges();
        }

        public void Dispose()
        {
            if (_db != null)
            {
                _db.Dispose();
                _db = null;
            }
        }

        public string GetCartId()
        {
            if (HttpContext.Current.Session[CartSessionKey] == null)
            {
                if (!string.IsNullOrWhiteSpace(HttpContext.Current.User.Identity.Name))
                {
                    HttpContext.Current.Session[CartSessionKey] = HttpContext.Current.User.Identity.Name;

                }
                else
                {
                    //generate a new random GUID using System.Cuid class
                    Guid tempCartId = Guid.NewGuid();
                    HttpContext.Current.Session[CartSessionKey] = tempCartId.ToString();

                }
            }
            return HttpContext.Current.Session[CartSessionKey].ToString();

        }

        public List<CartItem> GetCartItems()
        {
            ShoppingCartId = GetCartId();
            return _db.ShoppingCartItems.Where(
                c => c.CartId == ShoppingCartId).ToList();

        }

        public decimal GetTotal()
        {
            ShoppingCartId = GetCartId();
            //multiply product price by qty of that product to get the current price for each of those
            //products in the cart. Summ all product price totals to get the cart total
            decimal? total = decimal.Zero;
            total = (decimal?)(from cartItems in _db.ShoppingCartItems
                               where cartItems.CartId == ShoppingCartId
                               select (int?)cartItems.Quantity *
                               cartItems.Product.UnitPrice).Sum();
            return total ?? decimal.Zero;
        }


    }
}
