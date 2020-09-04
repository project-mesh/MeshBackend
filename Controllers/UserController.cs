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


        public class RegisterRequest
        {
            public string username { get; set; }
            public string password { get; set; }
        }
        
        public class LoginRequest
        {
            public string username { get; set; }
            public string password { get; set; }
            public string token { get; set; }
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
        public JsonResult Register(RegisterRequest request)
        {
            if (!CornerCaseCheckHelper.Check(request.username,50,CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(104, "Invalid username");
            }

            if (!CornerCaseCheckHelper.Check(request.password, 0, CornerCaseCheckHelper.PassWord))
            {
                return JsonReturnHelper.ErrorReturn(111, "Invalid password.");
            }
            
            var user = _meshContext.Users.FirstOrDefault(u => u.Email == request.username);
            if (user != null)
            {
                return JsonReturnHelper.ErrorReturn(101, "User already exists.");
            }
            HashPassword hashPassword = GetHashPassword(request.password);
            //Create new user
            var newUser = new User()
            {
                Email = request.username,
                Nickname = request.username,
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
                    username = request.username,
                    role = "user",
                },
            });

        }

        
        [HttpPost]
        [Route("login")]
        public JsonResult Login(LoginRequest request)
        {
            if (!CornerCaseCheckHelper.Check(request.username,50,CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(104, "Invalid username");
            }
            
            if (!CornerCaseCheckHelper.Check(request.password, 0, CornerCaseCheckHelper.PassWord))
            {
                return JsonReturnHelper.ErrorReturn(111, "Invalid password.");
            }

            var user = _meshContext.Users.FirstOrDefault(u => u.Email == request.username);
            
            //Check Password
            if (user == null|| !CheckHashPassword(request.password, user.PasswordSalt, user.PasswordDigest))
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

            var preferenceTeamId = -1;
            if (cooperation.FirstOrDefault() != null)
            {
                var preferenceTeamCount = cooperation.DefaultIfEmpty().Max(a => a.AccessCount);
                preferenceTeamId = cooperation.First(c => c.AccessCount == preferenceTeamCount).TeamId;
            }


            return Json(new
            {
                err_code = 0,
                data = new
                {
                    isSuccess = true,
                    username = request.username,
                    role = "user",
                    token = "",
                    avatar = user.Avatar,
                    preference = new
                    {
                      preferenceShowMode = user.RevealedPreference,
                      preferenceColor = user.ColorPreference,
                      preferenceLayout = user.LayoutPreference,
                      preferenceTeam = preferenceTeamId
                    },
                    teams = teams
                },
            });
        }


    }
}