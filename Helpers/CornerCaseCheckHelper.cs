using System;
using System.Text.RegularExpressions;
using Castle.Core.Internal;
using Microsoft.AspNetCore.Mvc;

namespace MeshBackend.Helpers
{
    public static class CornerCaseCheckHelper
    {
        public const int Username = 0;
        public const int Title = 1;
        public const int Description = 2;
        public const int Id = 4;
        public const int Time = 5;
        public const int PassWord = 6;

        public static bool Check(object target, int limit,int attr)
        {
            if (target == null && attr != Description)
            {
                return false;
            }

            switch (attr)
            {
                case Username:
                    var username = target.ToString();
                    return !username.IsNullOrEmpty() && username.IndexOf(@"\s") == -1 && username.Length <= limit;
                case Title:
                    var title = target.ToString();
                    return !title.IsNullOrEmpty() && title.Length <= limit;
                case Description:
                    var description = target.ToString();
                    return description.IsNullOrEmpty() || description.Length <= limit;
                case Id:
                    var id = (int) target;
                    return id > 0;
                case Time:
                    var time = target.ToString();
                    try
                    {
                        var dateTime = Convert.ToDateTime(time);
                    }
                    catch 
                    {
                        return false;
                    }
                    return true;
                case PassWord:
                    var password = target.ToString();
                    return !password.IsNullOrEmpty();
                default:
                    return false;
            }
        }
    }
}