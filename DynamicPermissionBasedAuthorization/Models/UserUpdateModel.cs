
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using DynamicPermissionBasedAuthorization.CustomClaimsType;

namespace DynamicPermissionBasedAuthorization.Models
{
    public class UserUpdateModel : BaseModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        
        public Dictionary<string, bool> PermissionContainer { get; set; } = new Dictionary<string, bool>();
        //public  string View = "Permissions.AuthTest.View";
        //public  string Create = "Permissions.AuthTest.Create";
        //public  string Edit = "Permissions.AuthTest.Edit";
        //public  string Delete = "Permissions.AuthTest.Delete";
       // public List<string> Permission { get; set; } = Permissions.Permissions.All();
        public UserUpdateModel()
        {
            _httpContextAccessor = Startup.container.GetRequiredService<IHttpContextAccessor>();
            _roleManager = Startup.container.GetRequiredService<RoleManager<IdentityRole>>();
            _userManager = Startup.container.GetRequiredService<UserManager<IdentityUser>>();
        }

        public UserUpdateModel(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public string Id { get; set; }
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        
        


        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        private IHttpContextAccessor _httpContextAccessor;
        public List<SelectListItem> Roles { get; set; }

        public string ReturnUrl { get; set; }
        public IList<string> ErrorMessage { get; set; }

        public async Task<IdentityResult> AddUser()
        {

            try
            {
                var user = new IdentityUser
                {
                    
                    UserName = this.UserName,
                    Email = this.Email,
                    PhoneNumber = this.PhoneNumber
                };
                var result = await _userManager.CreateAsync(user, this.Password);
                if (result.Succeeded)
                {
                    var role = await _roleManager.FindByIdAsync(this.Role);
                    var roleResult = await _userManager.AddToRoleAsync(user, role.Name);
                    if (roleResult.Succeeded)
                    {
                        var roleClaims = await _roleManager.GetClaimsAsync(role);
                        foreach (var claim in roleClaims)
                        {
                            await _roleManager.RemoveClaimAsync(role,claim );
                        }
                        foreach (var permission in PermissionContainer)
                        {
                            await _roleManager.AddClaimAsync(role, new Claim(CustomClaimsTypes.Permission, permission.Key));
                        }
                        foreach(var permission in Permissions.Permissions.All())
                        {
                            if (!PermissionContainer.ContainsKey(permission)) PermissionContainer.Add(permission, false);
                        }
                       // await _roleManager.AddClaimAsync(adminRole, new Claim(CustomClaimsTypes.Permission, Permissions.Permissions.Create));
                        Notification = new NotificationModel("Success !!", "Successfully Added User", NotificationModel.NotificationType.Success);
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                Notification = new NotificationModel("Failed !!", "Failed to Add User", NotificationModel.NotificationType.Fail);
                throw e;
            }
            Notification = new NotificationModel("Failed !!", "Already registered", NotificationModel.NotificationType.Fail);
            return null;
        }

        public void InitiatePermissionContainer()
        {           
            var permissions = Permissions.Permissions.All();           
            foreach (var permission in Permissions.Permissions.All())
            {
                PermissionContainer.Add(permission,false );
            }
            PermissionContainer.OrderBy(k => k.Key);
        }

       

        public async Task RearrangeDictionary()
        {
            await Task.Run(() =>
            {
                var dictionary = new Dictionary<string, bool>();
                foreach (var permission in Permissions.Permissions.All())
                {
                    dictionary.Add(permission, false);
                }
                foreach (var permission in PermissionContainer)
                {
                    if (permission.Value) dictionary[permission.Key] = true;
                }
                PermissionContainer = dictionary;
            });
            
        }

        public async Task LoadPermission(string id)
        {
           // var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            var user = await _userManager.FindByIdAsync(id);
            var userRoleNames = await _userManager.GetRolesAsync(user);
            var userRoles = await _roleManager.Roles.Where(x => userRoleNames.Contains(x.Name)).ToListAsync();
            foreach (var role in userRoles)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(role);
                var permissionsofRole = roleClaims.Where(x => x.Type == CustomClaimsType.CustomClaimsTypes.Permission)
                                            .Select(x => x.Value);
                foreach (var permission in permissionsofRole)
                {
                    PermissionContainer[permission] = true;
                }
                PermissionContainer.OrderBy(k => k.Key);

            }
           
        }
        public async Task EditUser()
        {
            try
            {
                var user = await _userManager.FindByIdAsync(Id);
                user.Email = this.Email;
                
                user.UserName = this.UserName;
             //   user.PhoneNumber = this.PhoneNumber;
                
                IdentityResult result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    var roles =await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, roles);
                    var roleResult = await _userManager.AddToRoleAsync(user, Role);
                    if (roleResult.Succeeded)
                    {
                        var role = await _roleManager.FindByNameAsync(this.Role);
                        var roleClaims = await _roleManager.GetClaimsAsync(role);
                        foreach (var claim in roleClaims)
                        {
                            await _roleManager.RemoveClaimAsync(role, claim);
                        }
                        foreach (var permission in PermissionContainer)
                        {
                            await _roleManager.AddClaimAsync(role, new Claim(CustomClaimsTypes.Permission, permission.Key));
                        }
                        foreach (var permission in Permissions.Permissions.All())
                        {
                            if (!PermissionContainer.ContainsKey(permission)) PermissionContainer.Add(permission, false);
                        }
                        Notification = new NotificationModel("Success !!", "Successfully Edited User", NotificationModel.NotificationType.Success);
                        PermissionContainer.OrderBy(k => k.Key);
                    }
                }
            }
            catch (Exception e)
            {
                Notification = new NotificationModel("Failed !!", "Failed to Add User", NotificationModel.NotificationType.Fail);
                throw e;
            }
           
           
        }
       
        public void LoadRoles()
        {
            Roles = (from r in _roleManager.Roles
                     select new SelectListItem
                     {
                         Text = r.Name,
                         Value = r.Id
                     }).ToList();
        }
        public async Task Load(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var roles = _roleManager.Roles.ToList();
            foreach(var role in roles)
            {
                if(await _userManager.IsInRoleAsync(user,role.Name))
                { 
                    Role = role.Name;
                }
            }
            if (user != null)
            {
                Id = user.Id;
                UserName = user.UserName;
                Email = user.Email;
                PhoneNumber = user.PhoneNumber;
                
            }
        }

    }
   
}
