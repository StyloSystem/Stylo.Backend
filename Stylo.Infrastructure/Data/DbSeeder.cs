using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stylo.Backend.Stylo.Domain.Entities;
using Stylo.Backend.Stylo.Domain.Enums;

namespace Stylo.Backend.Stylo.Infrastructure.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(
            AppDbContext context,
            UserManager<User> userManager)
        {
            // Make sure database exists and migrations are applied
            await context.Database.MigrateAsync();

            // =========================================
            // 1. Categories
            // =========================================

            var categories = new List<Category>
            {
                new Category { Name = "T-Shirts" },
                new Category { Name = "Shirts" },
                new Category { Name = "Pants" },
                new Category { Name = "Hoodies" },
                new Category { Name = "Dresses" }
            };

            foreach (var category in categories)
            {
                var exists = await context.Categories
                    .AnyAsync(c => c.Name == category.Name);

                if (!exists)
                {
                    context.Categories.Add(category);
                }
            }

            await context.SaveChangesAsync();

            // Reload categories from database
            var tShirtsCategory = await context.Categories
                .FirstAsync(c => c.Name == "T-Shirts");

            var shirtsCategory = await context.Categories
                .FirstAsync(c => c.Name == "Shirts");

            var pantsCategory = await context.Categories
                .FirstAsync(c => c.Name == "Pants");

            var hoodiesCategory = await context.Categories
                .FirstAsync(c => c.Name == "Hoodies");

            var dressesCategory = await context.Categories
                .FirstAsync(c => c.Name == "Dresses");


            // =========================================
            // 2. Users
            // =========================================

            var admin = await CreateUserIfNotExists(
                userManager,
                email: "admin@stylo.com",
                name: "Stylo Admin",
                role: "Admin",
                password: "Admin123!"
            );

            var customer1 = await CreateUserIfNotExists(
                userManager,
                email: "customer1@stylo.com",
                name: "Ahmed Mohamed",
                role: "Customer",
                password: "Customer123!"
            );

            var customer2 = await CreateUserIfNotExists(
                userManager,
                email: "customer2@stylo.com",
                name: "Omar Ali",
                role: "Customer",
                password: "Customer123!"
            );

            var customer3 = await CreateUserIfNotExists(
                userManager,
                email: "customer3@stylo.com",
                name: "Youssef Hassan",
                role: "Customer",
                password: "Customer123!"
            );


            // =========================================
            // 3. Products
            // =========================================

            await SeedProductsAsync(
                context,
                tShirtsCategory,
                shirtsCategory,
                pantsCategory,
                hoodiesCategory,
                dressesCategory
            );

            var blackTShirt = await context.Products
                .FirstAsync(p => p.Name == "Classic Black T-Shirt");

            var whiteTShirt = await context.Products
                .FirstAsync(p => p.Name == "Classic White T-Shirt");

            var blueShirt = await context.Products
                .FirstAsync(p => p.Name == "Oxford Blue Shirt");

            var blackPants = await context.Products
                .FirstAsync(p => p.Name == "Slim Fit Black Pants");

            var grayHoodie = await context.Products
                .FirstAsync(p => p.Name == "Essential Gray Hoodie");

            var summerDress = await context.Products
                .FirstAsync(p => p.Name == "Summer Floral Dress");


            // =========================================
            // 4. Product Sizes
            // =========================================

            await SeedProductSizesAsync(context, blackTShirt);
            await SeedProductSizesAsync(context, whiteTShirt);
            await SeedProductSizesAsync(context, blueShirt);
            await SeedProductSizesAsync(context, blackPants);
            await SeedProductSizesAsync(context, grayHoodie);
            await SeedProductSizesAsync(context, summerDress);


            // =========================================
            // 5. Carts
            // =========================================

            await CreateCartIfNotExists(context, customer1.Id);
            await CreateCartIfNotExists(context, customer2.Id);
            await CreateCartIfNotExists(context, customer3.Id);


            // =========================================
            // 6. Cart Items
            // =========================================

            var customer1Cart = await context.Carts
                .FirstAsync(c => c.UserId == customer1.Id);

            var customer2Cart = await context.Carts
                .FirstAsync(c => c.UserId == customer2.Id);

            await AddCartItemIfNotExists(
                context,
                customer1Cart.Id,
                blackTShirt.Id,
                Size.M,
                2
            );

            await AddCartItemIfNotExists(
                context,
                customer1Cart.Id,
                grayHoodie.Id,
                Size.L,
                1
            );

            await AddCartItemIfNotExists(
                context,
                customer2Cart.Id,
                summerDress.Id,
                Size.M,
                1
            );


            // =========================================
            // 7. Orders
            // =========================================

            var order1 = await CreateOrderIfNotExists(
                context,
                customer1,
                blackTShirt,
                Size.M,
                quantity: 2,
                status: OrderStatus.Confirmed
            );

            var order2 = await CreateOrderIfNotExists(
                context,
                customer2,
                summerDress,
                Size.M,
                quantity: 1,
                status: OrderStatus.Confirmed
            );

            var order3 = await CreateOrderIfNotExists(
                context,
                customer3,
                blueShirt,
                Size.L,
                quantity: 1,
                status: OrderStatus.Pending
            );


            // =========================================
            // 8. Favorites
            // =========================================

            await AddFavoriteIfNotExists(
                context,
                customer1.Id,
                whiteTShirt.Id
            );

            await AddFavoriteIfNotExists(
                context,
                customer1.Id,
                blueShirt.Id
            );

            await AddFavoriteIfNotExists(
                context,
                customer2.Id,
                grayHoodie.Id
            );

            await AddFavoriteIfNotExists(
                context,
                customer3.Id,
                blackPants.Id
            );


            // =========================================
            // 9. Product Feedbacks
            // =========================================

            await AddProductFeedbackIfNotExists(
                context,
                customer1,
                blackTShirt,
                order1,
                "The quality is really good and the size fits perfectly."
            );

            await AddProductFeedbackIfNotExists(
                context,
                customer2,
                summerDress,
                order2,
                "Very comfortable and the material feels great."
            );


            // =========================================
            // 10. Website Feedbacks
            // =========================================

            await AddWebsiteFeedbackIfNotExists(
                context,
                customer1.Id,
                "The website is very easy to use.",
                true
            );

            await AddWebsiteFeedbackIfNotExists(
                context,
                customer2.Id,
                "I really like the product browsing experience.",
                false
            );

            await AddWebsiteFeedbackIfNotExists(
                context,
                customer3.Id,
                "The checkout process was simple and fast.",
                true
            );


            await context.SaveChangesAsync();
        }


        // =========================================================
        // USERS
        // =========================================================

        private static async Task<User> CreateUserIfNotExists(
            UserManager<User> userManager,
            string email,
            string name,
            string role,
            string password)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user != null)
            {
                return user;
            }

            user = new User
            {
                UserName = email,
                Email = email,
                Name = name,
                Role = role,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    result.Errors.Select(e => e.Description)
                );

                throw new Exception(
                    $"Failed to create user {email}: {errors}"
                );
            }

            return user;
        }


        // =========================================================
        // PRODUCTS
        // =========================================================

        private static async Task SeedProductsAsync(
            AppDbContext context,
            Category tShirts,
            Category shirts,
            Category pants,
            Category hoodies,
            Category dresses)
        {
            var products = new List<Product>
            {
                new Product
                {
                    Name = "Classic Black T-Shirt",
                    Description = "Classic black cotton t-shirt.",
                    Price = 450,
                    Gender = Gender.Men,
                    CategoryId = tShirts.Id,
                    ImageUrl = "https://placehold.co/600x800?text=Black+T-Shirt"
                },

                new Product
                {
                    Name = "Classic White T-Shirt",
                    Description = "Simple white cotton t-shirt.",
                    Price = 400,
                    Gender = Gender.Men,
                    CategoryId = tShirts.Id,
                    ImageUrl = "https://placehold.co/600x800?text=White+T-Shirt"
                },

                new Product
                {
                    Name = "Oxford Blue Shirt",
                    Description = "Classic blue Oxford shirt.",
                    Price = 750,
                    Gender = Gender.Men,
                    CategoryId = shirts.Id,
                    ImageUrl = "https://placehold.co/600x800?text=Blue+Shirt"
                },

                new Product
                {
                    Name = "Slim Fit Black Pants",
                    Description = "Modern slim fit black pants.",
                    Price = 850,
                    Gender = Gender.Men,
                    CategoryId = pants.Id,
                    ImageUrl = "https://placehold.co/600x800?text=Black+Pants"
                },

                new Product
                {
                    Name = "Essential Gray Hoodie",
                    Description = "Comfortable everyday gray hoodie.",
                    Price = 950,
                    Gender = Gender.Men,
                    CategoryId = hoodies.Id,
                    ImageUrl = "https://placehold.co/600x800?text=Gray+Hoodie"
                },

                new Product
                {
                    Name = "Summer Floral Dress",
                    Description = "Light floral dress for summer.",
                    Price = 1100,
                    Gender = Gender.Women,
                    CategoryId = dresses.Id,
                    ImageUrl = "https://placehold.co/600x800?text=Summer+Dress"
                }
            };

            foreach (var product in products)
            {
                var exists = await context.Products
                    .AnyAsync(p => p.Name == product.Name);

                if (!exists)
                {
                    context.Products.Add(product);
                }
            }

            await context.SaveChangesAsync();
        }


        // =========================================================
        // PRODUCT SIZES
        // =========================================================

        private static async Task SeedProductSizesAsync(
            AppDbContext context,
            Product product)
        {
            var sizes = new[]
            {
                Size.S,
                Size.M,
                Size.L,
                Size.XL,
                Size.XXL
            };

            foreach (var size in sizes)
            {
                var exists = await context.ProductSizes
                    .AnyAsync(ps =>
                        ps.ProductId == product.Id &&
                        ps.Size == size);

                if (!exists)
                {
                    context.ProductSizes.Add(new ProductSize
                    {
                        ProductId = product.Id,
                        Size = size,
                        Stock = 20
                    });
                }
            }

            await context.SaveChangesAsync();
        }


        // =========================================================
        // CART
        // =========================================================

        private static async Task CreateCartIfNotExists(
            AppDbContext context,
            int userId)
        {
            var exists = await context.Carts
                .AnyAsync(c => c.UserId == userId);

            if (!exists)
            {
                context.Carts.Add(new Cart
                {
                    UserId = userId
                });

                await context.SaveChangesAsync();
            }
        }


        // =========================================================
        // CART ITEMS
        // =========================================================

        private static async Task AddCartItemIfNotExists(
            AppDbContext context,
            int cartId,
            int productId,
            Size size,
            int quantity)
        {
            var exists = await context.CartItems
                .AnyAsync(ci =>
                    ci.CartId == cartId &&
                    ci.ProductId == productId &&
                    ci.Size == size.ToString());

            if (!exists)
            {
                context.CartItems.Add(new CartItem
                {
                    CartId = cartId,
                    ProductId = productId,
                    Size = size.ToString(),
                    Quantity = quantity
                });

                await context.SaveChangesAsync();
            }
        }


        // =========================================================
        // ORDERS
        // =========================================================

        private static async Task<Order> CreateOrderIfNotExists(
            AppDbContext context,
            User user,
            Product product,
            Size size,
            int quantity,
            OrderStatus status)
        {
            var existingOrder = await context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o =>
                    o.UserId == user.Id &&
                    o.OrderItems.Any(oi =>
                        oi.ProductId == product.Id));

            if (existingOrder != null)
            {
                return existingOrder;
            }

            var totalPrice = product.Price * quantity;

            var order = new Order
            {
                UserId = user.Id,
                RecipientName = user.Name,
                ContactPhone = user.PhoneNumber ?? "01000000000",
                ShippingAddress = "Cairo, Egypt",
                PaymentMethod = PaymentMethod.CashOnDelivery,
                Status = status,
                TotalPrice = totalPrice,
                CreatedAt = DateTime.UtcNow
            };

            var orderItem = new OrderItem
            {
                Order = order,
                ProductId = product.Id,
                Size = size.ToString(),
                Quantity = quantity,
                UnitPriceAtPurchase = product.Price,
                Status = status == OrderStatus.Confirmed
                    ? OrderItemStatus.Confirmed
                    : OrderItemStatus.Pending
            };

            order.OrderItems.Add(orderItem);

            context.Orders.Add(order);

            await context.SaveChangesAsync();

            return order;
        }


        // =========================================================
        // FAVORITES
        // =========================================================

        private static async Task AddFavoriteIfNotExists(
            AppDbContext context,
            int userId,
            int productId)
        {
            var exists = await context.Favorites
                .AnyAsync(f =>
                    f.UserId == userId &&
                    f.ProductId == productId);

            if (!exists)
            {
                context.Favorites.Add(new Favorite
                {
                    UserId = userId,
                    ProductId = productId
                });

                await context.SaveChangesAsync();
            }
        }


        // =========================================================
        // PRODUCT FEEDBACK
        // =========================================================

        private static async Task AddProductFeedbackIfNotExists(
            AppDbContext context,
            User user,
            Product product,
            Order order,
            string message)
        {
            var exists = await context.ProductFeedbacks
                .AnyAsync(f =>
                    f.UserId == user.Id &&
                    f.ProductId == product.Id);

            if (!exists)
            {
                context.ProductFeedbacks.Add(new ProductFeedback
                {
                    UserId = user.Id,
                    ProductId = product.Id,
                    OrderId = order.Id,
                    Message = message,
                    IsFeatured = false,
                    CreatedAt = DateTime.UtcNow
                });

                await context.SaveChangesAsync();
            }
        }


        // =========================================================
        // WEBSITE FEEDBACK
        // =========================================================

        private static async Task AddWebsiteFeedbackIfNotExists(
            AppDbContext context,
            int userId,
            string message,
            bool isFeatured)
        {
            var exists = await context.WebsiteFeedbacks
                .AnyAsync(f =>
                    f.UserId == userId &&
                    f.Message == message);

            if (!exists)
            {
                context.WebsiteFeedbacks.Add(new WebsiteFeedback
                {
                    UserId = userId,
                    Message = message,
                    IsFeatured = isFeatured,
                    CreatedAt = DateTime.UtcNow
                });

                await context.SaveChangesAsync();
            }
        }
    }
}