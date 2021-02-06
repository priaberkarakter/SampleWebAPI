using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DenBagus
{
    public static class ExtensionMethods
    {
        public static string GetExceptionMessages(this Exception e, string msgs = "")
        {
            if (e == null) return string.Empty;
            if (msgs == "") msgs = e.Message;
            if (e.InnerException != null)
                msgs += "\r\nInnerException: " + GetExceptionMessages(e.InnerException);
            return msgs;
        }

        public static string GetFullName(this ClaimsPrincipal user)
        {
            var claims = user.FindFirst(ClaimTypes.NameIdentifier).Subject.Claims;

            var names = claims.Where(x => x.Type == "name");
            return (names.Count() > 0) ? names.FirstOrDefault().Value : "";
        }

        public static string GetCompanyName(this ClaimsPrincipal user)
        {
            var claims = user.FindFirst(ClaimTypes.NameIdentifier).Subject.Claims;

            var companies = claims.Where(x => x.Type == "company");
            return (companies.Count() > 0) ? companies.FirstOrDefault().Value : "";
        }
        public static string GetEmailAddress(this ClaimsPrincipal user)
        {
            var claims = user.FindFirst(ClaimTypes.NameIdentifier).Subject.Claims;

            const string emailType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";
            var emails = claims.Where(x => x.Type == emailType);
            return (emails.Count() > 0) ? emails.FirstOrDefault().Value : "";
        }

        public static string GetUsername(this ClaimsPrincipal user)
        {
            var claims = user.FindFirst(ClaimTypes.NameIdentifier).Subject.Claims;

            var emails = claims.Where(x => x.Type == "preferred_username");
            return (emails.Count() > 0) ? emails.FirstOrDefault().Value : "";
        }
    }
}
