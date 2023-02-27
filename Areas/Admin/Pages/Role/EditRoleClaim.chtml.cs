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
  public class EditRoleClaimModel : RolePageModel
  {
    public EditRoleClaimModel(RoleManager<IdentityRole> roleManager, AppDbContext context) : base(roleManager, context)
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
    public IdentityRoleClaim<string> claim { get; set; }

    public async Task<IActionResult> OnGetAsync(int? claimid)
    {
      if (claimid == null)
        return NotFound("Không tìm thấy claim");

      claim = _context.RoleClaims.Where(c => c.Id == claimid).FirstOrDefault();

      if (claim == null)
        return NotFound("Không tìm thấy claim");

      role = await _roleManager.FindByIdAsync(claim.RoleId);
      if (role == null)
        return NotFound("Không tìm thấy role");

      Input = new InputModel
      {
        ClaimType = claim.ClaimType,
        ClaimValue = claim.ClaimValue
      };

      return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? claimid)
    {
      if (claimid == null)
        return NotFound("Không tìm thấy claim");

      claim = _context.RoleClaims.Where(c => c.Id == claimid).FirstOrDefault();

      if (claim == null)
        return NotFound("Không tìm thấy claim");

      role = await _roleManager.FindByIdAsync(claim.RoleId);
      if (role == null)
        return NotFound("Không tìm thấy role");

      if (!ModelState.IsValid)
      {
        return Page();
      }

      if (_context.RoleClaims.Any(c => c.RoleId == role.Id && c.ClaimType == Input.ClaimType && c.ClaimValue == Input.ClaimValue && c.Id != claim.Id))
      {
        ModelState.AddModelError(string.Empty, "Claim này đã có trong role");
        return Page();
      }

      claim.ClaimType = Input.ClaimType;
      claim.ClaimValue = Input.ClaimValue;

      await _context.SaveChangesAsync();

      StatusMessage = "Vừa cập nhập claim";

      return RedirectToPage("./Edit", new
      {
        roleid = role.Id
      });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int? claimid)
    {
      if (claimid == null)
        return NotFound("Không tìm thấy role");

      claim = _context.RoleClaims.Where(c => c.Id == claimid).FirstOrDefault();

      if (claim == null)
        return NotFound("Không tìm thấy role");

      role = await _roleManager.FindByIdAsync(claim.RoleId);
      if (role == null)
        return NotFound("Không tìm thấy role");

      await _roleManager.RemoveClaimAsync(role, new Claim(claim.ClaimType, claim.ClaimValue));

      StatusMessage = "Vừa xóa claim";

      return RedirectToPage("./Edit", new
      {
        roleid = role.Id
      });
    }
  }
}
