using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace SET.Shared.Models
{
    public class EntityWithId
    {
        [Key]
        public long Id { get; set; }

        public override bool Equals( object obj )
        {
            return obj is EntityWithId entity && entity.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}

