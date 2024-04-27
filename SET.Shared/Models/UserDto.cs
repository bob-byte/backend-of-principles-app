using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SET.Shared.Models;

public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    public UserType UserType { get; set; }

    public string Email { get; set; }

    public Gender Gender { get; set; }
    public string MainSlogan { get; set; }
}