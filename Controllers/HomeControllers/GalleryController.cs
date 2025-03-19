using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using EgitimSitesi.Models;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using EgitimSitesi.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EgitimSitesi.Controllers.HomeControllers
{
    public class GalleryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GalleryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(GalleryType? type = null)
        {
            // Get all available gallery types from the enum
            var galleryTypes = Enum.GetValues(typeof(GalleryType))
                                  .Cast<GalleryType>()
                                  .ToList();
            
            // Create TypeOptions list for the sidebar
            var typeOptions = new List<GalleryTypeInfo>();
            
            foreach (var galleryType in galleryTypes)
            {
                var displayName = GetEnumDisplayName(galleryType);
                typeOptions.Add(new GalleryTypeInfo
                {
                    Type = galleryType,
                    DisplayName = displayName,
                    Url = Url.Action("Index", "Gallery", new { type = galleryType })
                });
            }
            
            // Set the selected type (default to first type if not specified)
            var selectedType = type ?? galleryTypes.FirstOrDefault();
            
            // Get actual images from the database
            IQueryable<GalleryModel> query = _context.Gallery
                .Where(g => g.IsActive); // Only active images
                
            // Filter by type if specified
            if (selectedType != null)
            {
                query = query.Where(g => g.Type == selectedType);
            }
            
            // Order the images
            var images = await query
                .OrderBy(g => g.Order)
                .ToListAsync();
            
            var viewModel = new GalleryViewModel
            {
                Images = images,
                SelectedType = selectedType,
                TypeDisplayName = GetEnumDisplayName(selectedType),
                TypeOptions = typeOptions
            };
            
            // Use a specific view path to avoid conflicts
            return View("/Views/Home/Gallery/Index.cshtml", viewModel);
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