using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EMAP.Web.ViewModels.Fyp
{
    public class BulkSupervisorUploadViewModel
    {
        [Required(ErrorMessage = "Please select an Excel file.")]
        public IFormFile? File { get; set; }
    }
}