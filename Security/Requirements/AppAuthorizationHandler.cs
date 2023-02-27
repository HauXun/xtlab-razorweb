using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using RazorWeb.Models;

namespace App.Security.Requirements;

public class AppAuthorizationHandler : IAuthorizationHandler
{
  private readonly ILogger<AppAuthorizationHandler> _logger;
  private readonly UserManager<AppUser> _userManager;

  public AppAuthorizationHandler(ILogger<AppAuthorizationHandler> logger, UserManager<AppUser> userManager)
  {
    _logger = logger;
    _userManager = userManager;
  }
  public Task HandleAsync(AuthorizationHandlerContext context)
  {
    var requirements = context.PendingRequirements.ToList();
    foreach (var requirement in requirements)
    {
      if (requirement is GenZRequirement)
      {
        if (IsGenZ(context.User, (GenZRequirement)requirement))
        {
          context.Succeed(requirement);
        }
      }

      if (requirement is ArticleUpdateRequirement)
      {
        bool canUpdate = CanUpdateArticle(context.User, context.Resource, (ArticleUpdateRequirement)requirement);
        if (canUpdate)
        {
          context.Succeed(requirement);
        }
      }
    }

    return Task.CompletedTask;
  }

  private bool CanUpdateArticle(ClaimsPrincipal user, object? resource, ArticleUpdateRequirement requirement)
  {
    if (user.IsInRole("Administrator"))
    {
      _logger.LogInformation("Administrator's allowed to update ...");

      return true;
    }

    var article = resource as Article;
    var dateCreated = article.Created;
    var dateCanUpdate = new DateTime(requirement.Year, requirement.Month, requirement.Date);

    if (dateCreated < dateCanUpdate)
    {
      _logger.LogInformation("Quá ngày cập nhập");
      return false;
    }

    return true;
  }

  private bool IsGenZ(ClaimsPrincipal user, GenZRequirement requirement)
  {
    var appUserTask = _userManager.GetUserAsync(user);
    Task.WaitAll(appUserTask);
    var appUser = appUserTask.Result;

    if (appUser.BirthDate == null)
    {
      _logger.LogInformation($"{appUser.UserName} không có ngày sinh, không thõa mãn GenZRequirement");
      return false;
    }

    int year = appUser.BirthDate.Value.Year;

    var success = (year >= requirement.FromYear && year <= requirement.ToYear);

    if (success)
    {
      _logger.LogInformation($"{appUser.UserName} thõa mãn GenZRequirement");
    }
    else
    {
      _logger.LogInformation($"{appUser.UserName} không thõa mãn GenZRequirement");
    }

    return success;
  }
}