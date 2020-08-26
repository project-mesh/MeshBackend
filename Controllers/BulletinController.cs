using System;
using System.Linq;
using Castle.Core.Internal;
using MeshBackend.Helpers;
using MeshBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeshBackend.Controllers
{
    [ApiController]
    [Route("api/mesh/bulletin")]
    [Produces("application/json")]
    public class BulletinController:Controller
    {
        private readonly ILogger<BulletinController> _logger;
        private readonly MeshContext _meshContext;
        private readonly PermissionCheckHelper _permissionCheck;

        public BulletinController(ILogger<BulletinController> logger, MeshContext meshContext)
        {
            _logger = logger;
            _meshContext = meshContext;
            _permissionCheck = new PermissionCheckHelper(meshContext);
        }
        
        public JsonResult CheckUsername(string username)
        {
            if (username.IsNullOrEmpty() || username.Length > 50)
            {
                return JsonReturnHelper.ErrorReturn(104, "Invalid username.");
            }
            return HttpContext.Session.GetString(username) == null ? JsonReturnHelper.ErrorReturn(2, "User status error.") : null;
        }
        
        public JsonResult CheckBulletin(string title, string description)
        {
            if (title == null || title.Length > 50)
            {
                return JsonReturnHelper.ErrorReturn(402, "Invalid bulletinName.");
            }

            if (description != null && description.Length > 100)
            {
                return JsonReturnHelper.ErrorReturn(403, "Description is too long.");
            }

            return null;
        }
        
        
        [HttpGet]
        public JsonResult QueryBulletin(string username, int projectId)
        {
            var checkUsername = CheckUsername(username);
            if (checkUsername != null)
            {
                return checkUsername;
            }
            
            //Find the target project
            var project = _meshContext.Projects.FirstOrDefault(p => p.Id == projectId);
            if (project != null)
            {
                var bulletins = _meshContext.BulletinBoards
                    .Where(b => b.ProjectId == projectId)
                    .Join(_meshContext.Bulletins, bb => bb.Id, b => b.BoardId, (bb, b) => new 
                    {
                        Id = b.Id,
                        Name = b.Title,
                        Content = b.Content,
                        BoardId = b.BoardId,
                        CreatedTime = b.CreatedTime
                    }).ToList();
                return Json(new
                {
                    err_code = 0,
                    data = new
                    {
                        isSuccess = true,
                        msg = "",
                        bulletins = bulletins
                    }
                });
            }
            else
            {
                return JsonReturnHelper.ErrorReturn(401, "Invalid projectId.");
            }

        }

        [HttpPost]
        public JsonResult CreateBulletin(string username, int projectId, string bulletinName, string description)
        {
            var checkUsername = CheckUsername(username);
            if (checkUsername != null)
            {
                return checkUsername;
            }

            var checkBulletin = CheckBulletin(bulletinName, description);
            if (checkBulletin != null)
            {
                return checkBulletin;
            }

            var project = _meshContext.Projects.FirstOrDefault(u => u.Id == projectId);
            if (project == null)
            {
                return JsonReturnHelper.ErrorReturn(401, "Invalid projectId.");
            }

            //Find bulletinBoard of this project
            var bulletinBoard = _meshContext.BulletinBoards.First(b => b.ProjectId == projectId);
            var user = _meshContext.Users.First(u => u.Email == username);
            if (_permissionCheck.CheckProjectPermission(username,project)!=PermissionCheckHelper.ProjectAdmin)
            {
                return JsonReturnHelper.ErrorReturn(421, "Permission denied.");
            }

            //Check if the bulletin already exists.
            var bulletin =
                _meshContext.Bulletins.FirstOrDefault(b => b.Title == bulletinName && b.BoardId == bulletinBoard.Id);
            if (bulletin != null)
            {
                return JsonReturnHelper.ErrorReturn(411, "Bulletin already exists.");
            }

            //Create the bulletin
            var newBulletin = new Bulletin()
            {

                Title = bulletinName,
                Content = description,
                BoardId = bulletinBoard.Id
            };
                
            //Update feed
            var feedUsers = _meshContext.Develops
                .Where(d => d.ProjectId == projectId);

            //Start Transaction to save the bulletin
            using (var transaction = _meshContext.Database.BeginTransaction())
            {
                try
                {

                    _meshContext.Bulletins.Add(newBulletin);
                    _meshContext.SaveChanges();
                    foreach (var feedUser in feedUsers)
                    {
                        _meshContext.BulletinFeeds.Add(new BulletinFeed()
                        {
                            UserId = user.Id,
                            BulletinId = newBulletin.Id
                        });
                    }

                    _meshContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                    return JsonReturnHelper.ErrorReturn(1, "Unexpected error.");
                }
            }

            return Json(new
                {
                    err_code = 0,
                    isSuccess = true,
                    msg = "",
                    bulletin = new
                    {
                        bulletinId = newBulletin.Id,
                        bullentinName = newBulletin.Title,
                        description = newBulletin.Content,
                        createTime = newBulletin.CreatedTime
                    }
                });
        }

        [HttpDelete]
        public JsonResult DeleteBulletin(int bulletinId, string username, int projectId)
        {
            var checkResult = CheckUsername(username);
            if (checkResult != null)
            {
                return checkResult;
            }
            
            //Check if target bulletin exists 
            var user = _meshContext.Users.First(u => u.Email == username);
            var bulletin = _meshContext.Bulletins.Find(bulletinId);
            var project = _meshContext.Projects.Find(projectId);
            if (bulletin == null || project == null)
            {
                return JsonReturnHelper.ErrorReturn(420, "Invalid bulletinId or projectId");
            }

            if (project.AdminId != user.Id)
            {
                return JsonReturnHelper.ErrorReturn(422, "Permission denied.");
            }

            try
            {
                _meshContext.Bulletins.Remove(bulletin);
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
        public JsonResult UpdateBulletin(int bulletinId, string username, int projectId, string bulletinName,
            string description)
        {
            var checkUsername = CheckUsername(username);
            if (checkUsername != null)
            {
                return checkUsername;
            }

            var checkBulletin = CheckBulletin(bulletinName, description);
            if (checkBulletin != null)
            {
                return checkBulletin;
            }

            //Check if target bulletin exists 
            var bulletin = _meshContext.Bulletins.Find(bulletinId);
            var project = _meshContext.Projects.Find(projectId);
            var user = _meshContext.Users.First(u => u.Email == username);
            if (bulletin == null || project == null)
            {
                return JsonReturnHelper.ErrorReturn(420, "Invalid bulletinId or projectId");
            }

            //Check if the user is admin of the project
            if (project.AdminId != user.Id)
            {
                return JsonReturnHelper.ErrorReturn(422, "Permission denied.");
            }

            try
            {
                if (bulletinName != null)
                {
                    bulletin.Title = bulletinName;
                }

                if (description != null)
                {
                    bulletin.Content = description;
                }
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
                masg = "",
                data = new
                {
                    bulletinId = bulletin.Id,
                    bulletinName = bulletin.Title,
                    description = bulletin.Content,
                    createTime = bulletin.CreatedTime
                }
            });
        }

    }
}