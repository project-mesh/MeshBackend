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
    [Route("api/mesh")]
    [Produces("application/json")]
    public class FeedController:Controller
    {
        private readonly MeshContext _meshContext;
        private readonly ILogger<FeedController> _logger;

        private const int BULLETIN = 0;
        private const int TASK = 1;
        
        public FeedController(ILogger<FeedController> logger, MeshContext meshContext)
        {
            _meshContext = meshContext;
            _logger = logger;
        }
        
        public JsonResult CheckUsername(string username)
        {
            if (!CornerCaseCheckHelper.Check(username,50,CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(104, "Invalid username.");
            }
            return HttpContext.Session.GetString(username) == null ? JsonReturnHelper.ErrorReturn(2, "User status error.") : null;
        }

        public class FeedRequest
        {
            public string username { get; set; }
            public int type { get; set; }
            public int id { get; set; }
        }
        
        [HttpGet]
        [Route("notification")]
        public JsonResult QueryNotification(string username)
        {
            var checkResult = CheckUsername(username);
            if (checkResult != null)
            {
                return checkResult;
            }

            var user = _meshContext.Users.First(u => u.Email==username);

            var bulletinFeeds = _meshContext.BulletinFeeds
                .Where(f => f.UserId == user.Id)
                .Join(_meshContext.Bulletins, f => f.BulletinId, b => b.Id, (f, b) => b)
                .Join(_meshContext.BulletinBoards, b => b.BoardId, bb => bb.Id, (b, bb) => new
                {
                    projectId = bb.ProjectId,
                    bulletin = b
                })
                .Join(_meshContext.Projects, b => b.projectId, p => p.Id, (b, p) => new
                {
                    project = p,
                    bulletin = b.bulletin
                })
                .Select(f=>new
                {
                    teamId = f.project.TeamId,
                    projectId = f.project.Id,
                    title = f.bulletin.Title,
                    description = f.bulletin.Content,
                    createdTime = f.bulletin.CreatedTime,
                }).ToList();

            var taskFeeds = _meshContext.TaskFeeds
                .Where(f => f.UserId == user.Id)
                .Join(_meshContext.Tasks, f => f.TaskId, t => t.Id, (f, t) => t)
                .Join(_meshContext.TaskBoards, t => t.BoardId, b => b.Id, (t, b) => new
                {
                    projectId = b.ProjectId,
                    task = t,
                })
                .Join(_meshContext.Projects, b => b.projectId, p => p.Id, (b, p) => new
                {
                    project = p,
                    task = b.task
                })
                .Select(f => new
                {
                    teamId = f.project.TeamId,
                    projectId = f.project.Id,
                    title = f.task.Name,
                    description = f.task.Description,
                    createdTime = f.task.CreatedTime,
                    isFinished = f.task.Finished,
                }).ToList();

            return Json(new
            {
                err_code = 0,
                data = new
                {
                    isSuccess = true,
                    msg = "",
                    notifications = new
                    {
                        bulletins = bulletinFeeds,
                        tasks = taskFeeds
                    }
                }
            });
        }

        [HttpDelete]
        [Route("notification")]
        public JsonResult DeleteNotification(string username, int type, int id)
        {
            var checkResult = CheckUsername(username);
            if (checkResult != null)
            {
                return checkResult;
            }

            if (!CornerCaseCheckHelper.Check(id, 0, CornerCaseCheckHelper.Id))
            {
                return JsonReturnHelper.ErrorReturn(910, "Invalid Id");
            }
            var user = _meshContext.Users.First(u => u.Email == username);

            switch (type)
            {
                case BULLETIN:
                {
                    var bulletin =
                        _meshContext.BulletinFeeds.FirstOrDefault(f => f.BulletinId == id && f.UserId == user.Id);
                    if (bulletin == null)
                    {
                        return JsonReturnHelper.ErrorReturn(901, "Invalid bulletinId");
                    }

                    try
                    {
                        _meshContext.BulletinFeeds.Remove(bulletin);
                        _meshContext.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e.ToString());
                        return JsonReturnHelper.ErrorReturn(1, "Unexpected error.");
                    }

                    return JsonReturnHelper.SuccessReturn();
                }
                case TASK:
                {
                    var task = _meshContext.TaskFeeds.FirstOrDefault(f => f.TaskId == id && f.UserId == user.Id);
                    if (task == null)
                    {
                        return JsonReturnHelper.ErrorReturn(902, "Invalid TaskId");
                    }

                    try
                    {
                        _meshContext.TaskFeeds.Remove(task);
                        _meshContext.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e.ToString());
                        return JsonReturnHelper.ErrorReturn(1, "Unexpected error.");
                    }

                    return JsonReturnHelper.SuccessReturn();
                }
                default:
                    return JsonReturnHelper.ErrorReturn(900, "Invalid type.");
            }
        }

    }
}