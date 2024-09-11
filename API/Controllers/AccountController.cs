using System;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace API.Controllers;

public class AccountController(DataContext context) : BaseApiController
{   
    [HttpPost("register")] //account/register
    public async Task<ActionResult<AppUser>> Register(RegisterDTO registerDTO)
    {
        if (await UserExists(registerDTO.UserName)) return BadRequest("Username is taken");
        using var hmac = new HMACSHA512();
        var user = new AppUser
        {
            UserName = registerDTO.UserName.ToLower(),
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
            PasswordSalt = hmac.Key
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        return user;
    }
    [HttpPost("login")] 
    public async Task<ActionResult<AppUser>> Login(LoginDTO loginDTO)
    {
        var user = await context.Users.FirstOrDefaultAsync(x =>
            x.UserName == loginDTO.UserName.ToLower());

        if (user == null) return Unauthorized("Invaild Username");

        using var hmac = new HMACSHA512(user.PasswordSalt);

        var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));

        for (int i = 0; i < computeHash.Length; i++)
        {
            if(computeHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");
        }   
        return user;
    }

    private async Task<bool> UserExists(string username)
    {
        return await context.Users.AnyAsync(x => x.UserName.ToLower() == username.ToLower()); //Bob!=bob
    }
}
