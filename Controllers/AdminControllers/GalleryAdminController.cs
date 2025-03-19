using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using EgitimSitesi.Data;
using EgitimSitesi.Models;
using EgitimSitesi.Services;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace EgitimSitesi.Controllers.AdminControllers
{
    [Authorize]
    [Route("Admin/Gallery")]
    public class GalleryAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CloudinaryService _cloudinaryService;

        public GalleryAdminController(ApplicationDbContext context, CloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        // GET: Admin/Gallery
        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm, string typeFilter, int page = 1)
        {
            int pageSize = 10;
            
            // Start with all gallery images
            var query = _context.Gallery.AsQueryable();
            
            // Apply filters if provided
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(g => g.Description.Contains(searchTerm));
            }
            
            if (!string.IsNullOrEmpty(typeFilter) && Enum.TryParse<GalleryType>(typeFilter, out var type))
            {
                query = query.Where(g => g.Type == type);
            }
            
            // Get total count for pagination
            var totalCount = await query.CountAsync();
            
            // Apply pagination
            var images = await query
                .OrderBy(g => g.Type)
                .ThenBy(g => g.Order)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            // Create view model using existing GalleryViewModel
            var viewModel = new GalleryViewModel
            {
                Images = images,
                SelectedType = !string.IsNullOrEmpty(typeFilter) && Enum.TryParse<GalleryType>(typeFilter, out var selectedType) 
                    ? selectedType 
                    : GalleryType.Sınıflarımız,
                TypeDisplayName = !string.IsNullOrEmpty(typeFilter) && Enum.TryParse<GalleryType>(typeFilter, out var displayType) 
                    ? GetEnumDisplayName(displayType) 
                    : "Tüm Resimler",
                TypeOptions = Enum.GetValues(typeof(GalleryType))
                    .Cast<GalleryType>()
                    .Select(t => new GalleryTypeInfo
                    {
                        Type = t,
                        DisplayName = GetEnumDisplayName(t),
                        Url = Url.Action("Index", "GalleryAdmin", new { typeFilter = t.ToString() })
                    }).ToList()
            };
            
            // Add pagination information to ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.SearchTerm = searchTerm;
            ViewBag.TypeFilter = typeFilter;
            
            return View("~/Views/Admin/Gallery/Index.cshtml", viewModel);
        }

        // GET: Admin/Gallery/Details/5
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var galleryImage = await _context.Gallery.FindAsync(id);
            
            if (galleryImage == null)
            {
                return NotFound();
            }
            
            return View("~/Views/Admin/Gallery/Details.cshtml", galleryImage);
        }

        // GET: Admin/Gallery/Create
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            ViewBag.NextOrder = await _context.Gallery.CountAsync() + 1;
            return View("~/Views/Admin/Gallery/Create.cshtml");
        }

        // POST: Admin/Gallery/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GalleryModel model, IFormFile imageFile)
        {
         
                // Check if image file was provided
                if (imageFile == null)
                {
                    ModelState.AddModelError("ImageFile", "Lütfen bir resim seçin");
                    return View("~/Views/Admin/Gallery/Create.cshtml", model);
                }
                
                // Upload the image to Cloudinary
                var uploadResult = await _cloudinaryService.UploadImageAsync(imageFile);
                
                if (uploadResult == null)
                {
                    ModelState.AddModelError("ImageFile", "Resim yüklenemedi. Lütfen tekrar deneyin.");
                    return View("~/Views/Admin/Gallery/Create.cshtml", model);
                }
                
                // Update the model with Cloudinary information
                model.ImagePath = uploadResult.SecureUrl.ToString();
                model.CloudinaryPublicId = uploadResult.PublicId;
                model.CreationDate = DateTime.Now;
                
                _context.Add(model);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Index));
            
            
            return View("~/Views/Admin/Gallery/Create.cshtml", model);
        }

        // GET: Admin/Gallery/Edit/5
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var galleryImage = await _context.Gallery.FindAsync(id);
            
            if (galleryImage == null)
            {
                return NotFound();
            }
            
            ViewBag.CurrentImage = galleryImage.ImagePath;
            return View("~/Views/Admin/Gallery/Edit.cshtml", galleryImage);
        }

        // POST: Admin/Gallery/Edit/5
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GalleryModel model, IFormFile imageFile)
        {
            if (id != model.Id)
            {
                return NotFound();
            }
            
       
                try
                {
                    var galleryImage = await _context.Gallery.FindAsync(id);
                    
                    if (galleryImage == null)
                    {
                        return NotFound();
                    }
                    
                    // Check if a new image was uploaded
                    if (imageFile != null)
                    {
                        // Upload the new image to Cloudinary
                        var uploadResult = await _cloudinaryService.UploadImageAsync(imageFile);
                        
                        if (uploadResult == null)
                        {
                            ModelState.AddModelError("ImageFile", "Yeni resim yüklenemedi. Lütfen tekrar deneyin.");
                            ViewBag.CurrentImage = galleryImage.ImagePath;
                            return View("~/Views/Admin/Gallery/Edit.cshtml", model);
                        }
                        
                        // Delete old image from Cloudinary if it exists
                        if (!string.IsNullOrEmpty(galleryImage.CloudinaryPublicId))
                        {
                            await _cloudinaryService.DeleteImageAsync(galleryImage.CloudinaryPublicId);
                        }
                        
                        // Update image properties
                        galleryImage.ImagePath = uploadResult.SecureUrl.ToString();
                        galleryImage.CloudinaryPublicId = uploadResult.PublicId;
                    }
                    
                    // Update other properties
                    galleryImage.Type = model.Type;
                    galleryImage.Description = model.Description;
                    galleryImage.Order = model.Order;
                    galleryImage.IsActive = model.IsActive;
                    
                    _context.Update(galleryImage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GalleryImageExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                
                return RedirectToAction(nameof(Index));
            
            
            ViewBag.CurrentImage = (await _context.Gallery.FindAsync(id))?.ImagePath;
            return View("~/Views/Admin/Gallery/Edit.cshtml", model);
        }

        // GET: Admin/Gallery/Delete/5
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var galleryImage = await _context.Gallery.FindAsync(id);
            
            if (galleryImage == null)
            {
                return NotFound();
            }
            
            return View("~/Views/Admin/Gallery/Delete.cshtml", galleryImage);
        }

        // POST: Admin/Gallery/Delete/5
        [HttpPost("Delete/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var galleryImage = await _context.Gallery.FindAsync(id);
            
            if (galleryImage == null)
            {
                return NotFound();
            }
            
            // Delete image from Cloudinary if it exists
            if (!string.IsNullOrEmpty(galleryImage.CloudinaryPublicId))
            {
                await _cloudinaryService.DeleteImageAsync(galleryImage.CloudinaryPublicId);
            }
            
            _context.Gallery.Remove(galleryImage);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Gallery/ToggleActive/5
        [HttpPost("ToggleActive/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var galleryImage = await _context.Gallery.FindAsync(id);
            
            if (galleryImage == null)
            {
                return NotFound();
            }
            
            // Toggle active status
            galleryImage.IsActive = !galleryImage.IsActive;
            
            _context.Update(galleryImage);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        private bool GalleryImageExists(int id)
        {
            return _context.Gallery.Any(e => e.Id == id);
        }
        
        // Helper method to get display name for enum values
        private string GetEnumDisplayName(GalleryType value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            
            if (fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false) is DisplayAttribute[] attributes && attributes.Length > 0)
            {
                return attributes[0].Name;
            }
            
            return value.ToString();
        }
    }
} 