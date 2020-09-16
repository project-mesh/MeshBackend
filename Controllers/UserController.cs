using System;
using System.Linq;
using System.Net;
using MeshBackend.Helpers;
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
        
        
        public class UserRequest
        {
            public string username { get; set; }
            public string password { get; set; }
            public string token { get; set; }
            public string oldPassword { get; set; }
        }


        public JsonResult UserReturnValue(User user)
        {
            //Find teams of the user
            var cooperation = _meshContext.Cooperations
                .Where(b => b.UserId == user.Id);
            var teams = _meshContext.Teams
                .Join(cooperation, t => t.Id, c => c.TeamId, (t, c) =>
                    new TeamInfo
                    {
                        TeamId = t.Id,
                        TeamName = t.Name,
                        AdminId = t.AdminId,
                        CreateTIme = t.CreatedTime.ToString(),
                        AdminName = _meshContext.Users.First(u=>u.Id==t.AdminId).Nickname
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
                    msg = "",
                    username = user.Email,
                    nickname = user.Nickname,
                    gender = user.Gender,
                    status = user.Status,
                    description = user.Description,
                    birthday = user.Birthday.ToString("yyyy-MM-dd"),
                    avatar = AvatarSaveHelper.GetObject(user.Avatar),
                    role = "user",
                    preference = new UserPreference()
                    {
                        preferenceColor = user.ColorPreference,
                        preferenceLayout = user.LayoutPreference,
                        preferenceShowMode = user.RevealedPreference,
                        preferenceTeam = preferenceTeamId
                    },

                    teams = teams
                }
            });
        }

        public JsonResult AdminReturnValue(Admin admin)
        {
            return Json(new
            {
                err_code = 0,
                data = new
                {
                    isSuccess = true,
                    msg = "",
                    username = admin.Email,
                    nickname = admin.Nickname,
                    role = "admin"
                }
            });
        }

        public bool CheckUserSession(string username)
        {
            if (HttpContext.Session.IsAvailable && HttpContext.Session.GetString(username) != null)
            {
                return true;
            }
            if (HttpContext.Session.IsAvailable)
            {
                HttpContext.Session.SetString(username,Guid.NewGuid().ToString());
            }
            return false;
        }


        
        [HttpPost]
        [Route("register")]
        public JsonResult Register(UserRequest request)
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
            var admin = _meshContext.Admins.FirstOrDefault(u => u.Email == request.username);
            if (user != null || admin != null)
            {
                return JsonReturnHelper.ErrorReturn(101, "User already exists.");
            }

            PasswordCheckHelper.HashPassword hashPassword = PasswordCheckHelper.GetHashPassword(request.password);
            //Create new user
            var newUser = new User()
            {
                Email = request.username,
                Nickname = request.username,
                PasswordDigest = hashPassword.PasswordDigest,
                PasswordSalt = hashPassword.PasswordSalt,
                Avatar = AvatarSaveHelper.PutObject(""),
                Birthday = Convert.ToDateTime("2020-01-01"),
                ColorPreference = "blue",
                LayoutPreference = "default",
                RevealedPreference = "card"
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

            return UserReturnValue(newUser);

        }

        [HttpPost]
        [Route("login")]
        public JsonResult Login(UserRequest request)
        {
            if (!CornerCaseCheckHelper.Check(request.username,50,CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(104, "Invalid username");
            }
            
            if (!CornerCaseCheckHelper.Check(request.password, 0, CornerCaseCheckHelper.PassWord))
            {
                return JsonReturnHelper.ErrorReturn(111, "Invalid password.");
            }

            var admin = _meshContext.Admins.FirstOrDefault(u => u.Email == request.username);
            if (admin != null &&
                PasswordCheckHelper.CheckHashPassword(request.password, admin.PasswordSalt, admin.PasswordDigest))
            {
                HttpContext.Session.SetString(request.username, Guid.NewGuid().ToString());
                return AdminReturnValue(admin);
            }

            var user = _meshContext.Users.FirstOrDefault(u => u.Email == request.username);

            //Check Password
            if (user == null|| !PasswordCheckHelper.CheckHashPassword(request.password, user.PasswordSalt, user.PasswordDigest))
            {
                return JsonReturnHelper.ErrorReturn(201, "Incorrect username or password.");
            }

            if (HttpContext.Session.IsAvailable && HttpContext.Session.GetString(request.username) == null)
            {
                HttpContext.Session.SetString(request.username,Guid.NewGuid().ToString());
            }
            
            return UserReturnValue(user);
        }

        [HttpPut]
        [Route("user/password")]
        public JsonResult UpdateUserPassword(UserRequest request)
        {
            if (!CornerCaseCheckHelper.Check(request.username,50,CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(104, "Invalid username");
            }
            
            if (!CornerCaseCheckHelper.Check(request.oldPassword, 0, CornerCaseCheckHelper.PassWord))
            {
                return JsonReturnHelper.ErrorReturn(112, "Invalid oldPassword.");
            }
            
            if (!CornerCaseCheckHelper.Check(request.password, 0, CornerCaseCheckHelper.PassWord))
            {
                return JsonReturnHelper.ErrorReturn(111, "Invalid password.");
            }

            if (request.password == request.oldPassword)
            {
                return JsonReturnHelper.ErrorReturn(113, "The old and new passwords are the same.");
            }

            var user = _meshContext.Users.FirstOrDefault(u => u.Email == request.username);
            
            //Check Password
            if (user == null|| !PasswordCheckHelper.CheckHashPassword(request.oldPassword, user.PasswordSalt, user.PasswordDigest))
            {
                return JsonReturnHelper.ErrorReturn(201, "Incorrect username or password.");
            }
            PasswordCheckHelper.HashPassword hashPassword = PasswordCheckHelper.GetHashPassword(request.password);
            try
            {
                user.PasswordDigest = hashPassword.PasswordDigest;
                user.PasswordSalt = hashPassword.PasswordSalt;
                _meshContext.Users.Update(user);
                _meshContext.SaveChanges();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return JsonReturnHelper.ErrorReturn(1, "Unexpected error.");
            }
            
            return JsonReturnHelper.SuccessReturn();
        }

        [HttpPatch]
        [Route("user")]
        public JsonResult UpdateUserInformation(UserInfo request)
        { 
            if (!CornerCaseCheckHelper.Check(request.username,50,CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(104, "Invalid username");
            }

            if (HttpContext.Session.GetString(request.username) == null)
            {
                return JsonReturnHelper.ErrorReturn(2, "User status error.");
            }

            if (!CornerCaseCheckHelper.Check(request.nickname, 50, CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(120, "Invalid nickname.");
            }

            if (!CornerCaseCheckHelper.Check(request.birthday, 0, CornerCaseCheckHelper.Time))
            {
                return JsonReturnHelper.ErrorReturn(121, "Invalid birthday.");
            }

            if (!CornerCaseCheckHelper.Check(request.description, 100, CornerCaseCheckHelper.Description))
            {
                return JsonReturnHelper.ErrorReturn(122, "Invalid description.");
            }

            var user = _meshContext.Users.First(u => u.Email == request.username);

            try
            {
                user.Nickname = request.nickname;
                user.Gender = request.gender;
                user.Status = request.status;
                user.Address = request.address;
                user.Description = request.description;
                user.Birthday = Convert.ToDateTime(request.birthday);
                user.Avatar = AvatarSaveHelper.PutObject(request.avatar,user.Avatar);
                _meshContext.Users.Update(user);
                _meshContext.SaveChanges();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return JsonReturnHelper.ErrorReturn(1, "Unexpected error.");
            }
            
            return UserReturnValue(user);

        }

        [HttpGet]
        [Route("user")]
        public JsonResult QueryUser(string username, string keyword)
        {
            if (!CornerCaseCheckHelper.Check(username, 50, CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(104, "Invalid username.");
            }

            if (!CornerCaseCheckHelper.Check(keyword, 50, CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(457, "Invalid keyword.");
            }

            if (HttpContext.Session.IsAvailable && HttpContext.Session.GetString(username) == null)
            {
                return JsonReturnHelper.ErrorReturn(2, "User status error.");
            }

            var userAccordingToUsername = _meshContext.Users
                .Where(u => u.Email.Contains(keyword));
            var userAccordingToNickname = _meshContext.Users
                .Where(u => u.Nickname.Contains(keyword));
            var userAccordingToDescription = _meshContext.Users
                .Where(u => u.Description.Contains(keyword));

            var users = userAccordingToUsername
                .Union(userAccordingToNickname)
                .Union(userAccordingToDescription)
                .Select(u => new
                {
                    username = u.Email,
                    avatar = AvatarSaveHelper.GetObject(u.Avatar),
                    nickname = u.Nickname,
                    gender = u.Gender,
                    status = u.Status,
                    address = u.Address,
                    description = u.Description,
                    birthday = u.Birthday
                })
                .ToList();


            return Json(new
            {
                err_code = 0,
                data = new
                {
                    users = users
                }
            });
        }
    }
}