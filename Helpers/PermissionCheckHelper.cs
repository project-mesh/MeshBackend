using System.Linq;
using MeshBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MeshBackend.Helpers
{
    public class PermissionCheckHelper:Controller
    {
        private readonly MeshContext _meshContext;
        public const int ProjectOutsider = 0;
        public const int ProjectMember = 1;
        public const int ProjectAdmin = 2;
        public const int TeamOutsider = 0;
        public const int TeamMember = 1;
        public const int TeamAdmin = 2;


        public PermissionCheckHelper(MeshContext meshContext)
        {
            _meshContext = meshContext;
        }
        
        public JsonResult CheckUsername(string username)
        {
            if (username == null || username.Length > 50)
            {
                return JsonReturnHelper.ErrorReturn(104, "Invalid username.");
            }
            if (HttpContext.Session.GetString(username) == null)
            {
                return JsonReturnHelper.ErrorReturn(2, "User status error.");
            }

            return null;
        }
        
        public int CheckProjectPermission(string username, Project project)
        {
            var user = _meshContext.Users.First(u => u.Nickname == username);
            var develop = _meshContext.Develops.FirstOrDefault(d => d.ProjectId == project.Id && d.UserId == user.Id);
            if (develop == null)
            {
                return ProjectOutsider;
            }

            return user.Id == project.AdminId ? ProjectAdmin : ProjectMember;
        }
        
        public int CheckTeamPermission(string username, Team team)
        {
            var user = _meshContext.Users.First(u => u.Nickname == username);
            var cooperation = _meshContext.Cooperations.FirstOrDefault(c => c.TeamId == team.Id && c.UserId == user.Id);
            if (cooperation == null)
            {
                return TeamOutsider;
            }

            return user.Id == team.AdminId ? TeamAdmin : TeamMember;
        }

    }
}