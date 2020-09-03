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
            if (!CornerCaseCheckHelper.Check(username,50,CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(104, "Invalid username.");
            }
            return HttpContext.Session.GetString(username) == null ? JsonReturnHelper.ErrorReturn(2, "User status error.") : null;
        }

        public class PreferenceRequest
        {
            public string username { get; set; }
            public string preferenceColor { get; set; }
            public string preferenceLayout { get; set; }
            public string showMode { get; set; }
        }
        
        [HttpPost]
        [Route("color")]
        public JsonResult PreferenceColor(PreferenceRequest request)
        {
            var checkResult = CheckUsername(request.username);
            if (checkResult != null)
            {
                return checkResult;
            }

            if (!CornerCaseCheckHelper.Check(request.preferenceColor, 50, CornerCaseCheckHelper.Title))
            {
                return JsonReturnHelper.ErrorReturn(110, "Invalid preferenceColor");
            }
            

            var user = _meshContext.Users.First(u => u.Email == request.username);
            try
            {
                user.ColorPreference = request.preferenceColor;
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
        public JsonResult PreferenceLayout(PreferenceRequest request)
        {
            var checkResult = CheckUsername(request.username);
            if (checkResult != null)
            {
                return checkResult;
            }

            if (!CornerCaseCheckHelper.Check(request.preferenceLayout, 50, CornerCaseCheckHelper.Title))
            {
                return JsonReturnHelper.ErrorReturn(111, "Invalid preferenceLayout");
            }
            
            var user = _meshContext.Users.First(u => u.Email == request.username);
            try
            {
                user.LayoutPreference = request.preferenceLayout;
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
        public JsonResult PreferenceShowMode(PreferenceRequest request)
        {
            var checkResult = CheckUsername(request.username);
            if (checkResult != null)
            {
                return checkResult;
            }
            
            if (!CornerCaseCheckHelper.Check(request.showMode, 50, CornerCaseCheckHelper.Title))
            {
                return JsonReturnHelper.ErrorReturn(112, "Invalid showMode");
            }

            var user = _meshContext.Users.First(u => u.Email == request.username);
            try
            {
                user.RevealedPreference = request.showMode;
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