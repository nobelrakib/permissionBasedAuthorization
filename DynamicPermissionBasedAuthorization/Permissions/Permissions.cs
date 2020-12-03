using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicPermissionBasedAuthorization.Permissions
{
    public static class Permissions
    {
        public const string View = "Permissions.AuthTest.View";
        public const string Create = "Permissions.AuthTest.Create";
        public const string Edit = "Permissions.AuthTest.Edit";
        public const string Delete = "Permissions.AuthTest.Delete";

        public static List<string> All()
        {
            Type t = typeof(Permissions);

            return t.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
            .Select(x => x.GetValue(null).ToString())
            .ToList();
        }
    }
}
