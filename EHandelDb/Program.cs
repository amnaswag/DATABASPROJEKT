using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.IO;
using System.Threading.Tasks;
using DATABASPROJEKT.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace DATABASPROJEKT
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("DB: " + Path.Combine(AppContext.BaseDirectory, "shop.db"));

            using (var db = new ShopContext())
            {
                await db.Database.MigrateAsync();

                if (!await db.Categories.AnyAsync())
                {
                    db.Categories.AddRange(
                        new Category { CategoryName = "Kylvaror", CategoryDescription = "Mjölkprodukter, ostar, smör, yoghurt, etc.." },
                        new Category { CategoryName = "Fryst", CategoryDescription = "Olika sorters fryst kött och fisk." },
                        new Category { CategoryName = "Skafferi", CategoryDescription = "Pasta, konserver, kaffe, etc." }
                    );

                    await db.SaveChangesAsync();
                    Console.WriteLine("Categories seeded to DB");
                }

                if (!await db.Customers.AnyAsync())
                {
                    db.Customers.AddRange(
                        new Customer { CustomerName = "Amna Swag", Email = "Amna.Swag@hotmail.com", City = "Stockholm" },
                        new Customer { CustomerName = "Ben Benson", Email = "BenB@gmail.com", City = "Malmö" },
                        new Customer { CustomerName = "Carl Carlson", Email = "CarlC@outlook.com", City = "Göteborg" }
                    );

                    await db.SaveChangesAsync();
                    Console.WriteLine("Customers seeded to DB");
                }

                if (!await db.Orders.AnyAsync())
                {
                    var customer1 = await db.Customers
                        .FirstAsync(c => c.CustomerName == "Amna Swag");
                    var customer2 = await db.Customers
                        .FirstAsync(c => c.CustomerName == "Ben Benson");

                    db.Orders.AddRange(
                        new Order { CustomerId = customer1.CustomerId, OrderDate = DateTime.Today.AddDays(-5), Status = "Pending" },
                        new Order { CustomerId = customer2.CustomerId, OrderDate = DateTime.Today.AddDays(-3), Status = "Completed" }
                    );

                    await db.SaveChangesAsync();
                    Console.WriteLine("Orders seeded to DB");
                }

                var dairy = await db.Categories.FirstAsync(g => g.CategoryName == "Kylvaror");
                var meat = await db.Categories.FirstAsync(g => g.CategoryName == "Fryst");
                var produce = await db.Categories.FirstAsync(g => g.CategoryName == "Skafferi");

                if (!await db.Products.AnyAsync())
                {
                    db.Products.AddRange(
                        new Product { ProductName = "Yoghurt", Price = 45, Category = dairy },
                        new Product { ProductName = "Linser", Price = 39, Category = produce },
                        new Product { ProductName = "Smör", Price = 59, Category = dairy },
                        new Product { ProductName = "Ägg", Price = 55, Category = dairy },
                        new Product { ProductName = "Kaffe", Price = 65, Category = produce },
                        new Product { ProductName = "Lax", Price = 179, Category = meat },
                        new Product { ProductName = "Kyckling", Price = 95, Category = meat }
                    );

                    await db.SaveChangesAsync();
                    Console.WriteLine("Products seeded to DB");
                }

                while (true)
                {
                    // Main Menu
                    Console.WriteLine("\n----Welcome to shop----");
                    Console.WriteLine("\nPick an option: 1 - Categories | 2 - Customers | 3 - Orders | 4 - Products | 5 - Exit ");
                    Console.WriteLine("Your choice: ");
                    string input = Console.ReadLine()?.Trim() ?? string.Empty;

                    switch (input)
                    {
                        case "1":
                            await CategoryMenuAsync();
                            break;
                        case "2":
                            await CustomerMenuAsync();
                            break;
                        case "3":
                            await OrderMenuAsync();
                            break;
                        case "4":
                            await ProductMenuAsync();
                            break;
                        case "5":
                            Console.WriteLine("Exiting...");
                            return;
                        default:
                            Console.WriteLine("Please enter a valid option.");
                            break;
                    }

                }

                // Menu selection methods
                // CRUD flow for Categories, Customers, Orders, Products
                static async Task CategoryMenuAsync()
                {
                    using var db = new ShopContext();

                    while (true)
                    {
                        Console.WriteLine("Would you like to: 1 - Show categories | 2 - Add category | 3 - Edit Category | 4 - Delete category | 5 - Return to menu ");
                        Console.WriteLine("Your choice: ");
                        string input = Console.ReadLine()?.Trim() ?? string.Empty;

                        switch (input)
                        {
                            case "1":
                                await ListCategoriesAsync();
                                break;
                            case "2":
                                await AddCategoryAsync();
                                break;
                            case "3":
                                Console.Write("Enter category id to edit: ");
                                var editInput = Console.ReadLine()?.Trim() ?? string.Empty;

                                if (!int.TryParse(editInput, out int editId))
                                {
                                    Console.WriteLine("You must enter a valid id.");
                                    Console.ReadKey();
                                    break;
                                }
                                await EditCategoryAsync(editId);
                                break;
                            case "4":
                                Console.Write("Enter category id to delete: ");
                                var deleteInput = Console.ReadLine()?.Trim() ?? string.Empty;

                                if (!int.TryParse(deleteInput, out int deleteId))
                                {
                                    Console.WriteLine("You must enter a valid id.");
                                    Console.ReadKey();
                                    break;
                                }
                                await DeleteCategoryAsync(deleteId);
                                break;
                            case "5":
                                Console.WriteLine("Returning to main menu...");
                                return;
                            default:
                                Console.WriteLine("Please enter a valid option");
                                break;
                        }
                    }
                }

                // Methods to view/add/edit/delete Customers
                static async Task CustomerMenuAsync()
                {
                    using var db = new ShopContext();

                    while (true)
                    {
                        Console.WriteLine("Would you like to: 1 - Show customers | 2 - Add customer | 3 - Edit customer | 4 - Delete customer | 5 - Return to menu ");
                        Console.WriteLine("Your choice: ");
                        string input = Console.ReadLine() ?? string.Empty;

                        switch (input)
                        {
                            case "1":
                                await ListCustomersAsync();
                                break;
                            case "2":
                                await AddCustomerAsync();
                                break;
                            case "3":
                                Console.Write("Enter customer id to edit: ");
                                var editInput = Console.ReadLine()?.Trim() ?? string.Empty;

                                if (!int.TryParse(editInput, out int editId))
                                {
                                    Console.WriteLine("You must enter a valid id.");
                                    Console.ReadKey();
                                    break;
                                }
                                await EditCustomerAsync(editId);
                                break;
                            case "4":
                                Console.Write("Enter customer id to delete: ");
                                var deleteInput = Console.ReadLine()?.Trim()?.Trim() ?? string.Empty;

                                if (!int.TryParse(deleteInput, out int deleteId))
                                {
                                    Console.WriteLine("You must enter a valid id.");
                                    Console.ReadKey();
                                    break;
                                }
                                await DeleteCustomerAsync(deleteId);
                                break;
                            case "5":
                                Console.WriteLine("Returning to main menu...");
                                return;
                            default:
                                Console.WriteLine("Please enter a valid option");
                                break;

                        }
                    }
                }

                // Methods to view/add/edit/delete Orders
                static async Task OrderMenuAsync()
                {
                    using var db = new ShopContext();

                    while (true)
                    {
                        Console.WriteLine("Would you like to: 1 - Show orders | 2 - Add order | 3 - Edit Order | 4 - Delete Order | 5 - Return to menu ");
                        Console.WriteLine("Your choice: ");
                        string input = Console.ReadLine()?.Trim() ?? string.Empty;


                        switch (input)
                        {
                            case "1":
                                await ListOrdersAsync();
                                break;
                            case "2":
                                await AddOrderAsync();
                                break;
                            case "3":
                                Console.Write("Enter customer id to edit: ");
                                var editInput = Console.ReadLine()?.Trim() ?? string.Empty;

                                if (!int.TryParse(editInput, out int editId))
                                {
                                    Console.WriteLine("You must enter a valid id.");
                                    Console.ReadKey();
                                    break;
                                }
                                await EditOrderAsync(editId);
                                break;
                            case "4":
                                Console.Write("Enter customer id to delete: ");
                                var deleteInput = Console.ReadLine()?.Trim()?.Trim() ?? string.Empty;

                                if (!int.TryParse(deleteInput, out int deleteId))
                                {
                                    Console.WriteLine("You must enter a valid id.");
                                    Console.ReadKey();
                                    break;
                                }
                                await DeleteOrderAsync(deleteId);
                                break;
                            case "5":
                                Console.WriteLine("Returning to main menu...");
                                return;
                            default:
                                Console.WriteLine("Please enter a valid option");
                                break;

                        }
                    }
                }

                // Methods to view/add/edit/delete Products
                static async Task ProductMenuAsync()
                {
                    using var db = new ShopContext();

                    while (true)
                    {
                        Console.WriteLine("Would you like to: 1 - Show products | 2 - Add product | 3 - Edit product | 4 - Delete product | 5 - Return to menu ");
                        Console.WriteLine("Your choice: ");
                        string input = Console.ReadLine()?.Trim() ?? string.Empty;


                        switch (input)
                        {
                            case "1":
                                await ListProductsAsync();
                                break;
                            case "2":
                                await AddProductAsync();
                                break;
                            case "3":
                                Console.Write("Enter product id to edit: ");
                                var editInput = Console.ReadLine()?.Trim() ?? string.Empty;

                                if (!int.TryParse(editInput, out int editId))
                                {
                                    Console.WriteLine("You must enter a valid id.");
                                    Console.ReadKey();
                                    break;
                                }
                                await EditProductAsync(editId);
                                break;
                            case "4":
                                Console.Write("Enter product id to delete: ");
                                var deleteInput = Console.ReadLine()?.Trim()?.Trim() ?? string.Empty;

                                if (!int.TryParse(deleteInput, out int deleteId))
                                {
                                    Console.WriteLine("You must enter a valid id.");
                                    Console.ReadKey();
                                    break;
                                }
                                await DeleteProductAsync(deleteId);
                                break;
                            case "5":
                                Console.WriteLine("Returning to main menu...");
                                return;
                            default:
                                Console.WriteLine("Please enter a valid option");
                                break;
                        }
                    }
                } 
                // After the main menu you click on these depending on what you choose.

                // Methods for viewing/adding/editing/deleting Categories
                
                static async Task ListCategoriesAsync()
                {
                    using var db = new ShopContext();

                    var categories = await db.Categories
                        .AsNoTracking()
                        .OrderBy(g => g.CategoryId)
                        .ToListAsync();

                    Console.WriteLine(" ID | Name | Description ");
                    foreach (var category in categories)
                    {
                        Console.WriteLine($" {category.CategoryId} | {category.CategoryName} | {category.CategoryDescription} ");
                    }
                }

                static async Task AddCategoryAsync()
                {
                    using var db = new ShopContext();

                    Console.Write("Choose a category name: ");
                    var name = Console.ReadLine()?.Trim() ?? string.Empty;

                    if (string.IsNullOrEmpty(name) || name.Length > 100)
                    {
                        Console.WriteLine("Name is required and must to be less than 100 characters.");
                        return;
                    }

                    Console.Write("Write a category description: ");
                    var description = Console.ReadLine()?.Trim() ?? string.Empty;

                    if (string.IsNullOrEmpty(description) || description.Length > 100)
                    {
                        Console.WriteLine("Description is required and must be less than 100 characters.");
                        return;
                    }

                    db.Categories.Add(new Category
                    {
                        CategoryName = name,
                        CategoryDescription = description
                    });

                    try
                    {
                        await db.SaveChangesAsync();
                        Console.WriteLine("Category added!");
                    }
                    catch (DbUpdateException exception)
                    {
                        Console.WriteLine("DB Error: " + exception.GetBaseException().Message);
                    }
                }

                static async Task EditCategoryAsync(int editId)
                {
                    using var db = new ShopContext();

                    var category = await db.Categories.FirstOrDefaultAsync(g => g.CategoryId == editId);
                    if (category == null)
                    {
                        Console.WriteLine("Category not found!");
                        return;
                    }

                    Console.WriteLine($"Editing category: {category.CategoryId}");
                    Console.WriteLine($"Current name: {category.CategoryName}");
                    Console.WriteLine($"Current description: {category.CategoryDescription}");

                    Console.Write($"New category name (Press enter to keep current): ");
                    var name = Console.ReadLine()?.Trim() ?? string.Empty;

                    if (!string.IsNullOrEmpty(name))
                    {
                        if (name.Length > 100)
                        {
                            Console.WriteLine("Category name is required and must be less than 100 characters.");
                            return;
                        }

                        category.CategoryName = name;
                    }

                    Console.Write($"New description (Press enter to keep current): ");
                    var description = Console.ReadLine()?.Trim() ?? string.Empty;

                    if (!string.IsNullOrEmpty(description))
                    {
                        if (description.Length > 100)
                        {
                            Console.WriteLine("Description is required and must be less than 100 characters");
                            return;
                        }

                        category.CategoryDescription = description;
                    }

                    try
                    {
                        await db.SaveChangesAsync();
                        Console.WriteLine("Changes saved!");
                    }
                    catch (DbUpdateException exception)
                    {
                        Console.WriteLine("DB Error: " + exception.GetBaseException().Message);
                    }
                }

                static async Task DeleteCategoryAsync(int deleteId)
                {
                    using var db = new ShopContext();

                    var category = await db.Categories.FirstOrDefaultAsync(g => g.CategoryId == deleteId);
                    if (category == null)
                    {
                        Console.WriteLine("Category not found!");
                        return;
                    }

                    db.Categories.Remove(category);

                    try
                    {
                        await db.SaveChangesAsync();
                        Console.WriteLine("Category deleted!");
                    }
                    catch (DbUpdateException exception)
                    {
                        Console.WriteLine("DB Error: " + exception.GetBaseException().Message);
                    }
                }

                // Methods to view/add/edit/delete Customers
                static async Task ListCustomersAsync()
                {
                    using var db = new ShopContext();

                    var customers = await db.Customers
                        .AsNoTracking()
                        .OrderBy(c => c.CustomerId)
                        .ToListAsync();

                    Console.WriteLine(" ID | Name | Email | City ");
                    foreach (var customer in customers)
                    {
                        Console.WriteLine($" {customer.CustomerId} | {customer.CustomerName} | {customer.Email} | {customer.City} ");
                    }
                }

                static async Task AddCustomerAsync()
                {
                    using var db = new ShopContext();

                    Console.WriteLine("Choose a customer name: ");
                    var name = Console.ReadLine()?.Trim() ?? string.Empty;

                    if (string.IsNullOrEmpty(name) || name.Length > 100)
                    {
                        Console.WriteLine("Name is required and must be less than 100 characters.");
                        return;
                    }

                    Console.Write("Email: ");
                    var mail = Console.ReadLine()?.Trim() ?? string.Empty;

                    if (string.IsNullOrEmpty(mail) || mail.Length > 100)
                    {
                        Console.WriteLine("Email is required and must be less than 100 characters.");
                        return;
                    }

                    Console.Write("City: ");
                    var city = Console.ReadLine()?.Trim() ?? string.Empty;
                    
                    if (string.IsNullOrEmpty(mail) ||  city.Length > 100)
                    {
                        Console.WriteLine("City is required and must be less than 100 characters.");
                        return;
                    }

                    var emailExists = await db.Customers.AnyAsync(c => c.Email == mail);
                    if (emailExists)
                    {
                        Console.WriteLine("A customer with this email already exists. Please choose a diffrent email.");
                        return;
                    }

                    db.Customers.Add(new Customer
                    {
                        CustomerName = name,
                        Email = mail,
                        City = city
                    });

                    try
                    {
                        await db.SaveChangesAsync();
                        Console.WriteLine("Customer added!");
                    }
                    catch (DbUpdateException exception)
                    {
                        Console.WriteLine("DB Error: " + exception.GetBaseException().Message);
                    }

                }

                static async Task EditCustomerAsync(int editId)
                {
                    using var db = new ShopContext();

                    var customer = await db.Customers.FirstOrDefaultAsync(c => c.CustomerId == editId);
                    if (customer == null)
                    {
                        Console.WriteLine("Customer not found.");
                        return;
                    }

                    Console.WriteLine($"Editing customer: {customer.CustomerId}");
                    Console.WriteLine($"Current name: {customer.CustomerName}");
                    Console.WriteLine($"Current email: {customer.Email}");
                    Console.WriteLine($"Current city: {customer.City}");

                    Console.Write($"New customer name (Press enter to keep current): ");
                    var name = Console.ReadLine()?.Trim() ?? string.Empty;

                    if (!string.IsNullOrEmpty(name))
                    {
                        if (name.Length > 100)
                        {
                            Console.WriteLine("Customer name is required and must be less than 100 characters.");
                            return;
                        }

                        customer.CustomerName = name;
                    }

                    Console.Write($"New email (Press enter to keep current): ");
                    var mail = Console.ReadLine()?.Trim() ?? string.Empty;

                    if (!string.IsNullOrEmpty(mail))
                    {
                        if (mail.Length > 100)
                        {
                            Console.WriteLine("Email is required and must be less than 100 characters.");
                            return;
                        }

                        var emailTaken = await db.Customers
                            .AnyAsync(c => c.Email == mail && c.CustomerId != editId);
                        if (emailTaken)
                        {
                            Console.WriteLine("This email is already taken, try another.");
                            return;
                        }

                        customer.Email = mail;
                    }

                    Console.Write($"New city (Press enter to keep current): ");
                    var city = Console.ReadLine()?.Trim() ?? string.Empty;

                    if (!string.IsNullOrEmpty(city))
                    {
                        if (city.Length > 100)
                        {
                            Console.WriteLine("City is required and must be less than 100 characters.");
                            return;
                        }

                        customer.City = city;
                    }

                    try
                    {
                        await db.SaveChangesAsync();
                        Console.WriteLine("Changes saved");
                    }
                    catch (DbUpdateException exception)
                    {
                        Console.WriteLine("DB Error: " + exception.GetBaseException().Message);
                    }

                }

                static async Task DeleteCustomerAsync(int deleteId)
                {
                    using var db = new ShopContext();

                    var customer = await db.Customers.FirstOrDefaultAsync(c => c.CustomerId == deleteId);
                    if (customer == null)
                    {
                        Console.WriteLine("Customer not found!");
                    }

                    db.Customers.Remove(customer!);

                    try
                    {
                        await db.SaveChangesAsync();
                        Console.WriteLine("Customer deleted!");
                    }
                    catch (DbUpdateException exception)
                    {
                        Console.WriteLine("DB Error: " + exception.GetBaseException().Message);
                    }
                }


                // Methods to view/add/edit/delete Orders
                static async Task ListOrdersAsync()
                {
                    using var db = new ShopContext();

                    var orders = await db.Orders
                        .AsNoTracking()
                        .Include(o => o.Customer)
                        .OrderBy(o => o.OrderId)
                        .ToListAsync();

                    Console.WriteLine(" OrderID | Customer | OrderDate | Status ");
                    foreach (var order in orders)
                    {
                        Console.WriteLine($" {order.OrderId} | {order.Customer?.CustomerName} | {order.OrderDate:yyyy-MM-dd} | {order.Status} ");
                    }
                }

                static async Task AddOrderAsync()
                {
                    using var db = new ShopContext();

                    var customers = await db.Customers
                        .AsNoTracking()
                        .OrderBy(c => c.CustomerId)
                        .ToListAsync();

                    if (!customers.Any())
                    {
                        Console.WriteLine("No customers found.");
                        return;
                    }

                    Console.WriteLine(" ID | Name | Email ");
                    foreach (var customer in customers)
                    {
                        Console.WriteLine($" {customer.CustomerId} | {customer.CustomerName} | {customer.Email} ");
                    }

                    Console.Write("Please enter customer id: ");
                    if (!int.TryParse(Console.ReadLine(), out var customerId) || 
                        !customers.Any(c => c.CustomerId == customerId))
                    {
                        Console.WriteLine("Invalid input of customer id");
                        return;
                    }

                    var order = new Order
                    {
                        CustomerId = customerId,
                        OrderDate = DateTime.Now,
                        Status = "Pending"
                    };

                    var orderRows = new List<OrderRow>();

                    while (true)
                    {
                        var products = await db.Products
                        .AsNoTracking()
                        .OrderBy(p => p.ProductId)
                        .ToListAsync();

                        if (!products.Any())
                        {
                            Console.WriteLine("No products found.");
                            return;
                        }

                        Console.WriteLine(" ID | Name | Price ");
                        foreach (var product in products)
                        {
                            Console.WriteLine($" {product.ProductId} | {product.ProductName} | {product.Price} ");
                        }

                        Console.Write("Enter product id: ");
                        if (!int.TryParse(Console.ReadLine(), out var productId))
                        {
                            Console.WriteLine("Invalid input of product id");
                            continue;
                        }

                        var chosenProduct = await db.Products.FirstOrDefaultAsync(p => p.ProductId == productId);

                        if (chosenProduct == null)
                        {
                            Console.WriteLine("Product not found.");
                            continue;
                        }

                        Console.Write("Enter quantity: ");
                        if (!int.TryParse(Console.ReadLine(),out var quantity) || quantity <= 0)
                        {
                            Console.WriteLine("Invalid input");
                            continue;
                        }

                        var row = new OrderRow
                        {
                            ProductId = productId,
                            Quantity = quantity,
                            UnitPrice = chosenProduct.Price

                        };

                        orderRows.Add(row);

                        Console.Write("Do you want to add a new order row ? yes/no: ");
                        var answer = Console.ReadLine()?.Trim().ToLowerInvariant();
                        if (answer != "yes")
                            break;
                    }

                    if (!orderRows.Any())
                    {
                        Console.WriteLine("Order must contain at least one order row.");
                        return;
                    }

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

                static async Task EditOrderAsync(int editId)
                {
                    using var db = new ShopContext();

                    var order = await db.Orders
                        .Include(c => c.Customer)
                        .FirstOrDefaultAsync(c => c.OrderId == editId);

                    if (order == null)
                    {
                        Console.WriteLine("Order not found.");
                        return;
                    }

                    Console.WriteLine($"Editing order: {order.OrderId}");
                    Console.WriteLine($"Current customer: {order.Customer?.CustomerName}");
                    Console.WriteLine($"Current order date: {order.OrderDate}");
                    Console.WriteLine($"Current status: {order.Status}");

                    Console.Write("Enter new customer id (Press enter to keep current): ");
                    var input = Console.ReadLine()?.Trim() ?? string.Empty;
                    if (!string.IsNullOrEmpty(input))
                    {
                        if (!int.TryParse(input, out int id) || id <= 0)
                        {
                            Console.WriteLine("Invalid customer id.");
                            return;
                        }

                        var customerExists = await db.Customers.AnyAsync(c => c.CustomerId == id);
                        if (!customerExists)
                        {
                            Console.WriteLine("Customer does not exist");
                            return;
                        }

                        order.CustomerId = id;
                    }

                    Console.Write("New Order date - yyyy-MM-dd (Press enter to keep current): ");
                    var dateInput = Console.ReadLine()?.Trim() ?? string.Empty;
                    if (!string.IsNullOrEmpty(dateInput))
                    {
                        if (!DateTime.TryParse(dateInput, out var newDate))
                        {
                            Console.WriteLine("Invalid date format.");
                            return;
                        }

                        order.OrderDate = newDate;
                    }

                    Console.Write("New status (Pending/Shipped/Cancelled) (Press enter to keep current): ");
                    var statusInput = Console.ReadLine()?.Trim() ?? string.Empty;
                    if (!string.IsNullOrEmpty(statusInput))
                    {
                        if (statusInput.Length > 100)
                        {
                            Console.WriteLine("Status too long, keep it under 100 characters.");
                            return;
                        }

                        order.Status = statusInput;
                    }

                    try
                    {
                        await db.SaveChangesAsync();
                        Console.WriteLine("Changes saved!");
                    }
                    catch (DbUpdateException exception)
                    {
                        Console.WriteLine("DB Error: " + exception.GetBaseException().Message);
                    }
                }

                static async Task DeleteOrderAsync(int deleteId)
                {
                    using var db = new ShopContext();

                    var order = await db.Orders.FirstOrDefaultAsync(o => o.OrderId == deleteId);
                    if (order == null)
                    {
                        Console.WriteLine("Order not found");
                        return;
                    }

                    db.Orders.Remove(order);

                    try
                    {
                        await db.SaveChangesAsync();
                        Console.WriteLine("Order deleted!");
                    }
                    catch (DbUpdateException exception)
                    {
                        Console.WriteLine("DB Error: " + exception.GetBaseException().Message);
                    }
                }

                // Methods to view/add/edit/delete Products
                static async Task ListProductsAsync()
                {
                    using var db = new ShopContext();

                    var products = await db.Products
                        .AsNoTracking()
                        .OrderBy(p => p.ProductId)
                        .ToListAsync();

                    Console.WriteLine(" ID | Name | Price ");
                    foreach (var product in products)
                    {
                        Console.WriteLine($" {product.ProductId} | {product.ProductName} | {product.Price} ");
                    }
                }

                static async Task AddProductAsync()
                {
                    using var db = new ShopContext();

                    var categories = await db.Categories
                        .AsNoTracking()
                        .OrderBy(c => c.CategoryId)
                        .ToListAsync();

                    if (!categories.Any())
                    {
                        Console.WriteLine("No categories found. Add a category first.");
                        return;
                    }

                    Console.WriteLine("Available categories: ");
                    Console.WriteLine(" ID | Name");
                    foreach (var category in categories)
                    {
                        Console.WriteLine($" {category.CategoryId} | {category.CategoryName} ");
                    }

                    Console.Write("Please enter category id: ");
                    var categoryInput = Console.ReadLine()?.Trim() ?? string.Empty;

                    if (!int.TryParse(categoryInput, out var categoryId) || 
                        !categories.Any(c => c.CategoryId == categoryId))
                    {
                        Console.WriteLine("Invalid category id");
                        return;
                    }

                    Console.Write("Enter product name: ");
                    var name = Console.ReadLine()?.Trim() ?? string.Empty;

                    if (!string.IsNullOrEmpty(name))
                    {
                        if (name.Length > 100)
                        {
                            Console.WriteLine("Product name must be less than 100 characters.");
                            return;
                        }
                    }

                    Console.Write("Enter product price: ");
                    var priceInput = Console.ReadLine()?.Trim() ?? string.Empty;

                    if (!decimal.TryParse(priceInput, out var price) || price < 0)
                    {
                        Console.WriteLine("Invalid price");
                        return;
                    }

                    var product = new Product
                    {
                        ProductName = name,
                        Price = price,
                        CategoryId = categoryId
                    };

                    db.Products.Add(product);

                    try
                    {
                        await db.SaveChangesAsync();
                        Console.WriteLine("Product added!");
                    }
                    catch (DbUpdateException exception)
                    {
                        Console.WriteLine("DB Error: " + exception.GetBaseException().Message);
                    }
                }

                static async Task EditProductAsync(int editId)
                {
                    using var db = new ShopContext();

                    var product = await db.Products
                        .Include(p => p.Category)
                        .FirstOrDefaultAsync(p => p.ProductId == editId);

                    if (product == null)
                    {
                        Console.WriteLine("Product not found.");
                        return;
                    }

                    Console.WriteLine($"Editing product: {product.ProductId}");
                    Console.WriteLine($"Current name: {product.ProductName}");
                    Console.WriteLine($"Current price: {product.Price}");
                    Console.WriteLine($"Current category: {product.Category?.CategoryName}");

                    Console.Write("Enter new product name (Press enter to keep current): ");
                    var nameInput = Console.ReadLine()?.Trim() ?? string.Empty;

                    if (!string.IsNullOrEmpty(nameInput))
                    {
                        if (nameInput.Length > 100)
                        {
                            Console.WriteLine("Product name must be less than 100 characters.");
                            return;
                        }

                        product.ProductName = nameInput;
                    }

                    Console.Write($"New price (Press enter to keep current): ");
                    var priceInput = Console.ReadLine()?.Trim() ?? string.Empty;

                    if (!string.IsNullOrEmpty(priceInput))
                    {
                        if (!decimal.TryParse(priceInput, out var price) || price < 0)
                        {
                            Console.WriteLine("Invalid price.");
                            return;
                        }

                        product.Price = price;
                    }

                    Console.Write($"New category id (Press enter to keep current): ");
                    var idInput = Console.ReadLine()?.Trim() ?? string.Empty;

                    if (!string.IsNullOrEmpty(idInput))
                    {
                        if (!int.TryParse(idInput, out var id) || id <= 0)
                        {
                            Console.WriteLine("Invalid category id.");
                            return;
                        }

                        var categoryExists = await db.Categories.AnyAsync(c => c.CategoryId == id);
                        if (!categoryExists)
                        {
                            Console.WriteLine("Category does not exist.");
                            return;
                        }

                        product.CategoryId = id;
                    }

                    try
                    {
                        await db.SaveChangesAsync();
                        Console.WriteLine("Changes saved!");
                    }
                    catch (DbUpdateException exception)
                    {
                        Console.WriteLine("DB Error: " + exception.GetBaseException().Message);
                    }

                }

                static async Task DeleteProductAsync(int deleteId)
                {
                    using var db = new ShopContext();

                    var product = await db.Products.FirstOrDefaultAsync(p => p.ProductId == deleteId);
                    if (product == null)
                    {
                        Console.WriteLine("Product not found!");
                        return;
                    }

                    db.Products.Remove(product);

                    try
                    {
                        await db.SaveChangesAsync();
                        Console.WriteLine("Product deleted!");
                    }
                    catch (DbUpdateException exception)
                    {
                        Console.WriteLine("DB Error: " + exception.GetBaseException().Message);
                    }
                }
            }
        }
    }
}