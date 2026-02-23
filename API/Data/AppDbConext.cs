using System;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class AppDbConext(DbContextOptions options) : DbContext(options)
{
    public DbSet<AppUser> Users {get; set;}
}
