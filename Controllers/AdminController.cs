using System;
using System.Linq;
using MeshBackend.Helpers;
using MeshBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MeshBackend.Controllers
{
    
    [ApiController]
    [Route("api/mesh/admin")]
    [Produces("application/json")]
    public class AdminController:Controller
    {
        private readonly MeshContext _meshContext;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public AdminController(MeshContext meshContext, ILogger logger,IConfiguration configuration)
        {
            _meshContext = meshContext;
            _logger = logger;
            _configuration = configuration;
        }

        public class AdminRequest
        {
            public string username { get; set; }
            public string password { get; set; }
        }

        
        
        [HttpPatch]
        [Route("password")]
        public JsonResult UpdateUserPasswordAdmin(AdminRequest request)
        {
            if (!CornerCaseCheckHelper.Check(request.username, 50, CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(104, "Invalid username.");
            }
            
            var user = _meshContext.Users.FirstOrDefault(u => u.Email == request.username);
            if (user == null)
            {
                return JsonReturnHelper.ErrorReturn(130, "User does not exist.");
            }
             
            PasswordCheckHelper.HashPassword newPassword = PasswordCheckHelper.GetHashPassword(request.password);

            try
            {
                user.PasswordDigest = newPassword.PasswordDigest;
                user.PasswordSalt = newPassword.PasswordSalt;
                _meshContext.Users.Update(user);
                _meshContext.SaveChanges();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return JsonReturnHelper.ErrorReturn(2, "Unexpected error.");
            }

            return JsonReturnHelper.SuccessReturn();

        }

        [HttpPatch]
        [Route("information")]
        public JsonResult UpdateUserInformationAdmin(UserInfo request)
        {
            if (HttpContext.Session.GetString(_configuration["AdminInformation:Username"])==null)
            {
                return JsonReturnHelper.ErrorReturn(2, "Admin status error.");
            }
            
            if (!CornerCaseCheckHelper.Check(request.username,50,CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(104, "Invalid username");
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
                _meshContext.Users.Update(user);
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
                    msg = "",
                    username = user.Email,
                    nickname = user.Nickname,
                    gender = user.Gender,
                    status = user.Status,
                    description = user.Description,
                    birthday = user.Birthday.ToLongDateString(),
                }
            });
        }
        
    }
}