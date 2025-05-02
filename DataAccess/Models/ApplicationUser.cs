using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DocumentFormat.OpenXml.Wordprocessing;
using ForQab.DataAccess.Models;
using Microsoft.AspNetCore.Identity;

namespace ForQab.DataAccess.Models;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    [PersonalData]
    [Column(TypeName = "nvarchar(100)")]
    public string? FirstName { get; set; }
    [PersonalData]
    [Column(TypeName = "nvarchar(100)")]
    public string? LastName { get; set; }
    
    [PersonalData]
    [Column(TypeName = "nvarchar(100)")]
    public string? Section { get; set; }
    [PersonalData]
    [Column(TypeName = "int")]
    public int? SectionId { get; set; }
    [PersonalData]
    [Column("IsAdmin")]
    public int? IsAdmin { get; set; }

}

