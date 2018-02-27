using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ConnectApi.Data;
using ConnectApi.Models;
using Newtonsoft.Json;

namespace connect_api.Data
{
    public class Seed
    {
        private readonly DataContext _context;
        public Seed(DataContext context) => this._context = context;

        public void SeedUser()
        {
            //_context.User.RemoveRange(_context.User);
            //_context.SaveChanges();

            //seed user
            string userData = File.ReadAllText("Data/connectSeed.json");
            var users = JsonConvert.DeserializeObject<List<User>>(userData);
            foreach (var user in users)
            {
                //create the password hash
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash("password", out passwordHash, out passwordSalt);

                user.PasswordHash = passwordHash;
                user.passwordSalt = passwordSalt;
                user.Username = user.Username.ToLower();
                this._context.User.Add(user);
            }

            this._context.SaveChanges();
        }
        void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }
    }
}