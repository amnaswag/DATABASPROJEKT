using EHandelDb.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EHandelDb.Services
{
    public class ShopService
    {
        public async Task RunMainMenuAsync()
        {
            await SeedDataAsync();

            while (true)
            {
                // Meny
                Console.WriteLine("\n----Welcome to Shop----");
                Console.WriteLine("\nPick an option: 1 - Categories | 2 - Customers | 3 - Orders | 4 - Products | 5 - Exit"); 
                Console.Write("Your choice: "); 
                string input = Console.ReadLine() ?? string.Empty;

                var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var cmd = parts[0].ToLowerInvariant();

                switch (cmd)
                {
                    case "1": await CategoryMenuAsync(); break;
                    case "2": await CustomerMenuAsync(); break;
                    case "3": await OrderMenuAsync(); break;
                    case "4": await ProductMenuAsync(); break;
                    case "5": Console.WriteLine("Exiting..."); return;
                    default: Console.WriteLine("Please enter a valid option."); break;
                }
            }
        }

        // --- SEEDING LOGIC (Bas-seeding) ---
        private async Task SeedDataAsync()
        {
            using var db = new ShopContext();
            await db.Database.MigrateAsync();

            if (!await db.Categories.AnyAsync())
            {
                db.Categories.AddRange(
                    new Category { CategoryName = "Mejeri", CategoryDescription = "Mjölkprodukter, smör, ost m.m." },
                    new Category { CategoryName = "Kött", CategoryDescription = "Köttprodukter av olika slag." },
                    new Category { CategoryName = "FruktOchGrönt", CategoryDescription = "Frukt och grönsaker." }
                );
                Console.WriteLine("Categories seeded to DB");
            }

            if (!await db.Customers.AnyAsync())
            {
                // Bas-seeding (Hash/Salt borttaget)
                db.Customers.AddRange(
                    new Customer { CustomerName = "Israa Tarabeih", Email = "Israa@hotmail.com", City = "Västervik"},
                    new Customer { CustomerName = "Anna Andersson", Email = "AnnaA@hotmail.com", City = "Stockholm"},
                    new Customer { CustomerName = "Arvid Castello", Email = "Arvid@gmail.com", City = "London"}
                );
                Console.WriteLine("Customers seeded to DB");
            }
            await db.SaveChangesAsync();

            var mejeri = await db.Categories.FirstAsync(c => c.CategoryName == "Mejeri");
            var kott = await db.Categories.FirstAsync(c => c.CategoryName == "Kött");
            var fruktochgrönt = await db.Categories.FirstAsync(c => c.CategoryName == "FruktOchGrönt");

            if (!await db.Products.AnyAsync())
            {
                db.Products.AddRange(
                    new Product { ProductName = "Mjölk", Price = 45, Category = mejeri },
                    new Product { ProductName = "Banan", Price = 34, Category = fruktochgrönt },
                    new Product { ProductName = "Ost", Price = 89, Category = mejeri },
                    new Product { ProductName = "Smör", Price = 48, Category = mejeri },
                    new Product { ProductName = "Äpple", Price = 22, Category = fruktochgrönt },
                    new Product { ProductName = "Köttfärs", Price = 129, Category = kott }
                );
                await db.SaveChangesAsync();
                Console.WriteLine("Products sedded to DB");
            }
        }

        
        
        public async Task CategoryMenuAsync()
        {
            using var db = new ShopContext();

            while (true)
            {
                Console.WriteLine("Would you like to: 1 - Show categories | 2 - Add category | 3 - Edit category <id> | 4 - Delete category <id> | 5 - Return to menu");
                Console.Write("Your choice: ");
                string input = Console.ReadLine() ?? string.Empty;

                var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var cmd = parts[0].ToLowerInvariant();

                switch (cmd)
                {
                    case "1": await ListCategoriesAsync(); break;
                    case "2": await AddCategoryAsync(); break;
                    case "3":
                        if (parts.Length < 2 || !int.TryParse(parts[1], out var id)) { Console.WriteLine("You must choose an id."); break; }
                        await EditCategoryAsync(id); break;
                    case "4":
                        if (parts.Length < 2 || !int.TryParse(parts[1], out var idD)) { Console.WriteLine("You must choose an id."); break; }
                        await DeleteCategoryAsync(idD); break;
                    case "5": Console.WriteLine("Returning to main menu..."); return;
                    default: Console.WriteLine("Please enter a valid option."); break;
                }
            }
        }

        public async Task CustomerMenuAsync()
        {
            using var db = new ShopContext();

            while (true)
            {
                Console.WriteLine("Would you like to: 1 - Show customers | 2 - Add Customer | 3 - Edit Customer <id> | 4 - Delete customer <id> | 5 - Return to menu");
                Console.Write("Your choice: ");
                string input = Console.ReadLine() ?? string.Empty;

                var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var cmd = parts[0].ToLowerInvariant();

                switch (cmd)
                {
                    case "1": await ListCustomersAsync(); break;
                    case "2": await AddCustomerAsync(); break;
                    case "3":
                        if (parts.Length < 2 || !int.TryParse(parts[1], out var id)) { Console.WriteLine("You must choose an id."); break; }
                        await EditCustomerAsync(id); break;
                    case "4":
                        if (parts.Length < 2 || !int.TryParse(parts[1], out var idD)) { Console.WriteLine("You must choose an id."); break; }
                        await DeleteCustomerAsync(idD); break;
                    case "5": Console.WriteLine("Returning to main menu..."); return;
                    default: Console.WriteLine("Please enter a valid option."); break;
                }
            }
        }

        public async Task OrderMenuAsync()
        {
            using var db = new ShopContext(); 

            while (true)
            {
                Console.WriteLine("Would you like to: 1 - Show orders | 2 - Add order | 3 - Edit order <id> | 4 - Delete order <id> ");
                Console.WriteLine("                   5 - Orders by status <status> | 6 - Orders page <page> <pageSize> | 7 - Return to menu");
                Console.Write("Your choice: ");
                string input = Console.ReadLine() ?? string.Empty;

                var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var cmd = parts[0].ToLowerInvariant();

                switch (cmd)
                {
                    case "1": await ListOrdersAsync(); break;
                    case "2": await AddOrderAsync(); break;
                    case "3":
                        if (parts.Length < 2 || !int.TryParse(parts[1], out var id)) { Console.WriteLine("You must choose an id."); break; }
                        await EditOrderAsync(id); break; 
                    case "4":
                        if (parts.Length < 2 || !int.TryParse(parts[1], out var idD)) { Console.WriteLine("You must choose an id."); break; }
                        await DeleteOrderAsync(idD); break;
                    case "5":
                        if (parts.Length < 2) { Console.WriteLine("Invalid input, write any of the following: Pending/Shipped/Cancelled."); break; }
                        await OrdersByStatusAsync(parts[1]); break;
                    case "6":
                        if (parts.Length < 3 || !int.TryParse(parts[1], out var page) || !int.TryParse(parts[2], out var pageSize) || page <= 0 || pageSize <= 0) { Console.WriteLine("You must enter two numbers: page and page size. For example: 6 2 4."); break; }
                        await OrdersPageAsync(page, pageSize); break;
                    case "7": Console.WriteLine("Returning to main menu..."); return;
                    default: Console.WriteLine("Please enter a valid option."); break;
                }
            }
        }

        public async Task ProductMenuAsync()
        {
            using var db = new ShopContext(); 

            while (true)
            {
                Console.WriteLine("Would you like to: 1 - Show products | 2 - Add product | 3 - Edit product <id> | 4 - Delete product <id> | 5 - Return to menu");
                Console.Write("Your choice: ");
                string input = Console.ReadLine() ?? string.Empty;

                var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var cmd = parts[0].ToLowerInvariant();

                switch (cmd)
                {
                    case "1": await ListProductsAsync(); break;
                    case "2": await AddProductAsync(); break;
                    case "3":
                        if (parts.Length < 2 || !int.TryParse(parts[1], out var id)) { Console.WriteLine("You must choose an id."); break; }
                        await EditProductAsync(id); break;
                    case "4":
                        if (parts.Length < 2 || !int.TryParse(parts[1], out var idD)) { Console.WriteLine("You must choose an id."); break; }
                        await DeleteProductAsync(idD); break;
                    case "5": Console.WriteLine("Returning to main menu..."); return;
                    default: Console.WriteLine("Please enter a valid option."); break;
                }
            }
        }
        
        // --- CRUD OCH LIST METODER  ---

        public async Task ListCustomersAsync()
        {
            using var db = new ShopContext(); 
            var customers = await db.Customers.AsNoTracking().OrderBy(c => c.CustomerId).ToListAsync();

            Console.WriteLine(" ID | Name | City | Email");
            foreach (var customer in customers)
            {
                Console.WriteLine($" {customer.CustomerId} | {customer.CustomerName} | {customer.City} | {customer.Email} ");
            }
        }

        public async Task AddCustomerAsync()
        {
            using var db = new ShopContext();

            Console.Write("Name: ");
            var name = Console.ReadLine()?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(name) || name.Length > 100) { Console.WriteLine("Name is required."); return; }

            Console.Write("Email: ");
            var mail = Console.ReadLine()?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(mail) || mail.Length > 100) { Console.WriteLine("Email is required."); return; }

            var emailExists = await db.Customers.AnyAsync(c => c.Email == mail);
            if (emailExists) { Console.WriteLine("A customer with this email already exists. Choose a diffrent email."); return; }
            
            var emptyPasswordHash = string.Empty;
            var emptySalt = string.Empty;
            
            db.Customers.Add(new Customer
            {
                CustomerName = name,
                Email = mail,
            });

            try
            {
                await db.SaveChangesAsync();
                Console.WriteLine("Customer added!");
            }
            catch (DbUpdateException exception)
            {
                Console.WriteLine("Error saving customer: " + exception.GetBaseException().Message);
            }
        }

        public async Task EditCustomerAsync(int id)
        {
            using var db = new ShopContext();

            var customer = await db.Customers.FirstOrDefaultAsync(c => c.CustomerId == id);
            if (customer == null) { Console.WriteLine("Customer not found"); return; }

            Console.Write($"New Name (Current: {customer.CustomerName}): ");
            var name = Console.ReadLine()?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(name) && name.Length < 100) { customer.CustomerName = name; }

            Console.Write($"New email (Current: {customer.Email}): ");
            var mail = Console.ReadLine()?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(mail) && mail.Length < 100) { customer.Email = mail; }
            

            try
            {
                await db.SaveChangesAsync();
                Console.WriteLine("Changes saved!");
            }
            catch (DbUpdateException exception)
            {
                Console.WriteLine("Error saving changes: " + exception.GetBaseException().Message);
            }
        }

        public async Task DeleteCustomerAsync(int idD)
        {
            using var db = new ShopContext();

            var customer = await db.Customers.FirstOrDefaultAsync(d => d.CustomerId == idD);
            if (customer == null) { Console.WriteLine("Customer not found!"); return; }

            db.Customers.Remove(customer);
            try
            {
                await db.SaveChangesAsync();
                Console.WriteLine("Customer deleted!");
            }
            catch (DbUpdateException exception)
            {
                Console.WriteLine("Error: " + exception.GetBaseException().Message);
            }
        }

        public async Task ListOrdersAsync()
        {
            using var db = new ShopContext();

            var orders = await db.Orders
                .AsNoTracking()
                .Include(c => c.Customer)
                .OrderBy(c => c.CustomerId)
                .ToListAsync();

            Console.WriteLine(" OrderID | Customer | OrderDate | Status ");
            foreach (var order in orders)
            {
                Console.WriteLine($" {order.OrderId} | {order.Customer?.CustomerName} | {order.OrderDate:yyyy-MM-dd} | {order.Status} ");
            }
        }

        public async Task AddOrderAsync()
        {
            using var db = new ShopContext();

            var customers = await db.Customers.AsNoTracking().ToListAsync();
            if (!customers.Any()) { Console.WriteLine("No customers found."); return; }

            Console.Write("Please enter customerId: ");
            if (!int.TryParse(Console.ReadLine(), out var customerId) || !customers.Any(c => c.CustomerId == customerId )) { Console.WriteLine("Invalid input of customerId"); return; }

            var order = new Order { CustomerId = customerId, OrderDate = DateTime.Now, Status = "Pending" };
            var orderRows = new List<OrderRow>();

            while (true)
            {
                var products = await db.Products.AsNoTracking().ToListAsync();
                if (!products.Any()) { Console.WriteLine("No products found."); return; }
                
                Console.Write("Enter productId: ");
                if (!int.TryParse(Console.ReadLine(), out var productId)) { Console.WriteLine("Invalid input of productId"); continue; }
                
                var chosenProduct = await db.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
                if (chosenProduct == null) { Console.WriteLine("Product not found"); continue; }

                Console.Write("Enter quantity: ");
                if(!int.TryParse(Console.ReadLine(), out var quantity) || quantity <= 0) { Console.WriteLine("Invalid input."); continue; }

                orderRows.Add(new OrderRow { ProductId = productId, Quantity = quantity, UnitPrice = chosenProduct.Price });

                Console.Write("Add a new order row? yes/no: ");
                if (Console.ReadLine()?.Trim().ToLowerInvariant() != "yes") break;
            }

            if (!orderRows.Any()) { Console.WriteLine("No order rows added. Order cancelled."); return; }

            order.OrderRows = orderRows;
            db.Orders.Add(order);

            try
            {
                await db.SaveChangesAsync();
                Console.WriteLine($"Order {order.OrderId} created!");
            }
            catch (DbUpdateException exception)
            {
                Console.WriteLine("DB Error: " + exception.GetBaseException().Message);
            }
        }

        public async Task EditOrderAsync(int id) { Console.WriteLine("Edit Order not implemented for G-level."); }
        public async Task DeleteOrderAsync(int idD) { Console.WriteLine("Delete Order not implemented for G-level."); }
        public async Task OrdersByStatusAsync(string status) { Console.WriteLine($"Orders By Status '{status}' not implemented for G-level."); }
        public async Task OrdersPageAsync(int page, int pageSize) { Console.WriteLine($"Orders Page {page} not implemented for G-level."); }

        public async Task ListProductsAsync()
        {
            using var db = new ShopContext();

            var products = await db.Products.AsNoTracking().OrderBy(p => p.ProductId).ToListAsync();

            Console.WriteLine(" ID | Name | Price ");
            foreach (var product in products)
            {
                Console.WriteLine($" {product.ProductId} | {product.ProductName} | {product.Price} ");
            }
        }

        public async Task AddProductAsync()
        {
            using var db = new ShopContext();

            var categories = await db.Categories.AsNoTracking().ToListAsync();
            if (!categories.Any()) { Console.WriteLine("No categories found. Add a category first."); return; }

            Console.Write("Please enter category id: ");
            if (!int.TryParse(Console.ReadLine(), out var categoryId) || !categories.Any(c => c.CategoryId == categoryId)) { Console.WriteLine("Invalid category id or category not found."); return; }

            Console.Write("Enter new product name: ");
            var name = Console.ReadLine()?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(name) || name.Length > 100) { Console.WriteLine("Invalid product name (max 100 characters"); return; }

            Console.Write("Enter a new price: ");
            if (!decimal.TryParse(Console.ReadLine(), out var price) || price < 0) { Console.WriteLine("Invalid price"); return; }

            db.Products.Add(new Product { ProductName = name, Price = price, CategoryId = categoryId });

            try
            {
                await db.SaveChangesAsync();
                Console.WriteLine("Product added!");
            }
            catch (DbUpdateException exception)
            {
                Console.WriteLine("Error saving product: " + exception.GetBaseException().Message);
            }
        }

        public async Task EditProductAsync(int id)
        {
            using var db = new ShopContext();

            var product = await db.Products.Include(c => c.Category).FirstOrDefaultAsync(c => c.ProductId == id);
            if (product == null) { Console.WriteLine("Product not found"); return; }

            Console.WriteLine($"Editing product: {product.ProductId}");
            Console.WriteLine($"Current name: {product.ProductName}");

            Console.Write("Enter new product name: ");
            var nameInput = Console.ReadLine()?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(nameInput) && nameInput.Length <= 100) { product.ProductName = nameInput; }

            Console.Write($"New price: ");
            var priceInput = Console.ReadLine()?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(priceInput) && decimal.TryParse(priceInput, out var newPrice)) { product.Price = newPrice; }

            Console.Write($"New categoryId: ");
            var catInput = Console.ReadLine()?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(catInput) && int.TryParse(catInput, out var newCatId))
            {
                var catExists = await db.Categories.AnyAsync(c => c.CategoryId == newCatId);
                if (catExists) { product.CategoryId = newCatId; } else { Console.WriteLine("Category does not exist."); return; }
            }

            try
            {
                await db.SaveChangesAsync();
                Console.WriteLine("Changes saved!");
            }
            catch (DbUpdateException exception)
            {
                Console.WriteLine("Error saving changes: " + exception.GetBaseException().Message);
            }
        }

        public async Task DeleteProductAsync(int idD)
        {
            using var db = new ShopContext();

            var product = await db.Products.FirstOrDefaultAsync(d => d.ProductId == idD);
            if (product == null) { Console.WriteLine("Product not found!"); return; }

            var isInOrder = await db.OrderRows.AnyAsync(r => r.ProductId == idD);
            if (isInOrder) { Console.WriteLine("Error: Cannot delete product. It is included in existing orders."); return; }

            db.Products.Remove(product);
            try
            {
                await db.SaveChangesAsync();
                Console.WriteLine("Product deleted!");
            }
            catch (DbUpdateException exception)
            {
                Console.WriteLine("Error: " + exception.GetBaseException().Message);
            }
        }

        public async Task ListCategoriesAsync()
        {
            using var db = new ShopContext();

            var categories = await db.Categories.AsNoTracking().OrderBy(g => g.CategoryId).ToListAsync();

            Console.WriteLine(" ID | Name | Description ");
            foreach (var category in categories)
            {
                Console.WriteLine($" {category.CategoryId} | {category.CategoryName} | {category.CategoryDescription} ");
            }
        }

        public async Task AddCategoryAsync()
        {
            using var db = new ShopContext();

            Console.Write("Choose a category name: ");
            var name = Console.ReadLine()?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(name) || name.Length > 100) { Console.WriteLine("Name is required, needs to be less 100 characters."); return; }

            Console.Write("Write a category description: ");
            var description = Console.ReadLine()?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(description) || description.Length > 100) { Console.WriteLine("Description is required, needs to be less than 100 characters"); return; }

            db.Categories.Add(new Category { CategoryName = name, CategoryDescription = description, });

            try
            {
                await db.SaveChangesAsync();
                Console.WriteLine("Category added!");
            }
            catch (DbUpdateException exception)
            {
                Console.WriteLine("Error saving category: " + exception.GetBaseException().Message);
            }
        }

        public async Task EditCategoryAsync(int id)
        {
            using var db = new ShopContext();

            var category = await db.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null) { Console.WriteLine("Category not found"); return; }

            Console.Write($"New category name (Current: {category.CategoryName}): ");
            var name = Console.ReadLine()?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(name) && name.Length < 100) { category.CategoryName = name; }

            Console.Write($"New description (Current: {category.CategoryDescription}): ");
            var description = Console.ReadLine()?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(description) && description.Length < 100) { category.CategoryDescription = description; }

            try
            {
                await db.SaveChangesAsync();
                Console.WriteLine("Changes saved!");
            }
            catch (DbUpdateException exception)
            {
                Console.WriteLine("Error saving changes: " + exception.GetBaseException().Message);
            }
        }

        public async Task DeleteCategoryAsync(int idD)
        {
            using var db = new ShopContext();

            var category = await db.Categories.FirstOrDefaultAsync(d => d.CategoryId == idD);
            if (category == null) { Console.WriteLine("Category not found!"); return; }

            var productsCount = await db.Products.CountAsync(p => p.CategoryId == idD);
            if (productsCount > 0) { Console.WriteLine($"Error: Cannot delete category '{category.CategoryName}'. {productsCount} product(s) are still linked to it."); return; }

            db.Categories.Remove(category);
            try
            {
                await db.SaveChangesAsync();
                Console.WriteLine("Category deleted!");
            }
            catch (DbUpdateException exception)
            {
                Console.WriteLine("Error: " + exception.GetBaseException().Message);
            }
        }
    }
}