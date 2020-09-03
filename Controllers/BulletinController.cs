using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using MeshBackend.Helpers;
using MeshBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;
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
            if (!CornerCaseCheckHelper.Check(username,50,CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(104, "Invalid username.");
            }
            return HttpContext.Session.GetString(username) == null ? JsonReturnHelper.ErrorReturn(2, "User status error.") : null;
        }
        
        public JsonResult CheckBulleti(string title, string description)
        {
            if (title.IsNullOrEmpty() || title.Length > 50)
            {
                return JsonReturnHelper.ErrorReturn(402, "Invalid bulletinName.");
            }

            if (description.IsNullOrEmpty() || description.Length > 100)
            {
                return JsonReturnHelper.ErrorReturn(403, "Invalid Description.");
            }

            return null;
        }

        public class BulletinRequest
        {
            public string username { get; set; }
            public int projectId { get; set; }
            public string bulletinName { get; set; }
            public string description { get; set; }
            public int bulletinId { get; set; }
        }
        

        [HttpGet]
        public JsonResult QueryBulletin(BulletinRequest request)
        {
            var checkUsername = CheckUsername(request.username);
            if (checkUsername != null)
            {
                return checkUsername;
            }

            //Find the target project
            var project = _meshContext.Projects.FirstOrDefault(p => p.Id == request.projectId);
            if (project == null)
            {
                return JsonReturnHelper.ErrorReturn(401, "Invalid projectId.");
            }

            var bulletins = _meshContext.BulletinBoards
                .Where(b => b.ProjectId == request.projectId)
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

        [HttpPost]
        public JsonResult CreateBulletin(BulletinRequest request)
        {
            var checkUsername = CheckUsername(request.username);
            if (checkUsername != null)
            {
                return checkUsername;
            }

            if (!CornerCaseCheckHelper.Check(request.bulletinName, 50, CornerCaseCheckHelper.Title))
            {
                return JsonReturnHelper.ErrorReturn(402, "Invalid bulletinName.");
            }

            if (!CornerCaseCheckHelper.Check(request.description, 100, CornerCaseCheckHelper.Description))
            {
                return JsonReturnHelper.ErrorReturn(403, "Invalid Description.");
            }

            if (!CornerCaseCheckHelper.Check(request.projectId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(401, "Invalid projectId.");

            }
            
            var project = _meshContext.Projects.FirstOrDefault(u => u.Id == request.projectId);
            if (project == null)
            {
                return JsonReturnHelper.ErrorReturn(411, "Project does not exist.");
            }

            //Find bulletinBoard of this project
            var bulletinBoard = _meshContext.BulletinBoards.First(b => b.ProjectId == request.projectId);
            var user = _meshContext.Users.First(u => u.Email == request.username);
            if (_permissionCheck.CheckProjectPermission(request.username,project)!=PermissionCheckHelper.ProjectAdmin)
            {
                return JsonReturnHelper.ErrorReturn(421, "Permission denied.");
            }

            //Check if the bulletin already exists.
            var bulletin =
                _meshContext.Bulletins.FirstOrDefault(b => b.Title == request.bulletinName && b.BoardId == bulletinBoard.Id);
            if (bulletin != null)
            {
                return JsonReturnHelper.ErrorReturn(411, "Bulletin already exists.");
            }

            //Create the bulletin
            var newBulletin = new Bulletin()
            {
                Title = request.bulletinName,
                Content = request.description,
                BoardId = bulletinBoard.Id
            };
                
            //Update feed
            var feedUsers = _meshContext.Develops
                .Where(d => d.ProjectId == request.projectId)
                .ToList();
            
            //Start Transaction to save the bulletin
            using (var transaction = _meshContext.Database.BeginTransaction())
            {
                try
                {

                    _meshContext.Bulletins.Add(newBulletin);
                    _meshContext.SaveChanges();
                    
                    var bulletinId = newBulletin.Id;
                    var feedList = new List<BulletinFeed>();
                    foreach (var feedUser in feedUsers)
                    {
                       feedList.Add(new BulletinFeed()
                       {
                           UserId = feedUser.UserId,
                           BulletinId = bulletinId
                       });
                    }

                    _meshContext.AddRange(feedList);
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
        public JsonResult DeleteBulletin(BulletinRequest request)
        {
            var checkResult = CheckUsername(request.username);
            if (checkResult != null)
            {
                return checkResult;
            }
            
            if (!CornerCaseCheckHelper.Check(request.bulletinId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(401, "Invalid bulletinId.");
            }
            
            
            if (!CornerCaseCheckHelper.Check(request.projectId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(401, "Invalid projectId.");
            }
            
            //Check if target bulletin exists 
            var user = _meshContext.Users.First(u => u.Email == request.username);
            var project = _meshContext.Projects.Find(request.projectId);
            if (project == null)
            {
                return JsonReturnHelper.ErrorReturn(420, "Project does not exist.");
            }
            var bulletin = _meshContext.Bulletins.Find(request.bulletinId);
            if (bulletin == null)
            {
                return JsonReturnHelper.ErrorReturn(401, "Bulletin does not exist.");
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
        public JsonResult UpdateBulletin(BulletinRequest request)
        {
            var checkUsername = CheckUsername(request.username);
            if (checkUsername != null)
            {
                return checkUsername;
            }

            if (!CornerCaseCheckHelper.Check(request.bulletinId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(401, "Invalid bulletinId.");
            }

            if (!CornerCaseCheckHelper.Check(request.bulletinName, 50, CornerCaseCheckHelper.Title))
            {
                return JsonReturnHelper.ErrorReturn(402, "Invalid bulletinName.");
            }

            if (!CornerCaseCheckHelper.Check(request.description, 100, CornerCaseCheckHelper.Description))
            {
                return JsonReturnHelper.ErrorReturn(403, "Invalid Description.");
            }

            if (!CornerCaseCheckHelper.Check(request.projectId, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(401, "Invalid projectId.");
            }

            //Check if target bulletin exists 
            var bulletin = _meshContext.Bulletins.Find(request.bulletinId);
            var project = _meshContext.Projects.Find(request.projectId);
            var user = _meshContext.Users.First(u => u.Email == request.username);
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
                if (!request.bulletinName.IsNullOrEmpty())
                {
                    bulletin.Title = request.bulletinName;
                }

                if (!request.description.IsNullOrEmpty())
                {
                    bulletin.Content = request.description;
                }

                _meshContext.Bulletins.Update(bulletin);
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