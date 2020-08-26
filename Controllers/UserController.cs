using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Castle.Core.Internal;
using MeshBackend.Helpers;
using MeshBackend.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MeshBackend.Controllers
{
    [ApiController]
    [Route("api/mesh")]
    [Produces("application/json")]
    public class UserController:Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly MeshContext _meshContext;
        
        public UserController(ILogger<UserController> logger, MeshContext meshContext)
        {
            _logger = logger;
            _meshContext = meshContext;
        }

        public bool CheckUserSession(string username)
        {
            if (HttpContext.Session.IsAvailable && HttpContext.Session.GetString(username) != null)
            {
                return true;
            }
            if (HttpContext.Session.IsAvailable)
            {
                HttpContext.Session.SetString(username,"");
            }
            return false;
        }

        public HashPassword GetHashPassword(string password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string passwordSalt = Convert.ToBase64String(salt);
            string passwordDigest = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 1000,
                numBytesRequested: 256 / 8
            ));

            return new HashPassword()
            {
                PasswordSalt = passwordSalt,
                PasswordDigest = passwordDigest
            };
        }

        public bool CheckHashPassword(string password, string passwordSalt, string passwordDigest)
        {
            var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: Convert.FromBase64String(passwordSalt),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 1000,
                numBytesRequested: 256 / 8
            ));
            return hashed == passwordDigest;
        }
        
        public class TeamInfo
        { 
            public string TeamName { get; set; }
            public int TeamId { get; set; }
            public int AdminId { get; set; }
        }
        

        public class HashPassword
        {
            public string PasswordSalt { get; set; }
            public string PasswordDigest { get;set; }
        }
        
        [HttpPost]
        [Route("register")]
        public JsonResult Register(string username, string password)
        {
            if (username.IsNullOrEmpty() || username.Length > 50)
            {
                return JsonReturnHelper.ErrorReturn(104, "Invalid username");
            }
            
            var user = _meshContext.Users.FirstOrDefault(u => u.Email == username);
            if (user != null)
            {
                return JsonReturnHelper.ErrorReturn(101, "User already exists.");
            }
            HashPassword hashPassword = GetHashPassword(password);
            //Create new user
            var newUser = new User()
            {
                Email = username,
                Nickname = username,
                PasswordDigest = hashPassword.PasswordDigest,
                PasswordSalt = hashPassword.PasswordSalt
            };
            //try to save the user
            try
            {
                _meshContext.Users.Add(newUser);
                _meshContext.SaveChanges();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return JsonReturnHelper.ErrorReturn(1, "Unexpected error.");
            }

            return Json(new
            {
                err_code = 0,
                data = new
                {
                    isSuccess = true,
                    username = username,
                    role = "user",
                },
            });

        }

        
        [HttpPost]
        [Route("login")]
        public JsonResult Login(string username, string password, string token)
        {
            if (username.IsNullOrEmpty() || username.Length > 50)
            {
                return JsonReturnHelper.ErrorReturn(104, "Invalid username");
            }
            
            var user = _meshContext.Users.FirstOrDefault(u => u.Email == username);
            
            //Check Password
            if (user == null|| !CheckHashPassword(password, user.PasswordSalt, user.PasswordDigest))
            {
                return JsonReturnHelper.ErrorReturn(201, "Incorrect username or password.");
            }

            if (CheckUserSession(user.Email))
            {
                return JsonReturnHelper.ErrorReturn(203, "User has already logged in.");
            }
                
            //Find teams of the user
            var cooperation = _meshContext.Cooperations
                .Where(b => b.UserId == user.Id);
            var teams = _meshContext.Teams
                .Join(cooperation, t => t.Id, c => c.TeamId, (t, c) =>
                    new TeamInfo
                    {
                        TeamId = t.Id,
                        TeamName = t.Name,
                        AdminId = t.AdminId
                    }).ToList();
                    
            return Json(new
            {
                err_code = 0,
                data = new
                {
                    isSuccess = true,
                    username = username,
                    role = "user",
                    token = "",
                    teams = teams
                },
            });
        }


    }
}