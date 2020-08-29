using System;
using System.Linq;
using Castle.Core.Internal;
using MeshBackend.Helpers;
using MeshBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MeshBackend.Controllers
{
    [ApiController]
    [Route("api/mesh/preference")]
    [Produces("application/json")]
    public class PreferenceController:Controller
    {
        private readonly ILogger<ProjectController> _logger;
        private readonly MeshContext _meshContext;
        
        public PreferenceController(ILogger<ProjectController> logger, MeshContext meshContext)
        {
            _logger = logger;
            _meshContext = meshContext;
        }
        
        public JsonResult CheckUsername(string username)
        {
            if (username.IsNullOrEmpty() || username.Length > 50)
            {
                return JsonReturnHelper.ErrorReturn(104, "Invalid username.");
            }
            return HttpContext.Session.GetString(username) == null ? JsonReturnHelper.ErrorReturn(2, "User status error.") : null;
        }

        [HttpPost]
        [Route("color")]
        public JsonResult PreferenceColor(string username, string preferenceColor)
        {
            var checkResult = CheckUsername(username);
            if (checkResult != null)
            {
                return checkResult;
            }

            var user = _meshContext.Users.First(u => u.Email == username);
            try
            {
                user.ColorPreference = preferenceColor;
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

        [HttpPost]
        [Route("layout")]
        public JsonResult PreferenceLayout(string username, string preferenceLayout)
        {
            var checkResult = CheckUsername(username);
            if (checkResult != null)
            {
                return checkResult;
            }

            var user = _meshContext.Users.First(u => u.Email == username);
            try
            {
                user.LayoutPreference = preferenceLayout;
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

        [HttpPost]
        [Route("show-mode")]
        public JsonResult PreferenceShowMode(string username, string showMode)
        {
            var checkResult = CheckUsername(username);
            if (checkResult != null)
            {
                return checkResult;
            }

            var user = _meshContext.Users.First(u => u.Email == username);
            try
            {
                user.RevealedPreference = showMode;
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
        
    }
}