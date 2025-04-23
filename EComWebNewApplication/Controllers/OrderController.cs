using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using EComWebNewApplication.Data;
using EComWebNewApplication.Orders;
using EComWebNewApplication.ViewModels;

namespace OrderInvoiceSystem.Controllers
{
    // Only authenticated users can access these actions.
    [Authorize]
    public class OrderController : Controller
    {
        // The database context used to access the data.
        private readonly ApplicationDbContext _context;

        // The configuration object to read settings (like email settings) from appsettings.json.
        private readonly IConfiguration _configuration;

        // Constructor that receives ApplicationDbContext and IConfiguration via dependency injection.
        public OrderController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: Order/Index
        // Displays a list of orders for the currently logged-in customer.
        public async Task<IActionResult> Index()
        {
            // Retrieve the logged-in user's CustomerId from the claims.
            var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (customerIdClaim == null)
                return RedirectToAction("Login", "Account");

            int customerId = int.Parse(customerIdClaim.Value);

            // Query orders for this customer, including the order items.
            var orders = await _context.Orders
                .AsNoTracking()  // Improves performance for read-only queries.
                .Include(o => o.OrderItems)
                .Where(o => o.CustomerId == customerId)
                .ToListAsync();

            // Pass the list of orders to the view.
            return View(orders);
        }

        // GET: Order/Create
        // Displays the order creation form.
        public IActionResult Create()
        {
            // For simplicity, we pass the list of available products using ViewBag.
            ViewBag.Products = _context.Products.AsNoTracking().ToList();
            return View();
        }

        // POST: Order/Create
        // Processes the submitted order creation form.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderCreateViewModel model)
        {
            // If the model state is invalid, return the view with validation errors.
            if (!ModelState.IsValid)
                return View(model);

            // Retrieve the logged-in customer's ID from the claims.
            var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (customerIdClaim == null)
                return RedirectToAction("Login", "Account");

            int customerId = int.Parse(customerIdClaim.Value);

            // Generate a unique order number using current ticks.
            var orderNumber = $"ORD-{DateTime.UtcNow.Ticks}";

            // Create a new Order entity with initial values.
            var order = new Order
            {
                CustomerId = customerId,
                OrderNumber = orderNumber,
                OrderDate = DateTime.UtcNow,
                OrderStatus = "Pending", // New orders start with "Pending" status.
                CreatedDate = DateTime.UtcNow,
                OrderItems = new List<OrderItem>()
            };

            decimal totalAmount = 0;

            // Iterate through each submitted order item.
            foreach (var item in model.OrderItems)
            {
                // Only include items where the quantity is greater than 0.
                if (item.Quantity <= 0)
                    continue;

                // Retrieve the product from the database using its ID.
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                    continue;

                // Create a new OrderItem entity.
                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.UnitPrice
                };

                // Add the computed item total to the order's total amount.
                totalAmount += orderItem.Total;

                // Add the order item to the order.
                order.OrderItems.Add(orderItem);
            }
            order.TotalAmount = totalAmount;

            // Add the new order to the context and save changes.
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Redirect the customer to the Payment page for this order.
            return RedirectToAction("Payment", new { orderId = order.Id });
        }

        // GET: Order/Payment
        // Displays the payment page with order summary details.
        public async Task<IActionResult> Payment(int orderId)
        {
            // Retrieve the order including customer information.
            var paymentViewModel = await _context.Orders.AsNoTracking()
                .Select(order => new PaymentViewModel()
                {
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    TotalAmount = order.TotalAmount
                })
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (paymentViewModel == null)
                return NotFound();

            // Store the order in ViewBag (for this simplified example) to display summary data.
            //ViewBag.Order = order;
            return View(paymentViewModel);
        }

        // POST: Order/Payment
        // Processes the payment for the order.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Payment(PaymentViewModel paymentViewModel)
        {
            // Retrieve the order (including customer) from the database.
            var order = await _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == paymentViewModel.OrderId);

            if (order == null)
                return NotFound();

            // Simulate payment processing based on selected payment method.
            // For example: if PayPal is chosen, simulate failure; if COD, set order to pending; otherwise, complete the order.
            string paymentStatus = "";
            if (paymentViewModel.PaymentMethod.ToUpper() == "PAYPAL")
            {
                order.OrderStatus = "Cancelled";
                paymentStatus = "Failed";
            }
            else if (paymentViewModel.PaymentMethod.ToUpper() == "COD")
            {
                order.OrderStatus = "Pending";
                paymentStatus = "Pending";
            }
            else
            {
                order.OrderStatus = "Completed";
                paymentStatus = "Success";
            }

            // Create a new Payment record.
            var payment = new Payment
            {
                OrderId = order.Id,
                AmountPaid = order.TotalAmount,
                PaymentMethod = paymentViewModel.PaymentMethod,
                PaymentStatus = paymentStatus,
                PaymentDate = DateTime.UtcNow
            };

            // Add the payment to the context.
            _context.Payments.Add(payment);

            // Save changes (update order status and add payment record).
            await _context.SaveChangesAsync();

            // Generate the invoice PDF and send it via email.
            byte[] pdfBytes = await GenerateInvoicePdfAsync(order.Id);
            await SendInvoiceEmail(order.Customer.Email, order.Customer.CustomerName,
                                   BuildEmailSubject(order),
                                   BuildEmailBody(order),
                                   pdfBytes);

            // Redirect to the Order Placed page, which displays the order details.
            return RedirectToAction("OrderPlaced", new { orderId = order.Id });
        }

        // GET: Order/OrderPlaced/{orderId}
        // Displays a confirmation page with the order details after the order is placed.
        public async Task<IActionResult> OrderPlaced(int orderId)
        {
            // Retrieve the order along with its order items and associated products, plus customer details.
            var order = await _context.Orders.AsNoTracking()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            // Pass the order to the view.
            return View(order);
        }

        // GET: Order/GenerateInvoice/{orderId}?sendEmail=false
        // Generates a PDF invoice for the given order.
        // If sendEmail is true, the invoice is emailed to the customer; otherwise, it is returned as a file.
        [AllowAnonymous]
        public async Task<IActionResult> GenerateInvoice(int orderId, bool sendEmail = false)
        {
            // Generate the invoice PDF.
            byte[] pdfBytes = await GenerateInvoicePdfAsync(orderId);

            // If sendEmail flag is true, look up the order and email the invoice.
            if (sendEmail)
            {
                var order = await _context.Orders.AsNoTracking()
                    .Include(o => o.Customer)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order != null)
                {
                    await SendInvoiceEmail(order.Customer.Email, order.Customer.CustomerName,
                                           BuildEmailSubject(order),
                                           BuildEmailBody(order),
                                           pdfBytes);
                }
            }

            // Return the PDF file for download.
            return File(pdfBytes, "application/pdf", $"OrderInvoice_{orderId}.pdf");
        }

        // Helper method to generate invoice PDF using IronPdf.
        // Loads the order details, selects an appropriate invoice template based on the order status,
        // replaces placeholders in the HTML with real order values, and returns the PDF as a byte array.
        private async Task<byte[]> GenerateInvoicePdfAsync(int orderId)
        {
            // Retrieve the order along with order items, associated products, and customer info.
            var order = await _context.Orders.AsNoTracking()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Customer)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new Exception("Order not found.");

            // Retrieve the associated payment record, if available.
            // var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == order.Id);

            // Determine which invoice template to use based on the order status.
            OrderStatus templateType;
            if (order.OrderStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                templateType = OrderStatus.OrderCompletion;
            }
            else if (order.OrderStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                templateType = OrderStatus.OrderCancelled;
            }
            else
            {
                // For pending orders, use the pending template.
                templateType = OrderStatus.OrderPending;
            }

            // Retrieve the invoice template from the database.
            var template = await _context.Templates.FirstOrDefaultAsync(t => t.orderStatus == templateType);
            if (template == null)
                throw new Exception("Invoice template not found.");

            // Build the HTML string for the product list by iterating through the order items.
            string productListHtml = "";
            foreach (var item in order.OrderItems)
            {
                productListHtml += $"<tr>" +
                                   $"<td style='padding:12px; border:1px solid #ddd;'>{item.Product.ProductName}</td>" +
                                   $"<td style='padding:12px; border:1px solid #ddd;'>{item.Quantity}</td>" +
                                   $"<td style='padding:12px; border:1px solid #ddd;'>{item.UnitPrice:C}</td>" +
                                   $"<td style='padding:12px; border:1px solid #ddd;'>{item.Total:C}</td>" +
                                   $"</tr>";
            }
            // Append closing tags for the table body if needed (assuming the template includes the table header).
            productListHtml += "</tbody></table>";

            // Replace placeholders in the HTML template with actual order values.
            string htmlContent = template.HtmlContent;
            htmlContent = ReplacePlaceholder(htmlContent, "CustomerName", order.Customer.CustomerName);
            htmlContent = ReplacePlaceholder(htmlContent, "OrderDate", order.OrderDate.ToString("MMMM dd, yyyy"));
            htmlContent = ReplacePlaceholder(htmlContent, "OrderNumber", order.OrderNumber);
            htmlContent = ReplacePlaceholder(htmlContent, "OrderStatus", order.OrderStatus);
            htmlContent = ReplacePlaceholder(htmlContent, "ProductList", productListHtml);
            htmlContent = ReplacePlaceholder(htmlContent, "TotalAmount", order.TotalAmount.ToString("C"));

            // Replace payment placeholders if a payment record exists.
            if (order.Payment != null)
            {
                htmlContent = ReplacePlaceholder(htmlContent, "PaymentStatus", order.Payment.PaymentStatus);
                htmlContent = ReplacePlaceholder(htmlContent, "PaymentMethod", order.Payment.PaymentMethod);
                htmlContent = ReplacePlaceholder(htmlContent, "PaymentDate", order.Payment.PaymentDate.ToString("MMMM dd, yyyy"));
            }

            // Create an instance of ChromePdfRenderer from IronPdf.
            var renderer = new ChromePdfRenderer();

            // Optional: If you have a license key, set it before rendering
            // IronPdf.License.LicenseKey = "Your IRON PDF License";

            // Render the HTML string to a PDF document.
            var pdfDocument = renderer.RenderHtmlAsPdf(htmlContent);

            // Return the PDF document as a byte array.
            return pdfDocument.BinaryData;
        }

        // Helper method for placeholder replacement.
        // Replaces occurrences of a placeholder (e.g., {CustomerName}) in the HTML with the provided value.
        private string ReplacePlaceholder(string html, string placeholder, string value)
        {
            return html.Replace("{" + placeholder + "}", value);
        }

        // Helper method to build a dynamic email subject based on the order status.
        private string BuildEmailSubject(Order order)
        {
            if (order.OrderStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                return $"Your Order Invoice - Order #{order.OrderNumber} Completed";
            else if (order.OrderStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                return $"Your Order Invoice - Order #{order.OrderNumber} Cancelled";
            else
                return $"Your Order Invoice - Order #{order.OrderNumber} Pending";
        }

        // Helper method to build a dynamic email body based on the order status.
        private string BuildEmailBody(Order order)
        {
            if (order.OrderStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                return $"Dear {order.Customer.CustomerName},<br/><br/>" +
                       $"Thank you for your order. Your order #{order.OrderNumber} has been completed successfully. " +
                       $"Please find attached your invoice.<br/><br/>Best regards,<br/>My Estore App";
            }
            else if (order.OrderStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                return $"Dear {order.Customer.CustomerName},<br/><br/>" +
                       $"We regret to inform you that your order #{order.OrderNumber} has been cancelled. " +
                       $"Please contact our support for further details.<br/><br/>Best regards,<br/>My Estore App";
            }
            else
            {
                return $"Dear {order.Customer.CustomerName},<br/><br/>" +
                       $"Please find attached your invoice for order #{order.OrderNumber}.<br/><br/>Best regards,<br/>My Estore App";
            }
        }

        // Helper method to send the invoice PDF via email using SMTP.
        // Retrieves SMTP settings from configuration, creates an email message with the PDF attached, and sends it.
        private async Task SendInvoiceEmail(string toEmail, string customerName, string subject, string htmlBody, byte[] pdfAttachment, bool isBodyHtml = true)
        {
            // Retrieve email settings from appsettings.json.
            string smtpServer = _configuration.GetValue<string>("EmailSettings:SmtpServer") ?? "";
            int smtpPort = int.Parse(_configuration.GetValue<string>("EmailSettings:SmtpPort") ?? "587");
            string senderName = _configuration.GetValue<string>("EmailSettings:SenderName") ?? "My Estore App";
            string senderEmail = _configuration.GetValue<string>("EmailSettings:SenderEmail") ?? "";
            string password = _configuration.GetValue<string>("EmailSettings:Password") ?? "";

            // Create a new MailMessage.
            using (var message = new MailMessage())
            {
                message.From = new MailAddress(senderEmail, senderName);
                message.To.Add(new MailAddress(toEmail));
                message.Subject = subject;
                message.Body = htmlBody;
                message.IsBodyHtml = isBodyHtml;

                // Attach the PDF invoice.
                message.Attachments.Add(new Attachment(new MemoryStream(pdfAttachment), "OrderInvoice.pdf", "application/pdf"));

                // Create an SMTP client using the provided settings.
                using (var client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.Credentials = new NetworkCredential(senderEmail, password);
                    client.EnableSsl = true;
                    // Send the email asynchronously.
                    await client.SendMailAsync(message);
                }
            }
        }
    }
}