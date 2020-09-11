using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace MeshBackend.Helpers
{ 
    public static class JsonReturnHelper
    {
        public static JsonResult ErrorReturn(int code, string msg)
        {
            return new JsonResult(new
                {
                    err_code = code ,
                    data = new
                    {
                        isScuess = false,
                        msg = msg
                    }
                });
        }

        
        public static JsonResult SuccessReturn()
        {
            return new JsonResult(new
                {
                    err_code = 0,
                    data = new
                    {
                        isSuccess = true
                    }
                });
        }
    }

    public class UserInfo
    {
        public string username;
        public string nickname;
        public int gender;
        public int status;
        public string address;
        public string description;
        public string birthday;
        public string avatar;
        public string role;
        public UserPreference preference;
        public List<TeamInfo> teams;
    }

    public class UserPreference
    {
        public string preferenceShowMode;
        public string preferenceColor;
        public string preferenceLayout;
        public int preferenceTeam;
    }
    
    public class TeamInfo
    { 
        public string TeamName { get; set; }
        public int TeamId { get; set; }
        public int AdminId { get; set; }
        
        public string AdminName { get; set; }
        
        public string CreateTIme { get; set; }
        
    }


    
}