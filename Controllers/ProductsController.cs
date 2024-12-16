using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Watermark.Models;
using Watermark.Services;

namespace Watermark.Controllers
{
public class ProductsController:Controller
{
        private readonly AppDbContext _context;
        private readonly RabbitMQPublisher _rabbitMQPublisher;

        public ProductsController(AppDbContext context,RabbitMQPublisher rabbitMQPublisher)
        {
            _context = context;
            _rabbitMQPublisher = rabbitMQPublisher;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
           return View(await _context.Products.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public  IActionResult  Create()
        {
            return  View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Price,Stock,ImageName")] Product product, IFormFile ImageFile)
        {
            product.Id = Guid.NewGuid();
            //if (!ModelState.IsValid) return View(product);

            if(ImageFile is { Length:>0 })
            {
                var randomImageName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", randomImageName);

                await using FileStream stream = new(path, FileMode.Create);
                await ImageFile.CopyToAsync(stream);

                _rabbitMQPublisher.Publish(new ProductImageCreatedEvent() { ImageName = randomImageName });

                product.ImageName = randomImageName;
            }

            _context.Add(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(Guid id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
