using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IronPdf;
using Microsoft.AspNetCore.Authorization;
using EComWebNewApplication.Data;
using EComWebNewApplication.Models;
using EComWebNewApplication.ViewModels;

namespace Ec.Controllers
{
    [Authorize]
    public class TemplateController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TemplateController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Template/Index
        // Retrieves and displays a list of all invoice templates.
        public async Task<IActionResult> Index()
        {
            // Fetch all templates without tracking for performance improvement on read-only queries.
            var templates = await _context.Templates.AsNoTracking().ToListAsync();
            return View(templates);
        }

        // GET: Template/Create
        // Displays a form to create a new invoice template.
        public IActionResult Create()
        {
            // Initialize a new InvoiceTemplateViewModel and pass it to the view.
            return View(new InvoiceTemplateViewModel());
        }

        // POST: Template/Create
        // Processes the form submission for creating a new invoice template.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvoiceTemplateViewModel model)
        {
            // Check if the form data is valid.
            if (ModelState.IsValid)
            {
                // Map the view model to the domain model.
                var template = new Template
                {
                    TemplateName = model.TemplateName,
                    HtmlContent = model.HtmlContent,
                    orderStatus = model.TemplateType,
                    CreatedDate = DateTime.Now,
                    // Set the creator; default to "Admin" if the user identity is not available.
                    CreatedBy = User.Identity?.Name ?? "Admin"
                };

                // Add the new template to the database context.
                _context.Templates.Add(template);

                // Save changes asynchronously.
                await _context.SaveChangesAsync();

                // Redirect to the Index action to show the list of templates.
                return RedirectToAction(nameof(Index));
            }
            // If validation fails, return the view with the current model to display validation errors.
            return View(model);
        }

        // GET: Template/Edit/{id}
        // Retrieves an existing invoice template for editing.
        public async Task<IActionResult> Edit(int id)
        {
            // Retrieve the template by id without tracking changes (read-only).
            var template = await _context.Templates
                .AsNoTracking().FirstOrDefaultAsync(temp => temp.Id == id);

            // If the template does not exist, return a 404 Not Found.
            if (template == null)
            {
                return NotFound();
            }

            // Map the domain model to the view model.
            var viewModel = new InvoiceTemplateViewModel
            {
                Id = template.Id,
                TemplateName = template.TemplateName,
                HtmlContent = template.HtmlContent,
                TemplateType = template.orderStatus
            };

            // Return the Edit view with the populated view model.
            return View(viewModel);
        }

        // POST: Template/Edit/{id}
        // Processes the form submission for editing an invoice template.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(InvoiceTemplateViewModel model)
        {
            // Check if the submitted data is valid.
            if (ModelState.IsValid)
            {
                // Find the existing template by its id.
                var template = await _context.Templates.FindAsync(model.Id);
                if (template == null)
                {
                    return NotFound();
                }

                // Update template properties with values from the view model.
                template.TemplateName = model.TemplateName;
                template.HtmlContent = model.HtmlContent;
                template.orderStatus = model.TemplateType;
                template.UpdatedDate = DateTime.Now;

                // Set the updater; default to "Admin" if no identity is available.
                template.UpdatedBy = User.Identity?.Name ?? "Admin";

                // Save changes asynchronously.
                await _context.SaveChangesAsync();

                // Redirect to the Index action to show the updated list of templates.
                return RedirectToAction(nameof(Index));
            }
            // If the data is invalid, redisplay the form with validation errors.
            return View(model);
        }

        // GET: Template/Delete/{id}
        // Displays a confirmation page to delete an invoice template.
        public async Task<IActionResult> Delete(int? id)
        {
            // Check if the id is provided; if not, return 404.
            if (id == null)
            {
                return NotFound();
            }

            // Retrieve the template without tracking (for display only).
            var template = await _context.Templates.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            // If the template is not found, return 404.
            if (template == null)
            {
                return NotFound();
            }

            // Return the Delete view with the template details.
            return View(template);
        }

        // POST: Template/Delete/{id}
        // Processes the deletion of an invoice template.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Find the template by id.
            var template = await _context.Templates.FindAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            // Remove the template from the context.
            _context.Templates.Remove(template);

            // Save changes asynchronously.
            await _context.SaveChangesAsync();

            // Redirect back to the Index view.
            return RedirectToAction(nameof(Index));
        }
    }
}