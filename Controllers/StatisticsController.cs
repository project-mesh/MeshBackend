using System;
using System.Collections.Generic;
using System.Linq;
using MeshBackend.Helpers;
using MeshBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MeshBackend.Controllers
{
    [ApiController]
    [Route("api/mesh/statistics")]
    [Produces("application/json")]
    public class StatisticsController:Controller
    {
        private readonly MeshContext _meshContext;
        private const int Male = 1;
        private const int Female = 2;
        private const int Unknown = 0;

        public StatisticsController(MeshContext meshContext)
        {
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

        public int GetAgeByBirthday(DateTime birthday)
        {
            var now = DateTime.Now;
            var age = DateTime.Now.Year - birthday.Year;
            if ((now.Month < birthday.Month) || (now.Month == birthday.Month && now.Day < birthday.Day))
            {
                --age;
            }
            return age >= 0 ? age : 0;
        }

        public class UserCountInfo
        {
            public int TotalUser { get; set; }
        }

        public class UserAge
        {
            public int Age { get; set; }
            public int UserCount { get; set; }
        }

        public class UserLocation
        {
            public string Location { get; set; }
            public int UserCount { get; set; }
        }
        
        [HttpGet]
        [Route("general")]
        public JsonResult QueryGeneralStatistics()
        {
            var currentOnlineUser = HttpContext.Session.Keys.Count();
            var avgTeamUser = _meshContext.Cooperations
                .GroupBy(c => c.TeamId)
                .Average(c => c.Key);
            var avgTeamProject = _meshContext.Projects
                .GroupBy(p => p.TeamId)
                .Average(p => p.Key);

            return Json(new
            {
                err_code = 0,
                data = new
                {
                    isSuccess = true,
                    msg = "",
                    currentOnlineUser = currentOnlineUser,
                    avgTeamUser = avgTeamUser,
                    avgTeamProject = avgTeamProject
                }
            });
        }


        [HttpGet]
        [Route("user")]
        public JsonResult QueryUserStatistics()
        {

            var users = _meshContext.Users.ToList();
            var maleUser = users.Count(u => u.Gender == Male);
            var femaleUser = users.Count(u => u.Gender == Female);
            var unknownUser = users.Count(u => u.Gender == Unknown);

            var userAgeList = new List<UserAge>();
            var userLocationList = new List<UserLocation>();


            foreach (var u in users)
            {
                var age = GetAgeByBirthday(u.Birthday);
                try
                {
                    var userAge = userAgeList.Find(a => a.Age == age);
                    ++userAge.UserCount;
                }
                catch 
                {
                    userAgeList.Add(new UserAge()
                    {
                        Age = age,
                        UserCount = 1
                    });
                }
                
                

                var location = u.Address.Split(" ")[0];
                var userLocation = userLocationList.Find(a => a.Location == location);
                if (userLocation == null)
                {
                    userLocationList.Add(new UserLocation()
                    {
                        Location = location,
                        UserCount = 1
                    });
                }
                else
                {
                    ++userLocation.UserCount;
                }

            }

            return Json(new
            {
                err_code = 0,
                data = new
                {
                    isSuccess = true,
                    msg = "",
                    maleUser=maleUser,
                    femaleUser=femaleUser,
                    unknownUser=unknownUser,
                    userAge = userAgeList,
                    userLocaion = userLocationList
                }
            });

        }

        [HttpGet]
        [Route("totaluser")]
        public JsonResult QueryTotalUser(int timeInterval, int itemCount)
        {
            if (timeInterval <= 0)
            {
                return JsonReturnHelper.ErrorReturn(457, "Invalid timeInterval.");
            }
            
            var currentTotalUser = _meshContext.Users.Count();

            var historyTotalUser = new List<UserCountInfo>();
            
            var totalUser = _meshContext.Users.ToList();
            var timeNow = DateTime.Now.Date;

            for (var i = 0; i < itemCount; ++i)
            {
                var userCount = totalUser.Count(u => u.CreatedTime.Date <= timeNow.AddDays(-i * timeInterval) &&
                                                     u.CreatedTime.Date >
                                                     timeNow.Date.AddDays(-(i + 1) * timeInterval));
                historyTotalUser.Add(new UserCountInfo()
                {
                    TotalUser = userCount
                });
            }

            return Json(new
            {
                err_code = 0,
                data = new
                {
                    isSuccess = true,
                    msg = "",
                    currentTotalUser = currentTotalUser,
                    historyTotalUser = historyTotalUser
                }
            });

        }


        [HttpGet]
        [Route("search-user")]
        public JsonResult QueryUserInfo(string username, string keyword)
        {
            var checkResult = CheckUsername(username);
            if (checkResult!=null)
            {
                return checkResult;
            }

            if (!CornerCaseCheckHelper.Check(keyword, 50, CornerCaseCheckHelper.Username))
            {
                return JsonReturnHelper.ErrorReturn(130, "Invalid keyword.");
            }

            var usersAccordingToUsername = _meshContext.Users
                .Where(u => u.Email.Contains(keyword));
            var usersAccordingToNickname = _meshContext.Users
                .Where(u => u.Nickname.Contains(keyword));
            var usersAccordingToDescription = _meshContext.Users
                .Where(u => u.Description.Contains(keyword));
            var usersUnion = usersAccordingToUsername
                .Union(usersAccordingToNickname)
                .Union(usersAccordingToDescription)
                .ToList();

            var users = new List<UserInfo>();
            
            foreach (var u in usersUnion)
            {
                var cooperation = _meshContext.Cooperations
                    .Where(c => c.UserId == u.Id)
                    .OrderByDescending(c => c.AccessCount)
                    .FirstOrDefault();
                var teams = _meshContext.Cooperations
                    .Where(c => c.UserId == u.Id)
                    .Join(_meshContext.Teams, c => c.TeamId, t => t.Id, (c, t) => new TeamInfo()
                    {
                        TeamId = t.Id,
                        TeamName = t.Name,
                        AdminId = t.AdminId,
                        AdminName = _meshContext.Users.First(s=>s.Id==t.AdminId).Nickname,
                        CreateTIme = t.CreatedTime.ToString()
                    })
                    .ToList();
                users.Add(new UserInfo()
                {
                    username = u.Email,
                    address = u.Address,
                    avatar = AvatarSaveHelper.GetObject(u.Avatar),
                    birthday = u.Birthday.ToString(),
                    description = u.Description,
                    preference = new UserPreference()
                    {
                        preferenceColor = u.ColorPreference,
                        preferenceLayout = u.LayoutPreference,
                        preferenceShowMode = u.RevealedPreference,
                        preferenceTeam = cooperation?.TeamId ?? -1
                    },
                    teams = teams
                });
            }

            return Json(new
            {
                err_code = 0,
                data = new
                {
                    isSuccess = true,
                    msg = "",
                    users = users
                }
            });

        }
        
        
    }
}