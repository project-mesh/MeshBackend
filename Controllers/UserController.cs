using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using MeshBackend.Models;
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
            if (HttpContext.Session.IsAvailable)
            {
                if (HttpContext.Session.GetString(id.ToString()) == null)
                {
                    HttpContext.Session.SetString(id.ToString(),"");
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        
        public class TeamInfo
        { 
            public string TeamName { get; set; }
            public int TeamId { get; set; }
            public int AdminId { get; set; }
        }

        [HttpPost]
        [Route("register")]
        public JsonResult Register(string username, string password)
        {
            var newUser = new User()
            {
                Email = username,
                Nickname = username,
                PasswordDigest = password
            };
            var user = _meshContext.Users.FirstOrDefault(u => u.Email == username);
            if (user == null)
            {
                try
                {
                    _meshContext.Users.Add(newUser);
                    _meshContext.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return Json(new
                    {
                        err_code = 1,
                        data = new
                        {
                            isSuccess = false,
                            msg = "Unexpected error."
                        }
                    });
                }

                return Json(new
                {
                    err_code = 0,
                    data = new
                    {
                        isSuccess = true,
                        msg = "",
                        username = username,
                        role = "user",
                        teams = new List<TeamInfo>()
                    },
                });
            }
            else
            {
                return Json(new
                {
                    err_code = 101,
                    data = new
                    {
                        isSuccess = false,
                        msg = "User already exists."
                    }
                });
            }
        }

        
        [HttpPost]
        [Route("login")]
        public JsonResult Login(string username, string password, string token)
        {
            
            var user = _meshContext.Users.FirstOrDefault(u => u.Email == username);
            if (user != null)
            {
                if (user.PasswordDigest == password)
                {
                    if (CheckUserSession(user.Id))
                    {
                        return Json(new
                        {
                            err_code = 203,
                            data = new
                            {
                                isSuccess = false,
                                msg = "User has already logged in."
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
                            });
                    
                    return Json(new
                    {
                        err_code = 0,
                        data = new
                        {
                            isSuccess = true,
                            msg = "",
                            username = username,
                            role = "user",
                            token = "",
                            teams = teams
                        },
                    });
                }
                else
                {
                    return Json(new
                    {
                        err_code = 201,
                        data = new
                        {
                            isSuccess = false,
                            msg = "Invalid password."
                        }
                    });
                }
            }
            else
            {
                return Json(new
                {
                    err_code = 202,
                    data = new
                    {
                        isSuccess = false,
                        msg = "User does not exist."
                    }
                });
            }
        }


    }
}