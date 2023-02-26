using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorWeb.Models;

namespace App.Admin.Role
{
  [Authorize(Roles = "Administrator")]
  public class EditModel : RolePageModel
  {
    public EditModel(RoleManager<IdentityRole> roleManager, MyBlogContext context) : base(roleManager, context)
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

    public IdentityRole role { get; set; }

    public async Task<IActionResult> OnGetAsync(string roleid)
    {
      if (roleid == null) return NotFound("Không tìm thấy role");

      role = await _roleManager.FindByIdAsync(roleid);
      if (role != null)
      {
        Input = new InputModel
        {
          Name = role.Name
        };
        return Page();
      }

      return NotFound("Không tìm thấy role");
    }

    public async Task<IActionResult> OnPostAsync(string roleid)
    {
      if (roleid == null) return NotFound("Không tìm thấy role");

      role = await _roleManager.FindByIdAsync(roleid);

      if (role == null) return NotFound("Không tìm thấy role");

      if (!ModelState.IsValid)
      {
        return Page();
      }

      role.Name = Input.Name;
      var result = await _roleManager.UpdateAsync(role);
      if (result.Succeeded)
      {
        StatusMessage = $"Bạn vừa đổi tên: {Input.Name}";
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
