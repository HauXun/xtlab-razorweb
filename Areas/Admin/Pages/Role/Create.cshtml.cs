using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using App.Models;

namespace App.Admin.Role
{
  [Authorize(Roles = "Administrator")]
  public class CreateModel : RolePageModel
  {
    public CreateModel(RoleManager<IdentityRole> roleManager, AppDbContext context) : base(roleManager, context)
    {
    }

    public class InputModel
    {
      [Display(Name = "Tên của role")]
      [Required(ErrorMessage = "Phải nhập {0}")]
      [StringLength(256, ErrorMessage = "{0} phải dài từ {2} đến {1} ký tự", MinimumLength = 3)]
      public string Name { get; set; }
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
      if (!ModelState.IsValid)
      {
        return Page();
      }

      var newRole = new IdentityRole(Input.Name);
      var result = await _roleManager.CreateAsync(newRole);
      if (result.Succeeded)
      {
        StatusMessage = $"Bạn vừa tạo role mới: {Input.Name}";
        return RedirectToPage("./Index");
      }
      else
      {
        result.Errors.ToList().ForEach(e =>
        {
          ModelState.AddModelError(string.Empty, e.Description);
        });
      }

      return Page();
    }
  }
}
