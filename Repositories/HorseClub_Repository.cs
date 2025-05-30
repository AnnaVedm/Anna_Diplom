using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TyutyunnikovaAnna_Diplom.Context;
using TyutyunnikovaAnna_Diplom.Models;

namespace TyutyunnikovaAnna_Diplom.Repositories
{
    public static class HorseClub_Repository
    {
        public static User LoadUser(string user_email, string login) //Подгрузка юзера для авторизации по email и login
        {
            using (var context = new DiplomHorseClubContext())
            {
                var user = context.Users.FirstOrDefault(a => a.Email == user_email || a.Login == user_email); //Ищем по email

                if (user != null)
                {
                    return user;
                }
            }
            return null;
        }
    }
}
