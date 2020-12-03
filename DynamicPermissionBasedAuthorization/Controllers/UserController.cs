using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using DynamicPermissionBasedAuthorization.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace DynamicPermissionBasedAuthorization.Controllers
{
    
    public class UserController : Controller
    {
        public UserManager<IdentityUser> userManager;
        public UserController(UserManager<IdentityUser> userManager)
        {
            this.userManager = userManager;
        }
        public IActionResult Index()
        {
            var model = new UserViewModel();
            return View(model);
        }

        
        public IActionResult GetUsers()
        {
            var tableModel = new DataTablesAjaxRequestModel(Request);
            var model = new UserViewModel();
            var data = model.GetUsers(tableModel);
            return Json(data);
        }

        [HttpGet]
        public IActionResult Add(string returnUrl = null)
        {
            var model = new UserUpdateModel();
            model.ReturnUrl = returnUrl;
            model.LoadRoles();
            model.InitiatePermissionContainer();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(UserUpdateModel model)
        {
            model.ReturnUrl = model.ReturnUrl ?? Url.Content("~/");
           
            await model.AddUser();
           // model.InitiatePermissionContainer();
             ViewBag.Message = "Success";

             ViewBag.Message = "Failed to Add User. Please Try Again.";
           
            model.LoadRoles();
            return View(model);
        }
        public async Task<IActionResult> Edit(string id)
        {
            var model = new UserUpdateModel();
            await model.Load(id);
            model.LoadRoles();
            model.InitiatePermissionContainer();
            await model.LoadPermission(id);
           // await model.UpdatePermission(id);
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserUpdateModel model)
        {  
            await model.EditUser();           
            await model.LoadPermission(model.Id);
            await model.RearrangeDictionary();
            model.LoadRoles();
            return View(model);
        }
        public async Task<IActionResult> Delete(string id)
        {
            var model = new UserViewModel();
            await model.Delete(id);
            return LocalRedirect("~/admin/User/Index");
        }
        public async Task<int> CheckPassword(string id,string password)
        {
            if (id == null) id = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await userManager.FindByIdAsync(id);
            var result = await userManager.CheckPasswordAsync(user, password);
            if (result == false) return 0;
            else return 1;
        }
        [HttpPost]
        public async Task<IActionResult> PasswordChange(UserViewModel model)
        {
           
            await model.ChangePassword();
            return LocalRedirect("~/User/Index");
        }
    }
}