using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using App.Models;

namespace App.Admin.Role
{
  [Authorize(Roles = "Administrator")]
  public class AddRoleClaimModel : RolePageModel
  {
    public AddRoleClaimModel(RoleManager<IdentityRole> roleManager, AppDbContext context) : base(roleManager, context)
    {
    }

    public class InputModel
    {
      [Display(Name = "Kiểu (tên) claim")]
      [Required(ErrorMessage = "Phải nhập {0}")]
      [StringLength(256, ErrorMessage = "{0} phải dài từ {2} đến {1} ký tự", MinimumLength = 3)]
      public string ClaimType { get; set; }

      [Display(Name = "Giá trị")]
      [Required(ErrorMessage = "Phải nhập {0}")]
      [StringLength(256, ErrorMessage = "{0} phải dài từ {2} đến {1} ký tự", MinimumLength = 3)]
      public string ClaimValue { get; set; }
    }

    [BindProperty]
    public InputModel Input { get; set; }
    public IdentityRole role { get; set; }

    public async Task<IActionResult> OnGetAsync(string roleid)
    {
      role = await _roleManager.FindByIdAsync(roleid);
      if (role == null)
      {
        return NotFound("Không tìm thấy role");
      }

      return Page();
    }

    public async Task<IActionResult> OnPostAsync(string roleid)
    {
      role = await _roleManager.FindByIdAsync(roleid);
      if (role == null)
      {
        return NotFound("Không tìm thấy role");
      }

      if (!ModelState.IsValid)
      {
        return Page();
      }

      if ((await _roleManager.GetClaimsAsync(role)).Any(c => c.Type == Input.ClaimType && c.Value == Input.ClaimValue))
      {
        ModelState.AddModelError(string.Empty, "Claim này đã có trong role");
        return Page();
      }

      var newClaim = new Claim(Input.ClaimType, Input.ClaimValue);
      var result = await _roleManager.AddClaimAsync(role, newClaim);

      if (!result.Succeeded)
      {
        result.Errors.ToList().ForEach(e =>
        {
          ModelState.AddModelError(string.Empty, e.Description);
        });

        return Page();
      }

      StatusMessage = "Vừa thêm đặc tính (claim) mới";

      return RedirectToPage("./Edit", new
      {
        roleid = role.Id
      });
    }
  }
}
