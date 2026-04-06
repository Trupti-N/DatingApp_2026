using System;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(AppDbConext conext, ITokenService tokenService) : BaseApiController
{
    [HttpPost("register")] //api/account/register
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if(await EmailExists(registerDto.Email)) 
            return BadRequest("Email taken");
        using var hmac = new HMACSHA512();
        var user = new AppUser
        {
            Email = registerDto.Email,
            DisplayName = registerDto.DisplayName,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key
        };

        conext.Users.Add(user);
        await conext.SaveChangesAsync();
        return user.ToDto(tokenService);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await conext.Users.SingleOrDefaultAsync(x=> x.Email == loginDto.Email);
        if(user == null)
            return Unauthorized("Invalid email address");
        using var hmac = new HMACSHA512(user.PasswordSalt);
        var ComputedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
        for(var i=0; i < ComputedHash.Length; i++)
        {
            if (ComputedHash[i] != user.PasswordHash[i])
                return Unauthorized("Invalid Password");
        }

        return user.ToDto(tokenService);
    }

    private async Task<bool> EmailExists(string Email)
    {
        return await conext.Users.AnyAsync(x => x.Email.ToLower() == Email.ToLower());
    }

}
