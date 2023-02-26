
#nullable disable

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RazorWeb.Models;

namespace App.Admin.User
{
  public class AddRoleModel : PageModel
  {
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AddRoleModel(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        RoleManager<IdentityRole> roleManager)
    {
      _userManager = userManager;
      _signInManager = signInManager;
      _roleManager = roleManager;
    }

    [TempData]
    public string StatusMessage { get; set; }
    public AppUser user { get; set; }

    [BindProperty]
    [DisplayName("Các role gán cho user")]
    public string[] RoleNames { get; set; }
    public SelectList allRoles { get; set; }

    public async Task<IActionResult> OnGetAsync(string id)
    {
      if (string.IsNullOrEmpty(id))
      {
        return NotFound($"Không có user.");
      }
      user = await _userManager.FindByIdAsync(id);
      if (user == null)
      {
        return NotFound($"Không thấy user, ID =  {id}");
      }

      RoleNames = (await _userManager.GetRolesAsync(user)).ToArray<string>();

      List<string> roleNames = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
      allRoles = new SelectList(roleNames);

      return Page();
    }

    public async Task<IActionResult> OnPostAsync(string id)
    {
      if (string.IsNullOrEmpty(id))
      {
        return NotFound($"Không có user.");
      }
      user = await _userManager.FindByIdAsync(id);
      if (user == null)
      {
        return NotFound($"Không thấy user, ID =  {id}");
      }

      var oldRoleNames = (await _userManager.GetRolesAsync(user)).ToArray();
      var deleteRoles = oldRoleNames.Where(r => !RoleNames.Contains(r));
      var addRoles = RoleNames.Where(r => !oldRoleNames.Contains(r));

      List<string> roleNames = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
      allRoles = new SelectList(roleNames);

      var resultDelete = await _userManager.RemoveFromRolesAsync(user, deleteRoles);
      if (!resultDelete.Succeeded)
      {
        foreach (var error in resultDelete.Errors)
        {
          ModelState.AddModelError(string.Empty, error.Description);
        }
        return Page();
      }

      var resultAdd = await _userManager.AddToRolesAsync(user, addRoles);
      if (!resultAdd.Succeeded)
      {
        foreach (var error in resultAdd.Errors)
        {
          ModelState.AddModelError(string.Empty, error.Description);
        }
        return Page();
      }

      StatusMessage = $"Vừa cập nhập role cho user {user.UserName}.";

      return RedirectToPage("./Index");
    }
  }
}
