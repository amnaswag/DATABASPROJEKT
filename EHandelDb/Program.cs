using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.IO;
using System.Threading.Tasks;
using DATABASPROJEKT.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Text; 
using System.Linq; 
 

namespace DATABASPROJEKT
{
    internal class Program
    {
        // Encryption for XOR-operationen
        private const byte XOR_KEY_BYTE = 0xAA; 

        // ncryption with XOR och Base64 code result
        private static string EncryptEmail(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Converts the string to bytes
            var inputBytes = Encoding.UTF8.GetBytes(input);
            
            // XOR. operation for every single byte 
            var encryptedBytes = inputBytes.Select(b => (byte)(b ^ XOR_KEY_BYTE)).ToArray();

            // Return as Base64-string 
            return Convert.ToBase64String(encryptedBytes);
        }

        // Decription Base64-string with the help of XOR
        private static string DecryptEmail(string encryptedInput)
        {
            if (string.IsNullOrEmpty(encryptedInput)) return encryptedInput;

            try
            {
                // Decode Base64
                var base64Bytes = Convert.FromBase64String(encryptedInput);
                
                var decryptedBytes = base64Bytes.Select(b => (byte)(b ^ XOR_KEY_BYTE)).ToArray();

                // Converts back to readable string 
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch (FormatException)
            {
                return encryptedInput;
            }
        }
        
        static async Task Main(string[] args)
        {
            Console.WriteLine("DB Location: " + Path.Combine(AppContext.BaseDirectory, "shop.db"));

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
                    Console.WriteLine("Categories initialized.");
                }

                if (!await db.Customers.AnyAsync())
                {
                    db.Customers.AddRange(
                        new Customer { CustomerName = "Amna Swag", Email = EncryptEmail("Amna.Swag@hotmail.com"), City = "Stockholm" },
                        new Customer { CustomerName = "Ben Benson", Email = EncryptEmail("BenB@gmail.com"), City = "Malmö" },
                        new Customer { CustomerName = "Carl Carlson", Email = EncryptEmail("CarlC@outlook.com"), City = "Göteborg" }
                    );

                    await db.SaveChangesAsync();
                    Console.WriteLine("Customers initialized.");
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
                    Console.WriteLine("Orders initialized.");
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
                    Console.WriteLine("Products initialized.");
                }

                while (true)
                {
                    // Main Menu
                    Console.WriteLine("\n--- Amna's shop ---");
                    Console.WriteLine("Select Option: 1-Categories | 2-Customers | 3-Orders | 4-Products | 5-Quit");
                    Console.Write("Choice: ");
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
                            Console.WriteLine("Exiting application.");
                            return;
                        default:
                            Console.WriteLine("Invalid selection. Try again.");
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
                        Console.WriteLine("Category Menu: 1-List | 2-Add | 3-Edit | 4-Delete | 5-Return");
                        Console.Write("Choice: ");
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
                                Console.Write("Enter category ID to edit: ");
                                var editInput = Console.ReadLine()?.Trim() ?? string.Empty;

                                if (!int.TryParse(editInput, out int editId))
                                {
                                    Console.WriteLine("Error: Must enter a valid ID.");
                                    Console.ReadKey();
                                    break;
                                }
                                await EditCategoryAsync(editId);
                                break;
                            case "4":
                                Console.Write("Enter category ID to delete: ");
                                var deleteInput = Console.ReadLine()?.Trim() ?? string.Empty;

                                if (!int.TryParse(deleteInput, out int deleteId))
                                {
                                    Console.WriteLine("Error: Must enter a valid ID.");
                                    Console.ReadKey();
                                    break;
                                }
                                await DeleteCategoryAsync(deleteId);
                                break;
                            case "5":
                                Console.WriteLine("Returning to main menu.");
                                return;
                            default:
                                Console.WriteLine("Invalid selection. Try again.");
                                break;
                        }
                    }
                }
                /// <summary>
                /// Displays menu and handles CRUD operations for customers.
                /// </summary>

                static async Task CustomerMenuAsync()
                {
                    using var db = new ShopContext();

                    while (true)
                    {
                        Console.WriteLine("Customer Menu: 1-List | 2-Add | 3-Edit | 4-Delete | 5-Return");
                        Console.Write("Choice: ");
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
                                Console.Write("Enter customer ID to edit: ");
                                var editInput = Console.ReadLine()?.Trim() ?? string.Empty;

                                if (!int.TryParse(editInput, out int editId))
                                {
                                    Console.WriteLine("Error: Must enter a valid ID.");
                                    Console.ReadKey();
                                    break;
                                }
                                await EditCustomerAsync(editId);
                                break;
                            case "4":
                                Console.Write("Enter customer ID to delete: ");
                                var deleteInput = Console.ReadLine()?.Trim()?.Trim() ?? string.Empty;

                                if (!int.TryParse(deleteInput, out int deleteId))
                                {
                                    Console.WriteLine("Error: Must enter a valid ID.");
                                    Console.ReadKey();
                                    break;
                                }
                                await DeleteCustomerAsync(deleteId);
                                break;
                            case "5":
                                Console.WriteLine("Returning to main menu.");
                                return;
                            default:
                                Console.WriteLine("Invalid selection. Try again.");
                                break;

                        }
                    }
                }
                
                /// <summary>
                /// Displays menu and handles CRUD operations for orders.
                /// </summary>
                static async Task OrderMenuAsync()
                {
                    using var db = new ShopContext();

                    while (true)
                    {
                        Console.WriteLine("Order Menu: 1-List | 2-Add | 3-Edit | 4-Delete | 5-Return");
                        Console.Write("Choice: ");
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
                                Console.Write("Enter order ID to edit: ");
                                var editInput = Console.ReadLine()?.Trim() ?? string.Empty;

                                if (!int.TryParse(editInput, out int editId))
                                {
                                    Console.WriteLine("Error: Must enter a valid ID.");
                                    Console.ReadKey();
                                    break;
                                }
                                await EditOrderAsync(editId);
                                break;
                            case "4":
                                Console.Write("Enter order ID to delete: ");
                                var deleteInput = Console.ReadLine()?.Trim()?.Trim() ?? string.Empty;

                                if (!int.TryParse(deleteInput, out int deleteId))
                                {
                                    Console.WriteLine("Error: Must enter a valid ID.");
                                    Console.ReadKey();
                                    break;
                                }
                                await DeleteOrderAsync(deleteId);
                                break;
                            case "5":
                                Console.WriteLine("Returning to main menu.");
                                return;
                            default:
                                Console.WriteLine("Invalid selection. Try again.");
                                break;

                        }
                    }
                }

                /// <summary>
                /// Displays menu and handles CRUD operations for products.
                /// </summary>
                static async Task ProductMenuAsync()
                {
                    using var db = new ShopContext();

                    while (true)
                    {
                        Console.WriteLine("Product Menu: 1-List | 2-Add | 3-Edit | 4-Delete | 5-Return");
                        Console.Write("Choice: ");
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
                                Console.Write("Enter product ID to edit: ");
                                var editInput = Console.ReadLine()?.Trim() ?? string.Empty;

                                if (!int.TryParse(editInput, out int editId))
                                {
                                    Console.WriteLine("Error: Must enter a valid ID.");
                                    Console.ReadKey();
                                    break;
                                }
                                await EditProductAsync(editId);
                                break;
                            case "4":
                                Console.Write("Enter product ID to delete: ");
                                var deleteInput = Console.ReadLine()?.Trim()?.Trim() ?? string.Empty;

                                if (!int.TryParse(deleteInput, out int deleteId))
                                {
                                    Console.WriteLine("Error: Must enter a valid ID.");
                                    Console.ReadKey();
                                    break;
                                }
                                await DeleteProductAsync(deleteId);
                                break;
                            case "5":
                                Console.WriteLine("Returning to main menu.");
                                return;
                            default:
                                Console.WriteLine("Invalid selection. Try again.");
                                break;
                        }
                    }
                } 
                /// <summary>
                /// Displays a list of all categories in the database.
                /// </summary>
                static async Task ListCategoriesAsync()
                {
                    using var db = new ShopContext();

                    var categories = await db.Categories
                        .AsNoTracking()
                        .OrderBy(g => g.CategoryId)
                        .ToListAsync();

                    Console.WriteLine("--- Categories ---");
                    Console.WriteLine(" ID | Name | Description ");
                    foreach (var category in categories)
                    {
                        Console.WriteLine($" {category.CategoryId} | {category.CategoryName} | {category.CategoryDescription} ");
                    }
                }
                
                /// <summary>
                /// Adds a new category based on user input.
                /// </summary>
                static async Task AddCategoryAsync()
                {
                    using var db = new ShopContext();

                    Console.Write("Enter category name: ");
                    var name = Console.ReadLine()?.Trim() ?? string.Empty;

                    if (string.IsNullOrEmpty(name) || name.Length > 100)
                    {
                        Console.WriteLine("Error: Name required (Max 100 char).");
                        return;
                    }

                    Console.Write("Enter category description: ");
                    var description = Console.ReadLine()?.Trim() ?? string.Empty;

                    if (string.IsNullOrEmpty(description) || description.Length > 100)
                    {
                        Console.WriteLine("Error: Description required (Max 100 char).");
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
                        Console.WriteLine("Success: Category saved.");
                    }
                    catch (DbUpdateException exception)
                    {
                        Console.WriteLine("DB Error: " + exception.GetBaseException().Message);
                    }
                }

                /// <summary>
                /// Edits an existing category.
                /// </summary>
                static async Task EditCategoryAsync(int editId)
                {
                    using var db = new ShopContext();

                    var category = await db.Categories.FirstOrDefaultAsync(g => g.CategoryId == editId);
                    if (category == null)
                    {
                        Console.WriteLine("Error: Category not found.");
                        return;
                    }

                    Console.WriteLine($"--- Editing Category ID: {category.CategoryId} ---");
                    Console.WriteLine($"Current name: {category.CategoryName}");
                    Console.WriteLine($"Current description: {category.CategoryDescription}");

                    Console.Write($"New name (Enter to keep current): ");
                    var name = Console.ReadLine()?.Trim() ?? string.Empty;

                    if (!string.IsNullOrEmpty(name))
                    {
                        if (name.Length > 100)
                        {
                            Console.WriteLine("Error: Name required (Max 100 char).");
                            return;
                        }

                        category.CategoryName = name;
                    }

                    Console.Write($"New description (Enter to keep current): ");
                    var description = Console.ReadLine()?.Trim() ?? string.Empty;

                    if (!string.IsNullOrEmpty(description))
                    {
                        if (description.Length > 100)
                        {
                            Console.WriteLine("Error: Description required (Max 100 char).");
                            return;
                        }

                        category.CategoryDescription = description;
                    }

                    try
                    {
                        await db.SaveChangesAsync();
                        Console.WriteLine("Success: Changes saved.");
                    }
                    catch (DbUpdateException exception)
                    {
                        Console.WriteLine("DB Error: " + exception.GetBaseException().Message);
                    }
                }

                /// <summary>
                /// Deletes a category based on its ID.
                /// </summary>
                static async Task DeleteCategoryAsync(int deleteId)
                {
                    using var db = new ShopContext();

                    var category = await db.Categories.FirstOrDefaultAsync(g => g.CategoryId == deleteId);
                    if (category == null)
                    {
                        Console.WriteLine("Error: Category not found.");
                        return;
                    }

                    db.Categories.Remove(category);

                    try
                    {
                        await db.SaveChangesAsync();
                        Console.WriteLine("Success: Category deleted.");
                    }
                    catch (DbUpdateException exception)
                    {
                        Console.WriteLine("DB Error: " + exception.GetBaseException().Message);
                    }
                }

                ///// <summary>
                /// Displays a list of all customers in the database.
                /// </summary>
                static async Task ListCustomersAsync()
                {
                    using var db = new ShopContext();

                    var customers = await db.Customers
                        .AsNoTracking()
                        .OrderBy(c => c.CustomerId)
                        .ToListAsync();

                    Console.WriteLine("--- Customers ---");
                    Console.WriteLine(" ID | Name | Email | City ");
                    foreach (var customer in customers)
                    {
                        var decryptedEmail = DecryptEmail(customer.Email);
                        Console.WriteLine($" {customer.CustomerId} | {customer.CustomerName} | {decryptedEmail} | {customer.City} ");
                    }
                }
                
                /// <summary>
                /// Adds a new customer based on user input.
                /// </summary>
                static async Task AddCustomerAsync()
                {
                    using var db = new ShopContext();

                    Console.Write("Enter customer name: ");
                    var name = Console.ReadLine()?.Trim() ?? string.Empty;

                    if (string.IsNullOrEmpty(name) || name.Length > 100)
                    {
                        Console.WriteLine("Error: Name required (Max 100 char).");
                        return;
                    }

                    Console.Write("Enter email: ");
                    var mail = Console.ReadLine()?.Trim() ?? string.Empty;

                    if (string.IsNullOrEmpty(mail) || mail.Length > 100)
                    {
                        Console.WriteLine("Error: Email required (Max 100 char).");
                        return;
                    }

                    Console.Write("Enter city: ");
                    var city = Console.ReadLine()?.Trim() ?? string.Empty;
                    
                    if (string.IsNullOrEmpty(mail) ||  city.Length > 100)
                    {
                        Console.WriteLine("Error: City required (Max 100 char).");
                        return;
                    }
                    
                    var encryptedMail = EncryptEmail(mail);
                    var emailExists = await db.Customers.AnyAsync(c => c.Email == encryptedMail);
                    if (emailExists)
                    {
                        Console.WriteLine("Error: Email already exists. Use a different email.");
                        return;
                    }
                    
                    db.Customers.Add(new Customer
                    {
                        CustomerName = name,
                        Email = encryptedMail, 
                        City = city
                    });

                    try
                    {
                        await db.SaveChangesAsync();
                        Console.WriteLine("Success: Customer saved.");
                    }
                    catch (DbUpdateException exception)
                    {
                        Console.WriteLine("DB Error: " + exception.GetBaseException().Message);
                    }

                }
                
                /// <summary>
                /// Edits an existing customer.
                /// </summary>
                static async Task EditCustomerAsync(int editId)
                {
                    using var db = new ShopContext();

                    var customer = await db.Customers.FirstOrDefaultAsync(c => c.CustomerId == editId);
                    if (customer == null)
                    {
                        Console.WriteLine("Error: Customer not found.");
                        return;
                    }
                    
                    var currentEmailDecrypted = DecryptEmail(customer.Email);

                    Console.WriteLine($"--- Editing Customer ID: {customer.CustomerId} ---");
                    Console.WriteLine($"Current name: {customer.CustomerName}");
                    Console.WriteLine($"Current email: {currentEmailDecrypted}"); 
                    Console.WriteLine($"Current city: {customer.City}");


                    Console.Write($"New name (Enter to keep current): ");
                    var name = Console.ReadLine()?.Trim() ?? string.Empty;

                    if (!string.IsNullOrEmpty(name))
                    {
                        if (name.Length > 100)
                        {
                            Console.WriteLine("Error: Name required (Max 100 char).");
                            return;
                        }

                        customer.CustomerName = name;
                    }

                    Console.Write($"New email (Enter to keep current): ");
                    var mail = Console.ReadLine()?.Trim() ?? string.Empty;

                    if (!string.IsNullOrEmpty(mail))
                    {
                        if (mail.Length > 100)
                        {
                            Console.WriteLine("Error: Email required (Max 100 char).");
                            return;
                        }
                        
                        var newEncryptedMail = EncryptEmail(mail);
                        
                        var emailTaken = await db.Customers
                            .AnyAsync(c => c.Email == newEncryptedMail && c.CustomerId != editId);
                        if (emailTaken)
                        {
                            Console.WriteLine("Error: Email already taken. Try another.");
                            return;
                        }

                        customer.Email = newEncryptedMail; 
                    }

                    Console.Write($"New city (Enter to keep current): ");
                    var city = Console.ReadLine()?.Trim() ?? string.Empty;

                    if (!string.IsNullOrEmpty(city))
                    {
                        if (city.Length > 100)
                        {
                            Console.WriteLine("Error: City required (Max 100 char).");
                            return;
                        }

                        customer.City = city;
                    }

                    try
                    {
                        await db.SaveChangesAsync();
                        Console.WriteLine("Success: Changes saved.");
                    }
                    catch (DbUpdateException exception)
                    {
                        Console.WriteLine("DB Error: " + exception.GetBaseException().Message);
                    }

                }
                
                /// <summary>
                /// Deletes a customer based on its ID.
                /// </summary>
                static async Task DeleteCustomerAsync(int deleteId)
                {
                    using var db = new ShopContext();

                    var customer = await db.Customers.FirstOrDefaultAsync(c => c.CustomerId == deleteId);
                    if (customer == null)
                    {
                        Console.WriteLine("Error: Customer not found.");
                    }

                    db.Customers.Remove(customer!);

                    try
                    {
                        await db.SaveChangesAsync();
                        Console.WriteLine("Success: Customer deleted.");
                    }
                    catch (DbUpdateException exception)
                    {
                        Console.WriteLine("DB Error: " + exception.GetBaseException().Message);
                    }
                }
                
                /// <summary>
                /// Displays a list of all orders, including the customer they belong to.
                /// </summary>
                static async Task ListOrdersAsync()
                {
                    using var db = new ShopContext();

                    var orders = await db.Orders
                        .AsNoTracking()
                        .Include(o => o.Customer)
                        .OrderBy(o => o.OrderId)
                        .ToListAsync();

                    Console.WriteLine("--- Orders ---");
                    Console.WriteLine(" OrderID | Customer | OrderDate | Status ");
                    foreach (var order in orders)
                    {
                        Console.WriteLine($" {order.OrderId} | {order.Customer?.CustomerName} | {order.OrderDate:yyyy-MM-dd} | {order.Status} ");
                    }
                }
                
                /// <summary>
                /// Adds a new order with associated order rows.
                /// </summary>
                static async Task AddOrderAsync()
                {
                    using var db = new ShopContext();

                    var customers = await db.Customers
                        .AsNoTracking()
                        .OrderBy(c => c.CustomerId)
                        .ToListAsync();

                    if (!customers.Any())
                    {
                        Console.WriteLine("Error: No customers found.");
                        return;
                    }

                    Console.WriteLine("--- Available Customers ---");
                    Console.WriteLine(" ID | Name | Email ");
                    foreach (var customer in customers)
                    {
                        var decryptedEmail = DecryptEmail(customer.Email);
                        Console.WriteLine($" {customer.CustomerId} | {customer.CustomerName} | {decryptedEmail} ");
                    }

                    Console.Write("Enter customer ID: ");
                    if (!int.TryParse(Console.ReadLine(), out var customerId) || 
                        !customers.Any(c => c.CustomerId == customerId))
                    {
                        Console.WriteLine("Error: Invalid customer ID.");
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
                            Console.WriteLine("Error: No products found.");
                            return;
                        }

                        Console.WriteLine("--- Available Products ---");
                        Console.WriteLine(" ID | Name | Price ");
                        foreach (var product in products)
                        {
                            Console.WriteLine($" {product.ProductId} | {product.ProductName} | {product.Price} ");
                        }

                        Console.Write("Enter product ID: ");
                        if (!int.TryParse(Console.ReadLine(), out var productId))
                        {
                            Console.WriteLine("Error: Invalid product ID.");
                            continue;
                        }

                        var chosenProduct = await db.Products.FirstOrDefaultAsync(p => p.ProductId == productId);

                        if (chosenProduct == null)
                        {
                            Console.WriteLine("Error: Product not found.");
                            continue;
                        }

                        Console.Write("Enter quantity: ");
                        if (!int.TryParse(Console.ReadLine(),out var quantity) || quantity <= 0)
                        {
                            Console.WriteLine("Error: Invalid quantity.");
                            continue;
                        }

                        var row = new OrderRow
                        {
                            ProductId = productId,
                            Quantity = quantity,
                            UnitPrice = chosenProduct.Price

                        };

                        orderRows.Add(row);

                        Console.Write("Add another order row? (yes/no): ");
                        var answer = Console.ReadLine()?.Trim().ToLowerInvariant();
                        if (answer != "yes")
                            break;
                    }

                    if (!orderRows.Any())
                    {
                        Console.WriteLine("Error: Order must contain items.");
                        return;
                    }

                    order.OrderRows = orderRows;

                    db.Orders.Add(order);

                    try
                    {
                        await db.SaveChangesAsync();
                        Console.WriteLine($"Success: Order {order.OrderId} created.");
                    }
                    catch (DbUpdateException exception)
                    {
                        Console.WriteLine("DB Error: " + exception.GetBaseException().Message);
                    }
                }
                
                /// <summary>
                /// Edits an existing order. Allows changing customer, date, and status.
                /// </summary>
                static async Task EditOrderAsync(int editId)
                {
                    using var db = new ShopContext();

                    var order = await db.Orders
                        .Include(c => c.Customer)
                        .FirstOrDefaultAsync(c => c.OrderId == editId);

                    if (order == null)
                    {
                        Console.WriteLine("Error: Order not found.");
                        return;
                    }

                    Console.WriteLine($"--- Editing Order ID: {order.OrderId} ---");
                    Console.WriteLine($"Current customer: {order.Customer?.CustomerName}");
                    Console.WriteLine($"Current date: {order.OrderDate}");
                    Console.WriteLine($"Current status: {order.Status}");

                    Console.Write("Enter new customer ID (Enter to keep current): ");
                    var input = Console.ReadLine()?.Trim() ?? string.Empty;
                    if (!string.IsNullOrEmpty(input))
                    {
                        if (!int.TryParse(input, out int id) || id <= 0)
                        {
                            Console.WriteLine("Error: Invalid customer ID.");
                            return;
                        }

                        var customerExists = await db.Customers.AnyAsync(c => c.CustomerId == id);
                        if (!customerExists)
                        {
                            Console.WriteLine("Error: Customer does not exist.");
                            return;
                        }

                        order.CustomerId = id;
                    }

                    Console.Write("New date (YYYY-MM-DD) (Enter to keep current): ");
                    var dateInput = Console.ReadLine()?.Trim() ?? string.Empty;
                    if (!string.IsNullOrEmpty(dateInput))
                    {
                        if (!DateTime.TryParse(dateInput, out var newDate))
                        {
                            Console.WriteLine("Error: Invalid date format.");
                            return;
                        }

                        order.OrderDate = newDate;
                    }

                    Console.Write("New status (Pending/Shipped/Cancelled) (Enter to keep current): ");
                    var statusInput = Console.ReadLine()?.Trim() ?? string.Empty;
                    if (!string.IsNullOrEmpty(statusInput))
                    {
                        if (statusInput.Length > 100)
                        {
                            Console.WriteLine("Error: Status too long (Max 100 char).");
                            return;
                        }

                        order.Status = statusInput;
                    }

                    try
                    {
                        await db.SaveChangesAsync();
                        Console.WriteLine("Success: Changes saved.");
                    }
                    catch (DbUpdateException exception)
                    {
                        Console.WriteLine("DB Error: " + exception.GetBaseException().Message);
                    }
                }
                
                /// <summary>
                /// Deletes an order based on its ID.
                /// </summary>
                static async Task DeleteOrderAsync(int deleteId)
                {
                    using var db = new ShopContext();

                    var order = await db.Orders.FirstOrDefaultAsync(o => o.OrderId == deleteId);
                    if (order == null)
                    {
                        Console.WriteLine("Error: Order not found.");
                        return;
                    }

                    db.Orders.Remove(order);

                    try
                    {
                        await db.SaveChangesAsync();
                        Console.WriteLine("Success: Order deleted.");
                    }
                    catch (DbUpdateException exception)
                    {
                        Console.WriteLine("DB Error: " + exception.GetBaseException().Message);
                    }
                }
                
                /// <summary>
                /// Displays a list of all products in the database.
                /// </summary>
                static async Task ListProductsAsync()
                {
                    using var db = new ShopContext();

                    var products = await db.Products
                        .AsNoTracking()
                        .OrderBy(p => p.ProductId)
                        .ToListAsync();

                    Console.WriteLine("--- Products ---");
                    Console.WriteLine(" ID | Name | Price ");
                    foreach (var product in products)
                    {
                        Console.WriteLine($" {product.ProductId} | {product.ProductName} | {product.Price} ");
                    }
                }
                
                /// <summary>
                /// Adds a new product based on user input and links it to an existing category.
                /// </summary>
                static async Task AddProductAsync()
                {
                    using var db = new ShopContext();

                    var categories = await db.Categories
                        .AsNoTracking()
                        .OrderBy(c => c.CategoryId)
                        .ToListAsync();

                    if (!categories.Any())
                    {
                        Console.WriteLine("Error: No categories found. Add a category first.");
                        return;
                    }

                    Console.WriteLine("--- Available Categories ---");
                    Console.WriteLine(" ID | Name");
                    foreach (var category in categories)
                    {
                        Console.WriteLine($" {category.CategoryId} | {category.CategoryName} ");
                    }

                    Console.Write("Enter category ID: ");
                    var categoryInput = Console.ReadLine()?.Trim() ?? string.Empty;

                    if (!int.TryParse(categoryInput, out var categoryId) || 
                        !categories.Any(c => c.CategoryId == categoryId))
                    {
                        Console.WriteLine("Error: Invalid category ID.");
                        return;
                    }

                    Console.Write("Enter product name: ");
                    var name = Console.ReadLine()?.Trim() ?? string.Empty;

                    if (!string.IsNullOrEmpty(name))
                    {
                        if (name.Length > 100)
                        {
                            Console.WriteLine("Error: Product name required (Max 100 char).");
                            return;
                        }
                    }

                    Console.Write("Enter product price: ");
                    var priceInput = Console.ReadLine()?.Trim() ?? string.Empty;

                    if (!decimal.TryParse(priceInput, out var price) || price < 0)
                    {
                        Console.WriteLine("Error: Invalid price.");
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
                        Console.WriteLine("Success: Product saved.");
                    }
                    catch (DbUpdateException exception)
                    {
                        Console.WriteLine("DB Error: " + exception.GetBaseException().Message);
                    }
                }
                
                /// <summary>
                /// Edits an existing product. Allows changing name, price, and category.
                /// </summary>
                static async Task EditProductAsync(int editId)
                {
                    using var db = new ShopContext();

                    var product = await db.Products
                        .Include(p => p.Category)
                        .FirstOrDefaultAsync(p => p.ProductId == editId);

                    if (product == null)
                    {
                        Console.WriteLine("Error: Product not found.");
                        return;
                    }

                    Console.WriteLine($"--- Editing Product ID: {product.ProductId} ---");
                    Console.WriteLine($"Current name: {product.ProductName}");
                    Console.WriteLine($"Current price: {product.Price}");
                    Console.WriteLine($"Current category: {product.Category?.CategoryName}");

                    Console.Write("New product name (Enter to keep current): ");
                    var nameInput = Console.ReadLine()?.Trim() ?? string.Empty;

                    if (!string.IsNullOrEmpty(nameInput))
                    {
                        if (nameInput.Length > 100)
                        {
                            Console.WriteLine("Error: Product name required (Max 100 char).");
                            return;
                        }

                        product.ProductName = nameInput;
                    }

                    Console.Write($"New price (Enter to keep current): ");
                    var priceInput = Console.ReadLine()?.Trim() ?? string.Empty;

                    if (!string.IsNullOrEmpty(priceInput))
                    {
                        if (!decimal.TryParse(priceInput, out var price) || price < 0)
                        {
                            Console.WriteLine("Error: Invalid price.");
                            return;
                        }

                        product.Price = price;
                    }

                    Console.Write($"New category ID (Enter to keep current): ");
                    var idInput = Console.ReadLine()?.Trim() ?? string.Empty;

                    if (!string.IsNullOrEmpty(idInput))
                    {
                        if (!int.TryParse(idInput, out var id) || id <= 0)
                        {
                            Console.WriteLine("Error: Invalid category ID.");
                            return;
                        }

                        var categoryExists = await db.Categories.AnyAsync(c => c.CategoryId == id);
                        if (!categoryExists)
                        {
                            Console.WriteLine("Error: Category does not exist.");
                            return;
                        }

                        product.CategoryId = id;
                    }

                    try
                    {
                        await db.SaveChangesAsync();
                        Console.WriteLine("Success: Changes saved.");
                    }
                    catch (DbUpdateException exception)
                    {
                        Console.WriteLine("DB Error: " + exception.GetBaseException().Message);
                    }
                }
                
                /// <summary>
                /// Deletes a product based on its ID.
                /// </summary>
                static async Task DeleteProductAsync(int deleteId)
                {
                    using var db = new ShopContext();

                    var product = await db.Products.FirstOrDefaultAsync(p => p.ProductId == deleteId);
                    if (product == null)
                    {
                        Console.WriteLine("Error: Product not found.");
                        return;
                    }

                    db.Products.Remove(product);

                    try
                    {
                        await db.SaveChangesAsync();
                        Console.WriteLine("Success: Product deleted.");
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