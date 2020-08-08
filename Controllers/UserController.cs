using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
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

        public bool CheckUserSession(int id)
        {
            if (HttpContext.Session.IsAvailable && HttpContext.Session.GetString(id.ToString()) != null)
            {
                return true;
            }
            else
            {
                if (HttpContext.Session.IsAvailable)
                {
                    HttpContext.Session.SetString(id.ToString(),"");
                }
                return false;
            }
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

        public class Data
        {
            public bool IsSuccess { get; set; }
            public string Msg { get; set; }
            public string Username { get; set; }
            public string Role { get; set; }
            public string Token { get; set; }
            public List<TeamInfo> Teams { get; set; }
        }

        public class ReturnValue
        {
            public int err_code { get; set; }
            public Data Data { get; set; }
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
            if (username==null || username.Length > 50)
            {
                return Json(new ReturnValue()
                    {
                        err_code = 104,
                        Data = new Data()
                        {
                            IsSuccess = false, 
                            Msg = "Invalid username."
                        }
                    }
                );
            }
            
            var user = _meshContext.Users.FirstOrDefault(u => u.Email == username);
            if (user == null)
            {
                HashPassword hashPassword = GetHashPassword(password);
                var newUser = new User()
                {
                    Email = username,
                    Nickname = username,
                    PasswordDigest = hashPassword.PasswordDigest,
                    PasswordSalt = hashPassword.PasswordSalt
                };
                try
                {
                    _meshContext.Users.Add(newUser);
                    _meshContext.SaveChanges();
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                    return Json(new ReturnValue()
                    {
                        err_code = 1,
                        Data = new Data()
                        {
                            IsSuccess = false,
                            Msg = "Unexpected error."
                        }
                    });
                }

                return Json(new ReturnValue()
                {
                    err_code = 0,
                    Data = new Data()
                    {
                        IsSuccess = true,
                        Username = username,
                        Role = "user",
                    },
                });
            }
            else
            {
                return Json(new ReturnValue()
                {
                    err_code = 101,
                    Data = new Data()
                    {
                        IsSuccess = false,
                        Msg = "User already exists."
                    }
                });
            }
        }

        
        [HttpPost]
        [Route("login")]
        public JsonResult Login(string username, string password, string token)
        {
            if (username == null || username.Length > 50)
            {
                return Json(new ReturnValue()
                    {
                        err_code = 204,
                        Data = new Data()
                        {
                            IsSuccess = false, 
                            Msg = "Invalid username."
                        }
                    }
                );
            }
            
            var user = _meshContext.Users.FirstOrDefault(u => u.Email == username);
            if (user != null && CheckHashPassword(password, user.PasswordSalt,user.PasswordDigest))
            {
                if (CheckUserSession(user.Id))
                {
                    return Json(new ReturnValue()
                    {
                        err_code = 203,
                        Data = new Data()
                        {
                            IsSuccess = false,
                            Msg = "User has already logged in."
                        }
                    });
                }
                    
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
                    
                return Json(new ReturnValue()
                {
                    err_code = 0,
                    Data = new Data()
                    {
                        IsSuccess = true,
                        Msg = "",
                        Username = username,
                        Role = "user",
                        Token = "",
                        Teams = teams
                    },
                });
            }
            else
            {
                return Json(new ReturnValue()
                {
                    err_code = 201,
                    Data = new Data()
                    {
                        IsSuccess = false,
                        Msg = "Incorrect username or password."
                    }
                });
            }
        }


    }
}