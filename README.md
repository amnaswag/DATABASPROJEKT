DATABASPROJEKT: E-HANDEL & ADMIN
Översikt
Detta är ett konsolapplikationsprojekt utvecklat i C# .NET 8 för kursen Databasutveckling. 
Projektet simulerar ett bakomliggande system för en e-handelslösning och demonstrerar centrala databasprinciper.

Teknisk Stack
Språk: C#
Ramverk: .NET 8
Databas: SQLite
Entity Framework Core (EF Core)

Databasdesign (3NF)
Datamodellen är normaliserad till Tredje Normalformen (3NF) 
och består av följande fem entiteter (tabeller) 
för att säkerställa dataintegritet och minimal redundans:
-Categories
-Products
-Customers
-Orders
-OrderRows

Funktionalitet
Applikationen har ett konsolbaserat menygränssnitt och implementerar fullständig CRUD-funktionalitet (Create, Read, Update, Delete) 
för alla huvudentiteter (Categories, Products, Customers, Orders).
*CRUD-Hantering: Hanteras via dedikerade C# metoder som använder LINQ för dataåtkomst.
*Validering: Inkluderar validering av obligatoriska fält och längdbegränsningar på användarinmatning.
*Seeding: Initial data för kunder (Amna Swag, Ben Benson, Carl Carlson), kategorier och produkter laddas automatiskt vid första körningen.
