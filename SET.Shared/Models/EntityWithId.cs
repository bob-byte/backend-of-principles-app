using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SET.Shared.Models
{
    public class EntityWithId
    {
        public Guid Id { get; set; }

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

