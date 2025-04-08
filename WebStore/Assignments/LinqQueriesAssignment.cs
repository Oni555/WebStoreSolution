using Microsoft.EntityFrameworkCore;
using WebStore.Entities;

namespace WebStore.Assignments
{
    /// Additional tutorial materials https://dotnettutorials.net/lesson/linq-to-entities-in-entity-framework-core/

    /// <summary>
    /// This class demonstrates various LINQ query tasks 
    /// to practice querying an EF Core database.
    /// 
    /// ASSIGNMENT INSTRUCTIONS:
    ///   1. For each method labeled "TODO", write the necessary
    ///      LINQ query to return or display the required data.
    ///      
    ///   2. Print meaningful output to the console (or return
    ///      collections, as needed).
    ///      
    ///   3. Test each method by calling it from your Program.cs
    ///      or test harness.
    /// </summary>
    public class LinqQueriesAssignment
    {

        private readonly WebStoreContext _dbContext;

        public LinqQueriesAssignment(WebStoreContext context)
        {
            _dbContext = context;
        }


        /// <summary>
        /// 1. List all customers in the database:
        ///    - Print each customer's full name (First + Last) and Email.
        /// </summary>
        public async Task Task01_ListAllCustomers()
        {
            var customers = await _dbContext.Customers
                .Select(c => new { c.FirstName, c.LastName, c.Email })
                .ToListAsync();

            Console.WriteLine("=== TASK 01: List All Customers ===");

            foreach (var c in customers)
            {
                Console.WriteLine($"{c.FirstName} {c.LastName} - {c.Email}");
            }


        }

        /// <summary>
        /// 2. Fetch all orders along with:
        ///    - Customer Name
        ///    - Order ID
        ///    - Order Status
        ///    - Number of items in each order (the sum of OrderItems.Quantity)
        /// </summary>
        public async Task Task02_ListOrdersWithItemCount()
        {
            // TODO: Write a query to return all orders,
            //       along with the associated customer name, order status,
            //       and the total quantity of items in that order.

            // HINT: Use Include/ThenInclude or projection with .Select(...).
            //       Summing the quantities: order.OrderItems.Sum(oi => oi.Quantity).

            var result = await _dbContext.Orders
                .Select(order => new
                {
                    order.Customer.FirstName,
                    order.Customer.LastName,
                    order.OrderStatus,
                    Quantity = order.OrderItems.Sum(orderItem => orderItem.Quantity)
                })
                .ToListAsync();

            Console.WriteLine(" ");
            Console.WriteLine("=== TASK 02: List Orders With Item Count ===");

            foreach (var r in result)
            {
                Console.WriteLine($"{r.FirstName} {r.LastName} {r.OrderStatus} {r.Quantity}");
            }
        }

        /// <summary>
        /// 3. List all products (ProductName, Price),
        ///    sorted by price descending (highest first).
        /// </summary>
        public async Task Task03_ListProductsByDescendingPrice()
        {
            // TODO: Write a query to fetch all products and sort them
            //       by descending price.
            // HINT: context.Products.OrderByDescending(p => p.Price)
            Console.WriteLine("\n=== Task 03: List Products By Descending Price ===");

            var result = await _dbContext.Products
                .OrderByDescending(p => p.Price)
                .ToListAsync();

            foreach (var p in result)
            {
                Console.WriteLine($"Id: {p.ProductId}, name: {p.ProductName}, price: {p.Price}€");
            }


        }

        /// <summary>
        /// 4. Find all "Pending" orders (order status = "Pending")
        ///    and display:
        ///      - Customer Name
        ///      - Order ID
        ///      - Order Date
        ///      - Total price (sum of unit_price * quantity - discount) for each order
        /// </summary>
        public async Task Task04_ListPendingOrdersWithTotalPrice()
        {
            // TODO: Write a query to fetch only PENDING orders,
            //       and calculate their total price.
            // HINT: The total can be computed from each OrderItem:
            //       (oi.UnitPrice * oi.Quantity) - oi.Discount
            Console.WriteLine(" ");
            Console.WriteLine("=== Task 04: List Pending Orders With Total Price ===");

            var result = await _dbContext.Orders
                .Where(o => o.OrderStatus == "Pending")
                .Select(o => new
                {
                    o.Customer,
                    o.OrderId,
                    o.OrderDate,
                    TotalPrice = o.OrderItems.Sum(item => (item.UnitPrice * item.Quantity) - item.Discount)

                })
                .ToListAsync();

            foreach (var o in result)
            {
                Console.WriteLine($"Name: {o.Customer.FirstName} {o.Customer.LastName}");
                Console.WriteLine($"id: {o.OrderId}");
                Console.WriteLine($"OrderDate: {o.OrderDate}");
                Console.WriteLine($"Total Price: {o.TotalPrice}");
            }
        }

        /// <summary>
        /// 5. List the total number of orders each customer has placed.
        ///    Output should show:
        ///      - Customer Full Name
        ///      - Number of Orders
        /// </summary>
        public async Task Task05_OrderCountPerCustomer()
        {
            // TODO: Write a query that groups by Customer,
            //       counting the number of orders each has.

            // HINT: 
            //  1) Join Orders and Customers, or
            //  2) Use the navigation (context.Orders or context.Customers),
            //     then group by customer ID or by the customer entity.
            Console.WriteLine("\n=== Task 05: Order Count Per Customer ===");

            var result = await _dbContext.Orders
                .Join(
                _dbContext.Customers,
                order => order.CustomerId,
                customer => customer.CustomerId,
                (order, customer) => new
                {
                    order,
                    customer
                })
                .GroupBy(joined => joined.customer)
                .Select(group => new
                {
                    customer = group.Key,
                    OrderedQuantity = group.Count()
                })
                .ToListAsync();

            foreach (var group in result)
            {
                Console.WriteLine($"Name: {group.customer.FirstName} {group.customer.LastName}");
                Console.WriteLine($"OrderQuantity: {group.OrderedQuantity}");
            }
        }

        /// <summary>
        /// 6. Show the top 3 customers who have placed the highest total order value overall.
        ///    - For each customer, calculate SUM of (OrderItems * Price).
        ///      Then pick the top 3.
        /// </summary>
        public async Task Task06_Top3CustomersByOrderValue()
        {
            // TODO: Calculate each customer's total order value 
            //       using their Orders -> OrderItems -> (UnitPrice * Quantity - Discount).
            //       Sort descending and take top 3.

            // HINT: You can do this in a single query or multiple steps.
            //       One approach:
            //         1) Summarize each Order's total
            //         2) Summarize for each Customer
            //         3) Order by descending total
            //         4) Take(3)
            Console.WriteLine("\n=== Task 06: Top 3 Customers By Order Value ===");

            var result = await _dbContext.Customers
                .Select(c => new
                {
                    c.CustomerId,
                    Name = c.FirstName + " " + c.LastName,
                    OrderTotal = c.Orders.Select(o => o.OrderItems.Sum(oi => oi.UnitPrice * oi.Quantity - oi.Discount)).Sum()
                })
                .OrderByDescending(o => o.OrderTotal)
                .Take(3)
                .ToListAsync();

            foreach (var top in result)
            {
                Console.WriteLine($"Name: {top.Name}");
                Console.WriteLine($"OrderTotal: {Math.Round((decimal)top.OrderTotal, 2)}");
            }
        }

        /// <summary>
        /// 7. Show all orders placed in the last 30 days (relative to now).
        ///    - Display order ID, date, and customer name.
        /// </summary>
        public async Task Task07_RecentOrders()
        {
            // TODO: Filter orders to only those with OrderDate >= (DateTime.Now - 30 days).
            //       Output ID, date, and the customer's name.
            Console.WriteLine("\n=== Task 07: Recent Orders ===");
            var result = await _dbContext.Orders
                .Where(o => o.OrderDate <= (DateTime.Now.AddDays(-30)))
                .ToListAsync();

            foreach (var order in result)
            {
                Console.WriteLine($"OrderId: {order.OrderId}");
                Console.WriteLine($"Date: {order.OrderDate}");
                Console.WriteLine($"Name : {order.Customer.FirstName} {order.Customer.LastName}");
            }
        }

        /// <summary>
        /// 8. For each product, display how many total items have been sold
        ///    across all orders.
        ///    - Product name, total sold quantity.
        ///    - Sort by total sold descending.
        /// </summary>
        public async Task Task08_TotalSoldPerProduct()
        {
            // TODO: Group or join OrdersItems by Product.
            //       Summation of quantity.
            Console.WriteLine("\n=== Task 08: Total Sold Per Product ===");

            var result = await _dbContext.Products
                .Join(_dbContext.OrderItems,
                product => product.ProductId,
                orderItem => orderItem.ProductId,
                (product, orderItem) => new
                {
                    product,
                    orderItem
                })
                .Select(joined => new
                {
                    joined.product,
                    OrderedQuantity = joined.orderItem.Quantity
                })
                .ToListAsync();

            foreach (var product in result)
            {
                Console.WriteLine($"Product name: {product.product.ProductName}");
                Console.WriteLine($"Total sold quantity: {product.OrderedQuantity}");
            }
        }

        /// <summary>
        /// 9. List any orders that have at least one OrderItem with a Discount > 0.
        ///    - Show Order ID, Customer name, and which products were discounted.
        /// </summary>
        public async Task Task09_DiscountedOrders()
        {
            // TODO: Identify orders with any OrderItem having (Discount > 0).
            //       Display order details, plus the discounted products.
            Console.WriteLine("\n=== Task 09: Discounted Orders ===");

            var result = await _dbContext.Orders
                .Where(order => order.OrderItems.Any(item => item.Discount > 0) && order.OrderItems.Count() > 0)
                .ToListAsync();

            foreach (var order in result)
            {
                Console.WriteLine($"OrderId: {order.OrderId}");
                Console.WriteLine($"Customer name: {order.Customer.FirstName} {order.Customer.LastName}");
                foreach (var item in order.OrderItems)
                {
                    Console.WriteLine($"Discounted Products: {item.Product.ProductName}");
                    Console.WriteLine($"Discount: {item.Discount}");
                }
            }

            // En tiedä toimiiko tämä. Molempien ordereitten orderitems näyttäisi oleva tyhjä debugissa mutta se menee silti läpi. En ymmärrä.
        }

        /// <summary>
        /// 10. (Open-ended) Combine multiple joins or navigation properties
        ///     to retrieve a more complex set of data. For example:
        ///     - All orders that contain products in a certain category
        ///       (e.g., "Electronics"), including the store where each product
        ///       is stocked most. (Requires `Stocks`, `Store`, `ProductCategory`, etc.)
        ///     - Or any custom scenario that spans multiple tables.
        /// </summary>
        public async Task Task10_AdvancedQueryExample()
        {
            // TODO: Design your own complex query that demonstrates
            //       multiple joins or navigation paths. For example:
            //       - Orders that contain any product from "Electronics" category.
            //       - Then, find which store has the highest stock of that product.
            //       - Print results.

            // Here's an outline you could explore:
            // 1) Filter products by category name "Electronics"
            // 2) Find any orders that contain these products
            // 3) For each of those products, find the store with the max stock
            //    (requires .MaxBy(...) in .NET 6+ or custom code in older versions)
            // 4) Print a combined result

            // (Implementation is left as an exercise.)
            Console.WriteLine("\n=== Task 10: Advanced Query Example ===");

            // En osannut tehdä enempää.

            /*var result = await _dbContext.Products
                .Where(product => product.Categories.Any(category => category.CategoryName == "Electronics"))
                .Join(_dbContext.OrderItems,
                product => product.ProductId,
                orderItem => orderItem.ProductId,
                (product, orderItem) => new
                {
                    product,
                    orderItem
                })
                .Join(_dbContext.Orders,
                joined => joined.orderItem.OrderId,
                order => order.OrderId,
                (joined, order) => new
                {
                    Order = order,
                    Product = joined.product,
                    OrderItem = joined.orderItem
                })
                .Join(
                _dbContext.Stocks,
                orderItem => orderItem.Product.ProductId,
                stock => stock.ProductId,
                (orderItem, stocks) => new
                {
                    Stocks = stocks,
                    OrderItems = orderItem
                })
                .Join(
                _dbContext.Stores,
                joined => joined.Stocks.StoreId,
                store => store.StoreId,
                (joined, store) => new
                {
                    Store = store,
                    OrderItems = joined.OrderItems
                })
                .Select(j => j.Store.Stocks.Any(s => s.QuantityInStock))
                .ToListAsync();*/

            var chatgptResult = await _dbContext.Products
            .Where(p => p.Categories.Any(c => c.CategoryName == "Electronics") && p.OrderItems.Any())
            .Select(p => new
            {
                Product = p,
                MaxStock = p.Stocks
                    .OrderByDescending(s => s.QuantityInStock)
                    .Select(s => new
                    {
                        Store = s.Store,
                        Quantity = s.QuantityInStock
                    })
                    .FirstOrDefault()
            })
            .ToListAsync();

            foreach (var result in chatgptResult)
            {
                Console.WriteLine($"Product {result.Product}:");
                Console.WriteLine($"StoreId: {result.MaxStock.Store.StoreId} Quantity: {result.MaxStock.Quantity}");
            }
        }

    }
}
