using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Web;
using System.ComponentModel;
using Task = System.Threading.Tasks.Task;
using System.Net;
using Microsoft.AspNetCore.Builder;
using HSBors.Models;

namespace HSBors.Services
{
    public class UserService
    {
        private readonly HSBorsDb _context;
        public UserService(HSBorsDb context)
        {
            _context = context;
        }
        public async Task<User> Authenticate(string national_code, string password)
        {
            User user = await _context.Users.FirstOrDefaultAsync(x => x.national_code.ToLower().Equals(national_code.ToLower()));
            if (user == null)
            {
                return null;
            }
            else if (!VerifyPasswordHash(password, user.password_hash, user.password_salt))
            {
                return null;
            }

            UserSession.Id = user.id;
            UserSession.UserData = user;
            return user;


        }
        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public bool Authorization(string action)
        {
            UserSession.SetAccesses(_context);
            bool is_allowed = UserSession.Accesses.Distinct().Contains(action);

            return is_allowed;
        }

        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }

    }

    public class UserSession
    {
        public static long Id { get; set; }
        public static List<string> Accesses { get; set; }
        public static User UserData { get; set; }

        public static void SetAccesses(HSBorsDb _context)
        {
            List<string> all_access = _context.UserRoles.Where(x => x.user_id == Id)
     .Include(x => x.role)
     .Select(x => x.role.accesses).ToList();
            List<string> accesses = new List<string>();
            all_access.ForEach(x => accesses.AddRange(x.Split(",").ToList()));
            Accesses = accesses.Distinct().ToList();

        }
        public static bool GetNonActionAccess(NonActionAccess name)
        {
            return Accesses.Contains(name.ToString());
        }

        public static Dictionary<string, object> Session
        {
            get
            {
                return new Dictionary<string, object>
                {
                    [nameof(Id)] = Id,
                    [nameof(Accesses)] = Accesses,
                    [nameof(UserData)] = new List<User> { UserData }
                        .Select(x => new
                        {
                            x.id,
                            x.first_name,
                            x.last_name,
                            x.create_date,
                            x.full_name,
                            x.mobile,
                            x.national_code
                        }).First()
                };
            }

        }
    }

}
