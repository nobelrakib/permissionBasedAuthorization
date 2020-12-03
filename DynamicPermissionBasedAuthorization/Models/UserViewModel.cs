
using DynamicPermissionBasedAuthorization.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DynamicPermissionBasedAuthorization.Models
{
    public class UserViewModel:BaseModel
    {
        public string Id { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string OldPassword { get; set; }


        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _db;
        public UserViewModel() 
        {
            _roleManager = Startup.container.GetRequiredService<RoleManager<IdentityRole>>();
            _userManager = Startup.container.GetRequiredService<UserManager<IdentityUser>>();
            _httpContextAccessor = Startup.container.GetRequiredService<IHttpContextAccessor>();
            _db = Startup.container.GetRequiredService<ApplicationDbContext>();
        }

        public object GetUsers(DataTablesAjaxRequestModel tableModel)
        {
            int total = 0;
            int totalFiltered = 0;
            var start = (tableModel.PageIndex - 1) * tableModel.PageSize;
            IEnumerable<IdentityUser> records = null;

            if (string.IsNullOrWhiteSpace(tableModel.SearchText))
                records = _userManager.Users.Skip(start).Take(tableModel.PageSize);
            else
                records = _userManager.Users.Where(x => x.Email.Contains(tableModel.SearchText));
           

            total = _userManager.Users.AsQueryable().Count();
            totalFiltered = _userManager.Users.AsQueryable().Where(x => x.Email.Contains(tableModel.SearchText)).Count();
            
            
            var userRole = _db.UserRoles.ToList();
            var roles = _roleManager.Roles.ToList();
            
            
            return new
            {
                recordsTotal = total,
                recordsFiltered = totalFiltered,
                data = (from record in records
                        select new string[]
                        {
                           // record.Id.ToString(),
                            roles.Where(x => userRole.Any(y => y.RoleId == x.Id && y.UserId == record.Id)).Select(z => z.Name).FirstOrDefault(),
                            record.UserName,
                            record.Email,
                            record.Id.ToString()
                        }
                    ).ToArray()
            };
        }

        public async Task Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            await _userManager.DeleteAsync(user);
        }
        public async Task ChangePassword()
        {
            if(Id==null) Id=_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(Id);
            var result = await _userManager.ChangePasswordAsync(user, OldPassword, Password);
            
        }
    }
}
