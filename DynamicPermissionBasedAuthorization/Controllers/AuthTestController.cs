using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DynamicPermissionBasedAuthorization.Controllers
{
    public class AuthTestController : Controller
    {
        

        [Authorize(Policy=Permissions.Permissions.View)]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Policy=Permissions.Permissions.Create)]
        public ActionResult Create()
        {
            return View();
        }

        [Authorize(Policy=Permissions.Permissions.Edit)]
        public ActionResult Edit()
        {
            return View();
        }

        [Authorize(Policy=Permissions.Permissions.Delete)]
        public ActionResult Delete(int id)
        {
            return View();
        }
    }
}
