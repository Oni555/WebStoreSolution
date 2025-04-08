## **Assignment: Enhance the E-Store to Track Shipments**

### **Overview**

You have an existing **e-commerce** (e-store) project—possibly created via **scaffolding** from an existing database (Database First). The system **lacks** detailed shipping and carrier information for each order. Your task is to **extend** the data model and **code** so that you can:

1. Record **which carrier** (e.g., “DHL”, “UPS”) is used for each order.
2. Store a **tracking number**, **shipped date**, and **delivered date** on each order.
3. **Initialize** EF Core migrations (if you previously only had scaffolding) to manage schema updates going forward.
4. **Manually insert** data into the database to prove that your new fields work, rather than relying on the application code to seed or update.

---

## **Part A: EF Core Migration Initialization**

**(Skip if you already have a Migrations folder and `__EFMigrationsHistory` table.)**

1. **Check Entities vs. DB**

   - Ensure your existing scaffolding code matches the current DB schema (before adding carriers).

2. **Create an Empty Baseline Migration**

   - In the terminal:
     ```bash
     dotnet ef migrations add InitialBaseline
     ```
   - Open the generated migration file inside the Migrations folder and **remove** or **empty** the `Up()` and `Down()` methods:

     ```csharp
     protected override void Up(MigrationBuilder migrationBuilder)
     {
         // empty
     }

     protected override void Down(MigrationBuilder migrationBuilder)
     {
         // empty
     }
     ```

3. **Apply This Baseline Migration**
   - Run:
     ```bash
     dotnet ef database update
     ```
   - This ensures EF recognizes your existing tables without re-creating them.

---

## **Part B: Update the Domain Model**

1. **Clone / Open the Existing Project**

   - If the project was created via **scaffolding** (e.g., `dotnet ef dbcontext scaffold ...`, Done in learning assignment 3), confirm it runs and the entity classes match the DB.

2. **Create a New `Carrier` Entity**

   - In your **Entities** folder, add `Carrier.cs`:

     ```csharp
     public class Carrier
     {
         public int CarrierId { get; set; }
         public string CarrierName { get; set; } = null!;
         public string? ContactUrl { get; set; }
         public string? ContactPhone { get; set; }

         // Navigation back to orders
         public ICollection<Order>? Orders { get; set; }
     }
     ```

3. **Add Shipping Fields to `Order`**

   - In `Order.cs`, **add** or **replace**:

     ```csharp
     public int? CarrierId { get; set; }
     public string? TrackingNumber { get; set; }
     public DateTime? ShippedDate { get; set; }
     public DateTime? DeliveredDate { get; set; }

     /// <summary>
     /// Navigation to the carrier (e.g. "UPS", "FedEx")
     /// </summary>
     public Carrier? Carrier { get; set; }
     ```

   - Keep them **nullable** so an order can exist without having shipped yet.

4. **Register `Carrier` in the `DbContext`**

   - Open your `DbContext` file (e.g. `WebStoreContext.cs`) in the **`OnModelCreating(ModelBuilder modelBuilder)`** method. You likely already have some configurations for `Orders`, `Customers`, etc. **Add** or **update** the mappings for the new `Carrier` and the updated `Order` fields:

   - Inside `WebStoreContext` (or equivalent):
     ```csharp
     public DbSet<Carrier> Carriers => Set<Carrier>();
     ```
   - Configure the one-to-many relationship:

     ```csharp
     modelBuilder.Entity<Carrier>(entity =>
     {
        entity.HasKey(e => e.CarrierId).HasName("carriers_pkey");

        entity.ToTable("carriers");
        entity.Property(e => e.CarrierName)
            .HasMaxLength(50)
            .HasColumnName("carrier_name");

        entity.Property(e => e.ContactUrl)
            .HasMaxLength(50)
            .HasColumnName("contact_url");

        entity.Property(e => e.ContactPhone)
            .HasMaxLength(50)
            .HasColumnName("contact_phone");

        entity.HasMany(c => c.Orders)
            .WithOne(o => o.Carrier)
            .HasForeignKey(o => o.CarrierId)
            .OnDelete(DeleteBehavior.SetNull); // If carrier is deleted -> order reference is set to null
     });

     modelBuilder.Entity<Order>(entity =>
     {
        // Key, columns, etc.
        // entity.HasKey(o => o.OrderId);

        // For the order tracking fields:
        entity.Property(o => o.TrackingNumber)
              .HasColumnName("tracking_number")
              .HasMaxLength(50);

        entity.Property(o => o.ShippedDate)
              .HasColumnName("shipped_date");

        entity.Property(o => o.DeliveredDate)
              .HasColumnName("delivered_date");
     });
     ```

   - This means if you remove a carrier, any orders referencing it become `CarrierId = NULL`.

---

## **Part C: Create & Apply the New Migration**

1. **Add the New Migration** for the Carrier and Order changes:
   ```bash
   dotnet ef migrations add AddOrderTracking
   ```
2. **Apply**:
   ```bash
   dotnet ef database update
   ```
   - This creates a `carriers` table and updates the `orders` table to have `carrier_id`, `tracking_number`, `shipped_date`, and `delivered_date`.

---

## **Part D: Manually Insert & Update Order Tracking Data (SQL)**

Now that your DB schema is updated, you must **manually** insert records for carriers and orders—**not** via the C# context code, but rather by using **SQL statements** in your favorite DB tool (e.g., pgAdmin, Azure Data Studio, DBeaver, etc.) or command-line (e.g., `psql` for PostgreSQL).

### 1. Insert Carriers

**Task**: Create at least **two** carriers. For example, “DHL” and “UPS.”  
Use an **INSERT** statement like:

```sql
INSERT INTO carriers (
    carrier_name,
    contact_url,
    contact_phone
)
VALUES
    ('DHL', 'https://www.dhl.com', '+49 228 767 676'),
    ('UPS', 'https://www.ups.com', '+1 800 742 5877');
```

- **Verify** they appear in the `carriers` table:
  ```sql
  SELECT * FROM carriers;
  ```

### 2. Assign a Carrier & Mark Order as Shipped

**Task**: Suppose you want to **update** the order to use “DHL” as the carrier. First, find the `carrier_id` from the `carriers` table (say it’s `1`) and the `order_id` (say it’s `10` for your new order). Then run:

    ```sql
    UPDATE orders
    SET carrier_id      = 1,        -- DHL
        tracking_number = 'DH123456789',
        shipped_date    = NOW(),
        order_status    = 'Shipped'
    WHERE order_id = 1;
    ```

- Check that the correct order was updated by running **SELECT** query into the **orders** table

  ```sql
   SELECT o.order_id,
          o.order_status,
          c.carrier_name,
          o.tracking_number,
          o.shipped_date,
          o.delivered_date
   FROM orders o
   LEFT JOIN carriers c ON o.carrier_id = c.carrier_id
   WHERE o.order_id = 1;
  ```

- Confirm you see “Shipped” with the correct carrier info.

### 4. Mark Order as Delivered

**Task**: Once delivered, you set the `delivered_date`:

    ```sql
    UPDATE orders
    SET delivered_date = NOW(),
        order_status   = 'Delivered'
    WHERE order_id = 1;
    ```

- Verify your data is inserted and updated properly:
  ```sql
  SELECT o.order_id,
         o.order_status,
         c.carrier_name,
         o.tracking_number,
         o.shipped_date,
         o.delivered_date
  FROM orders o
  LEFT JOIN carriers c ON o.carrier_id = c.carrier_id
  WHERE o.order_id = 1;
  ```
- Confirm you see “Delivered” with the correct carrier info.

---

## **Submission Requirements**

1. **Updated Entity Classes**:
   - `Carrier.cs` file and updated `Order.cs`.
2. **Migrations**:
   - Include your **baseline** migration (empty) if needed, plus the new **AddCarrierTable** migration in source control.
3. **Manual SQL Scripts**:
   - Provide new **INSERT** and **UPDATE** statements as `.sql` files.

---
