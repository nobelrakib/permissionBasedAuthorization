using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicPermissionBasedAuthorization.Models
{
    public class NotificationModel
    {
        public string TypeCssClass { get; private set; }
        public string SignCssClass { get; private set; }
        public string Message { get; set; }
        public string HeaderText { get; set; }

        public NotificationModel(string headerText, string message, NotificationType type)
        {
            Message = message;
            HeaderText = headerText;
            TypeCssClass = type == NotificationType.Success ? "success" : "danger";
            SignCssClass = type == NotificationType.Success ? "check" : "ban";
        }
        public enum NotificationType
        {
            Fail,
            Success
        }
    }
}
