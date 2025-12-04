using System.IO;
using EHandelDb.Services;

Console.WriteLine("DB: " + Path.Combine(AppContext.BaseDirectory, "shop.db"));

var shopService = new ShopService();
await shopService.RunMainMenuAsync();