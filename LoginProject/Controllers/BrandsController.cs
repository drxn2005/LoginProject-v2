using LoginProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoginProject.Data;
using System.Security.Claims;

namespace LoginProject.Controllers
{
    [Authorize]
    public class BrandsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BrandsController(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        // GET: Brands
        public async Task<IActionResult> Index(int page = 1, int pageSize = 12, string search = "")
        {
            var query = _context.Brands.Where(b => !b.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(b =>
                    b.Name.Contains(search) ||
                    b.Description.Contains(search));
            }

            var totalCount = await query.CountAsync();
            var brands = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(); // هنا التغيير

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.Search = search;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return View(brands);
        }

        // GET: Brands/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var brand = await _context.Brands
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (brand == null)
                return NotFound();

            return View(brand);
        }

        // GET: Brands/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Brands/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Brand brand)
        {
            if (ModelState.IsValid)
            {
                brand.CreatedAt = DateTime.UtcNow;
                brand.UpdatedAt = DateTime.UtcNow;
                brand.CreatedByUserId = GetCurrentUserId();

                _context.Add(brand);
                await _context.SaveChangesAsync();

                TempData["Success"] = "تم إنشاء العلامة التجارية بنجاح.";
                return RedirectToAction(nameof(Index));
            }
            return View(brand);
        }

        // GET: Brands/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var brand = await _context.Brands.FindAsync(id);
            if (brand == null || brand.IsDeleted)
                return NotFound();

            return View(brand);
        }

        // POST: Brands/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Brand brand)
        {
            if (id != brand.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBrand = await _context.Brands.FindAsync(id);
                    if (existingBrand == null || existingBrand.IsDeleted)
                        return NotFound();

                    existingBrand.Name = brand.Name;
                    existingBrand.Description = brand.Description;
                    existingBrand.LogoUrl = brand.LogoUrl;
                    existingBrand.Website = brand.Website;
                    existingBrand.IsActive = brand.IsActive;
                    existingBrand.UpdatedAt = DateTime.UtcNow;
                    existingBrand.UpdatedByUserId = GetCurrentUserId();

                    _context.Update(existingBrand);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "تم تحديث العلامة التجارية بنجاح.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BrandExists(id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(brand);
        }

        // GET: Brands/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var brand = await _context.Brands
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (brand == null)
                return NotFound();

            return View(brand);
        }

        // POST: Brands/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand != null && !brand.IsDeleted)
            {
                brand.IsDeleted = true;
                brand.DeletedAt = DateTime.UtcNow;
                brand.UpdatedAt = DateTime.UtcNow;
                brand.UpdatedByUserId = GetCurrentUserId();

                _context.Update(brand);
                await _context.SaveChangesAsync();

                TempData["Success"] = "تم حذف العلامة التجارية بنجاح.";
            }
            else
            {
                TempData["Error"] = "العلامة التجارية غير موجودة أو محذوفة بالفعل.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Add Sample Brands
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSampleBrands()
        {
            var sampleBrands = new List<Brand>
            {
                new Brand
                {
                    Name = "ستاربكس",
                    Description = "مقهى عالمي مشهور بالقهوة المميزة",
                    LogoUrl = "https://logo.clearbit.com/starbucks.com",
                    Website = "https://www.starbucks.com",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    CreatedByUserId = GetCurrentUserId()
                },
                new Brand
                {
                    Name = "كافيه كوستا",
                    Description = "سلسلة مقاهي بريطانية الأصل",
                    LogoUrl = "https://logo.clearbit.com/costa.co.uk",
                    Website = "https://www.costa.co.uk",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-25),
                    CreatedByUserId = GetCurrentUserId()
                },
                new Brand
                {
                    Name = "دونكين",
                    Description = "سلسلة مقاهي أمريكية متخصصة في القهوة والكعك",
                    LogoUrl = "https://logo.clearbit.com/dunkindonuts.com",
                    Website = "https://www.dunkindonuts.com",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-20),
                    CreatedByUserId = GetCurrentUserId()
                }
            };

            _context.Brands.AddRange(sampleBrands);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم إضافة 3 علامات تجارية تجريبية بنجاح!";
            return RedirectToAction(nameof(Index));
        }

        private bool BrandExists(int id)
        {
            return _context.Brands.Any(e => e.Id == id && !e.IsDeleted);
        }
    }
}